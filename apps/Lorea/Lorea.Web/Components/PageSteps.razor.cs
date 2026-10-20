namespace Lorea.Web.Components;

public partial class PageSteps
{
    [Inject] private IStringLocalizer<PageSteps> Text { get; set; } = default!;

    [Parameter] public Step? Previous { get; set; }
    [Parameter] public Step? Next { get; set; }

    public sealed record Step(string Title, string Href);
}
