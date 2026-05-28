using Grow2Go.Helpers;
using Grow2Go1.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace Grow2Go1.Repositories
{
    public class OrderRepository
    {
        // ── Get all orders for a farm (with customer name) ───────────────────
        public List<Order> GetOrdersByFarm(int farmId)
        {
            var orders = new List<Order>();
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT o.order_id, o.customer_id, o.farm_id,
                               o.total_amount, o.status, o.created_at,
                               u.full_name AS customer_name
                        FROM orders o
                        JOIN users u ON o.customer_id = u.user_id
                        WHERE o.farm_id = @farmId
                        ORDER BY o.created_at DESC";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@farmId", farmId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                orders.Add(new Order
                                {
                                    OrderId = reader.GetInt32("order_id"),
                                    CustomerId = reader.GetInt32("customer_id"),
                                    FarmId = reader.GetInt32("farm_id"),
                                    TotalAmount = reader.GetDecimal("total_amount"),
                                    Status = reader.GetString("status"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    CustomerName = reader.GetString("customer_name")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetOrdersByFarm error: " + ex.Message);
            }
            return orders;
        }

        // ── Get all items for a specific order ───────────────────────────────
        public List<OrderItem> GetOrderItems(int orderId)
        {
            var items = new List<OrderItem>();
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT oi.item_id, oi.order_id, oi.product_id,
                               oi.quantity, oi.unit_price,
                               p.product_name
                        FROM order_items oi
                        JOIN products p ON oi.product_id = p.product_id
                        WHERE oi.order_id = @orderId";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@orderId", orderId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(new OrderItem
                                {
                                    ItemId = reader.GetInt32("item_id"),
                                    OrderId = reader.GetInt32("order_id"),
                                    ProductId = reader.GetInt32("product_id"),
                                    ProductName = reader.GetString("product_name"),
                                    Quantity = reader.GetInt32("quantity"),
                                    UnitPrice = reader.GetDecimal("unit_price")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetOrderItems error: " + ex.Message);
            }
            return items;
        }

        // ── Update order status ──────────────────────────────────────────────
        public bool UpdateOrderStatus(int orderId, string status)
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    string query = "UPDATE orders SET status = @status WHERE order_id = @orderId";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@status", status);
                        cmd.Parameters.AddWithValue("@orderId", orderId);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateOrderStatus error: " + ex.Message);
                return false;
            }
        }
    }
}