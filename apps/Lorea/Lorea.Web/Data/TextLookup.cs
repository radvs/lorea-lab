namespace Lorea.Web.Data;

// The group a library is listed under is data (an id), the shown name is a text key; both list views resolve it here.
public static class TextLookup
{
    public static string GroupName(this IStringLocalizer text, string id) =>
        id.Length == 0
            ? text["home.groupOther"]
            : text[$"groups.{id}"] is { ResourceNotFound: false } name ? name.Value : id;

    public static string Count(this IStringLocalizer text, string one, string many, int value, ICultureProfile profile) =>
        value == 1 ? text[one] : text[many, profile.ToNativeDigits(value.ToString(CultureInfo.InvariantCulture))];
}
