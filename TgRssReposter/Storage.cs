namespace TgRssReposter;

public class Storage(string storagePath)
{
    private readonly HashSet<Post> _posts = new();

    public async Task Load()
    {
        _posts.Clear();
        if (!File.Exists(storagePath))
        {
            return;
        }
        using var rdr = File.OpenText(storagePath);
        while (await rdr.ReadLineAsync() is { } line)
        {
            _posts.Add(Post.Parse(line));
        }
        rdr.Close();
    }

    public async Task RegisterPublishedPost(Post post)
    {
        await using var wrt = File.AppendText(storagePath);
        await wrt.WriteAsync($"{post}\n");
        wrt.Close();
        _posts.Add(post);
    }

    public bool IsPublished(Post post)
    {
        return _posts.Contains(post);
    }
}