using Lorea.Web.Components.Map;

namespace Lorea.Web.Components;

public partial class StackMap : IAsyncDisposable
{
    private const string WiresView = "wires";
    private const string GroupsView = "groups";
    private const string ViewPanel = "map-view";
    private const double MinNodeWidth = 150;
    private const double NameChar = 7.9;
    private const double TitleChar = 6.6;
    private const double NodePad = 36;

    private static readonly IReadOnlyList<string> Views = [WiresView, GroupsView];

    [Inject] private IStringLocalizer<StackMap> Text { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime Js { get; set; } = default!;
    [CascadingParameter] public ICultureProfile Profile { get; set; } = default!;

    // Libraries whose family is unfolded on the map.
    private readonly HashSet<string> _open = [];

    private MapGraph _graph = default!;
    private MapLayout _layout = default!;
    private IReadOnlyList<MapWire> _wires = [];
    private MapGraph? _flat;
    private string _built = string.Empty;
    private string _view = WiresView;
    private string? _selected;
    private string? _hover;
    private string _query = string.Empty;
    private bool _reveal;
    private IJSObjectReference? _module;

    private static IReadOnlyList<LibraryEntry> Entries => LibraryCatalog.Entries;

    private MapGraph Graph => _graph;

    private MapLayout Layout => _layout;

    private IReadOnlyList<MapWire> Wires => _wires;

    // Every family unfolded: the groups view lists each package on its own.
    private MapGraph Flat => _flat ??= new MapGraph(Entries, Entries.Select(entry => entry.Family).OfType<string>().ToHashSet());

    private MapGraph PanelGraph => _view == WiresView ? _graph : Flat;

    private MapNode? Active => PanelGraph.Find(PanelGraph.NodeOf(_selected ?? string.Empty));

    private bool CanToggleAddOns => _view == WiresView && (_graph.HasFolded || _open.Count > 0);

    private bool Searching => _query.Length > 0;

    protected override void OnParametersSet() => Rebuild();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_reveal)
            return;

        _reveal = false;
        if (_view != WiresView || Active is not { } node)
            return;

