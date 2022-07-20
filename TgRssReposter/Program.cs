using System.Globalization;
using System.Xml;

namespace TgRssReposter;

public static class Program
{
    private static readonly string FeedUrl = Environment.GetEnvironmentVariable("FEED_URL") ?? "";
    private static readonly string StorageFile = Environment.GetEnvironmentVariable("STORAGE_FILE") ?? "";
    private static readonly string ApiToken = Environment.GetEnvironmentVariable("API_TOKEN") ?? "";
    private static readonly string ChannelId = Environment.GetEnvironmentVariable("CHANNEL_ID") ?? "";
    private static readonly string? RHash = Environment.GetEnvironmentVariable("RHASH");

    public static async Task Main()
    {
        if (string.IsNullOrEmpty(FeedUrl))
        {
            throw new Exception("please set 'FEED_URL' environment variable");
        }

        if (string.IsNullOrEmpty(StorageFile))
        {
            throw new Exception("please set 'STORAGE_FILE' environment variable");
        }

        if (string.IsNullOrEmpty(ApiToken))
        {
            throw new Exception("please set 'API_TOKEN' environment variable");
        }

        if (string.IsNullOrEmpty(ChannelId))
        {
            throw new Exception("please set 'CHANNEL_ID' environment variable");
        }

        var telegram = new Telegram(ApiToken, ChannelId, RHash);
        var storage = new Storage(StorageFile);
        await storage.Load();
        var postsFromFeed = new List<Post>();
        using var reader =
            XmlReader.Create(FeedUrl, new XmlReaderSettings { Async = true });
        while (await reader.ReadAsync())
        {
            if (reader.NodeType != XmlNodeType.Element || reader.Name != "item")
            {
                continue;
            }
            string? title = null;
            string? link = null;
            DateTime? pubDate = null;
            while ((title == null || link == null || pubDate == null) && await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "title":
                            await reader.ReadAsync();
                            if (reader.NodeType != XmlNodeType.Text)
                            {
                                throw new Exception(
                                    $"unexpected node type: '{reader.NodeType}' after 'title' element");
                            }

                            title = reader.Value;
                            break;
                        case "link":
                            await reader.ReadAsync();
                            if (reader.NodeType != XmlNodeType.Text)
                            {
                                throw new Exception(
                                    $"unexpected node type: '{reader.NodeType}' after 'link' element");
                            }

                            link = reader.Value;
                            break;
                        case "pubDate":
                            await reader.ReadAsync();
                            if (reader.NodeType != XmlNodeType.Text)
                            {
                                throw new Exception(
                                    $"unexpected node type: '{reader.NodeType}' after 'pubDate' element");
                            }

                            pubDate = DateTime.Parse(reader.Value, CultureInfo.GetCultureInfo("en-us"));
                            break;
                    }
                }
            }

            if (title == null)
            {
                throw new Exception("'title' can't be null");
            }

            if (link == null)
            {
                throw new Exception("'link' can't be null");
            }

            if (!pubDate.HasValue)
            {
                throw new Exception("'pubDate' can't be null");
            }

            postsFromFeed.Add(new Post(title, link, pubDate.Value));
        }

        foreach (var post in postsFromFeed.Where(post => !storage.IsPublished(post)).OrderBy(post => post.PubDate))
        {
            await telegram.PublishPost(post);
            await storage.RegisterPublishedPost(post);
        }

        Console.WriteLine("OK");
    }
}