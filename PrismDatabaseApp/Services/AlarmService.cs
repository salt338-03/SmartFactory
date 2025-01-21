using PrismDatabaseApp.Data;
using PrismDatabaseApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrismDatabaseApp.Services
{
    public class AlarmService
    {
        private readonly AppDbContext _context;

        public AlarmService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// 알람 저장
        /// </summary>
        public void SaveAlarm(Alarm alarm)
        {
            if (alarm == null) throw new ArgumentNullException(nameof(alarm));

            _context.Alarms.Add(alarm);
            _context.SaveChanges();
        }

        /// <summary>
        /// 알람 조회 (최신순)
        /// </summary>
        public List<Alarm> GetRecentAlarms(int count)
        {
            return _context.Alarms
                           .OrderByDescending(a => a.Id)
                           .Take(count)
                           .ToList();
        }
    }
}
