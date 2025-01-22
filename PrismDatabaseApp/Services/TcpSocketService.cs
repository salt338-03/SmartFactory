using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public interface ITcpSocketService
{
    event Action<string> DataReceived;
    event Action<string> SlurrySupplyDataReceived;
    event Action<string> CoatingProcessDataReceived;
    event Action<string> DryingProcessDataReceived;

    void Configure(string ip, int port);
    void StartListening();
    void StopListening();
}

public class TcpSocketService : ITcpSocketService
{
    private TcpListener _listener;
    private bool _isRunning;
    private string _ip;
    private int _port;
    private string _dataBuffer = string.Empty; // Buffer to handle partial or multiple JSON data

    public event Action<string> DataReceived;
    public event Action<string> SlurrySupplyDataReceived;
    public event Action<string> CoatingProcessDataReceived;
    public event Action<string> DryingProcessDataReceived;

    public void Configure(string ip, int port)
    {
        _ip = ip == "IPAddress.Any" ? "0.0.0.0" : ip;
        _port = port;
    }

    public void StartListening()
    {
        if (string.IsNullOrEmpty(_ip) || _port == 0)
            throw new InvalidOperationException("IP and Port must be configured before starting the service.");

        _listener = new TcpListener(IPAddress.Parse(_ip), _port);
        _listener.Start();
        _isRunning = true;

        Task.Run(async () =>
        {
            while (_isRunning)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    _ = HandleClientAsync(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Listener error: {ex.Message}");
                    await Task.Delay(1000);
                }
            }
        });
    }

    public void StopListening()
    {
        _isRunning = false;
        _listener?.Stop();
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
            var stream = client.GetStream();
            var buffer = new byte[4096]; // 데이터 읽기 버퍼
            var dataBuffer = new List<byte>(); // 수신된 데이터를 저장할 버퍼

            while (_isRunning && client.Connected)
            {
                var byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (byteCount == 0) break;

                // 수신된 데이터를 dataBuffer에 추가
                dataBuffer.AddRange(buffer.Take(byteCount));

                // 길이 정보 처리
                while (dataBuffer.Count >= 4) // 최소 4바이트(길이 정보)가 있어야 처리 가능
                {
                    var lengthBytes = dataBuffer.Take(4).ToArray();
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(lengthBytes);
                    }

                    int messageLength = BitConverter.ToInt32(lengthBytes, 0);
                    if (dataBuffer.Count - 4 < messageLength)
                    {
                        // 메시지 데이터가 완전히 수신되지 않음
                        break;
                    }

                    // 완전한 메시지 추출
                    var jsonDataBytes = dataBuffer.Skip(4).Take(messageLength).ToArray();
                    var jsonString = Encoding.UTF8.GetString(jsonDataBytes);

                    // 처리된 데이터 제거
                    dataBuffer.RemoveRange(0, 4 + messageLength);

                    try
                    {
                        // JSON 데이터 파싱
                        var jsonData = JObject.Parse(jsonString);

                        // 이벤트 트리거
                        if (jsonData.ContainsKey("SlurryTank"))
                            SlurrySupplyDataReceived?.Invoke(jsonString);

                        if (jsonData.ContainsKey("CoatingProcess"))
                            CoatingProcessDataReceived?.Invoke(jsonString);

                        if (jsonData.ContainsKey("DryingProcess"))
                            DryingProcessDataReceived?.Invoke(jsonString);

                        DataReceived?.Invoke(jsonString);
                    }
                    catch (Exception parseEx)
                    {
                        Console.WriteLine($"JSON Parsing error: {parseEx.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client handling error: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Closing client connection.");
            client.Close();
        }
    }

}