        _module ??= await Js.InvokeAsync<IJSObjectReference>("import", $"{Navigation.BaseUri}js/lorea-map.js");
        await _module.InvokeVoidAsync("reveal", NodeDomId(node.Id));
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
            await _module.DisposeAsync();
    }

    private void Rebuild()
    {
        var key = $"{CultureInfo.CurrentUICulture.Name}|{string.Join(',', _open.Order(StringComparer.Ordinal))}";
        if (key == _built)
            return;

        _built = key;
        _graph = new MapGraph(Entries, _open);
        _layout = MapLayout.Build(_graph, WidthOf);
        _wires = MapRouter.Route(_layout);
    }

    private double WidthOf(MapNode node) =>
        Math.Max(MinNodeWidth, Math.Max(node.Name.Length * NameChar, TitleOf(node).Length * TitleChar) + NodePad);

    private string TitleOf(MapNode node) =>
        node.Entry is { } entry ? Text[entry.TextKey("title")] : Text["map.family", Digits(node.Members.Count)];

    private string FamilyOf(MapNode node) => LibraryCatalog.Find(node.Members[0].Family)?.Name ?? node.Name;

    private string NodeLabel(MapNode node) => $"{node.Name}, {TitleOf(node)}";

    private string LevelText(int depth) => depth == 0 ? Text["map.base"] : Text["map.level", Digits(depth)];

    private string Digits(int value) => Profile.ToNativeDigits(value.ToString(CultureInfo.InvariantCulture));

    private string ViewText(string view) => Text[$"map.view.{view}"];

    private string PackageUrl(LibraryEntry entry) => $"{Navigation.BaseUri}packages/{entry.Slug}";

    private string TopicUrl(LibraryEntry entry, LibraryTopic topic) => Navigation.BaseUri + entry.TopicRoute(topic);

    private static string NodeDomId(string id) => $"map-node-{id}";

    private static string TypeName(ApiType type) => type.Key.Replace('{', '<').Replace('}', '>');

    private static string F(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    private static LibraryTopic? TopicOf(LibraryEntry entry, ApiType type) =>
        entry.Topics.FirstOrDefault(topic => topic.Namespaces.Contains(type.Namespace) && (topic.Types is null || topic.Types.Contains(type.Key)));

    private List<MapNode> NodesOf(IEnumerable<string> ids) =>
        [.. ids.Select(PanelGraph.Find).OfType<MapNode>().OrderBy(node => node.Name, StringComparer.Ordinal)];

    private string GroupTitle(string id)
    {
        if (id.Length == 0)
            return Text["map.groupOther"];

        var text = Text[$"groups.{id}"];
        return text.ResourceNotFound ? id : text.Value;
    }

    // Groups in the order of their lowest library (the base first); a library without a group goes last, under "other".
    private List<MapGroup> GroupsOf(string query) =>
    [
        .. Entries
            .Where(entry => query.Length == 0 || Matches(entry.Name, Text[entry.TextKey("title")], query))
            .GroupBy(entry => entry.Group ?? string.Empty)
            .Select(group => new MapGroup(group.Key, [.. group.OrderBy(entry => LibraryCatalog.Depth(entry.Slug)).ThenBy(entry => entry.Name, StringComparer.Ordinal)]))
            .OrderBy(group => group.Id.Length == 0)
            .ThenBy(group => group.Entries.Min(entry => LibraryCatalog.Depth(entry.Slug)))
            .ThenBy(group => group.Id, StringComparer.Ordinal)
    ];

    private bool Matches(MapNode node) =>
        Searching && (Matches(node.Name, TitleOf(node), _query) || node.Members.Any(member => Matches(member.Name, Text[member.TextKey("title")], _query)));

    private static bool Matches(string name, string title, string query) =>
        name.Contains(query, StringComparison.OrdinalIgnoreCase) || title.Contains(query, StringComparison.OrdinalIgnoreCase);

    private MapFocus FocusOf(string? id) =>
        _graph.Find(id) is null ? new MapFocus(null, EmptySet, EmptySet) : new MapFocus(id, _graph.Down(id!), _graph.Up(id!));

    private string NodeClass(MapNode node, MapFocus focus)
    {
        var related = focus.Id is not null && (node.Id == focus.Id || focus.Down.Contains(node.Id) || focus.Up.Contains(node.Id));
        var hit = Matches(node);
        var off = focus.Id is null ? Searching && !hit : !related;
        return TextEx.Conjoin(" ", "map-node",
            node.Id == Active?.Id ? "is-selected" : null,
            related && node.Id != Active?.Id ? "is-on" : null,
            hit ? "is-hit" : null,
            off ? "is-off" : null);
    }

    private string WireClass(MapWire wire, MapFocus focus)
    {
        var on = focus.Id is not null
            && (wire.From == focus.Id || focus.Down.Contains(wire.From) || wire.To == focus.Id || focus.Up.Contains(wire.To));
        var direct = wire.From == focus.Id || wire.To == focus.Id;
        return TextEx.Conjoin(" ", "map-wire",
            on ? "is-on" : null,
            on && !direct ? "is-indirect" : null,
            focus.Id is null ? (Searching ? "is-off" : null) : (on ? null : "is-off"));
    }

    private void ShowView(string view)
    {
        _view = view;
        if (Active is null)
            _selected = null;
    }

    private void Search(string? value) => _query = value?.Trim() ?? string.Empty;

    private void Pick(string id)
    {
        _selected = _selected == id ? null : id;
        _reveal = _selected is not null;
    }

    private void PickMember(LibraryEntry member)
    {
        _open.Add(member.Family!);
        Rebuild();
        _selected = member.Slug;
        _reveal = true;
    }

    private void Hover(string? id) => _hover = id;

    private void Clear()
    {
        _selected = null;
        _hover = null;
    }

    private void ToggleAddOns()
    {
        if (_graph.HasFolded)
            _open.UnionWith(Entries.Select(entry => entry.Family).OfType<string>());
        else
            _open.Clear();

        Rebuild();
        if (Active is null)
            _selected = null;
    }

    private void OnKey(KeyboardEventArgs e, string id)
    {
        if (e.Key is "Enter" or " ")
            Pick(id);
    }

    private static readonly IReadOnlySet<string> EmptySet = new HashSet<string>();

    private readonly record struct MapFocus(string? Id, IReadOnlySet<string> Down, IReadOnlySet<string> Up);

    private sealed record MapGroup(string Id, IReadOnlyList<LibraryEntry> Entries);
}
