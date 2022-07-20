using System.Net;

namespace TgRssReposter;

public class Telegram
{
    private readonly string _apiToken;
    private readonly string _channelId;
    private readonly string? _rhash;
    private static readonly HttpClient HttpClient = new();

    public Telegram(string apiToken, string channelId, string? rhash)
    {
        _apiToken = apiToken;
        _channelId = channelId;
        _rhash = rhash;
    }

    public async Task PublishPost(Post post)
    {
        var result =
            await HttpClient.GetAsync($"https://api.telegram.org/bot{_apiToken}/sendMessage" +
                                      $"?chat_id={_channelId}" +
                                      "&parse_mode=HTML" +
                                      $"&text={post.Encode(_rhash)}");
        if (result.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception($"can't publish post:\n\t{await result.Content.ReadAsStringAsync()}");
        }
    }
}