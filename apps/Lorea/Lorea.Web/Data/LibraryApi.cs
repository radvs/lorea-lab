namespace Lorea.Web.Data;

// Content/{slug}/{slug}.api.json, written by tools/Lorea.Indexer from the built library and its samples on every build.
// It is not kept in git and never edited by hand.
public sealed record LibraryApi(IReadOnlyList<ApiType> Types, IReadOnlyList<ApiSample> Samples)
{
    public static LibraryApi Empty { get; } = new([], []);
}

// Key is the type's name with its generic parameters in braces (OSResult{T}); text keys use it.
public sealed record ApiType(string Namespace, string Key, string Signature, IReadOnlyList<ApiMember> Members, string? Obsolete = null);

public sealed record ApiMember(string Name, IReadOnlyList<string> Signatures, string? Obsolete = null);

// Topic is empty for the package page's samples. Output is what the sample returned when it ran; samples that only
// show code have none. Language is empty for C# and names the language of any other sample (Razor).
public sealed record ApiSample(string Key, string Code, string? Topic = null, string? Output = null, string? Language = null);
