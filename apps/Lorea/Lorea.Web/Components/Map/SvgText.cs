namespace Lorea.Web.Components.Map;

// The SVG <text> element has to be built here: Razor reserves the <text> tag for literal text.
public sealed class SvgText : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public double X { get; set; }
    [Parameter] public double Y { get; set; }
    [Parameter] public string? TextAnchor { get; set; }
    [Parameter] public string? Content { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "text");
        builder.AddAttribute(1, "class", Class);
        builder.AddAttribute(2, "x", Number(X));
        builder.AddAttribute(3, "y", Number(Y));
        if (TextAnchor is not null)
            builder.AddAttribute(4, "text-anchor", TextAnchor);

        builder.AddContent(5, Content);
        builder.CloseElement();
    }

    private static string Number(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);
}
