using System;
using MySql.Data.MySqlClient;
using Grow2Go.Models;
using Grow2Go.Helpers;

namespace Grow2Go.Repositories
{
    public class UserRepository
    {
        // ── LOGIN: Find user by email + password + role ──────────────────────
        public User GetUserByCredentials(string email, string password, string role)
        {
            User user = null;
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT * FROM users WHERE email=@email AND password=@password AND role=@role";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@role", role);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user = new User
                                {
                                    UserId = Convert.ToInt32(reader["user_id"]),
                                    FullName = reader["full_name"].ToString(),
                                    Email = reader["email"].ToString(),
                                    Role = reader["role"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Login error: " + ex.Message);
            }
            return user;
        }

        // ── REGISTER: Check if email already exists ───────────────────────────
        public bool EmailExists(string email)
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM users WHERE email=@email";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EmailExists error: " + ex.Message);
                return false;
            }
        }

        // ── REGISTER: Insert new user into DB ─────────────────────────────────
        public bool CreateUser(string fullName, string email, string password, string role)
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO users (full_name, email, password, role) VALUES (@name, @email, @pass, @role)";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", fullName);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@pass", password);
                        cmd.Parameters.AddWithValue("@role", role);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("CreateUser error: " + ex.Message);
                return false;
            }
        }

        // ── Get user's farm ID (for farmers) ──────────────────────────────────
        public int GetFarmIdByUserId(int userId)
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT farm_id FROM farms WHERE user_id=@userId LIMIT 1";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        var result = cmd.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : 0;
                    }
                }
            }
            catch { return 0; }
        }

        // ── Create a farm entry for new farmer ────────────────────────────────
        public bool CreateFarm(int userId, string farmName)
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO farms (user_id, farm_name) VALUES (@uid, @name)";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", userId);
                        cmd.Parameters.AddWithValue("@name", farmName + "'s Farm");
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("CreateFarm error: " + ex.Message);
                return false;
            }
        }

        // ── Get last inserted user ID ─────────────────────────────────────────
        public int GetLastInsertedUserId(string email)
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT user_id FROM users WHERE email=@email";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        var result = cmd.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : 0;
                    }
                }
            }
            catch { return 0; }
        }
    }
}