namespace Lorea.Web.Components.Map;

// Layered layout: a package sits one row above the deepest package it uses, so everything stands on what is below.
// Edges that skip rows pass through hidden bend slots, rows are ordered by the barycenter of their neighbours, and
// the columns are relaxed so wires run straight where they can.
internal sealed class MapLayout
{
    public const double NodeHeight = 54;
    public const double RowHeight = 122;
    public const double Pad = 28;
    public const double LevelLabels = 72;

    private const double Gap = 22;
    private const double BendWidth = 10;
    private const int OrderRounds = 10;
    private const int PlaceRounds = 16;

    private MapLayout(IReadOnlyDictionary<string, MapSpot> spots, IReadOnlyList<MapChain> chains, int levels, double width)
    {
        Spots = spots;
        Chains = chains;
        Levels = levels;
        Width = width;
    }

    public IReadOnlyDictionary<string, MapSpot> Spots { get; }

    public IReadOnlyList<MapChain> Chains { get; }

    // Number of rows above the base row.
    public int Levels { get; }

    public double Width { get; }

    public double Height => Pad * 2 + Levels * RowHeight + NodeHeight;

    public double RowY(int depth) => Pad + (Levels - depth) * RowHeight;

    public static MapLayout Build(MapGraph graph, Func<MapNode, double> widthOf)
    {
        if (graph.Count == 0)
            return new MapLayout(new Dictionary<string, MapSpot>(), [], 0, Pad * 2);

        var nodes = graph.Nodes.ToDictionary(node => node.Id, StringComparer.Ordinal);
        var wires = nodes.Keys.ToDictionary(id => id, graph.Wires, StringComparer.Ordinal);
        var depth = DepthsOf(wires);
        var levels = depth.Count == 0 ? 0 : depth.Values.Max();

        Dictionary<string, Slot> slots = nodes.Values.ToDictionary(
            node => node.Id,
            node => new Slot(node.Id, node.Name, widthOf(node), depth[node.Id], node.Entry?.Family, false),
            StringComparer.Ordinal);

        List<MapChain> chains = [];
        foreach (var (user, used) in wires.SelectMany(pair => pair.Value.Select(to => (pair.Key, to))))
        {
            List<string> steps = [user];
            for (var d = depth[user] - 1; d > depth[used]; d--)
            {
                var bend = $"{user}>{used}#{d}";
                slots[bend] = new Slot(bend, bend, BendWidth, d, null, true);
                steps.Add(bend);
            }

            steps.Add(used);
            chains.Add(new MapChain(user, used, steps));
        }

        var near = slots.Keys.ToDictionary(id => id, _ => new List<string>(), StringComparer.Ordinal);
        foreach (var chain in chains)
        {
            for (var i = 0; i < chain.Steps.Count - 1; i++)
            {
                near[chain.Steps[i]].Add(chain.Steps[i + 1]);
                near[chain.Steps[i + 1]].Add(chain.Steps[i]);
            }
        }

        var rows = Enumerable.Range(0, levels + 1)
            .Select(d => slots.Values.Where(slot => slot.Depth == d).OrderBy(slot => slot.Family ?? "", StringComparer.Ordinal).ThenBy(slot => slot.Name, StringComparer.Ordinal).Select(slot => slot.Id).ToList())
            .ToArray();

        Order(rows, slots, near);
        var x = Place(rows, slots, near);

        var left = slots.Values.Min(slot => x[slot.Id] - slot.Width / 2);
        var right = slots.Values.Max(slot => x[slot.Id] + slot.Width / 2);
        var shift = Pad + LevelLabels / 2 - left;
        var width = right - left + Pad * 2 + LevelLabels;

        var spots = slots.Values.ToDictionary(
            slot => slot.Id,
            slot => new MapSpot(x[slot.Id] + shift, Pad + (levels - slot.Depth) * RowHeight, slot.Width, slot.Depth, slot.IsBend),
            StringComparer.Ordinal);

        return new MapLayout(spots, chains, levels, width);
    }

