namespace Lorea.Web.Data;

// One list, one order: the home catalog, the package rail and the map group the packages the same way.
public static class LibraryBrowse
{
    public static IReadOnlyList<LibraryGroup> Group(IEnumerable<LibraryEntry> entries) =>
        [.. entries
            .GroupBy(entry => entry.Group ?? string.Empty)
            .Select(group => new LibraryGroup(group.Key, [.. group.OrderBy(entry => entry.Name, StringComparer.Ordinal)]))
            .OrderBy(group => group.Id.Length == 0)
            .ThenBy(group => group.Entries.Min(entry => LibraryCatalog.Depth(entry.Slug)))
            .ThenBy(group => group.Id, StringComparer.Ordinal)];

    public static bool Matches(LibraryEntry entry, string query, Func<string, string> text, bool summary = true) =>
        query.Length == 0
        || entry.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
        || text(entry.TextKey("title")).Contains(query, StringComparison.OrdinalIgnoreCase)
        || (summary && text(entry.TextKey("summary")).Contains(query, StringComparison.OrdinalIgnoreCase));
}

public sealed record LibraryGroup(string Id, IReadOnlyList<LibraryEntry> Entries);
