using LeFauxMods.ColorfulChests.Services;
using LeFauxMods.Common.Utilities;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using StardewValley.Objects;

namespace LeFauxMods.ColorfulChests.Utilities;

internal static class ModExtensions
{
    public static Color GetColorFromSelection(this Item item, int selection)
    {
        if (item is not Chest chest ||
            !Game1.bigCraftableData.TryGetValue(chest.ItemId, out var bigCraftableData) ||
            bigCraftableData.CustomFields?.GetBool(Constants.ModEnabled) != true)
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
}