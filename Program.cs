using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

public class MyEntity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    public string? Name { get; set; }

    // [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [ConcurrencyCheck]
    [Column(TypeName = "bigint")]
    public virtual long Version { get; set; }
}

public class MyDbContext : DbContext
{
    public static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
    });

    public DbSet<MyEntity> MyEntities { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connStringBuilder = new MySqlConnectionStringBuilder("Server=localhost;Database=myDatabase;Uid=root;Pwd=admin123;") { UseAffectedRows = true };
        optionsBuilder.UseMySQL(connStringBuilder.ConnectionString)
            .UseLoggerFactory(MyLoggerFactory)
            .EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // modelBuilder.Entity<MyEntity>()
        //     .HasKey(x => x.Id);

        // modelBuilder.Entity<MyEntity>()
        //     .Property(e => e.Version)
        //     .IsConcurrencyToken();
    }
}

public class Program
{
    public static async Task Main()
    {
        using var db = new MyDbContext();

        await db.Database.EnsureDeletedAsync();

        // Ensure that the database exists
        await db.Database.EnsureCreatedAsync();

        // Delete a non-existent entity
        var deletedEntity = new MyEntity { Id = 99999 };
        db.Entry(deletedEntity).State = EntityState.Deleted;

        try
        {
            var entitiesWritten = await db.SaveChangesAsync();
            Console.WriteLine($"the number of state entries written to the database: {entitiesWritten}");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle concurrency conflict
            var entry = ex.Entries.Single();
            var clientValues = (MyEntity)entry.Entity;
            var databaseEntry = entry.GetDatabaseValues();
            if (databaseEntry == null)
            {
                throw new DbUpdateConcurrencyException("The record you were trying to delete does not exist.", ex);
            }
            else
            {
                var databaseValues = (MyEntity)databaseEntry.ToObject();
                if (databaseValues.Version != clientValues.Version)
                {
                    throw new DbUpdateConcurrencyException("Concurrency conflict detected.", ex);
                }
            }
        }
    }
}
