namespace Lorea.Web.Data;

public enum HomeSearchKind
{
    Package,
    Api,
    Guide,
    Code
}

public sealed record HomeSearchResult(
    HomeSearchKind Kind,
    string Title,
    string Detail,
    string SearchText,
    string Url);

public static class HomeSearchIndex
{
    public static IReadOnlyList<HomeSearchResult> Build(
        IEnumerable<LibraryEntry> entries,
        Func<string, string?> text,
        Func<LibraryEntry, string> packageUrl,
        Func<LibraryEntry, LibraryTopic, string> topicUrl)
    {
        List<HomeSearchResult> results = [];

        foreach (var entry in entries)
        {
            var packageTitle = text(entry.TextKey("title"));
            var packageSummary = text(entry.TextKey("summary"));
            var packageText = new List<string?>
            {
                entry.Name,
                entry.Version,
                entry.Group,
                entry.Group is null ? null : text($"groups.{entry.Group}"),
                packageTitle,
                packageSummary,
                $"dotnet add package {entry.Name}",
            };
            packageText.AddRange(entry.Uses.Select(slug => LibraryCatalog.Find(slug)?.Name));
            AddValues(packageText, "gives", entry.TextKey, text);

            foreach (var sample in entry.Samples)
            {
                var sampleTitle = text(entry.TextKey($"sample.{sample.Key}")) ?? sample.Key;
                packageText.Add(sampleTitle);
                Add(results, HomeSearchKind.Code, sampleTitle, entry.Name,
                    [sampleTitle, entry.Name, sample.Code, sample.Output],
                    $"{packageUrl(entry)}#quick-start");
            }

            Add(results, HomeSearchKind.Package, entry.Name, packageTitle ?? entry.Slug,
                packageText, packageUrl(entry));

            foreach (var topic in entry.Topics)
            {
                var topicTitle = text(entry.TopicKey(topic, "title")) ?? topic.Slug;
                var topicSummary = text(entry.TopicKey(topic, "summary"));
                var topicText = new List<string?> { topicTitle, topicSummary, text(entry.TopicKey(topic, "about")) };
                AddSeries(topicText, "points", name => entry.TopicKey(topic, name), text);
                AddSeries(topicText, "parts", name => entry.TopicKey(topic, name), text);

                foreach (var sample in entry.SamplesOf(topic))
                {
                    var sampleTitle = text(entry.TopicKey(topic, $"sample.{sample.Key}.title")) ?? sample.Key;
                    var sampleNote = text(entry.TopicKey(topic, $"sample.{sample.Key}.note"));
                    topicText.AddRange([sampleTitle, sampleNote, sample.Code, sample.Output]);
                    Add(results, HomeSearchKind.Code, sampleTitle, $"{entry.Name} · {topicTitle}",
                        [sampleTitle, sampleNote, sample.Code, sample.Output],
                        $"{topicUrl(entry, topic)}#samples");
                }

                Add(results, HomeSearchKind.Guide, topicTitle, $"{entry.Name} · {topicSummary}",
                    topicText, topicUrl(entry, topic));
            }

            var types = entry.Api.Types.Select(type => (
                Type: type,
                Topic: entry.Topics.FirstOrDefault(topic => entry.TypesOf(topic).Any(item => item.Key == type.Key))));

            foreach (var type in types)
            {
                var typeTitle = TypeName(type.Type.Key);
                var typeNote = type.Topic is null ? null : text(entry.TopicKey(type.Topic, $"type.{type.Type.Key}"));
                var typeUrl = type.Topic is null ? $"{packageUrl(entry)}#api" : $"{topicUrl(entry, type.Topic)}#api";
                Add(results, HomeSearchKind.Api, typeTitle, $"{entry.Name} · {type.Type.Signature}",
                    [typeTitle, type.Type.Namespace, type.Type.Signature, type.Type.Obsolete, typeNote],
                    typeUrl);

                foreach (var member in type.Type.Members)
                {
                    var memberNote = type.Topic is null ? null : text(entry.TopicKey(type.Topic, $"member.{type.Type.Key}.{member.Name}"));
                    var signatures = string.Join(" · ", member.Signatures);
                    Add(results, HomeSearchKind.Api, member.Name, $"{entry.Name} · {typeTitle}",
                        [member.Name, type.Type.Key, type.Type.Namespace, signatures, member.Obsolete, memberNote],
                        typeUrl);
                }
            }
        }

        return results;
    }

    private static void AddValues(List<string?> values, string series, Func<string, string> key, Func<string, string?> text)
    {
        for (var index = 1; ; index++)
        {
            var value = text(key($"{series}.{index}"));
            if (value is null)
                break;

            values.Add(value);
        }
    }

    public static IReadOnlyList<HomeSearchResult> Find(
        IReadOnlyList<HomeSearchResult> index,
        string query,
        int limit = 12)
    {
        var normalized = string.Join(' ', query.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (normalized.Length == 0)
            return [];

        var terms = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return [.. index
            .Select(result => (Result: result, Rank: Rank(result, normalized, terms)))
            .Where(item => item.Rank < int.MaxValue)
            .OrderBy(item => item.Rank)
            .ThenBy(item => item.Result.Kind)
            .ThenBy(item => item.Result.Title, StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .Select(item => item.Result)];
    }

    private static void AddSeries(List<string?> values, string series, Func<string, string> key, Func<string, string?> text)
    {
        for (var index = 1; ; index++)
        {
            var title = text(key($"{series}.{index}.title"));
            var body = text(key($"{series}.{index}.text"));
            if (title is null && body is null)
                break;

            values.Add(title);
            values.Add(body);
        }
    }

    private static void Add(
        ICollection<HomeSearchResult> results,
        HomeSearchKind kind,
        string title,
        string detail,
        IEnumerable<string?> searchFields,
        string url)
    {
        var searchable = string.Join(' ', new[] { title, detail }.Concat(searchFields.OfType<string>()).Where(value => value.Length > 0));
        results.Add(new HomeSearchResult(kind, title, detail, searchable, url));
    }

    private static int Rank(HomeSearchResult result, string query, IReadOnlyList<string> terms)
    {
        if (!terms.All(term => result.SearchText.Contains(term, StringComparison.OrdinalIgnoreCase)))
            return int.MaxValue;

        if (string.Equals(result.Title, query, StringComparison.OrdinalIgnoreCase))
            return 0;
        if (result.Title.StartsWith(query, StringComparison.OrdinalIgnoreCase))
            return 1;
        if (result.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
            return 2;
        if (result.SearchText.Contains(query, StringComparison.OrdinalIgnoreCase))
            return 3;
        return 4;
    }

    private static string TypeName(string key) => key.Replace('{', '<').Replace('}', '>');
}
