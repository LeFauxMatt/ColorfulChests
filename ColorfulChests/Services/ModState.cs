using LeFauxMods.Common.Integrations.ColorfulChests;
using LeFauxMods.Common.Models;
using LeFauxMods.Common.Services;
using LeFauxMods.Common.Utilities;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley.GameData.BigCraftables;

namespace LeFauxMods.ColorfulChests.Services;

/// <summary>Responsible for managing state.</summary>
internal sealed class ModState
{
    private static ModState? Instance;
    private readonly IModHelper helper;
    private readonly IManifest manifest;
    private readonly ConfigHelper<ModConfig> configHelper;
    private Texture2D? icons;
    private ConfigMenu? configMenu;

    private ModState(IModHelper helper, IManifest manifest)
    {
        // Init
        this.helper = helper;
        this.manifest = manifest;
        this.configHelper = new ConfigHelper<ModConfig>(helper);

        // Events
        helper.Events.Content.AssetRequested += OnAssetRequested;
        helper.Events.Content.AssetsInvalidated += this.OnAssetsInvalidated;
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

        ModEvents.Subscribe<ConfigChangedEventArgs<ModConfig>>(this.OnConfigChanged);
    }

    public static ModConfig Config => Instance!.configHelper.Config;

    public static ConfigHelper<ModConfig> ConfigHelper => Instance!.configHelper;

    public static List<PaletteHandler> Handlers { get; } = [];

    public static Texture2D Icons =>
        Instance!.icons ??= Instance.helper.GameContent.Load<Texture2D>(Constants.IconsPath);

    public static void Init(IModHelper helper, IManifest manifest) => Instance ??= new ModState(helper, manifest);

    private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (!e.NameWithoutLocale.IsEquivalentTo(Constants.BigCraftableData))
        {
            return;
        }

        // Add config options to the data
        e.Edit(static assetData =>
            {
                var bigCraftableData = assetData.AsDictionary<string, BigCraftableData>().Data;
                foreach (var id in Config.EnabledIds)
                {
                    if (!bigCraftableData.TryGetValue(id, out var data))
                    {
                        continue;
                    }

                    data.CustomFields ??= [];
                    data.CustomFields.Add(Constants.ModEnabled, "true");
                }
            },
            AssetEditPriority.Late);
    }

    private void OnConfigChanged(ConfigChangedEventArgs<ModConfig> e) =>
        _ = this.helper.GameContent.InvalidateCache(Constants.BigCraftableData);

    private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(static assetName => assetName.IsEquivalentTo(Constants.IconsPath)))
        {
            this.helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        this.helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this.icons = null;
        this.configMenu?.SetupMenu();
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var themeHelper = ThemeHelper.Init(this.helper);
        themeHelper.AddAsset(Constants.IconsPath, this.helper.ModContent.Load<IRawTextureData>("assets/icons.png"));

        this.configMenu = new ConfigMenu(this.helper, this.manifest);
    }
}