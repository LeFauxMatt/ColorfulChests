using LeFauxMods.Common.Interface;
using LeFauxMods.Common.Models;
using Microsoft.Xna.Framework;

namespace LeFauxMods.ColorfulChests;

/// <summary>Represents the mod configuration.</summary>
internal class ModConfig : IConfigWithLogAmount
{
    /// <summary>Gets or sets the color palette.</summary>
    public Color[] ColorPalette { get; set; } =
    [
        new(85, 85, 255),
        new(119, 191, 255),
        new(0, 170, 170),
        new(0, 234, 175),
        new(0, 170, 0),
        new(159, 236, 0),
        new(255, 234, 18),
        new(255, 167, 18),
        new(255, 105, 18),
        new(255, 0, 0),
        new(135, 0, 35),
        new(255, 173, 199),
        new(255, 117, 195),
        new(172, 0, 198),
        new(143, 0, 255),
        new(89, 11, 142),
        new(64, 64, 64),
        new(100, 100, 100),
        new(200, 200, 200),
        new(254, 254, 254)
    ];


    /// <inheritdoc />
    public LogAmount LogAmount { get; set; }

    /// <summary>
    ///     Copies the values from this instance to another instance.
    /// </summary>
    /// <param name="other">The other config instance.</param>
    public void CopyTo(ModConfig other)
    {
        this.ColorPalette.CopyTo(other.ColorPalette, 0);
        other.LogAmount = this.LogAmount;
    }
}
