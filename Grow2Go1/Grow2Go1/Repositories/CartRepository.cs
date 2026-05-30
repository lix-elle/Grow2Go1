using Grow2Go.Helpers;
using Grow2Go1.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace Grow2Go1.Repositories
{
    public class CartRepository
    {
        // ── Load cart from DB ─────────────────────────────────────────────
        public List<CartItem> GetCart(int customerId)
        {
            var items = new List<CartItem>();
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT c.quantity,
                               p.product_id, p.farm_id, p.product_name, p.price,
                               p.category, p.unit, p.image_path, p.description,
                               p.stock_quantity, p.is_available,
                               f.farm_name, f.profile_pic_path
                        FROM cart c
                        JOIN products p ON c.product_id = p.product_id
                        JOIN farms f    ON p.farm_id    = f.farm_id
                        WHERE c.customer_id = @customerId";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@customerId", customerId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(new CartItem
                                {
                                    Quantity = reader.GetInt32("quantity"),
                                    Product = new Product
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
                                        FarmName = reader.IsDBNull(reader.GetOrdinal("farm_name")) ? "" : reader.GetString("farm_name"),
                                        FarmProfilePicPath = reader.IsDBNull(reader.GetOrdinal("profile_pic_path")) ? "" : reader.GetString("profile_pic_path")
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetCart error: " + ex.Message);
            }
            return items;
        }

        // ── Add item to cart ──────────────────────────────────────────────
        public void AddToCart(int customerId, int productId, int quantity)
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    // If already exists, update quantity
                    string query = @"
                        INSERT INTO cart (customer_id, product_id, quantity)
                        VALUES (@customerId, @productId, @quantity)
                        ON DUPLICATE KEY UPDATE quantity = quantity + @quantity";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@customerId", customerId);
                        cmd.Parameters.AddWithValue("@productId", productId);
                        cmd.Parameters.AddWithValue("@quantity", quantity);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddToCart error: " + ex.Message);
            }
        }

        // ── Update item quantity ──────────────────────────────────────────
        public void UpdateQuantity(int customerId, int productId, int quantity)
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"UPDATE cart SET quantity = @quantity
                                     WHERE customer_id = @customerId
                                     AND product_id = @productId";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@quantity", quantity);
                        cmd.Parameters.AddWithValue("@customerId", customerId);
                        cmd.Parameters.AddWithValue("@productId", productId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateQuantity error: " + ex.Message);
            }
        }

        // ── Remove item from cart ─────────────────────────────────────────
        public void RemoveFromCart(int customerId, int productId)
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"DELETE FROM cart
                                     WHERE customer_id = @customerId
                                     AND product_id = @productId";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@customerId", customerId);
                        cmd.Parameters.AddWithValue("@productId", productId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("RemoveFromCart error: " + ex.Message);
            }
        }

        // ── Clear entire cart ─────────────────────────────────────────────
        public void ClearCart(int customerId)
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    string query = "DELETE FROM cart WHERE customer_id = @customerId";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@customerId", customerId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ClearCart error: " + ex.Message);
            }
        }
    }
}