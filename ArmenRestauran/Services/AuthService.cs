using ArmenRestauran.Models;
using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmenRestauran.Services
{
    public class AuthService
    {
        private readonly string _connectionString = "Host=localhost;Username=postgres;Password=Passw0rd;Database=ArmenianRestaurant";

        public static User CurrentUser { get; private set; }

        public bool Login(string login, string password)
        {
            using var connection = new NpgsqlConnection(_connectionString);

            const string query = "SELECT * FROM Users WHERE Login = @login AND Password = @password";

            var user = connection.QuerySingleOrDefault<User>(query, new { login, password });

            if (user != null)
            {
                CurrentUser = user;
                return true;
            }

            return false;
        }

        public void Logout()
        {
            CurrentUser = null;
        }

        
        public bool IsAdmin() => CurrentUser?.RoleID == 1; 
    }
}
