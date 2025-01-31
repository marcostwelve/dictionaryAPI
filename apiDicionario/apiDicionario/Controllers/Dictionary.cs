using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace apiDicionario.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Dictionary : ControllerBase
    {

        private readonly ILogger<Dictionary> _logger;
        private readonly HttpClient _client;

        public Dictionary(ILogger<Dictionary> logger, HttpClient client)
        {
            _logger = logger;
            _client = client;
        }

        [HttpGet("/textFormat/{word}")]
        public async Task<IActionResult> GetWordTextFormat(string word)
        {
            var getWord = $"https://pt.wiktionary.org/w/api.php?action=query&format=json&titles={word}&prop=extracts&explaintext=true&origin=*";

            try
            {
                HttpResponseMessage response = await _client.GetAsync(getWord);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                JsonDocument json = JsonDocument.Parse(responseBody);
                JsonElement root = json.RootElement.GetProperty("query").GetProperty("pages");

                foreach (var item in root.EnumerateObject())
                {
                    string pageId = item.Name;
                    if (pageId == "-1")
                    {
                        return NotFound(new { message = "Palavra não encontrada" });
                    }

                    else
                    {
                        var wordDefinition = item.Value.GetProperty("extract").GetString();
                        return Ok(wordDefinition);
                    }

                }

                return NotFound(new { message = "Palavra não encontrada" });
            }
            catch (HttpRequestException e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet("/jsonFormat/{word}")]
        public async Task<IActionResult> GetWordFormatJson(string word)
        {
            var getWord = $"https://pt.wiktionary.org/w/api.php?action=query&format=json&titles={word}&prop=extracts&explaintext=true&origin=*";

            try
            {
                var response = await _client.GetAsync(getWord);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new { message = response.ReasonPhrase });
                }

                var jsonContent = await response.Content.ReadAsStringAsync();

                var jsonObject = JsonObject.Parse(jsonContent);

                return Ok(jsonObject);
            }
            catch (HttpRequestException e)
            {
                return BadRequest(new { message = e.Message });
            }

        }
    }
}