    private static Dictionary<string, int> DepthsOf(Dictionary<string, IReadOnlyList<string>> wires)
    {
        Dictionary<string, int> depth = new(StringComparer.Ordinal);
        HashSet<string> walking = [];

        int Of(string id)
        {
            if (depth.TryGetValue(id, out var known))
                return known;

            if (!walking.Add(id))
                throw new OSException($"Package '{id}' depends on itself through a cycle.");

            var value = wires[id].Count == 0 ? 0 : 1 + wires[id].Max(Of);
            walking.Remove(id);
            return depth[id] = value;
        }

        foreach (var id in wires.Keys)
            Of(id);

        return depth;
    }

    private static IEnumerable<int> Sweep(int round, int rows) =>
        round % 2 == 1 ? Enumerable.Range(0, rows) : Enumerable.Range(0, rows).Reverse();

    private static void Order(List<string>[] rows, Dictionary<string, Slot> slots, Dictionary<string, List<string>> near)
    {
        Dictionary<string, double> rank = [];
        void Rank(int d)
        {
            for (var i = 0; i < rows[d].Count; i++)
                rank[rows[d][i]] = (i + 0.5) / rows[d].Count;
        }

        for (var d = 0; d < rows.Length; d++)
            Rank(d);

        for (var round = 0; round < OrderRounds; round++)
        {
            foreach (var d in Sweep(round, rows.Length))
            {
                var key = rows[d].ToDictionary(id => id, id =>
                {
                    var next = near[id].Where(other => Math.Abs(slots[other].Depth - d) == 1).ToList();
                    return next.Count > 0 ? next.Average(other => rank[other]) : rank[id];
                });

                // Members of one family stay together, ordered by where the family as a whole is pulled.
                var family = rows[d].Where(id => slots[id].Family is not null)
                    .GroupBy(id => slots[id].Family!)
                    .ToDictionary(group => group.Key, group => group.Average(id => key[id]));

                double Key(string id) => slots[id].Family is { } name ? family[name] : key[id];

                rows[d] = [.. rows[d].OrderBy(Key).ThenBy(id => slots[id].Family ?? "", StringComparer.Ordinal).ThenBy(id => slots[id].Name, StringComparer.Ordinal)];
                Rank(d);
            }
        }
    }

    private static Dictionary<string, double> Place(List<string>[] rows, Dictionary<string, Slot> slots, Dictionary<string, List<string>> near)
    {
        double Apart(string a, string b) => (slots[a].Width + slots[b].Width) / 2 + Gap;

        var widths = rows.Select(row => row.Sum(id => slots[id].Width) + Math.Max(row.Count - 1, 0) * Gap).ToArray();
        var widest = widths.Max();
        Dictionary<string, double> x = [];
        for (var d = 0; d < rows.Length; d++)
        {
            var cursor = (widest - widths[d]) / 2;
            foreach (var id in rows[d])
            {
                x[id] = cursor + slots[id].Width / 2;
                cursor += slots[id].Width + Gap;
            }
        }

        for (var round = 0; round < PlaceRounds; round++)
        {
            foreach (var d in Sweep(round, rows.Length))
            {
                var row = rows[d];
                if (row.Count == 0)
                    continue;

                var wanted = row.Select(id => near[id].Count > 0 ? near[id].Average(other => x[other]) : x[id]).ToArray();
                var placed = (double[])wanted.Clone();
                for (var i = 1; i < placed.Length; i++)
                    placed[i] = Math.Max(placed[i], placed[i - 1] + Apart(row[i - 1], row[i]));

                for (var i = placed.Length - 2; i >= 0; i--)
                    placed[i] = Math.Min(placed[i], placed[i + 1] - Apart(row[i], row[i + 1]));

                var offset = wanted.Average() - placed.Average();
                for (var i = 0; i < row.Count; i++)
                    x[row[i]] = placed[i] + offset;
            }
        }

        return x;
    }

    private sealed record Slot(string Id, string Name, double Width, int Depth, string? Family, bool IsBend);
}

// X is the centre of the slot and Y its top edge. A bend is the hidden slot a long wire passes through.
internal sealed record MapSpot(double X, double Y, double Width, int Depth, bool IsBend);

// One wire from a package to a package it uses, with the slots it passes through, user first.
internal sealed record MapChain(string From, string To, IReadOnlyList<string> Steps);
