namespace Lorea.Web.Pages;

public partial class Home : IAsyncDisposable
{
    [Inject] private IStringLocalizer<Home> Text { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime Js { get; set; } = default!;
    [CascadingParameter] public ICultureProfile Profile { get; set; } = default!;

    private string _query = string.Empty;
    private string? _openGroupId;
    private IJSObjectReference? _module;
    private IReadOnlyList<HomeSearchResult> _searchIndex = [];

    private string InitialView =>
        Uri.TryCreate(Navigation.Uri, UriKind.Absolute, out var uri) && uri.Fragment == "#packages"
            ? "packages"
            : "map";

    private IReadOnlyList<LibraryGroup> Groups => LibraryBrowse.Group(LibraryCatalog.Entries);

    private IReadOnlyList<HomeSearchResult> SearchResults => HomeSearchIndex.Find(_searchIndex, _query);

    private IEnumerable<IGrouping<HomeSearchKind, HomeSearchResult>> SearchGroups => SearchResults.GroupBy(result => result.Kind);

    private bool HasSearch => !string.IsNullOrWhiteSpace(_query);

    private void Search(string? value) => _query = value ?? string.Empty;

    private string Guides(LibraryEntry entry) => Text.Count("home.guidesCountOne", "home.guidesCount", entry.Topics.Count, Profile);

    private string Digits(int value) => Profile.ToNativeDigits(value.ToString(System.Globalization.CultureInfo.InvariantCulture));

    private static string Anchor(string group) => group.Length == 0 ? "other" : group;

    private string PackageUrl(LibraryEntry entry) => $"{Navigation.BaseUri}packages/{entry.Slug}";

    private string TopicUrl(LibraryEntry entry, LibraryTopic topic) => $"{Navigation.BaseUri}{entry.TopicRoute(topic)}";

    private string? Localized(string key) => Text[key] is { ResourceNotFound: false } value ? value.Value : null;

    protected override void OnInitialized()
    {
        _searchIndex = HomeSearchIndex.Build(LibraryCatalog.Entries, Localized, PackageUrl, TopicUrl);
        _openGroupId = Groups.Count > 0 ? $"group-{Anchor(Groups[0].Id)}" : null;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        _module = await Js.InvokeAsync<IJSObjectReference>("import", $"{Navigation.BaseUri}js/lorea-home.js");
        await _module.InvokeVoidAsync("watchHomeViews");
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is null)
            return;

        await _module.InvokeVoidAsync("stopHomeViews");
        await _module.DisposeAsync();
    }
}
