namespace Grow2Go.Helpers
{
    public static class SessionManager
    {
        public static int UserId { get; set; }
        public static string FullName { get; set; }
        public static string Email { get; set; }
        public static string Role { get; set; }   // "farmer" or "customer"
        public static int FarmId { get; set; }    // Only used if role = farmer

        public static void Clear()
        {
            UserId = 0;
            FullName = string.Empty;
            Email = string.Empty;
            Role = string.Empty;
            FarmId = 0;
        }
    }
}