using LeFauxMods.Common.Integrations.ColorfulChests;
using LeFauxMods.Common.Services;

namespace LeFauxMods.ColorfulChests.Services;

/// <summary>Responsible for managing state.</summary>
internal sealed class ModState
{
    private static ModState? Instance;
    private readonly ConfigHelper<ModConfig> configHelper;
    private readonly IModHelper helper;

    private ModState(IModHelper helper)
    {
        this.helper = helper;
        this.configHelper = new ConfigHelper<ModConfig>(helper);
    }

    public static ModConfig Config => Instance!.configHelper.Config;

    public static ConfigHelper<ModConfig> ConfigHelper => Instance!.configHelper;

    public static List<PaletteHandler> Handlers { get; } = [];

    public static void Init(IModHelper helper) => Instance ??= new ModState(helper);
}