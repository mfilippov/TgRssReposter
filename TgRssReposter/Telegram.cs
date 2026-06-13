using System.Net;

namespace TgRssReposter;

public class Telegram(string apiToken, string channelId, string? rhash)
{
    private static readonly HttpClient HttpClient = new();

    public async Task PublishPost(Post post)
    {
        var result =
            await HttpClient.GetAsync($"https://api.telegram.org/bot{apiToken}/sendMessage" +
                                      $"?chat_id={channelId}" +
                                      "&parse_mode=HTML" +
                                      $"&text={post.Encode(rhash)}");
        if (result.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception($"can't publish post:\n\t{await result.Content.ReadAsStringAsync()}");
        }
    }
}