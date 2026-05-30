using Grow2Go.Helpers;
using Grow2Go1.Models;
using MySql.Data.MySqlClient;
using System;
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

        // ── Get all products for a specific farm ─────────────────────────────
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
                            Name = reader.GetString("product_name"),
                            Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description"),
                            Category = reader.IsDBNull(reader.GetOrdinal("category")) ? "" : reader.GetString("category"),
                            Unit = reader.IsDBNull(reader.GetOrdinal("unit")) ? "" : reader.GetString("unit"),
                            Price = reader.GetDecimal("price"),
                            Stock = reader.GetInt32("stock_quantity"),
                            IsAvailable = reader.GetBoolean("is_available"),
                            ImagePath = reader.IsDBNull(reader.GetOrdinal("image_path")) ? "" : reader.GetString("image_path")
                        });
                    }
                }
            }
            return products;
        }

        // ── Add a new product ────────────────────────────────────────────────
        public bool AddProduct(int farmId, string name, string category, string unit,
                               string description, decimal price, int stockQuantity,
                               string imagePath)
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"INSERT INTO products 
                                (farm_id, product_name, category, unit, description, 
                                 price, stock_quantity, is_available, image_path) 
                                VALUES (@farmId, @name, @category, @unit, @desc, 
                                        @price, @stock, 1, @imagePath)";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@farmId", farmId);
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@category", category);
                        cmd.Parameters.AddWithValue("@unit", unit);
                        cmd.Parameters.AddWithValue("@desc", description);
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.Parameters.AddWithValue("@stock", stockQuantity);
                        cmd.Parameters.AddWithValue("@imagePath", imagePath);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddProduct error: " + ex.Message);
                return false;
            }
        }

        // ── Update an existing product ───────────────────────────────────────
        public bool UpdateProduct(int productId, string name, string category, string unit,
                                  string description, decimal price, int stockQuantity,
                                  string imagePath)
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"UPDATE products SET
                                product_name   = @name,
                                category       = @category,
                                unit           = @unit,
                                description    = @desc,
                                price          = @price,
                                stock_quantity = @stock,
                                image_path     = @imagePath
                                WHERE product_id = @productId";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@productId", productId);
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@category", category);
                        cmd.Parameters.AddWithValue("@unit", unit);
                        cmd.Parameters.AddWithValue("@desc", description);
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.Parameters.AddWithValue("@stock", stockQuantity);
                        cmd.Parameters.AddWithValue("@imagePath", imagePath);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateProduct error: " + ex.Message);
                return false;
            }
        }

        public bool DeleteProduct(int productId)
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    string query = "DELETE FROM products WHERE product_id = @productId";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@productId", productId);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DeleteProduct error: " + ex.Message);
                return false;
            }
        }

        public List<Product> GetAllAvailableProducts()
        {
            var products = new List<Product>();

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"
                  SELECT p.product_id, p.farm_id, p.product_name, p.price, p.category,
                   p.unit, p.image_path, p.description, p.stock_quantity, p.is_available,
                  f.farm_name, f.profile_pic_path
            FROM products p
            JOIN farms f ON p.farm_id = f.farm_id
            WHERE p.is_available = 1 AND p.stock_quantity > 0
            ORDER BY p.product_name";

                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            ProductId = reader.GetInt32("product_id"),
                            FarmId = reader.GetInt32("farm_id"),
                            Name = reader.GetString("product_name"),
                            Price = reader.GetDecimal("price"),
                            Category = reader.IsDBNull(reader.GetOrdinal("category")) ? "" : reader.GetString("category"),
                            Unit = reader.IsDBNull(reader.GetOrdinal("unit")) ? "" : reader.GetString("unit"),
                            ImagePath = reader.IsDBNull(reader.GetOrdinal("image_path")) ? "" : reader.GetString("image_path"),
                            Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description"),
                            Stock = reader.GetInt32("stock_quantity"),
                            IsAvailable = reader.GetBoolean("is_available"),
                            FarmName = reader.GetString("farm_name"),
                            FarmProfilePicPath = reader.IsDBNull(reader.GetOrdinal("profile_pic_path"))
                     ? "" : reader.GetString("profile_pic_path")
                        });

                    }
                }
            }

            return products;
        }
    }
}