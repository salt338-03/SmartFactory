using Microsoft.EntityFrameworkCore;
using PrismDatabaseApp.Data;
using PrismDatabaseApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrismDatabaseApp.Services
{
    /// <summary>
    /// 데이터베이스 조회를 위한 서비스 클래스
    /// </summary>
    public class NotificationInquiryService
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public NotificationInquiryService(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory), "DbContextFactory cannot be null.");
        }

        /// <summary>
        /// 날짜 범위와 배치번호에 따라 필터링된 데이터를 반환합니다.
        /// </summary>
        public async Task<List<Alarm>> GetProductsByDateRangeAndBatch(DateTime startDate, DateTime endDate)
        {
            // SQL 쿼리 생성
            StringBuilder query = new StringBuilder();
            query.Append("SELECT [id], [Message], [Timestamp],[AlarmCode],[Value] ");
            query.Append("FROM [SlurryCoatingDB].[dbo].[Alarms] ");
            query.Append("WHERE CAST([Timestamp] AS DATE) BETWEEN @StartDate AND @EndDate ");

            using (var context = _dbContextFactory.CreateDbContext())
            {
                // 비동기로 쿼리 실행
                return await context.NotificationInquiry
                                     .FromSqlRaw(query.ToString(), new object[]
                                     {
                                         new Microsoft.Data.SqlClient.SqlParameter("@StartDate", startDate.Date),
                                         new Microsoft.Data.SqlClient.SqlParameter("@EndDate", endDate.Date),
                                     })
                                     .AsNoTracking()
                                     .ToListAsync();
            }
        }
    }
}
