namespace Lorea.Web.Pages;

public partial class Topic
{
    [Inject] private IStringLocalizer<Topic> Text { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ITemplateRenderer Templates { get; set; } = default!;
    [CascadingParameter] public ICultureProfile Profile { get; set; } = default!;

    [Parameter] public string? Slug { get; set; }
    [Parameter] public string? TopicSlug { get; set; }

    private LibraryEntry? Entry { get; set; }
    private LibraryTopic? Current { get; set; }
    private IReadOnlyList<ApiType> Types { get; set; } = [];

    private IReadOnlyList<(string Title, string Text)> Points { get; set; } = [];
    private IReadOnlyList<(string Title, string Text)> Parts { get; set; } = [];

    private bool ShowNamespaces => Current is not null && !(Current.Namespaces is [var only] && only == Entry?.Name);

    private LibraryTopic? Previous => Neighbour(-1);

    private LibraryTopic? Next => Neighbour(1);

    private string PageUrl => Navigation.Uri.Split('#')[0];
    private string PackageUrl => $"{Navigation.BaseUri}packages/{Entry?.Slug}";

    private string Title => Text[Key("title")];

    private string PageTitle => $"{Title} · {Templates.Localize("package.pageTitle", Entry)}";

    private string Key(string name) => Entry!.TopicKey(Current!, name);

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
        Types = Entry is null || Current is null ? [] : [.. Entry.TypesOf(Current)];
        Points = Entry is null || Current is null ? [] : Series("points");
        Parts = Entry is null || Current is null ? [] : Series("parts");
    }
}
