using System.Globalization;
using System.Net;

namespace TgRssReposter;

public class Post(string title, string link, DateTime pubDate)
{
    private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss zzz";
    private const char UnitSeparator = (char)31;
    public string Title { get; } = title;
    public string Link { get; } = link;
    public DateTime PubDate { get; } = pubDate;

    public string Encode(string? rhash)
    {
        var ivUrl = $"https://t.me/iv?url={Uri.EscapeDataString(Link)}";
        if (rhash != null)
        {
            ivUrl += $"&rhash={Uri.EscapeDataString(rhash)}";
        }

        return Uri.EscapeDataString(
            $"{WebUtility.HtmlEncode(Title)}\n<a href=\"{WebUtility.HtmlEncode(ivUrl)}\">{WebUtility.HtmlEncode(Link)}</a>");
    }

    public Post WithLink(string newLink)
    {
        return new Post(Title, newLink, PubDate);
    }

    public override string ToString()
    {
        return
            $"{Title}{UnitSeparator}{Link}{UnitSeparator}{PubDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture)}";
    }

    public static Post Parse(string str)
    {
        var parts = str.Split(UnitSeparator, 3);
        if (parts.Length != 3)
        {
            throw new Exception($"invalid format: '{str}'");
        }

        return new Post(parts[0], parts[1],
            DateTime.ParseExact(parts[2], DateTimeFormat, CultureInfo.InvariantCulture));
    }

    private bool Equals(Post other)
    {
        return Title == other.Title && Link == other.Link && PubDate.Equals(other.PubDate);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Post)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Title, Link, PubDate);
    }

    public static bool operator ==(Post? left, Post? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Post? left, Post? right)
    {
        return !Equals(left, right);
    }
}