using Prism.Commands;
using Prism.Mvvm;
using PrismDatabaseApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PrismDatabaseApp.ViewModels
{
    class NotificationInquiryViewModel : BindableBase
    {
        private readonly NotificationInquiryService _NotificationInquiryService;

        public ObservableCollection<Alarm> Alarm { get; set; } = new ObservableCollection<Alarm>();
        private bool _isSearching = false;
        // 2024년 1월 1일로 초기화
        private DateTime _startDate = new DateTime(2025, 1, 1);
        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        private DateTime _endDate = DateTime.Now;
        /// <summary>
        /// 검색 끝 날짜
        /// </summary>
        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public DelegateCommand SearchCommand { get; }

        public NotificationInquiryViewModel(NotificationInquiryService NotificationInquiryService)
        {
            _NotificationInquiryService = NotificationInquiryService ?? throw new ArgumentNullException(nameof(NotificationInquiryService), "NotificationInquiryService cannot be null.");

            // Search 메서드와 검색 버튼을 연결
            SearchCommand = new DelegateCommand(async () => await SearchAsync());

        }
        private async Task SearchAsync()
        {
            if (_isSearching) return;

            try
            {
                _isSearching = true;
                // UI 갱신 전에 기존 데이터를 초기화
                Alarm.Clear();



                // 데이터베이스에서 조건에 맞는 데이터를 가져옴
                var results = await Task.Run(() =>
                    _NotificationInquiryService.GetProductsByDateRangeAndBatch(StartDate, EndDate));

                // 조회된 데이터를 UI에 반영
                foreach (var product in results)
                {
                    Alarm.Add(product);
                }

                // 사용자에게 결과 알림
                MessageBox.Show($"{results.Count}개의 결과를 가져왔습니다.", "검색 완료", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // 예외 발생 시 사용자에게 알림
                MessageBox.Show($"데이터를 검색하는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isSearching = false;
            }
        }
    }
}
