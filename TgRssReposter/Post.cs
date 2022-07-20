using System.Globalization;

namespace TgRssReposter;

public class Post
{
    private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss zzz";
    private const char UnitSeparator = (char)31;
    private string Title { get; }
    private string Link { get; }
    public DateTime PubDate { get; }

    public Post(string title, string link, DateTime pubDate)
    {
        Title = title;
        Link = link;
        PubDate = pubDate;
    }

    public string Encode(string? rhash)
    {
        return rhash == null
            ? Uri.EscapeDataString($"{Title}\n<a href=\"https://t.me/iv?url={Link}\">{Link}</a>")
            : Uri.EscapeDataString($"{Title}\n<a href=\"https://t.me/iv?url={Link}&rhash={rhash}\">{Link}</a>");
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