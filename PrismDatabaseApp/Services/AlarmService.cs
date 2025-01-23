using PrismDatabaseApp.Data;
using PrismDatabaseApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PrismDatabaseApp.Services
{
    public class AlarmService
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public AlarmService(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        /// <summary>
        /// 알람 저장
        /// </summary>
        public async Task SaveAlarm(Alarm alarm)
        {
            if (alarm == null) throw new ArgumentNullException(nameof(alarm));

            using (var context = _dbContextFactory.CreateDbContext())
            {
                context.Alarms.Add(alarm);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 알람 조회 (최신순)
        /// </summary>
        public async Task<List<Alarm>> GetRecentAlarmsAsync(int count)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.Alarms
                                   .OrderByDescending(a => a.Id)
                                   .Take(count)
                                   .ToListAsync();
            }
        }
    }
}
