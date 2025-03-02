using MongoDB.Driver;
using System;
using Microsoft.Extensions.Configuration;

public sealed class MongoConnectionManager
{
    private static readonly Lazy<IMongoClient> _lazyClient =
        new(() =>
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration["MongoDB:ConnectionString"];
            Console.WriteLine($"Attempting to update configuration for {connectionString}");

            return new MongoClient(connectionString);
        });

    public static IMongoClient Client => _lazyClient.Value;

    private MongoConnectionManager() { }
}
