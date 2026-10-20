namespace Lorea.Web.Components;

public partial class DocSection
{
    [CascadingParameter] public ICultureProfile Profile { get; set; } = default!;

    [Parameter, EditorRequired] public string Id { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter] public int Index { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string Number => Profile.ToNativeDigits(Index.ToString("00", CultureInfo.InvariantCulture));
}
