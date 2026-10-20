namespace Lorea.Web.Components;

public partial class PackageLinks
{
    [Inject] private IStringLocalizer<PackageLinks> Text { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Parameter, EditorRequired] public IReadOnlyList<LibraryEntry> Items { get; set; } = [];

    [Parameter] public string? Empty { get; set; }
}
