namespace Lorea.Web.Pages;

public partial class Package
{
    [Inject] private IStringLocalizer<Package> Text { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ITemplateRenderer Templates { get; set; } = default!;
    [CascadingParameter] public ICultureProfile Profile { get; set; } = default!;

    [Parameter] public string? Slug { get; set; }

    private LibraryEntry? Entry { get; set; }
    private IReadOnlyList<ChainStep> Chain { get; set; } = [];

    private static readonly (string Id, string Key)[] Sections =
    [
        ("overview", "package.sections.overview"),
        ("install", "package.sections.install"),
        ("quick-start", "package.sections.quickStart"),
        ("guides", "package.sections.guides"),
        ("api", "package.sections.api"),
    ];

    private IReadOnlyList<(LibraryTopic Topic, IReadOnlyList<ApiType> Types)> ApiGroups { get; set; } = [];

    // Anchors must keep the page path: the app's base href would otherwise send "#id" to the home page.
    private string PageUrl => Navigation.Uri.Split('#')[0];

    private string InstallCommand => $"dotnet add package {Entry?.Name}";

    private string PageTitle => Templates.Localize("package.pageTitle", Entry);

    protected override void OnParametersSet()
    {
        Entry = LibraryCatalog.Find(Slug);
        Chain = Entry is null ? [] : LibraryCatalog.Chain(Entry.Slug);
        ApiGroups = Entry is null ? [] : [.. Entry.Topics.Select(topic => (topic, (IReadOnlyList<ApiType>)[.. Entry.TypesOf(topic)])).Where(group => group.Item2.Count > 0)];
    }
}
