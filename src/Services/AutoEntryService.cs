using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using IndYLib.Interfaces;
using IndYLib.Exceptions;
using IndYBot.Helpers;
using Dapper;

namespace IndYBot.Services;

public class AutoEntryService : BackgroundService
{
   private readonly SQLHelper _sqlHelper;
   private readonly IIndyAuth _indyAuth;
   private readonly DiscordSocketClient _discordClient;

   public AutoEntryService(SQLHelper sqlHelper, IIndyAuth indyAuth, DiscordSocketClient discordClient)
   {
      _sqlHelper = sqlHelper;
      _indyAuth = indyAuth;
      _discordClient = discordClient;
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
            var autoEntries = await con.QueryAsync<(ulong Id, string Status)>(getterSql, new {
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

   private async Task MakeEntries(IEnumerable<(ulong Id, string Status)> autoEntries)
   {
      foreach (var autoEntry in autoEntries)
      {
         if (!autoEntry.Status.Equals("Enabled")) continue;

         var userId = autoEntry.Id;

         var con = _sqlHelper.CreateConnection();
         
         var sql = "SELECT name, password FROM user WHERE id = @Id;";
         var userCredentials = await con.QueryFirstOrDefaultAsync<(string Name, string Password)>(sql, new { Id = userId });

         if (userCredentials == default)
         {
            await SetFailedStatus(userId);
            await NotifyUser(userId, "You need to save your user credentials with '/save'.");
            continue; 
         }

         var client = await _indyAuth.CreateClientAsync(userCredentials.Name, userCredentials.Password);

         (string Teacher, string Subject, string Activity) entryParams;
         var entryDayName = DateTime.Today.AddDays(1).DayOfWeek.ToString();
         try
         {
            entryParams = await GetEntryParameters(userId, entryDayName);
         }
         catch (Exception)
         {
            await SetFailedStatus(userId);
            await NotifyUser(userId, "You need to save standards with '/standard set'.");
            continue;
         }

         var success = await TryMakeEntry(client, entryParams.Teacher, entryParams.Subject, entryParams.Activity);
         if (!success.Success)
         {
            await SetFailedStatus(userId);
            await NotifyUser(userId, "Something went wrong at entry making! Check things like values in your standards with '/standard list'.");
            continue;
         }
      }
   }

   private async Task SetFailedStatus(ulong userId)
   {
      var con = _sqlHelper.CreateConnection();

      var sql = "UPDATE auto_entry SET status = 'Failed' WHERE id = @Id;";
      await con.QueryAsync(sql, new { Id = userId });
   }

   private async Task NotifyUser(ulong userId, string msg)
   {
      var con = _sqlHelper.CreateConnection();

      var sql = "SELECT guild.default_channel, guild.auto_entry_channel FROM guild INNER JOIN user_guild ON guild.id = user_guild.guild_id WHERE user_id = @UserId;";
      var result = await con.QueryFirstOrDefaultAsync<(ulong DefaultChannel, ulong AutoEntryChannel)>(sql, new { UserId = userId });

      IMessageChannel? channel;
      if (result.AutoEntryChannel != default)
         channel = _discordClient.GetChannel(result.AutoEntryChannel) as IMessageChannel;
      else
         channel = _discordClient.GetChannel(result.DefaultChannel) as IMessageChannel;

      var user = await _discordClient.GetUserAsync(userId);

      await channel!.SendMessageAsync($"{user.Mention}{msg}", allowedMentions: AllowedMentions.All);
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

   private async Task<(bool Success, string FailReason)> TryMakeEntry(IIndyClient client, string tid, string subject, string activity)
   {
      try {
         await client.MakeNormalEntryAsync(DateOnly.FromDateTime(DateTime.Today), tid, subject, activity);

         return (true, "");
      }
      catch (NotFoundException)
      {
         return (false, "No hour for this teacher on this day!" );
      }
      catch (InvalidIndyDayException)
      {
         return (false, "Not a valid IndY-Day!");
      }
      catch (Exception)
      {
         return (false, "Something went wrong!");
      }
   }

   private async Task<(string TeacherId, string Subject, string Activity)> GetEntryParameters(ulong id, string dayName)
   {
      var con = _sqlHelper.CreateConnection();

      var sql = $"SELECT type, value FROM user_standard WHERE id = @Id AND type LIKE '{dayName}%' OR type LIKE 'Global%';";
      var queryResult = await con.QueryAsync<(string Type, string Value)>(sql, new { Id = id });
      var standards = queryResult.ToDictionary(x => x.Type, x => x.Value);

      var daySpecificStandards = standards.Where(x => x.Key.Contains(dayName)).ToDictionary();
      try
      {
         return GetStandardValues(dayName, daySpecificStandards);
      }
      catch (Exception)
      {
         var globalStandards = standards.Where(x => x.Key.Contains("Global")).ToDictionary();
         return GetStandardValues("Global", globalStandards);
      }
   }
   
   private (string TeacherId, string Subject, string Activity) GetStandardValues(string standardType, Dictionary<string, string> standards)
   {
      if (standards.Count() != 3)
         throw new Exception($"Standards of type: {standardType}, are not fully made!");

      var teacher = standards.GetValueOrDefault($"{standardType}Teacher");
      if (string.IsNullOrEmpty(teacher))
         throw new Exception($"Standard '{standardType}Teacher' is empty!");

      var subject = standards.GetValueOrDefault($"{standardType}Subject");
      if (string.IsNullOrEmpty(subject))
         throw new Exception($"Standard '{standardType}Subject' is empty!");

      var activity = standards.GetValueOrDefault($"{standardType}Description");
      if (string.IsNullOrEmpty(activity))
         throw new Exception($"Standard '{standardType}Description' is empty!");

      return (teacher, subject, activity);
   }
}
