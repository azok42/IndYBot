using Microsoft.Extensions.Hosting;
using IndYLib.Interfaces;
using IndYBot.Helpers;
using Dapper;

public class AutoEntryService : BackgroundService
{
   private readonly SQLHelper _sqlHelper;
   private readonly IIndyAuth _indyAuth;

   public AutoEntryService(SQLHelper sqlHelper, IIndyAuth indyAuth)
   {
      _sqlHelper = sqlHelper;
      _indyAuth = indyAuth;
   }

   protected override async Task ExecuteAsync(CancellationToken stoppingToken)
   {
      PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMinutes(10));

      while (await timer.WaitForNextTickAsync(stoppingToken))
      {
         try
         {
            var nowRounded = RoundUpTime(DateTime.Now);
            if (!IsBeforeIndyDay(nowRounded)) continue;

            var con = _sqlHelper.CreateConnection();

            var getterSql = "SELECT id, status FROM auto_entry WHERE time BETWEEN @TimeRangeBegin AND @TimeRangeEnd;";
            var autoEntries = await con.QueryAsync<(long Id, string Status)>(getterSql, new {
                  TimeRangeBegin = nowRounded.AddMinutes(-10).AddSeconds(1),
                  TimeRangeEnd = nowRounded
            });
            if (autoEntries == null || !autoEntries.Any()) return;

            await MakeEntries(autoEntries);
         }
         catch (Exception)
         {
            Console.WriteLine("Error at making auto entries");
            return;
         }
      }
   }

   private async Task MakeEntries(IEnumerable<(long Id, string Status)> autoEntries)
   {
      foreach (var autoEntry in autoEntries)
      {
         if (!autoEntry.Status.Equals("Enabled")) continue;

         var con = _sqlHelper.CreateConnection();
         
         var sql = "SELECT name, password FROM user WHERE id = @Id;";
         var userCredentials = await con.QueryFirstOrDefaultAsync<(string Name, string Password)>(sql, new { Id = autoEntry.Id });

         if (userCredentials == default)
            continue; // TODO: Set status to failed and tell user somehow

         var client = await _indyAuth.CreateClientAsync(userCredentials.Name, userCredentials.Password);

         // TODO: make actual entries
      }
   }

   private bool IsBeforeIndyDay(DateTime date)
   {
      if (date.DayOfWeek == DayOfWeek.Sunday ||
          date.DayOfWeek == DayOfWeek.Tuesday ||
          date.DayOfWeek == DayOfWeek.Thursday) return true;

      return false;
   }

   private DateTime RoundUpTime(DateTime time)
   {
      var ticks10Minutes = TimeSpan.FromMinutes(10).Ticks;
      long remainder = time.Ticks % ticks10Minutes;

      if (remainder == 0) return time;

      return time.AddTicks(ticks10Minutes - remainder);
   }
}
