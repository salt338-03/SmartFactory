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
        private DateTime lastAlarmTimestamp = DateTime.MinValue; // 마지막 알람 발생 시간 초기화
        private readonly TimeSpan alarmInterval = TimeSpan.FromSeconds(300); // 알람 간격 설정 (예: 5초)

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

                    DateTime timestamp;
                    if (!DateTime.TryParse(timestampString, out timestamp))
                    {
                        timestamp = DateTime.Now; // 변환 실패 시 현재 시간을 사용
                    }

                    if (remainingVolume < 99) // 잔여량이 99L 이하
                    {
                        if ( DateTime.Now - lastAlarmTimestamp > alarmInterval) // 시간 제한 확인
                        {
                            //slurryTankFlag--; // 플래그 감소
                            lastAlarmTimestamp = DateTime.Now; // 마지막 알람 시간 갱신

                            var newAlarm = new Alarm
                            {
                                Message = $"잔여량이 임계치 미만입니다. 현재 잔여량 - {remainingVolume}L",
                                Timestamp = timestamp
                            };

                            App.Current.Dispatcher.Invoke(() => Alarms.Add(newAlarm));

                            // 데이터베이스 저장
                            await SaveAlarmToDatabaseAsync(newAlarm);
                        }
                    }
                    else // 잔여량이 99L 이상으로 회복
                    {
                        //slurryTankFlag = 3; // 플래그 초기화
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
