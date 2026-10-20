namespace Lorea.Web.Layout;

public partial class MainLayout
{
    [Inject] private ILocaleRegistry Locale { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private IStringLocalizer<MainLayout> Text { get; set; } = default!;

    private ICultureProfile _culture = default!;
    private string? _appliedDocumentCulture;
    private IJSObjectReference? _pages;

    protected override async Task OnInitializedAsync()
    {
        _culture = Locale.DefaultCulture;
        ApplyCulture(_culture);

        var savedCulture = await JS.InvokeAsync<string?>("lorea.getPreferredCulture");
        _culture = Locale.GetCulture(savedCulture);
        ApplyCulture(_culture);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Long code lines scroll sideways; the browser must still reach them with the keyboard.
            _pages = await JS.InvokeAsync<IJSObjectReference>("import", $"{Navigation.BaseUri}js/lorea-pages.js");
            await _pages.InvokeVoidAsync("watchScrollers");
        }

        if (_appliedDocumentCulture == _culture.Name)
            return;

        await JS.InvokeVoidAsync("lorea.setDocumentCulture", _culture.Culture.TwoLetterISOLanguageName, Direction);
        _appliedDocumentCulture = _culture.Name;
    }

    private string Direction => _culture.IsRightToLeft ? "rtl" : "ltr";

    private async Task ChangeCulture(ICultureProfile culture)
    {
        await JS.InvokeVoidAsync("lorea.setPreferredCulture", culture.Name);
        _culture = culture;
        ApplyCulture(culture);
    }

    private static void ApplyCulture(ICultureProfile culture)
    {
        CultureInfo.DefaultThreadCurrentCulture = culture.Culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture.Culture;
        CultureInfo.CurrentCulture = culture.Culture;
        CultureInfo.CurrentUICulture = culture.Culture;
    }
}
