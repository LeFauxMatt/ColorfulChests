using LeFauxMods.ColorfulChests.Services;
using LeFauxMods.Common.Integrations.ColorfulChests;

namespace LeFauxMods.ColorfulChests;

/// <inheritdoc />
public sealed class ModApi : IColorfulChestsApi
{
    /// <inheritdoc />
    public void AddHandler(PaletteHandler handler) => ModState.Handlers.Add(handler);

    /// <inheritdoc />
    public void RemoveHandler(PaletteHandler handler) => ModState.Handlers.Remove(handler);
}