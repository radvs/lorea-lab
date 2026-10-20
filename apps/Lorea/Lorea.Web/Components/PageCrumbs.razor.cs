namespace Lorea.Web.Components;

public partial class PageCrumbs
{
    [Inject] private IStringLocalizer<PageCrumbs> Text { get; set; } = default!;

    [Parameter, EditorRequired] public IReadOnlyList<Crumb> Items { get; set; } = [];

    public sealed record Crumb(string Label, string? Href = null);
}
