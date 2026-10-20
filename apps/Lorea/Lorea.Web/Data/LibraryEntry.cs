namespace Lorea.Web.Data;

// Types narrows the API table to these type keys, for a library that keeps several topics in one namespace.
public sealed record LibraryTopic(string Slug, IReadOnlyList<string> Namespaces, string? Since = null, IReadOnlyList<string>? Types = null);

// Content/{slug}/{slug}.json. Also compiled into tools/Lorea.Indexer, which reads the same files. Group is the id of the
// group the library is listed under (its name is the text "groups.{id}"); Family is the slug of the library this one
// extends, so the home map can fold such packages into one box.
public sealed record LibraryEntry(
    string Slug,
    string Name,
    string Version,
    IReadOnlyList<string> Uses,
    IReadOnlyList<LibraryTopic> Topics,
    string? Group = null,
    string? Family = null)
{
    [JsonIgnore]
    public LibraryApi Api { get; init; } = LibraryApi.Empty;

    public IEnumerable<ApiSample> Samples => Api.Samples.Where(sample => sample.Topic is null);

    public string TextKey(string name) => $"{Slug}.{name}";

    public string TopicKey(LibraryTopic topic, string name) => TextKey($"topics.{topic.Slug}.{name}");

    public string TopicRoute(LibraryTopic topic) => $"packages/{Slug}/{topic.Slug}";

    public LibraryTopic? FindTopic(string? slug) =>
        Topics.FirstOrDefault(topic => string.Equals(topic.Slug, slug, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<ApiSample> SamplesOf(LibraryTopic topic) => Api.Samples.Where(sample => sample.Topic == topic.Slug);

    public IEnumerable<ApiType> TypesOf(LibraryTopic topic) => Api.Types.Where(type => topic.Namespaces.Contains(type.Namespace) && (topic.Types is null || topic.Types.Contains(type.Key)));
}

public sealed record ChainStep(LibraryEntry Entry, int Depth);
