namespace Lorea.Web.Components;

public partial class MobileDocNav
{
    [Inject] private IStringLocalizer<MobileDocNav> Text { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime Js { get; set; } = default!;

    [Parameter, EditorRequired] public LibraryEntry Entry { get; set; } = default!;
    [Parameter] public LibraryTopic? Current { get; set; }
    [Parameter, EditorRequired] public IReadOnlyList<DocAside.SectionLink> Sections { get; set; } = [];

    private string PackageUrl => $"{Navigation.BaseUri}packages/{Entry.Slug}";
    private string PageUrl => Navigation.Uri.Split('#')[0];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || Current is null)
            return;

        var pages = await Js.InvokeAsync<IJSObjectReference>("import", $"{Navigation.BaseUri}js/lorea-pages.js");
        await pages.InvokeVoidAsync("revealActiveTopic");
    }
}
