using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace TgRssReposter;

/// <summary>
/// devblogs.microsoft.com re-publishes the VS Code feed under /vscode-blog/, but the links it puts
/// into RSS point to non-existent pages (404). This class replaces such links with the original ones
/// from the VS Code feed, matching posts by title.
/// </summary>
public static partial class VsCodeLinkResolver
{
    private const string VsCodeFeedUrl = "https://code.visualstudio.com/feed.xml";
    private static readonly XNamespace Atom = "http://www.w3.org/2005/Atom";
    private static readonly HttpClient HttpClient = new();
    private static Dictionary<string, string>? _linksByTitle;

    public static async Task<Post> Resolve(Post post)
    {
        if (!IsBrokenVsCodeLink(post.Link))
        {
            return post;
        }

        var links = await LoadLinks();
        if (links.TryGetValue(Normalize(post.Title), out var link))
        {
            return post.WithLink(link);
        }

        // Release notes have a predictable URL even if the post is not in the VS Code feed (yet or anymore).
        var match = ReleaseTitleRegex().Match(post.Title);
        if (match.Success)
        {
            return post.WithLink(
                $"https://code.visualstudio.com/updates/v{match.Groups[1].Value}_{match.Groups[2].Value}");
        }

        Console.WriteLine($"can't resolve VS Code link for '{post.Title}', keeping '{post.Link}'");
        return post;
    }

    private static bool IsBrokenVsCodeLink(string link)
    {
        return Uri.TryCreate(link, UriKind.Absolute, out var uri) &&
               uri.Host.Equals("devblogs.microsoft.com", StringComparison.OrdinalIgnoreCase) &&
               uri.AbsolutePath.StartsWith("/vscode-blog/", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<Dictionary<string, string>> LoadLinks()
    {
        if (_linksByTitle != null)
        {
            return _linksByTitle;
        }

        _linksByTitle = new Dictionary<string, string>();
        try
        {
            await using var stream = await HttpClient.GetStreamAsync(VsCodeFeedUrl);
            var doc = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
            foreach (var entry in doc.Descendants(Atom + "entry"))
            {
                var title = entry.Element(Atom + "title")?.Value;
                var href = entry.Elements(Atom + "link")
                    .FirstOrDefault(l => (string?)l.Attribute("rel") is null or "alternate")
                    ?.Attribute("href")?.Value;
                if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(href))
                {
                    _linksByTitle.TryAdd(Normalize(title), href);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"can't load VS Code feed: {e.Message}");
        }

        return _linksByTitle;
    }

    private static string Normalize(string title)
    {
        var normalized = title.Replace('’', '\'').Replace('‘', '\'')
            .Replace('“', '"').Replace('”', '"');
        return WhitespaceRegex().Replace(normalized, " ").Trim().ToLowerInvariant();
    }

    [GeneratedRegex(@"^\s*Visual Studio Code (\d+)\.(\d+)\b")]
    private static partial Regex ReleaseTitleRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
