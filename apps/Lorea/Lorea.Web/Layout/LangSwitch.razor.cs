using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using OS.Locale;

namespace Lorea.Web.Layout;

public partial class LangSwitch
{
    [Inject] private ILocaleRegistry Locale { get; set; } = default!;
    [Inject] private IStringLocalizer<LangSwitch> Text { get; set; } = default!;

    [Parameter, EditorRequired] public ICultureProfile Current { get; set; } = default!;
    [Parameter, EditorRequired] public EventCallback<ICultureProfile> CultureChanged { get; set; }
}
