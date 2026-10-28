namespace ServiceA.Controllers
{

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;
    using System.Net.Http.Json;

    namespace ServiceA.Controllers
    {
        [ApiController]
        [Route("Api")]
        public class ApiController : ControllerBase
        {
            private readonly HttpClient _client;
            private readonly ILogger<ApiController> _logger;

            public ApiController(
                IHttpClientFactory factory,
                ILogger<ApiController> logger)
            {
                _client = factory.CreateClient("ServiceB");
                _logger = logger;
            }

            /// <summary>
            /// Forwards a request to Service B to get all notes from the database.
            /// </summary>
            /// <returns>An IActionResult containing the notes loaded from Service B.</returns>
            
            [HttpGet("GetNotes")]
            public async Task<IActionResult> GetNotes()
            {
                try
                {
                    var response = await _client.GetAsync(
                        "/Api/GetNotes");

                    var content = await response.Content
                        .ReadAsStringAsync();

                    return new ContentResult
                    {
                        Content = content,
                        ContentType = "application/json",
                        StatusCode = (int)response.StatusCode
                    };
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Could not load notes from Service B");

                    return StatusCode(502, "The Service B is unavailable");
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogError(ex, "Timeout when calling Service B");

                    return StatusCode(504, "The Service B timed out");
                }
            }

            /// <summary>
            /// Forwards a note to ServiceB so that it can be inserted into the database.
            /// </summary>
            /// <param name="note">The note to insert.</param>
            /// <returns>An IActionResult indicating the outcome of the operation.</returns>
            [HttpPost("InsertNote")]
            public async Task<IActionResult> InsertNote([FromBody] string note){

                if (string.IsNullOrWhiteSpace(note) || note.Length > 200)
                {
                    return BadRequest("Note should contain less than 200 characters");
                }

                try
                {
                    var response = await _client.PostAsJsonAsync("/Api/InsertNote", note);

                    return StatusCode((int)response.StatusCode);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Could not save the note in Service B");

                    return StatusCode(502, "The Service B is not available");
                }
                catch (TaskCanceledException ex)
                {
                    string errorMessage = "There was a timeout when calling Service B";
                    _logger.LogError(ex, errorMessage);

                    return StatusCode(504, errorMessage);
                }
            }
        }
    }
}
