using Prism.Mvvm;
using LiveCharts;
using LiveCharts.Defaults;
using System;
using PrismDatabaseApp.Models;
using Prism.Commands;
using System.Collections.ObjectModel;
using PrismDatabaseApp;
using LiveCharts.Wpf;

public class SlurrySupplyProcessViewModel : BindableBase
{
    private readonly ITcpSocketService _tcpSocketService;
    public SeriesCollection SpeedChartSeries { get; set; }
    public SeriesCollection TemperatureChartSeries { get; set; }
    public SeriesCollection VolumeChartSeries { get; set; }

    private double _SlurrySupplySpeed;
    public double SlurrySupplySpeed
    {
        get => _SlurrySupplySpeed;
        set => SetProperty(ref _SlurrySupplySpeed, value);
    }
    private double _SlurrySupplyTemperature;
    public double SlurrySupplyTemperature
    {
        get => _SlurrySupplyTemperature;
        set => SetProperty(ref _SlurrySupplyTemperature, value);
    }
    private double _SlurrySupplyVolume;
    public double SlurrySupplyVolume
    {
        get => _SlurrySupplyVolume;
        set => SetProperty(ref _SlurrySupplyVolume, value);
    }
    private double _xAxisMin;
    public double XAxisMin
    {
        get => _xAxisMin;
        set => SetProperty(ref _xAxisMin, value);
    }

    private double _xAxisMax;
    public double XAxisMax
    {
        get => _xAxisMax;
        set => SetProperty(ref _xAxisMax, value);
    }

    private DateTime _startTime;

    public Func<double, string> XAxisFormatter { get; set; }

    public SlurrySupplyProcessViewModel(ITcpSocketService tcpSocketService)
    {

        _tcpSocketService = tcpSocketService;

        // TcpSocketService 이벤트 구독
        _tcpSocketService.SlurrySupplyDataReceived -= OnSlurrySupplyDataReceived;
        _tcpSocketService.SlurrySupplyDataReceived += OnSlurrySupplyDataReceived;

        _startTime = DateTime.Now;

        // 그래프 데이터 초기화
        SpeedChartSeries = InitializeChartSeries("Speed");
        TemperatureChartSeries = InitializeChartSeries("Temperature");
        VolumeChartSeries = InitializeChartSeries("Volume");

        XAxisMin = 0;
        XAxisMax = 10;

        // X축 포맷터 설정
        XAxisFormatter = value => _startTime.AddSeconds(value).ToString("hh:mm:ss");
    }

    private SeriesCollection InitializeChartSeries(string title)
    {
        return new SeriesCollection
        {
            new LineSeries
            {
                Title = title,
                Values = new ChartValues<ObservablePoint>()
            }
        };
    }


    private void OnSlurrySupplyDataReceived(string data)
    {
        try
        {
            //Console.WriteLine($"OnSlurrySupplyDataReceived called with data: {data}");

            // JSON 데이터를 ProcessData 객체로 변환
            var processData = System.Text.Json.JsonSerializer.Deserialize<ProcessData>(data);

            // SlurryTank의 Timestamp를 DateTime으로 변환
            if (!DateTime.TryParse(processData.SlurryTank.Timestamp, out DateTime timestamp))
            {
                throw new FormatException("Invalid timestamp format.");
            }

            // X축 시간 계산 (초 단위)
            double elapsedTime = (timestamp - _startTime).TotalSeconds;
            SlurrySupplySpeed = processData.SlurryTank.SupplySpeed;
            SlurrySupplyTemperature = processData.SlurryTank.Temperature;
            SlurrySupplyVolume = processData.SlurryTank.RemainingVolume;
            // UI 스레드에서 데이터 추가
            App.Current.Dispatcher.Invoke(() => UpdateChartData(elapsedTime, processData));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnSlurrySupplyDataReceived: {ex.Message}");
        }
    }

    private void UpdateChartData(double elapsedTime, ProcessData processData)
    {
        // Slurry Supply Speed 데이터 추가
        SpeedChartSeries[0].Values.Add(new ObservablePoint(elapsedTime, processData.SlurryTank.SupplySpeed));
        
        // Slurry Supply Temperature 데이터 추가
        TemperatureChartSeries[0].Values.Add(new ObservablePoint(elapsedTime, processData.SlurryTank.Temperature));

        // Slurry Supply Volume 데이터 추가
        VolumeChartSeries[0].Values.Add(new ObservablePoint(elapsedTime, processData.SlurryTank.RemainingVolume));

        // 오래된 데이터 제거 (최신 30개 데이터 유지)
        TrimOldData(SpeedChartSeries[0].Values);
        TrimOldData(TemperatureChartSeries[0].Values);
        TrimOldData(VolumeChartSeries[0].Values);

        // X축 범위 업데이트
        XAxisMin = Math.Max(0, elapsedTime - 30); // 최신 30초 범위 유지
        XAxisMax = elapsedTime;
    }

    private void TrimOldData(IChartValues values)
    {
        while (values.Count > 30)
        {
            values.RemoveAt(0);
        }
    }
}
