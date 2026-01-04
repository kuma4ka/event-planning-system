using AngleSharp.Html.Parser;

namespace EventPlanning.IntegrationTests.Helpers;

public static class HtmlHelpers
{
    public static async Task<string> GetAntiForgeryTokenAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();
        var parser = new HtmlParser();
        var document = await parser.ParseDocumentAsync(html);
        
        var tokenInput = document.QuerySelector("input[name='__RequestVerificationToken']");
        var token = tokenInput?.GetAttribute("value");
        
        if (string.IsNullOrEmpty(token))
        {
            throw new Exception($"Anti-forgery token not found on page {url}");
        }

        return token;
    }
}
