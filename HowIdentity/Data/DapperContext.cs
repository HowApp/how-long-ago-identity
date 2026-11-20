namespace HowIdentity.Data;

using Npgsql;

public class DapperContext : IDisposable, IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    public DapperContext(string connectionString)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseNodaTime();
        _dataSource = dataSourceBuilder.Build();
    }
    
    public NpgsqlConnection InitConnection()
    {
        return _dataSource.OpenConnection();
    }

    public ValueTask<NpgsqlConnection> InitConnectionAsync(CancellationToken cancellationToken = default)
    {
        return _dataSource.OpenConnectionAsync(cancellationToken);
    }

    public void Dispose()
    {
        _dataSource.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return _dataSource.DisposeAsync();
    }
}