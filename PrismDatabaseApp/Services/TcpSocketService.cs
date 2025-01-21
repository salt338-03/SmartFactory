using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;

public interface ITcpSocketService
{
    event Action<string> DataReceived;
    event Action<string> SlurrySupplyDataReceived;
    event Action<string> CoatingProcessDataReceived;
    event Action<string> DryingProcessDataReceived;


    void StopListening();
}

public class TcpSocketService : ITcpSocketService
{
    private TcpListener _listener;
    private bool _isRunning;
    private string _ip;
    private int _port;

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
            //Console.WriteLine("Client connected: {0}", client.Client.RemoteEndPoint);
            var stream = client.GetStream();
            var buffer = new byte[1024];

            while (_isRunning && client.Connected)
            {
                var byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (byteCount == 0) break;

                var data = Encoding.UTF8.GetString(buffer, 0, byteCount);
                //Console.WriteLine($"Raw data received: {data}");

                // JSON 데이터 파싱
                JObject jsonData = JObject.Parse(data);
                
                // SlurryTank 키 확인 후 이벤트 호출
                if (jsonData.ContainsKey("SlurryTank"))
                {
                    SlurrySupplyDataReceived?.Invoke(data);
                }

                // CoatingProcess 키 확인 후 이벤트 호출
                if (jsonData.ContainsKey("CoatingProcess"))
                {
                    CoatingProcessDataReceived?.Invoke(data);
                }

                // DryingProcess 키 확인 후 이벤트 호출
                if (jsonData.ContainsKey("DryingProcess"))
                {
                    DryingProcessDataReceived?.Invoke(data);
                }
                DataReceived?.Invoke(data);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client error: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Closing client connection.");
            client.Close();
        }
    }

}
