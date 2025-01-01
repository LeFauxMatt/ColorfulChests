using System.Reflection.Emit;
using HarmonyLib;
using LeFauxMods.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using StardewValley.Objects;

namespace LeFauxMods.ColorfulChests.Services;

/// <summary>Encapsulates mod patches.</summary>
internal static class ModPatches
{
    private static readonly Harmony Harmony = new(Constants.ModId);

    public static void Apply()
    {
        Log.Info("Applying Patches");

        try
        {
            _ = Harmony.Patch(
                AccessTools.DeclaredMethod(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.draw)),
                transpiler: new HarmonyMethod(typeof(ModPatches), nameof(DiscreteColorPicker_draw_transpiler)));

            _ = Harmony.Patch(
                AccessTools.DeclaredMethod(
                    typeof(Chest),
                    nameof(Chest.draw),
                    [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)]),
                transpiler: new HarmonyMethod(typeof(ModPatches), nameof(Chest_draw_transpiler)));

            _ = Harmony.Patch(
                AccessTools.DeclaredMethod(
                    typeof(Chest),
                    nameof(Chest.draw),
                    [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(bool)]),
                transpiler: new HarmonyMethod(typeof(ModPatches), nameof(Chest_draw_transpiler)));
        }
        catch
        {
            Log.Warn("Failed to apply patches");
        }
    }

    private static IEnumerable<CodeInstruction> Chest_draw_transpiler(IEnumerable<CodeInstruction> instructions) =>
        new CodeMatcher(instructions)
            .MatchEndForward(new CodeMatch(CodeInstruction.LoadField(typeof(Chest), nameof(Chest.playerChoiceColor))))
            .Repeat(matcher =>
            {
                matcher.Advance(2)
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        CodeInstruction.Call(typeof(ModPatches), nameof(GetNewColor)));
            })
            .InstructionEnumeration();

    private static IEnumerable<CodeInstruction> DiscreteColorPicker_draw_transpiler(
        IEnumerable<CodeInstruction> instructions) =>
        new CodeMatcher(instructions)
            .MatchEndForward(
                new CodeMatch(
                    static instruction => instruction.Calls(AccessTools.DeclaredMethod(typeof(DiscreteColorPicker),
                        nameof(DiscreteColorPicker.getColorFromSelection)))))
            .RemoveInstruction()
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                CodeInstruction.Call(typeof(ModPatches), nameof(GetColorFromSelection)))
            .InstructionEnumeration();

    private static Color GetColorFromSelection(int selection, DiscreteColorPicker colorPicker)
    {
        if (colorPicker.itemToDrawColored is not Chest chest)
        {
            return DiscreteColorPicker.getColorFromSelection(selection);
        }

        Color[]? palette = null;
        foreach (var handler in ModState.Handlers)
        {
            if (handler.Invoke(chest, out palette))
            {
                break;
            }
        }

        palette ??= ModState.Config.ColorPalette;
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
        foreach (var handler in ModState.Handlers)
        {
            if (handler.Invoke(chest, out palette))
            {
                break;
            }
        }

        palette ??= ModState.Config.ColorPalette;
        if (selection <= 0 || selection > palette.Length)
        {
            return oldColor;
        }

        return palette[selection - 1] is { R: 0, G: 0, B: 0 }
            ? Utility.GetPrismaticColor(0, 2f)
            : palette[selection - 1];
    }
}