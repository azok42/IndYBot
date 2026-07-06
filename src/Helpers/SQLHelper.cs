using MySqlConnector;

namespace IndYBot.Helpers;

public class SQLHelper
{
   private readonly string _connectionString;

   public SQLHelper(string connectionString)
   {
      _connectionString = connectionString;
   }

   public MySqlConnection CreateConnection()
   {
      return new MySqlConnection(_connectionString);
   }
}
