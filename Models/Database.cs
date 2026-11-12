using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;

namespace S_L.Models
{
    public class Database
    {
        private static readonly string connectionString =
            ConfigurationManager.ConnectionStrings["StrongLightDB"].ConnectionString;

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}