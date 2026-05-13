using MySqlConnector;
using System.Data;

namespace H2BIG.Data
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        // Execute a query that returns a DataTable (for SELECT)
        public DataTable ExecuteQuery(string query, MySqlParameter[]? parameters = null)
        {
            using var connection = GetConnection();
            using var command = new MySqlCommand(query, connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            var dataTable = new DataTable();
            using var adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            return dataTable;
        }

        // Execute a command that returns number of affected rows (for INSERT, UPDATE, DELETE)
        public int ExecuteNonQuery(string query, MySqlParameter[]? parameters = null)
        {
            using var connection = GetConnection();
            connection.Open();
            using var command = new MySqlCommand(query, connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            return command.ExecuteNonQuery();
        }

        // Execute a command that returns a single value
        public object? ExecuteScalar(string query, MySqlParameter[]? parameters = null)
        {
            using var connection = GetConnection();
            connection.Open();
            using var command = new MySqlCommand(query, connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            return command.ExecuteScalar();
        }
    }
}
