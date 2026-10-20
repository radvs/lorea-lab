namespace Lorea.Web.Pages;

public partial class Package
{
    [Inject] private IStringLocalizer<Package> Text { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ITemplateRenderer Templates { get; set; } = default!;
    [CascadingParameter] public ICultureProfile Profile { get; set; } = default!;

    [Parameter] public string? Slug { get; set; }

    private static readonly (string Id, string Key)[] Sections =
    [
        ("overview", "package.sections.overview"),
        ("install", "package.sections.install"),
        ("quick-start", "package.sections.quickStart"),
        ("guides", "package.sections.guides"),
        ("api", "package.sections.api"),
    ];

    private LibraryEntry? Entry { get; set; }
    private IReadOnlyList<ChainStep> Chain { get; set; } = [];
    private IReadOnlyList<ApiGroup> ApiGroups { get; set; } = [];
    private IReadOnlyList<string> Gives { get; set; } = [];
    private IReadOnlyList<PageCrumbs.Crumb> Crumbs { get; set; } = [];
    private IReadOnlyList<PageCrumbs.Crumb> MissingCrumbs { get; set; } = [];

    private IReadOnlyList<DocAside.SectionLink> SectionLinks =>
        [.. Sections.Select(section => new DocAside.SectionLink(section.Id, Text[section.Key]))];

    private string InstallCommand => $"dotnet add package {Entry?.Name}";

    private string PageTitle => Templates.Localize("package.pageTitle", Entry);

    private string? GroupName => Entry?.Group is { } group ? Text.GroupName(group) : null;

    private string GuidesCount => Number(Entry?.Topics.Count ?? 0);

    private int DependsCount => Entry?.Uses.Count ?? 0;

    private int UsedByCount => Entry is null ? 0 : LibraryCatalog.UsedBy(Entry.Slug).Count;

    private string Number(int value) => Profile.ToNativeDigits(value.ToString(CultureInfo.InvariantCulture));

    private static string TypeName(ApiType type) => type.Key.Replace('{', '<').Replace('}', '>');

    protected override void OnParametersSet()
    {
        Entry = LibraryCatalog.Find(Slug);

        MissingCrumbs =
        [
            new(Text["layout.brand"], Navigation.BaseUri),
            new(Text["notFound.packages"], $"{Navigation.BaseUri}#packages"),
            new(Text["package.missingTitle"], null),
        ];

        if (Entry is null)
        {
            Chain = [];
            ApiGroups = [];
            Gives = [];
            Crumbs = [];
            return;
        }

        Chain = LibraryCatalog.Chain(Entry.Slug);
        ApiGroups =
        [
            .. Entry.Topics
                .Select(topic => new ApiGroup(topic, [.. Entry.TypesOf(topic)]))
                .Where(group => group.Types.Count > 0)
        ];
        Gives = [.. Enumerable.Range(1, 3).Select(index => Text[Entry.TextKey($"gives.{index}")]).Where(text => !text.ResourceNotFound).Select(text => text.Value)];
        Crumbs =
        [
            new(Text["layout.brand"], Navigation.BaseUri),
            new(Text["package.packages"], $"{Navigation.BaseUri}#packages"),
            new(Text[Entry.TextKey("title")], null),
        ];
    }

    private sealed record ApiGroup(LibraryTopic Topic, IReadOnlyList<ApiType> Types);
}
