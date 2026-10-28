using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace ServiceB.Controllers
{
    [ApiController]
    [Route("Api")]
    public class ApiController : ControllerBase
    {
        private readonly string connectionString;
        
        public ApiController(IConfiguration configuration)
        {
            var builder = new SqlConnectionStringBuilder(configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Missing connection string"));

            var password = configuration["MSSQL_SA_PASSWORD"];

            if (!string.IsNullOrEmpty(password))
            {
                builder.Password = password;
            }

            connectionString = builder.ConnectionString;
        }


        /// <summary>
        /// Returns all the notes from the database.
        /// </summary>
        /// <returns>A list of TestRow objects containing notes.</returns>
        [HttpGet("GetNotes")]
        public async Task<ActionResult<List<TestRow>>> GetNotes()
        {
            var rows = new List<TestRow>();

            await using var connection = new SqlConnection(connectionString);

            await connection.OpenAsync();

            const string sql = "SELECT Id, Note, CreatedAt FROM Notes";

            await using var command = new SqlCommand(sql, connection);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                rows.Add(new TestRow
                {
                    Id = reader.GetInt32(0),
                    Note = reader.GetString(1),
                    CreatedAt = reader.GetDateTime(2)
                });
            }

            return Ok(rows);
        }

        /// <summary>
        /// Inserts a new note into the database.
        /// </summary>
        /// <param name="note">The note to insert.</param>
        /// <returns>An IActionResult indicating the outcome of the operation.</returns>
        [HttpPost("InsertNote")]
        public async Task<IActionResult> InsertNote([FromBody] string note)
        {
            await using var connection = new SqlConnection(connectionString);

            await connection.OpenAsync();

            const string sql = "INSERT INTO Notes (Note) VALUES (@Note)";

            await using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@Note", SqlDbType.NVarChar, 200).Value = note;

            await command.ExecuteNonQueryAsync();

            return Ok("Inserted");
        }


        /// <summary>
        /// Represents a row in the Notes table.
        /// </summary>
        public class TestRow
        {
            public int Id { get; set; }
            public string Note { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
