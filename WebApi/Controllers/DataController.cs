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
            var connection = GetConnection();

            CreateTable(connection);

            if (request != null && DataHelper.IsValidUrl(request.Url))
            {
                try
                {
                    string responce = await client.GetStringAsync(request.Url);

                    var items = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(responce);

                    foreach (var item in items)
                    {
                        foreach (var key in item)
                        {
                            if (settings.ContainsKey(key.Key))
                            {
                                var value = key.Value;
                                SqlCommand Command = new SqlCommand("use ApiDatabase insert into PostegrySql (Name, Value) values('" + key.Key + "', '" + value + "')", connection);
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

            var exist = (int)command.ExecuteScalar();

            if (exist == 0)
            {
                command = new SqlCommand("use ApiDatabase create table PostegrySql (Id int IDENTITY(1,1) PRIMARY KEY , Name varchar(255), Value varchar(255))", connection);
                command.ExecuteNonQuery();
            }

        }

        private SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            var dbExist = new Server(connection.DataSource).Databases.Contains("ApiDatabase");

            if (dbExist == false)
            {
                SqlCommand command = new SqlCommand("CREATE DATABASE ApiDatabase", connection);

                command.ExecuteNonQuery();
            }

            return connection;

        }
       
        
    }
}
