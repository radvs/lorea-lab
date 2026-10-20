namespace Lorea.Web.Pages;

public partial class NotFound
{
    [Inject] private IStringLocalizer<NotFound> Text { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private IReadOnlyList<PageCrumbs.Crumb> Crumbs =>
    [
        new(Text["layout.brand"], Navigation.BaseUri),
        new(Text["notFound.heading"], null),
    ];
}
