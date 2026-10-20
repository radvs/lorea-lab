namespace Lorea.Web.Pages;

public partial class Home
{
    [Inject] private IStringLocalizer<Home> Text { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [CascadingParameter] public ICultureProfile Profile { get; set; } = default!;

    private string _query = string.Empty;
    private IReadOnlyList<LibraryGroup>? _allGroups;

    private IReadOnlyList<LibraryGroup> Groups =>
        LibraryBrowse.Group(LibraryCatalog.Entries.Where(entry => LibraryBrowse.Matches(entry, _query.Trim(), key => Text[key])));

    // The group index in the opening band always lists every group, even while the catalog below is filtered.
    private IReadOnlyList<LibraryGroup> AllGroups => _allGroups ??= LibraryBrowse.Group(LibraryCatalog.Entries);

    private string Count => Text.Count("home.countOne", "home.count", Groups.Sum(group => group.Entries.Count), Profile);

    private void Filter(string? value) => _query = value ?? string.Empty;

    private string Guides(LibraryEntry entry) => Text.Count("home.guidesCountOne", "home.guidesCount", entry.Topics.Count, Profile);

    private string Number(int value) => Profile.ToNativeDigits(value.ToString(CultureInfo.InvariantCulture));

    private static string Anchor(string group) => group.Length == 0 ? "other" : group;

    private string PackageUrl(LibraryEntry entry) => $"{Navigation.BaseUri}packages/{entry.Slug}";
}
