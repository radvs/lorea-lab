namespace Lorea.Web.Data;

// Content/{slug}/{slug}.json is the structure, {slug}.api.json the generated API and samples, and {slug}.{culture}.json
// the texts, registered in Program.cs.
public static class LibraryCatalog
{
    private const string Folder = ".Content.";

    public static IReadOnlyList<LibraryEntry> Entries { get; } = Load();

    public static LibraryEntry? Find(string? slug) =>
        Entries.FirstOrDefault(entry => string.Equals(entry.Slug, slug, StringComparison.OrdinalIgnoreCase));

    public static IReadOnlyList<LibraryEntry> UsedBy(string slug) =>
        [.. Entries.Where(entry => entry.Uses.Contains(slug))];

    public static IReadOnlyList<LibraryEntry> Used(LibraryEntry entry) =>
        [.. entry.Uses.Select(Find).OfType<LibraryEntry>()];

    // Packages listed under the same group, so a reader can move sideways without going back to the map.
    public static IReadOnlyList<LibraryEntry> Similar(LibraryEntry entry) =>
        entry.Group is null ? [] : [.. Entries.Where(other => other.Group == entry.Group && other.Slug != entry.Slug)];

    public static IReadOnlyList<ChainStep> Chain(string slug)
    {
        var start = Find(slug);
        if (start is null)
            return [];

        var steps = new List<ChainStep> { new(start, 0) };
        var seen = new HashSet<string> { start.Slug };
        var queue = new Queue<ChainStep>(steps);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var used in current.Entry.Uses)
            {
                if (!seen.Add(used) || Find(used) is not { } entry)
                    continue;

                var step = new ChainStep(entry, current.Depth + 1);
                steps.Add(step);
                queue.Enqueue(step);
            }
        }

        return steps;
    }

    // Longest chain of packages under this one; 0 when it uses nothing.
    public static int Depth(string slug) => Depth(slug, []);

    private static int Depth(string slug, HashSet<string> path)
    {
        if (Find(slug) is not { } entry || !path.Add(entry.Slug))
            return 0;

        var depth = entry.Uses.Select(used => Find(used) is null ? 0 : Depth(used, path) + 1).DefaultIfEmpty(0).Max();
        path.Remove(entry.Slug);
        return depth;
    }

    private static LibraryEntry[] Load()
    {
        var assembly = typeof(LibraryCatalog).Assembly;
        var files = assembly.GetManifestResourceNames()
            .Select(name => (Name: name, Parts: PartsOf(name)))
            .Where(file => file.Parts.Length > 0)
            .ToArray();

        var apis = files
            .Where(file => file.Parts is [_, _, "api", "json"])
            .ToDictionary(file => file.Parts[1], file => file.Name, StringComparer.Ordinal);

        return [.. files
            .Where(file => file.Parts is [_, _, "json"])
            .OrderBy(file => file.Name, StringComparer.Ordinal)
            .Select(file => Entry(assembly, file.Name, file.Parts[1], apis.GetValueOrDefault(file.Parts[1])))];
    }

    // "{root}.Content.{folder}.{file}" split by dots: [folder, slug, "json"] is a library, [folder, slug, "api", "json"]
    // its generated API and samples, [folder, slug, culture, "json"] its texts.
    private static string[] PartsOf(string resource)
    {
        var start = resource.IndexOf(Folder, StringComparison.Ordinal);
        return start < 0 ? [] : resource[(start + Folder.Length)..].Split('.');
    }

    private static LibraryEntry Entry(Assembly assembly, string resource, string slug, string? api)
    {
        var entry = Read(assembly, resource, LibraryJson.Default.LibraryEntry);
        if (entry.Slug != slug)
            throw new OSException($"Library file '{resource}' declares the slug '{entry.Slug}'; the file name and the slug must match.");

        return api is null ? entry : entry with { Api = Read(assembly, api, LibraryJson.Default.LibraryApi) };
    }

    private static T Read<T>(Assembly assembly, string resource, JsonTypeInfo<T> info)
    {
        using var stream = assembly.GetManifestResourceStream(resource)
            ?? throw new OSException($"Library file '{resource}' could not be opened.");

        return JsonSerializer.Deserialize(stream, info)
            ?? throw new OSException($"Library file '{resource}' is empty.");
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(LibraryEntry))]
[JsonSerializable(typeof(LibraryApi))]
internal sealed partial class LibraryJson : JsonSerializerContext;
