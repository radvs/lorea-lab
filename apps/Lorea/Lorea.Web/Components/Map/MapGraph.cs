namespace Lorea.Web.Components.Map;

// The packages as a dependency graph. A family (packages whose Family names the same library) folds into one
// "{Name}.*" node until its library is in the open set; a family of one never folds.
internal sealed class MapGraph
{
    private const string FamilyMark = "*";

    private readonly Dictionary<string, MapNode> _nodes;
    private readonly Dictionary<string, string> _nodeOf = [];
    private readonly Dictionary<string, IReadOnlySet<string>> _down = [];
    private readonly Dictionary<string, IReadOnlySet<string>> _up = [];

    public MapGraph(IReadOnlyList<LibraryEntry> entries, IReadOnlySet<string> open)
    {
        HashSet<string> known = [.. entries.Select(entry => entry.Slug)];
        var folded = entries
            .Where(entry => entry.Family is not null && known.Contains(entry.Family) && !open.Contains(entry.Family))
            .GroupBy(entry => entry.Family!)
            .Where(family => family.Count() > 1)
            .ToDictionary(family => family.Key, family => (IReadOnlyList<LibraryEntry>)[.. family]);

        foreach (var (parent, members) in folded)
        {
            foreach (var member in members)
                _nodeOf[member.Slug] = parent + FamilyMark;
        }

        IReadOnlyList<string> Mapped(IEnumerable<string> uses) =>
            [.. uses.Where(known.Contains).Select(NodeOf).Distinct(StringComparer.Ordinal)];

        var nodes = entries
            .Where(entry => !_nodeOf.ContainsKey(entry.Slug))
            .Select(entry => new MapNode(entry.Slug, entry.Name, entry, Mapped(entry.Uses), []))
            .ToList();

        foreach (var (parent, members) in folded)
        {
            var id = parent + FamilyMark;
            var name = entries.First(entry => entry.Slug == parent).Name + FamilyMark;
            nodes.Add(new MapNode(id, name, null, [.. Mapped(members.SelectMany(member => member.Uses)).Where(used => used != id)], members));
        }

        _nodes = nodes.ToDictionary(node => node.Id, StringComparer.Ordinal);
    }

    public IEnumerable<MapNode> Nodes => _nodes.Values;

    public int Count => _nodes.Count;

    public bool HasFolded => _nodes.Values.Any(node => node.IsFamily);

    public MapNode? Find(string? id) => id is not null && _nodes.TryGetValue(id, out var node) ? node : null;

    // The node that shows this library: its own, or the folded family it belongs to.
    public string NodeOf(string slug) => _nodeOf.GetValueOrDefault(slug, slug);

    // Everything the node stands on, directly or through others.
    public IReadOnlySet<string> Down(string id) => Reach(id, _down, node => node.Uses);

    // Everything that stands on the node, directly or through others.
    public IReadOnlySet<string> Up(string id) =>
        Reach(id, _up, node => _nodes.Values.Where(other => other.Uses.Contains(node.Id)).Select(other => other.Id));

    // The dependencies that are not already reached through another one; only these get a wire.
    public IReadOnlyList<string> Wires(string id)
    {
        var uses = _nodes[id].Uses;
        return [.. uses.Where(used => !uses.Any(other => other != used && Down(other).Contains(used)))];
    }

    private IReadOnlySet<string> Reach(string id, Dictionary<string, IReadOnlySet<string>> memo, Func<MapNode, IEnumerable<string>> next)
    {
        if (memo.TryGetValue(id, out var cached))
            return cached;

        HashSet<string> seen = [];
        Stack<string> pending = new([id]);
        while (pending.TryPop(out var current))
        {
            if (!_nodes.TryGetValue(current, out var node))
                continue;

            foreach (var step in next(node).Where(seen.Add))
                pending.Push(step);
        }

        return memo[id] = seen;
    }
}
