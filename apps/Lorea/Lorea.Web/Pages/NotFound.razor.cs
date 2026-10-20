namespace Lorea.Web.Pages;

public partial class NotFound
{
    [Inject] private IStringLocalizer<NotFound> Text { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
}
