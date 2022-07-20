namespace TgRssReposter;

public class Storage
{
    private readonly HashSet<Post> _posts = new();
    private readonly string _storagePath;

    public Storage(string storagePath)
    {
        _storagePath = storagePath;
    }

    public async Task Load()
    {
        _posts.Clear();
        if (!File.Exists(_storagePath))
        {
            return;
        }
        using var rdr = File.OpenText(_storagePath);
        while (await rdr.ReadLineAsync() is { } line)
        {
            _posts.Add(Post.Parse(line));
        }
        rdr.Close();
    }

    public async Task RegisterPublishedPost(Post post)
    {
        await using var wrt = File.AppendText(_storagePath);
        await wrt.WriteAsync($"{post}\n");
        wrt.Close();
        _posts.Add(post);
    }

    public bool IsPublished(Post post)
    {
        return _posts.Contains(post);
    }
}