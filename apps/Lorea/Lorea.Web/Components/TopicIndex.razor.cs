namespace Lorea.Web.Components;

public partial class TopicIndex
{
    [Inject] private IStringLocalizer<TopicIndex> Text { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Parameter, EditorRequired] public LibraryEntry Entry { get; set; } = default!;
    [Parameter] public LibraryTopic? Current { get; set; }

    private string PackageUrl => $"{Navigation.BaseUri}packages/{Entry.Slug}";
}
