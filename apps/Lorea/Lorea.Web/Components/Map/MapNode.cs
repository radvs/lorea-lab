namespace Lorea.Web.Components.Map;

// One box on the map: a package, or the folded family of packages that extend one (Entry is null then).
internal sealed record MapNode(string Id, string Name, LibraryEntry? Entry, IReadOnlyList<string> Uses, IReadOnlyList<LibraryEntry> Members)
{
    public bool IsFamily => Entry is null;
}
