using LeFauxMods.ColorfulChests.Services;
using LeFauxMods.Common.Utilities;
using StardewModdingAPI.Events;

namespace LeFauxMods.ColorfulChests;

/// <inheritdoc />
public class ModEntry : Mod
{
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(helper.Translation);
        ModState.Init(helper);
        Log.Init(this.Monitor, ModState.Config);
        ModPatches.Apply();

        // Events
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    /// <inheritdoc />
    public override object GetApi(IModInfo mod) => new ModApi();

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) =>
        _ = new ConfigMenu(this.Helper, this.ModManifest);
}