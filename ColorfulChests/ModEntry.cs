using LeFauxMods.ColorfulChests.Services;
using LeFauxMods.Common.Utilities;

namespace LeFauxMods.ColorfulChests;

/// <inheritdoc />
public class ModEntry : Mod
{
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(helper.Translation);
        ModState.Init(helper, this.ModManifest);
        Log.Init(this.Monitor, ModState.Config);
        ModPatches.Apply();
    }

    /// <inheritdoc />
    public override object GetApi(IModInfo mod) => new ModApi();
}