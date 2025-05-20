using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient; // Changed from System.Data.SqlClient
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public DatabaseController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("backup")]
    public async Task<IActionResult> BackupDatabase()
    {
        string dbName = "NutriTrack";
        string folderPath = @"C:\DatabaseBackups";
        string fileName = $"{dbName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
        string backupPath = System.IO.Path.Combine(folderPath, fileName);

        string connectionString = _configuration.GetConnectionString("DefaultConnection");

        string sql = $@"
            BACKUP DATABASE [{dbName}]
            TO DISK = N'{backupPath}'
            WITH FORMAT, INIT, NAME = 'Backup of {dbName}', COMPRESSION";

        try
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            using SqlCommand command = new SqlCommand(sql, connection);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return Ok($"✅ The backup is created: {backupPath}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"❌ Error: {ex.Message}");
        }
    }
}