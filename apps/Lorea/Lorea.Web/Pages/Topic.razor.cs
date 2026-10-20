namespace Lorea.Web.Pages;

public partial class Topic
{
    [Inject] private IStringLocalizer<Topic> Text { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ITemplateRenderer Templates { get; set; } = default!;
    [CascadingParameter] public ICultureProfile Profile { get; set; } = default!;

    [Parameter] public string? Slug { get; set; }
    [Parameter] public string? TopicSlug { get; set; }

    private static readonly (string Id, string Key)[] Sections =
    [
        ("glance", "topic.glance"),
        ("samples", "topic.samples"),
        ("details", "topic.details"),
        ("api", "topic.api"),
    ];

    private LibraryEntry? Entry { get; set; }
    private LibraryTopic? Current { get; set; }
    private IReadOnlyList<ApiType> Types { get; set; } = [];
    private IReadOnlyList<(string Title, string Text)> Points { get; set; } = [];
    private IReadOnlyList<(string Title, string Text)> Parts { get; set; } = [];
    private IReadOnlyList<PageCrumbs.Crumb> Crumbs { get; set; } = [];
    private IReadOnlyList<PageCrumbs.Crumb> MissingCrumbs { get; set; } = [];

    private IReadOnlyList<DocAside.SectionLink> SectionLinks =>
        [.. Sections.Select(section => new DocAside.SectionLink(section.Id, Text[section.Key]))];

    private IEnumerable<ApiSample> Samples => Entry is null || Current is null ? [] : Entry.SamplesOf(Current);

    private bool ShowNamespaces => Current is not null && !(Current.Namespaces is [var only] && only == Entry?.Name);

    private string? About => Note("about");

    private LibraryTopic? Previous => Neighbour(-1);

    private LibraryTopic? Next => Neighbour(1);

    private PageSteps.Step? PreviousStep => Previous is { } topic ? new(Text[Entry!.TopicKey(topic, "title")], TopicUrl(topic)) : null;

    private PageSteps.Step? NextStep => Next is { } topic ? new(Text[Entry!.TopicKey(topic, "title")], TopicUrl(topic)) : null;

    private string PackageUrl => $"{Navigation.BaseUri}packages/{Entry?.Slug}";

    private string Title => Text[Key("title")];

    private string PageTitle => $"{Title} · {Templates.Localize("package.pageTitle", Entry)}";

    private string Key(string name) => Entry!.TopicKey(Current!, name);

    private string TopicUrl(LibraryTopic topic) => $"{Navigation.BaseUri}{Entry!.TopicRoute(topic)}";

    // Notes are optional: a member or sample without one shows nothing instead of its key.
    private string? Note(string name) => Text[Key(name)] is { ResourceNotFound: false } text ? text.Value : null;

    private string? MemberNote(ApiType type, ApiMember member) => Note($"member.{type.Key}.{member.Name}");

    // points.{n}.title / points.{n}.text and parts.{n}.title / parts.{n}.text, counted from 1 until one is missing.
    private List<(string Title, string Text)> Series(string name)
    {
        List<(string, string)> items = [];
        for (var i = 1; Note($"{name}.{i}.title") is { } title && Note($"{name}.{i}.text") is { } text; i++)
            items.Add((title, text));

        return items;
    }

    private LibraryTopic? Neighbour(int step)
    {
        var index = Entry is null || Current is null ? -1 : Entry.Topics.ToList().IndexOf(Current) + step;
        return Entry is not null && index >= 0 && index < Entry.Topics.Count ? Entry.Topics[index] : null;
    }

    protected override void OnParametersSet()
    {
        Entry = LibraryCatalog.Find(Slug);
        Current = Entry?.FindTopic(TopicSlug);

        MissingCrumbs =
        [
            new(Text["layout.brand"], Navigation.BaseUri),
            new(Text["notFound.packages"], $"{Navigation.BaseUri}#packages"),
            new(Text["topic.missingTitle"], null),
        ];

        if (Entry is null || Current is null)
        {
            Types = [];
            Points = [];
            Parts = [];
            Crumbs = [];
            return;
        }

        Types = [.. Entry.TypesOf(Current)];
        Points = Series("points");
        Parts = Series("parts");
        Crumbs =
        [
            new(Text["layout.brand"], Navigation.BaseUri),
            new(Text["package.packages"], $"{Navigation.BaseUri}#packages"),
            new(Text[Entry.TextKey("title")], PackageUrl),
            new(Title, null),
        ];
    }
}
