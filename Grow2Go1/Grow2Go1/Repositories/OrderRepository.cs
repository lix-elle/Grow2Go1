using Grow2Go.Helpers;
using Grow2Go1.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

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
                                   u.full_name AS customer_name,
                                   u.profile_pic_path AS customer_profile_pic_path
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
                                    CustomerName = reader.GetString("customer_name"),
                                    CustomerProfilePicPath = reader.IsDBNull(reader.GetOrdinal("customer_profile_pic_path"))
                                        ? ""
                                        : reader.GetString("customer_profile_pic_path")
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
        public List<Order> GetOrdersByCustomer(int customerId)
        {
            var orders = new List<Order>();
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"
                SELECT o.order_id, o.farm_id, o.total_amount,
                       o.status, o.created_at, o.delivery_mode,
                       o.payment_method,o.estimated_delivery, f.farm_name
                FROM orders o
                JOIN farms f ON o.farm_id = f.farm_id
                WHERE o.customer_id = @customerId
                ORDER BY o.created_at DESC";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@customerId", customerId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                orders.Add(new Order
                                {
                                    OrderId = reader.GetInt32("order_id"),
                                    FarmId = reader.GetInt32("farm_id"),
                                    TotalAmount = reader.GetDecimal("total_amount"),
                                    Status = reader.GetString("status"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    DeliveryMode = reader.IsDBNull(reader.GetOrdinal("delivery_mode")) ? "" : reader.GetString("delivery_mode"),
                                    PaymentMethod = reader.IsDBNull(reader.GetOrdinal("payment_method")) ? "" : reader.GetString("payment_method"),
                                    FarmName = reader.IsDBNull(reader.GetOrdinal("farm_name")) ? "" : reader.GetString("farm_name"),
                                    EstimatedDelivery = reader.IsDBNull(reader.GetOrdinal("estimated_delivery"))
                                          ? (DateTime?)null
                                     : reader.GetDateTime("estimated_delivery")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetOrdersByCustomer error: " + ex.Message);
            }
            return orders;
        }
        public bool PlaceOrder(int customerId, List<CartItem> cartItems,
               string deliveryMode, string paymentMethod)
        {
            try
            {
                if (cartItems == null || cartItems.Count == 0)
                    return false;

                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Group cart items by farm.
                            // This creates ONE order per farm.
                            var groupedByFarm = cartItems
                                .GroupBy(item => item.Product.FarmId)
                                .ToList();

                            foreach (var farmGroup in groupedByFarm)
                            {
                                int farmId = farmGroup.Key;

                                decimal total = farmGroup.Sum(item =>
                                    item.Product.Price * item.Quantity);

                                DateTime estimatedDate = deliveryMode == "Pickup"
                                    ? DateTime.Now.AddDays(1)
                                    : DateTime.Now.AddDays(5);

                                // Insert ONE order for this farm
                                string orderQuery = @"
                            INSERT INTO orders 
                                (customer_id, farm_id, total_amount, 
                                 status, delivery_mode, payment_method,
                                 estimated_delivery)
                            VALUES 
                                (@customerId, @farmId, @total, 
                                 'pending', @delivery, @payment,
                                 @estimatedDelivery)";

                                int orderId;

                                using (var cmd = new MySqlCommand(orderQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@customerId", customerId);
                                    cmd.Parameters.AddWithValue("@farmId", farmId);
                                    cmd.Parameters.AddWithValue("@total", total);
                                    cmd.Parameters.AddWithValue("@delivery", deliveryMode);
                                    cmd.Parameters.AddWithValue("@payment", paymentMethod);
                                    cmd.Parameters.AddWithValue("@estimatedDelivery", estimatedDate.ToString("yyyy-MM-dd"));

                                    cmd.ExecuteNonQuery();
                                    orderId = (int)cmd.LastInsertedId;
                                }

                                // Insert all items from this farm under the same order_id
                                foreach (var item in farmGroup)
                                {
                                    string itemQuery = @"
                                INSERT INTO order_items 
                                    (order_id, product_id, quantity, unit_price)
                                VALUES 
                                    (@orderId, @productId, @quantity, @price)";

                                    using (var cmd = new MySqlCommand(itemQuery, conn, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@orderId", orderId);
                                        cmd.Parameters.AddWithValue("@productId", item.Product.ProductId);
                                        cmd.Parameters.AddWithValue("@quantity", item.Quantity);
                                        cmd.Parameters.AddWithValue("@price", item.Product.Price);

                                        cmd.ExecuteNonQuery();
                                    }

                                    // Reduce stock for each product
                                    string stockQuery = @"
                                UPDATE products 
                                SET stock_quantity = stock_quantity - @qty
                                WHERE product_id = @productId";

                                    using (var cmd = new MySqlCommand(stockQuery, conn, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@qty", item.Quantity);
                                        cmd.Parameters.AddWithValue("@productId", item.Product.ProductId);

                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine("PlaceOrder transaction error: " + ex.Message);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("PlaceOrder error: " + ex.Message);
                return false;
            }
        }
        public bool CancelOrder(int orderId)    
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string restoreStock = @"
                        UPDATE products p
                        JOIN order_items oi ON p.product_id = oi.product_id
                        SET p.stock_quantity = p.stock_quantity + oi.quantity
                        WHERE oi.order_id = @orderId";

                            using (var cmd = new MySqlCommand(restoreStock, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@orderId", orderId);
                                cmd.ExecuteNonQuery();
                            }

                            string cancelQuery = @"
                        UPDATE orders SET status = 'cancelled'
                        WHERE order_id = @orderId";

                            using (var cmd = new MySqlCommand(cancelQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@orderId", orderId);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("CancelOrder error: " + ex.Message);
                return false;
            }
        }
    }
}