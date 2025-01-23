using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Mvvm;
using PrismDatabaseApp.Models;
using PrismDatabaseApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PrismDatabaseApp.ViewModels
{
    public class AlarmBarViewModel : BindableBase
    {
        private int slurryTankFlag = 3;
                                                                 // 각 알람 조건별로 타임스탬프 추가
        private DateTime lastAlarmTimestamp1 = DateTime.MinValue;
        private DateTime lastAlarmTimestamp2 = DateTime.MinValue;
        private DateTime lastAlarmTimestamp3 = DateTime.MinValue;
        private DateTime lastAlarmTimestamp4 = DateTime.MinValue;

        private readonly TimeSpan alarmInterval1 = TimeSpan.FromSeconds(180); // 알람 간격 설정 (예: 5초)
        private readonly TimeSpan alarmInterval2 = TimeSpan.FromSeconds(180); // 알람 간격 설정 (예: 5초)
        private readonly TimeSpan alarmInterval3 = TimeSpan.FromSeconds(180); // 알람 간격 설정 (예: 5초)
        private readonly TimeSpan alarmInterval4= TimeSpan.FromSeconds(180); // 알람 간격 설정 (예: 5초)


        private readonly ITcpSocketService _tcpSocketService;
        private readonly AlarmService _alarmService;

        public DelegateCommand<Alarm> DeleteAlarmCommand { get; }
        public ObservableCollection<Alarm> Alarms { get; } = new ObservableCollection<Alarm>();
        public DelegateCommand<Alarm> SendDetailsCommand { get; }

        public AlarmBarViewModel(ITcpSocketService tcpSocketService, AlarmService alarmService)
        {
            _tcpSocketService = tcpSocketService;
            _alarmService = alarmService;

            _tcpSocketService.DataReceived += OnDataReceived;
            DeleteAlarmCommand = new DelegateCommand<Alarm>(DeleteAlarm);
            SendDetailsCommand = new DelegateCommand<Alarm>(SendDetails);
        }

        private void SendDetails(Alarm alarm)
        {
            if (alarm != null)
            {
                Console.WriteLine($"Sending Details: Message={alarm.Message}, Timestamp={alarm.Timestamp}");
            }
        }

        private void DeleteAlarm(Alarm alarm)
        {
            if (alarm != null)
            {
                Alarms.Remove(alarm);
            }
        }

        private async void OnDataReceived(string data)
        {
            try
            {
                var jsonData = JObject.Parse(data);

                // SlurryTank 데이터 처리
                if (jsonData.ContainsKey("SlurryTank"))
                {
                    var slurryData = jsonData["SlurryTank"];
                    var remainingVolume = slurryData["RemainingVolume"]?.Value<double>() ?? 0;
                    var timestampString = slurryData["Timestamp"]?.ToString();
                    var temperature = slurryData["Temperature"]?.Value<double>() ?? 0;
                    var supplySpeed = slurryData["SupplySpeed"]?.Value<double>() ?? 0;

                   

                    DateTime timestamp;
                    if (!DateTime.TryParse(timestampString, out timestamp))
                    {
                        timestamp = DateTime.Now; // 변환 실패 시 현재 시간을 사용
                    }

                    if (remainingVolume <= 50)
                    {
                        if (DateTime.Now - lastAlarmTimestamp1 > alarmInterval1) // 시간 제한 확인
                        {
                            //slurryTankFlag--; // 플래그 감소
                            lastAlarmTimestamp1 = DateTime.Now; // 마지막 알람 시간 갱신

                            var newAlarm = new Alarm
                            {
                                Message = $"슬러리 탱크의 잔여량이 임계치 미만입니다.",
                                AlarmCode = "A001",
                                Value = remainingVolume,
                                Timestamp = timestamp
                            };

                            App.Current.Dispatcher.Invoke(() => Alarms.Add(newAlarm));

                            // 데이터베이스 저장
                            await SaveAlarmToDatabaseAsync(newAlarm);
                        }
                    }
                    if (temperature >= 29) // 
                    {
                        if (DateTime.Now - lastAlarmTimestamp2 > alarmInterval2) // 시간 제한 확인
                        {
                            //slurryTankFlag--; // 플래그 감소
                            lastAlarmTimestamp2 = DateTime.Now; // 마지막 알람 시간 갱신

                            var newAlarm = new Alarm
                            {
                                Message = $"슬러리 온도가 임계치 이상입니다.",
                                AlarmCode = "A002",
                                Value = temperature,
                                Timestamp = timestamp
                            };

                            App.Current.Dispatcher.Invoke(() => Alarms.Add(newAlarm));

                            // 데이터베이스 저장
                            await SaveAlarmToDatabaseAsync(newAlarm);
                        }
                    }
                    if (temperature <= 21) // 
                    {
                        if (DateTime.Now - lastAlarmTimestamp3 > alarmInterval3) // 시간 제한 확인
                        {
                            //slurryTankFlag--; // 플래그 감소
                            lastAlarmTimestamp3 = DateTime.Now; // 마지막 알람 시간 갱신

                            var newAlarm = new Alarm
                            {
                                Message = $"슬러리 온도가 임계치 이하입니다.",
                                AlarmCode = "A002",
                                Value = temperature,
                                Timestamp = timestamp
                            };

                            App.Current.Dispatcher.Invoke(() => Alarms.Add(newAlarm));

                            // 데이터베이스 저장
                            await SaveAlarmToDatabaseAsync(newAlarm);
                        }
                    }
                }
                if (jsonData.ContainsKey("CoatingProcess"))
                {
                    var CoatingData = jsonData["CoatingProcess"];
                    var CoatingThickness = CoatingData["Thickness"]?.Value<double>() ?? 0;
                    var timestampString = CoatingData["Timestamp"]?.ToString();
                    DateTime timestamp;
                    if (!DateTime.TryParse(timestampString, out timestamp))
                    {
                        timestamp = DateTime.Now; // 변환 실패 시 현재 시간을 사용
                    }
                    if (CoatingThickness >= 8) // 
                    {
                        if (DateTime.Now - lastAlarmTimestamp4 > alarmInterval4) // 시간 제한 확인
                        {
                            //slurryTankFlag--; // 플래그 감소
                            lastAlarmTimestamp4 = DateTime.Now; // 마지막 알람 시간 갱신

                            var newAlarm = new Alarm
                            {
                                Message = $"코팅 두께가 임계치 이상입니다.",
                                AlarmCode = "B001",
                                Value = CoatingThickness,
                                Timestamp = timestamp
                            };

                            App.Current.Dispatcher.Invoke(() => Alarms.Add(newAlarm));

                            // 데이터베이스 저장
                            await SaveAlarmToDatabaseAsync(newAlarm);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorAlarm = new Alarm
                {
                    Message = $"데이터 처리 중 오류 발생: {ex.Message}",
                    Timestamp = DateTime.Now // 현재 시간 사용
                };

                App.Current.Dispatcher.Invoke(() => Alarms.Add(errorAlarm));

                // 데이터베이스 저장
                await SaveAlarmToDatabaseAsync(errorAlarm);
            }
        }

        private async Task SaveAlarmToDatabaseAsync(Alarm alarm)
        {
            try
            {
                _alarmService.SaveAlarm(alarm);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB 저장 중 오류 발생: {ex.Message}");
            }
        }
    }
}
