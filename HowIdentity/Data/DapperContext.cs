namespace HowIdentity.Data;

using Npgsql;

public class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public NpgsqlConnection InitConnection()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
        dataSourceBuilder.UseNodaTime();
        var dataSource = dataSourceBuilder.Build();
        
        var connection = dataSource.OpenConnection();
        return connection;
    }
}