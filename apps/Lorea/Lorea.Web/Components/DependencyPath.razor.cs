namespace Lorea.Web.Components;

public partial class DependencyPath
{
    [Inject] private IStringLocalizer<DependencyPath> Text { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [CascadingParameter] public ICultureProfile Profile { get; set; } = default!;

    [Parameter, EditorRequired] public IReadOnlyList<ChainStep> Steps { get; set; } = [];

    private static string KindKey(int depth) => depth switch
    {
        0 => "path.self",
        1 => "path.direct",
        _ => "path.indirect"
    };

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);
}
