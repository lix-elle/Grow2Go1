using MySql.Data.MySqlClient;
using Grow2Go1.Models;
using System.Collections.Generic;

namespace Grow2Go1.Repositories
{
    public class ProductRepository
    {
        private string _connectionString;

        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Product> GetProductsByFarm(int farmId)
        {
            var products = new List<Product>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM products WHERE farm_id = @farmId";
                var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@farmId", farmId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            ProductId = reader.GetInt32("product_id"),
                            FarmId = reader.GetInt32("farm_id"),
                            Name = reader.GetString("name"),
                            Price = reader.GetDecimal("price"),
                            Stock = reader.GetInt32("stock")
                        });
                    }
                }
            }
            return products;
        }
    }
}
