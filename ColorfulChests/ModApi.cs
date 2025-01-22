using LeFauxMods.ColorfulChests.Services;
using LeFauxMods.ColorfulChests.Utilities;
using LeFauxMods.Common.Integrations.ColorfulChests;
using Microsoft.Xna.Framework;
using StardewValley.Objects;

namespace LeFauxMods.ColorfulChests;

/// <inheritdoc />
public sealed class ModApi : IColorfulChestsApi
{
    /// <inheritdoc />
    public void AddHandler(PaletteHandler handler) => ModState.Handlers.Add(handler);

    /// <inheritdoc />
    public void RemoveHandler(PaletteHandler handler) => ModState.Handlers.Remove(handler);

    /// <inheritdoc />
    public Color GetColorFromSelection(Chest chest, int selection) => chest.GetColorFromSelection(selection);
}