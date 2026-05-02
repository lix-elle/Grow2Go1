using System;
using MySql.Data.MySqlClient;

namespace Grow2Go.Helpers
{
    public class DBConnection
    {
        private static string connectionString =
            "Server=localhost;Database=grow2go;Uid=root;Pwd=12345;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        public static bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    Console.WriteLine("Database connected successfully!");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection failed: " + ex.Message);
                return false;
            }
        }
    }
}