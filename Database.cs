using System.Data;
using Microsoft.Data.SqlClient;

namespace SistemaPanol;

internal static class Database
{
    // Cambiar servidor/instancia según la instalación de SQL Server.
    private const string ConnectionString =
        @"Server=.\SQLEXPRESS;Database=PanolEscolar;Trusted_Connection=True;TrustServerCertificate=True;";

    public static SqlConnection GetConnection() => new(ConnectionString);

    public static DataTable Query(string sql, params SqlParameter[] parameters)
    {
        using var connection = GetConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);
        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public static int Execute(string sql, params SqlParameter[] parameters)
    {
        using var connection = GetConnection();
        connection.Open();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);
        return command.ExecuteNonQuery();
    }

    public static SqlParameter Param(string name, object? value) =>
        new(name, value ?? DBNull.Value);
}
