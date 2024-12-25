using System.Reflection.Emit;
using HarmonyLib;
using LeFauxMods.Common.Integrations.ColorfulChests;
using LeFauxMods.Common.Integrations.GenericModConfigMenu;
using LeFauxMods.Common.Services;
using LeFauxMods.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using StardewValley.Objects;

namespace LeFauxMods.ColorfulChests;

/// <inheritdoc />
public class ModEntry : Mod
{
    private static readonly List<PaletteHandler> Handlers = [];
    private static ModConfig config = null!;
    private ConfigHelper<ModConfig> configHelper = null!;
    private GenericModConfigMenuIntegration gmcm = null!;

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(this.Helper.Translation);
        this.configHelper = new ConfigHelper<ModConfig>(helper);
        config = this.configHelper.Load();
        Log.Init(this.Monitor, config);

        this.gmcm = new GenericModConfigMenuIntegration(this.ModManifest, helper.ModRegistry);

        // Patches
        var harmony = new Harmony(this.ModManifest.UniqueID);

        _ = harmony.Patch(
            AccessTools.DeclaredMethod(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.draw)),
            transpiler: new HarmonyMethod(typeof(ModEntry), nameof(DiscreteColorPicker_draw_transpiler)));

        _ = harmony.Patch(
            AccessTools.DeclaredMethod(
                typeof(Chest),
                nameof(Chest.draw),
                [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)]),
            transpiler: new HarmonyMethod(typeof(ModEntry), nameof(Chest_draw_transpiler)));

        _ = harmony.Patch(
            AccessTools.DeclaredMethod(
                typeof(Chest),
                nameof(Chest.draw),
                [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(bool)]),
            transpiler: new HarmonyMethod(typeof(ModEntry), nameof(Chest_draw_transpiler)));

        // Events
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    /// <inheritdoc />
    public override object GetApi(IModInfo mod) => new ModApi(Handlers);

    private static IEnumerable<CodeInstruction> Chest_draw_transpiler(IEnumerable<CodeInstruction> instructions) =>
        new CodeMatcher(instructions)
            .MatchEndForward(new CodeMatch(CodeInstruction.LoadField(typeof(Chest), nameof(Chest.playerChoiceColor))))
            .Repeat(matcher =>
            {
                matcher.MatchEndForward(
                        new CodeMatch(CodeInstruction.LoadField(typeof(Chest), nameof(Chest.playerChoiceColor))))
                    .Advance(2)
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        CodeInstruction.Call(typeof(ModEntry), nameof(GetNewColor)));
            })
            .InstructionEnumeration();

    private static IEnumerable<CodeInstruction> DiscreteColorPicker_draw_transpiler(
        IEnumerable<CodeInstruction> instructions) =>
        new CodeMatcher(instructions)
            .MatchEndForward(
                new CodeMatch(
                    instruction => instruction.Calls(AccessTools.DeclaredMethod(typeof(DiscreteColorPicker),
                        nameof(DiscreteColorPicker.getColorFromSelection)))))
            .RemoveInstruction()
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                CodeInstruction.Call(typeof(ModEntry), nameof(GetColorFromSelection)))
            .InstructionEnumeration();

    private static Color GetColorFromSelection(int selection, DiscreteColorPicker colorPicker)
    {
        if (colorPicker.itemToDrawColored is not Chest chest)
        {
            return DiscreteColorPicker.getColorFromSelection(selection);
        }

        Color[]? palette = null;
        foreach (var handler in Handlers)
        {
            if (handler.Invoke(chest, out palette))
            {
                break;
            }
        }

        palette ??= config.ColorPalette;
        if (selection <= 0 || selection > palette.Length)
        {
            return DiscreteColorPicker.getColorFromSelection(selection);
        }

        return palette[selection - 1] is { R: 0, G: 0, B: 0 }
            ? Utility.GetPrismaticColor(0, 2f)
            : palette[selection - 1];
    }

    private static Color GetNewColor(Color oldColor, Chest chest)
    {
        var selection = Math.Max(0, DiscreteColorPicker.getSelectionFromColor(oldColor));
        Color[]? palette = null;
        foreach (var handler in Handlers)
        {
            if (handler.Invoke(chest, out palette))
            {
                break;
            }
        }

        palette ??= config.ColorPalette;
        if (selection <= 0 || selection > palette.Length)
        {
            return oldColor;
        }

        return palette[selection - 1] is { R: 0, G: 0, B: 0 }
            ? Utility.GetPrismaticColor(0, 2f)
            : palette[selection - 1];
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        if (!this.gmcm.IsLoaded)
        {
            return;
        }

        var defaultConfig = new ModConfig();
        var tempConfig = this.configHelper.Load();

        this.gmcm.Register(
            () => defaultConfig.CopyTo(tempConfig),
            () =>
            {
                tempConfig.CopyTo(config);
                this.configHelper.Save(tempConfig);
            });

        this.gmcm.AddComplexOption(new ColorPicker(this.Helper, tempConfig));
    }
}
