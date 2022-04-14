using Microsoft.SqlServer.Management.Smo;
using WebApi.Helpers;
using WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Text.Json;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataController : ControllerBase
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly String connectionString = DataHelper.GetConnectionString();
        private static readonly Dictionary<string, string> settings = DataHelper.GetAppSettings();

        [HttpPost]
        public async Task<string> Post(RequestModel request)
        {
            if (request != null && DataHelper.IsValidUrl(request.Url))
            {
                var connection = GetConnection();

                CreateTable(connection);

                try
                {
                    string responce = await client.GetStringAsync(request.Url);

                    var items = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(responce);

                    foreach (var item in items)
                    {
                        foreach (var keyValue in item)
                        {
                            if (settings.ContainsKey(keyValue.Key))
                            {
                                SqlCommand Command = new SqlCommand("use ApiDatabase insert into PostegrySql (Name, Value) values('" + keyValue.Key + "', '" + keyValue.Value + "')", connection);
                                Command.ExecuteNonQuery();
                            }
                        }
                    }
                    return ("Ok");
                }
                catch (Exception e)
                {
                    return (e.Message);
                }
            }
            else return ("Url is empty");
        }

        private void CreateTable(SqlConnection connection)
        {

            SqlCommand command = new SqlCommand("use ApiDatabase SELECT count(*) as Exist from INFORMATION_SCHEMA.TABLES where table_name = 'PostegrySql'", connection);

            var tableExists = (int)command.ExecuteScalar();

            if (tableExists == 0)
            {
                command = new SqlCommand("use ApiDatabase create table PostegrySql (Id int IDENTITY(1,1) PRIMARY KEY , Name varchar(255), Value varchar(255))", connection);
                command.ExecuteNonQuery();
            }

        }

        private SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            var dbExists = new Server(connection.DataSource).Databases.Contains("ApiDatabase");

            if (!dbExists)
            {
                SqlCommand command = new SqlCommand("CREATE DATABASE ApiDatabase", connection);

                command.ExecuteNonQuery();
            }

            return connection;

        }
    }
}