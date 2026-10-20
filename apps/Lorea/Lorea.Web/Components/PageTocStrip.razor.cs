namespace Lorea.Web.Components;

public partial class PageTocStrip : IAsyncDisposable
{
    [Inject] private IStringLocalizer<PageTocStrip> Text { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime Js { get; set; } = default!;

    [Parameter, EditorRequired] public IReadOnlyList<PageSection> Sections { get; set; } = [];

    private IJSObjectReference? _module;

    private string PageUrl => Navigation.Uri.Split('#')[0];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        _module ??= await Js.InvokeAsync<IJSObjectReference>("import", $"{Navigation.BaseUri}js/lorea-pages.js");
        await _module.InvokeVoidAsync("watchToc");
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is null)
            return;

        try
        {
            await _module.InvokeVoidAsync("stopToc");
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // The browser page is already gone; nothing left to detach.
        }
    }
}
