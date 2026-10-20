internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddOSBlazor(options =>
        {
            options.Locale.DefaultCulture = CultureNames.English;
            options.Locale.Text.AddEmbedded(typeof(App).Assembly, "Locales");

            // Each library keeps its texts next to its structure: Content/{slug}/{slug}.{culture}.json.
            foreach (var entry in LibraryCatalog.Entries)
                options.Locale.Text.AddEmbedded(typeof(App).Assembly, entry.Slug);

            // The names of the groups the libraries are listed under: Content/groups/groups.{culture}.json.
            options.Locale.Text.AddEmbedded(typeof(App).Assembly, "groups");

            options.Theme.Key = "lorea.theme";
        });

        await builder.Build().RunAsync();
    }
}
