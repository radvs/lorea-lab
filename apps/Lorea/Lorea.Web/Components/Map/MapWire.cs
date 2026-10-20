namespace Lorea.Web.Components.Map;

// One drawn wire: From uses To. Path is an SVG path of straight runs with rounded corners.
internal sealed record MapWire(string From, string To, string Path);
