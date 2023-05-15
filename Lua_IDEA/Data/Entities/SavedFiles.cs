namespace Lua_IDEA.Data.Entities;

public class SavedFile
{
    public int Id { get; set; }
    public string Path { get; set; }

    public bool IsFavorite { get; set; }

    public bool IsRecent { get; set; }
}
