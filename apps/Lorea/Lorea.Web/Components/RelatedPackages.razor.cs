namespace Lorea.Web.Components;

public partial class RelatedPackages
{
    [Inject] private IStringLocalizer<RelatedPackages> Text { get; set; } = default!;

    [Parameter, EditorRequired] public LibraryEntry Entry { get; set; } = default!;

    private IReadOnlyList<LibraryEntry> Relies => LibraryCatalog.Used(Entry);

    private IReadOnlyList<LibraryEntry> BuiltOn => LibraryCatalog.UsedBy(Entry.Slug);

    private IReadOnlyList<LibraryEntry> Similar => LibraryCatalog.Similar(Entry);
}
