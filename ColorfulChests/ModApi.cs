using LeFauxMods.Common.Integrations.ColorfulChests;

namespace LeFauxMods.ColorfulChests;

/// <inheritdoc />
public sealed class ModApi(List<PaletteHandler> handlers) : IColorfulChestsApi
{
    /// <inheritdoc />
    public void AddHandler(PaletteHandler handler) => handlers.Add(handler);

    /// <inheritdoc />
    public void RemoveHandler(PaletteHandler handler) => handlers.Remove(handler);
}
