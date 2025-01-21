using Prism.Mvvm;
using LiveCharts;
using LiveCharts.Defaults;
using System;
using PrismDatabaseApp.Models;
using PrismDatabaseApp;
using LiveCharts.Wpf;
using Prism.Commands;
using System.Collections.ObjectModel;

namespace PrismDatabaseApp.ViewModels
{

    public class CoatingProcessViewModel : BindableBase
    {
        private readonly ITcpSocketService _tcpSocketService;
        public SeriesCollection CoatingSpeedChartSeries { get; set; }
        public SeriesCollection CoatingThicknessChartSeries { get; set; }

        private double _CoatingSpeed;
        public double CoatingSpeed
        {
            get => _CoatingSpeed;
            set => SetProperty(ref _CoatingSpeed, value);
        }
        private double _CoatingThickness;
        public double CoatingThickness
        {
            get => _CoatingThickness;
            set => SetProperty(ref _CoatingThickness, value);
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
        public CoatingProcessViewModel(ITcpSocketService tcpSocketService)
        {
            _tcpSocketService = tcpSocketService;

            // TcpSocketService 이벤트 구독
            _tcpSocketService.CoatingProcessDataReceived -= OnCoatingProcessDataReceived;
            _tcpSocketService.CoatingProcessDataReceived += OnCoatingProcessDataReceived;

            _startTime = DateTime.Now;

            CoatingSpeedChartSeries = InitializeChartSeries("Speed");
            CoatingThicknessChartSeries = InitializeChartSeries("Thickness");
            

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
        private void UpdateChartData(double elapsedTime, ProcessData processData)
        {
            //  CoatingSpeed 데이터 추가
            CoatingSpeedChartSeries[0].Values.Add(new ObservablePoint(elapsedTime, processData.CoatingProcess.Speed));

            // CoatingThickness 데이터 추가
            CoatingThicknessChartSeries[0].Values.Add(new ObservablePoint(elapsedTime, processData.CoatingProcess.Thickness));


            // 오래된 데이터 제거 (최신 30개 데이터 유지)
            TrimOldData(CoatingSpeedChartSeries[0].Values);
            TrimOldData(CoatingThicknessChartSeries[0].Values);
           
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
        private void OnCoatingProcessDataReceived(string data)
        {
            try
            {
                // JSON 데이터를 ProcessData 객체로 변환
                var processData = System.Text.Json.JsonSerializer.Deserialize<ProcessData>(data);

                // CoatingProcess의 Timestamp를 DateTime으로 변환
                if (!DateTime.TryParse(processData.CoatingProcess.Timestamp, out DateTime timestamp))
                {
                    throw new FormatException("Invalid timestamp format.");
                }

                // X축 시간 계산 (초 단위)
                double elapsedTime = (timestamp - _startTime).TotalSeconds;
                CoatingSpeed = processData.CoatingProcess.Speed;
                CoatingThickness = processData.CoatingProcess.Thickness;
                // UI 스레드에서 데이터 추가
                App.Current.Dispatcher.Invoke(() => UpdateChartData(elapsedTime, processData));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnCoatingProcessDataReceived: {ex.Message}");
            }
        }
    }
}
