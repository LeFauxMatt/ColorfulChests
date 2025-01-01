using System.Globalization;
using LeFauxMods.Common.Integrations.GenericModConfigMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace LeFauxMods.ColorfulChests.Services;

internal sealed class ColorPicker(IModHelper helper) : ComplexOption
{
    private const int OffsetX = -192;

    private static readonly Colors[] Bars = new Colors[20];

    private readonly ClickableTextureComponent copy = new(
        new Rectangle(0, 0, 64, 64),
        helper.ModContent.Load<Texture2D>("assets/icons.png"),
        new Rectangle(0, 0, 16, 16),
        Game1.pixelZoom);

    private readonly ClickableTextureComponent paste = new(
        new Rectangle(0, 0, 64, 64),
        helper.ModContent.Load<Texture2D>("assets/icons.png"),
        new Rectangle(16, 0, 16, 16),
        Game1.pixelZoom);

    private Held currentHeld = Held.None;
    private int currentIndex;

    /// <inheritdoc />
    public override int Height => 148;

    /// <inheritdoc />
    public override string Name => I18n.ConfigOption_ColorPalette_Name();

    /// <inheritdoc />
    public override string Tooltip => I18n.ConfigOption_ColorPalette_Description();

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch, Vector2 pos)
    {
        var position = pos.ToPoint();
        var (mouseX, mouseY) = Utility
            .ModifyCoordinatesForUIScale(helper.Input.GetCursorPosition().GetScaledScreenPixels())
            .ToPoint();

        var mouseLeft = helper.Input.GetState(SButton.MouseLeft);
        var mouseRight = helper.Input.GetState(SButton.MouseRight);
        var hoverY = mouseY >= position.Y && mouseY < position.Y + this.Height;
        var clicked = hoverY && (mouseLeft is SButtonState.Pressed || mouseRight is SButtonState.Pressed);
        var hoverText = string.Empty;

        for (var selection = 0; selection < 20; selection++)
        {
            var rect = new Rectangle(position.X + (selection * 36) + OffsetX, position.Y + 4, 28, 28);
            var color = ModState.Config.ColorPalette[selection];
            if (color is { R: 0, G: 0, B: 0 })
            {
                color = Utility.GetPrismaticColor(0, 2f);
            }

            spriteBatch.Draw(Game1.staminaRect, rect, color);

            if (selection == this.currentIndex)
            {
                IClickableMenu.drawTextureBox(
                    spriteBatch,
                    Game1.mouseCursors,
                    new Rectangle(375, 357, 3, 3),
                    rect.X - 4, rect.Y - 4, 36, 36,
                    Color.Black,
                    Game1.pixelZoom,
                    false);
            }

            if (clicked && rect.Contains(mouseX, mouseY))
            {
                this.currentIndex = selection;
            }
        }

        if (Bars[this.currentIndex] is not { } bars)
        {
            bars = new Colors(() => ModState.Config.ColorPalette[this.currentIndex],
                value => ModState.Config.ColorPalette[this.currentIndex] = value);

            Bars[this.currentIndex] = bars;
        }

        bars.Color = ModState.Config.ColorPalette[this.currentIndex];

        // Check for release
        if (this.currentHeld is not Held.None && mouseLeft is not (SButtonState.Held or SButtonState.Pressed))
        {
            this.currentHeld = Held.None;
        }
        else if (clicked && mouseX >= position.X + OffsetX && mouseX <= position.X + OffsetX + 720)
        {
            if (mouseY >= position.Y + 56 && mouseY <= position.Y + 56 + 28)
            {
                this.currentHeld = Held.Hue;
            }
            else if (mouseX <= position.X + OffsetX + 320 && mouseY >= position.Y + 112 &&
                     mouseY <= position.Y + 112 + 28)
            {
                this.currentHeld = Held.Saturation;
            }
            else if (mouseX >= position.X + OffsetX + 400 && mouseY >= position.Y + 112 &&
                     mouseY <= position.Y + 112 + 28)
            {
                this.currentHeld = Held.Lightness;
            }
        }

        var currentHue = 0;
        var constrainedX = Math.Min(position.X + 718 + OffsetX, Math.Max(position.X + OffsetX, mouseX));
        for (var i = 0; i < 360; i++)
        {
            var rect = new Rectangle(position.X + (i * 2) + OffsetX, position.Y + 56, 2, 28);
            spriteBatch.Draw(Game1.staminaRect, rect, bars.Hue[i]);

            if (this.currentHeld is Held.Hue && rect.Contains(constrainedX, position.Y + 56))
            {
                bars.Color = bars.Hue[i];
                currentHue = i;
            }
            else if (currentHue == 0 && bars.Hue[i].Equals(bars.Color))
            {
                currentHue = i;
            }
        }

        spriteBatch.Draw(
            Game1.mouseCursors,
            new Rectangle(position.X + (currentHue * 2) + OffsetX - 9, position.Y + 76, 20, 16),
            new Rectangle(412, 495, 5, 4),
            Color.White);

        spriteBatch.DrawString(
            Game1.smallFont,
            I18n.ConfigOption_Hue_Value(currentHue.ToString(CultureInfo.InvariantCulture)),
            new Vector2(position.X - 540, position.Y + 50),
            SpriteText.color_Gray);

        var currentSaturation = 0;
        constrainedX = 1 + Math.Min(position.X + 318 + OffsetX, Math.Max(position.X + OffsetX, mouseX));
        for (var i = 0; i < 160; i++)
        {
            var rect = new Rectangle(position.X + (i * 2) + OffsetX, position.Y + 112, 2, 28);
            spriteBatch.Draw(Game1.staminaRect, rect, bars.Saturation[i]);

            if (this.currentHeld is Held.Saturation && rect.Contains(constrainedX, position.Y + 112))
            {
                bars.Color = bars.Saturation[i];
                currentSaturation = i;
            }
            else if (currentSaturation == 0 && bars.Saturation[i].Equals(bars.Color))
            {
                currentSaturation = i;
            }
        }

        spriteBatch.Draw(
            Game1.mouseCursors,
            new Rectangle(position.X + (currentSaturation * 2) + OffsetX - 12, position.Y + 132, 20,
                16),
            new Rectangle(412, 495, 5, 4),
            Color.White);

        spriteBatch.DrawString(
            Game1.smallFont,
            I18n.ConfigOption_Saturation_Value((currentSaturation / 160f).ToString("F3", CultureInfo.InvariantCulture)),
            new Vector2(position.X - 540, position.Y + 76),
            SpriteText.color_Gray);

        var currentLightness = 0;
        constrainedX = 1 + Math.Min(position.X + 318 + OffsetX + 400, Math.Max(position.X + 208, mouseX));
        for (var i = 0; i < 160; i++)
        {
            var rect = new Rectangle(position.X + (i * 2) + OffsetX + 400, position.Y + 112, 2, 28);
            spriteBatch.Draw(Game1.staminaRect, rect, bars.Lightness[i]);

            if (this.currentHeld is Held.Lightness && rect.Contains(constrainedX, position.Y + 112))
            {
                bars.Color = bars.Lightness[i];
                currentLightness = i;
            }
            else if (currentLightness == 0 && bars.Lightness[i].Equals(bars.Color))
            {
                currentLightness = i;
            }
        }

        spriteBatch.Draw(
            Game1.mouseCursors,
            new Rectangle(position.X + (currentLightness * 2) + OffsetX + 400 - 12, position.Y + 132, 20,
                16),
            new Rectangle(412, 495, 5, 4),
            Color.White);

        spriteBatch.DrawString(
            Game1.smallFont,
            I18n.ConfigOption_Lightness_Value((currentLightness / 160f).ToString("F3", CultureInfo.InvariantCulture)),
            new Vector2(position.X - 540, position.Y + 102),
            SpriteText.color_Gray);

        this.copy.draw(
            spriteBatch,
            Color.White,
            1f,
            0,
            position.X - 270,
            position.Y + 40);

        if (this.copy.containsPoint(mouseX - position.X + 270, mouseY - position.Y - 40))
        {
            hoverText = I18n.ConfigOption_Copy_Tooltip();

            if (clicked)
            {
                var textColor = $"{bars.Color.R} {bars.Color.G} {bars.Color.B} {bars.Color.A}";
                DesktopClipboard.SetText(textColor);
            }
        }

        this.paste.draw(
            spriteBatch,
            Color.White,
            1f,
            0,
            position.X - 270,
            position.Y + 90);

        if (this.paste.containsPoint(mouseX - position.X + 270, mouseY - position.Y - 90))
        {
            hoverText = I18n.ConfigOption_Paste_Tooltip();

            if (clicked)
            {
                var textColor = string.Empty;
                DesktopClipboard.GetText(ref textColor);
                if (Utility.StringToColor(textColor) is { } color)
                {
                    bars.Color = color;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(hoverText))
        {
            IClickableMenu.drawToolTip(spriteBatch, hoverText, null, null);
        }
    }

    private enum Held
    {
        None,
        Hue,
        Saturation,
        Lightness
    }

    private sealed class Colors(Func<Color> getColor, Action<Color> setColor)
    {
        private Color color = Color.Black;

        public Color Color
        {
            get => this.color;
            set
            {
                if (this.color.Equals(value) && this.Hue.Count > 0)
                {
                    return;
                }

                this.color = value;
                setColor(this.color);
                Utility.RGBtoHSL(value.R, value.G, value.B, out var hue, out var saturation, out var lightness);

                this.Hue =
                [
                    ..Enumerable.Range(0, 360).Select(step =>
                    {
                        if (step == (int)hue)
                        {
                            return value;
                        }

                        Utility.HSLtoRGB(step, saturation, lightness, out var r, out var g, out var b);
                        return new Color(r, g, b);
                    })
                ];

                this.Saturation =
                [
                    ..Enumerable.Range(1, 160).Select(step =>
                    {
                        if (Math.Abs((step / 160f) - saturation) < 1 / 320f)
                        {
                            return value;
                        }

                        Utility.HSLtoRGB(hue, step / 160f, lightness, out var r, out var g, out var b);
                        return new Color(r, g, b);
                    })
                ];

                this.Lightness =
                [
                    ..Enumerable.Range(1, 160).Select(step =>
                    {
                        if (Math.Abs((step / 160f) - lightness) < 1 / 320f)
                        {
                            return value;
                        }

                        Utility.HSLtoRGB(hue, saturation, step / 160f, out var r, out var g, out var b);
                        return new Color(r, g, b);
                    })
                ];
            }
        }

        public List<Color> Hue { get; private set; } = [];

        public List<Color> Lightness { get; private set; } = [];

        public List<Color> Saturation { get; private set; } = [];
    }
}