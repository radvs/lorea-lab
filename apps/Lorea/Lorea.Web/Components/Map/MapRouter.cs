namespace Lorea.Web.Components.Map;

// Routes every chain as an orthogonal wire. A wire that has to change column gets its own horizontal channel in the gap
// between two rows, so wires that leave one package side by side never run on top of each other.
internal static class MapRouter
{
    private const double Corner = 9;

    public static IReadOnlyList<MapWire> Route(MapLayout layout)
    {
        var spots = layout.Spots;
        List<Run> runs = [];
        for (var c = 0; c < layout.Chains.Count; c++)
        {
            var steps = layout.Chains[c].Steps;
            for (var i = 0; i < steps.Count - 1; i++)
                runs.Add(new Run(c, i, spots[steps[i]], spots[steps[i + 1]]));
        }

        Dictionary<(int Chain, int Step), (int Index, int Count)> channel = [];
        foreach (var gap in runs.Where(run => run.Shifts).GroupBy(run => run.Lower.Depth))
        {
            var right = gap.Where(run => run.Lower.X > run.Upper.X).OrderByDescending(run => run.Upper.X).ThenBy(run => run.Lower.X);
            var left = gap.Where(run => run.Lower.X < run.Upper.X).OrderBy(run => run.Upper.X).ThenByDescending(run => run.Lower.X);
            var ordered = right.Concat(left).ToList();
            for (var k = 0; k < ordered.Count; k++)
                channel[(ordered[k].Chain, ordered[k].Step)] = (k, ordered.Count);
        }

        List<MapWire> wires = [];
        for (var c = 0; c < layout.Chains.Count; c++)
        {
            var chain = layout.Chains[c];
            var first = spots[chain.Steps[0]];
            List<(double X, double Y)> points = [(first.X, first.Y + MapLayout.NodeHeight)];
            foreach (var run in runs.Where(run => run.Chain == c))
            {
                if (run.Shifts)
                {
                    var (index, count) = channel[(c, run.Step)];
                    var top = run.Upper.Y + MapLayout.NodeHeight;
                    var y = top + (run.Lower.Y - top) * (index + 1) / (count + 1);
                    points.Add((run.Upper.X, y));
                    points.Add((run.Lower.X, y));
                }

                points.Add((run.Lower.X, run.Lower.Y));
                if (run.Lower.IsBend)
                    points.Add((run.Lower.X, run.Lower.Y + MapLayout.NodeHeight));
            }

            wires.Add(new MapWire(chain.From, chain.To, Rounded(points)));
        }

        return wires;
    }

    // Corner points become short curves; repeated and in-line points are dropped first.
    private static string Rounded(List<(double X, double Y)> raw)
    {
        List<(double X, double Y)> points = [raw[0]];
        foreach (var point in raw.Skip(1).Where(point => point != points[^1]))
            points.Add(point);

        List<(double X, double Y)> turns = [points[0]];
        for (var i = 1; i < points.Count - 1; i++)
        {
            var a = turns[^1];
            var b = points[i];
            var n = points[i + 1];
            if (!((a.X == b.X && b.X == n.X) || (a.Y == b.Y && b.Y == n.Y)))
                turns.Add(b);
        }

        turns.Add(points[^1]);

        StringBuilder path = new($"M{F(turns[0].X)},{F(turns[0].Y)}");
        for (var i = 1; i < turns.Count - 1; i++)
        {
            var (a, b, n) = (turns[i - 1], turns[i], turns[i + 1]);
            var before = double.Hypot(b.X - a.X, b.Y - a.Y);
            var after = double.Hypot(n.X - b.X, n.Y - b.Y);
            var radius = Math.Min(Corner, Math.Min(before, after) / 2);
            var start = (X: b.X - (b.X - a.X) / before * radius, Y: b.Y - (b.Y - a.Y) / before * radius);
            var end = (X: b.X + (n.X - b.X) / after * radius, Y: b.Y + (n.Y - b.Y) / after * radius);
            path.Append($" L{F(start.X)},{F(start.Y)} Q{F(b.X)},{F(b.Y)} {F(end.X)},{F(end.Y)}");
        }

        return path.Append($" L{F(turns[^1].X)},{F(turns[^1].Y)}").ToString();
    }

    private static string F(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    private sealed record Run(int Chain, int Step, MapSpot Upper, MapSpot Lower)
    {
        public bool Shifts => Math.Abs(Upper.X - Lower.X) > 1;
    }
}
