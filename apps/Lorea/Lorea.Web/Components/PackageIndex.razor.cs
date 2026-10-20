namespace Lorea.Web.Components;

public partial class PackageIndex
{
    [Inject] private IStringLocalizer<PackageIndex> Text { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [CascadingParameter] public ICultureProfile Profile { get; set; } = default!;

    [Parameter] public LibraryEntry? Current { get; set; }

    private string _query = string.Empty;

    // The rail is for finding a package by its name, not for searching the text of every summary.
    private IReadOnlyList<LibraryGroup> Groups =>
        LibraryBrowse.Group(LibraryCatalog.Entries.Where(entry => LibraryBrowse.Matches(entry, _query.Trim(), key => Text[key], summary: false)));

    private void Filter(string? value) => _query = value ?? string.Empty;
}
