using System.Globalization;
using LeFauxMods.Common.Integrations.GenericModConfigMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Objects;

namespace LeFauxMods.ColorfulChests.Services;

internal sealed class ColorPickerOption : ComplexOption
{
    private readonly List<ClickableTextureComponent> components = [];
    private readonly IModHelper helper;
    private readonly List<Chest> samples = [];
    private readonly Selector[] selectors = new Selector[20];
    private Held currentHeld = Held.None;
    private int height;
    private int selectedIndex;

    public ColorPickerOption(IModHelper helper, IList<Color> colorPalette)
    {
        this.helper = helper;

        for (var index = 0; index < 20; index++)
        {
            var currentIndex = index;
            this.selectors[index] = new Selector(
                currentIndex,
                () => colorPalette[currentIndex],
                value => colorPalette[currentIndex] = value);
        }

        this.components.Add(new ClickableTextureComponent(
            "copy",
            new Rectangle(0, 0, 64, 64),
            null,
            I18n.ConfigOption_Copy_Tooltip(),
            ModState.Icons,
            new Rectangle(0, 0, 16, 16),
            Game1.pixelZoom));

        this.components.Add(new ClickableTextureComponent(
            "paste",
            new Rectangle(Game1.tileSize, 0, 64, 64),
            null,
            I18n.ConfigOption_Paste_Tooltip(),
            ModState.Icons,
            new Rectangle(16, 0, 16, 16),
            Game1.pixelZoom));

        this.samples.Add(new Chest(true, new Vector2(this.samples.Count * Game1.tileSize * 1.5f, 0)));
        this.samples.Add(new Chest(true, new Vector2(this.samples.Count * Game1.tileSize * 1.5f, 0), "232"));
        this.samples.Add(new Chest(true, new Vector2(this.samples.Count * Game1.tileSize * 1.5f, 0), "BigChest"));
        this.samples.Add(new Chest(true, new Vector2(this.samples.Count * Game1.tileSize * 1.5f, 0), "BigStoneChest"));

        foreach (var chest in this.samples)
        {
            chest.resetLidFrame();
        }
    }

    private enum Held
    {
        None,
        Hue,
        Saturation,
        Lightness
    }

    /// <inheritdoc />
    public override int Height => this.height;

    /// <inheritdoc />
    public override string Name => I18n.ConfigOption_ColorPalette_Name();

    /// <inheritdoc />
    public override string Tooltip => I18n.ConfigOption_ColorPalette_Description();

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch, Vector2 pos)
    {
        var availableWidth = Math.Min(1200, Game1.uiViewport.Width - 200);
        pos.X -= availableWidth / 2f;
        var (originX, originY) = pos.ToPoint();
        var (mouseX, mouseY) = this.helper.Input.GetCursorPosition().GetScaledScreenPixels().ToPoint();

        mouseX -= originX;
        mouseY -= originY;

        var mouseLeft = this.helper.Input.GetState(SButton.MouseLeft);
        var controllerA = this.helper.Input.GetState(SButton.ControllerA);
        var pressed = mouseLeft is SButtonState.Pressed || controllerA is SButtonState.Pressed;
        var held = mouseLeft is SButtonState.Held || controllerA is SButtonState.Held;
        var released = mouseLeft is SButtonState.Released || controllerA is SButtonState.Released;
        var hoverText = default(string);
        Selector? selector;

        var (_, lineHeight) = Game1.dialogueFont.MeasureString(I18n.ConfigOption_ColorPalette_Name());
        foreach (var component in this.components)
        {
            component.tryHover(mouseX - (int)pos.X + originX, mouseY - (int)(pos.Y + lineHeight) + originY);
            component.draw(
                spriteBatch,
                Color.White,
                1f,
                0,
                (int)pos.X,
                (int)(pos.Y + lineHeight));

            if (!component.containsPoint(mouseX - (int)pos.X + originX, mouseY - (int)(pos.Y + lineHeight) + originY))
            {
                continue;
            }

            hoverText ??= component.hoverText;
            if (pressed)
            {
                selector = this.selectors[this.selectedIndex];
                Game1.playSound("smallSelect");

                var textColor = default(string);
                switch (component.name)
                {
                    case "copy":
                        textColor = $"{selector.Color.R} {selector.Color.G} {selector.Color.B} {selector.Color.A}";
                        DesktopClipboard.SetText(textColor);
                        break;
                    case "paste":
                        DesktopClipboard.GetText(ref textColor);
                        if (Utility.StringToColor(textColor) is { } color)
                        {
                            selector.Color = color;
                            Utility.RGBtoHSL(color.R, color.G, color.B, out var h, out var s, out var l);
                            selector.H = h;
                            selector.S = s;
                            selector.L = l;
                        }

                        break;
                }
            }
        }

        var alignX = originX + availableWidth - 720;
        for (var index = 0; index < 20; index++)
        {
            selector = this.selectors[index];
            var bounds = selector.Component.bounds with
            {
                X = selector.Component.bounds.X + alignX, Y = selector.Component.bounds.Y + (int)pos.Y
            };

            spriteBatch.Draw(
                Game1.staminaRect,
                bounds,
                selector.Color is { R: 0, G: 0, B: 0 }
                    ? Utility.GetPrismaticColor(0, 2f)
                    : selector.Color);

            if (!pressed || !bounds.Contains(mouseX + originX, mouseY + originY))
            {
                continue;
            }

            Game1.playSound("smallSelect");
            this.selectedIndex = index;
        }

        selector = this.selectors[this.selectedIndex];
        IClickableMenu.drawTextureBox(
            spriteBatch,
            Game1.mouseCursors,
            new Rectangle(375, 357, 3, 3),
            selector.Component.bounds.X + (int)pos.X + availableWidth - 724,
            selector.Component.bounds.Y + (int)pos.Y - 4,
            36,
            36,
            Color.Black,
            Game1.pixelZoom,
            false);

        pos.Y += lineHeight;

        if (released)
        {
            this.currentHeld = Held.None;
        }

        var (textWidth, _) = Game1.smallFont.MeasureString(I18n.ConfigOption_Hue_Name());
        spriteBatch.DrawString(
            Game1.smallFont,
            I18n.ConfigOption_Hue_Name(),
            new Vector2(alignX - textWidth - 8, pos.Y),
            SpriteText.color_Gray);

        var width = 720 / selector.Hue.Length;
        for (var i = 0; i < selector.Hue.Length; i++)
        {
            var bounds = new Rectangle(
                alignX + (i * width),
                (int)pos.Y,
                width,
                28);

            spriteBatch.Draw(
                Game1.staminaRect,
                bounds,
                selector.Hue[i]);

            switch (this.currentHeld)
            {
                case Held.None when pressed && bounds.Contains(mouseX + originX, mouseY + originY):
                    this.currentHeld = Held.Hue;
                    Game1.playSound("smallSelect");
                    break;
                case Held.Hue when held &&
                    (i == 0 || mouseX + originX >= bounds.Left) &&
                    (i == selector.Hue.Length - 1 || mouseX + originX < bounds.Right):
                    selector.H = i;
                    break;
            }
        }

        spriteBatch.Draw(
            Game1.mouseCursors,
            new Rectangle(alignX + (int)(selector.H * width) - 10, (int)pos.Y + 28, 20, 16),
            new Rectangle(412, 495, 5, 4),
            Color.White);

        pos.Y += lineHeight;

        (textWidth, _) = Game1.smallFont.MeasureString(I18n.ConfigOption_Saturation_Name());
        spriteBatch.DrawString(
            Game1.smallFont,
            I18n.ConfigOption_Saturation_Name(),
            new Vector2(alignX - textWidth - 8, pos.Y),
            SpriteText.color_Gray);

        width = 720 / selector.Saturation.Length;
        for (var i = 0; i < selector.Saturation.Length; i++)
        {
            var bounds = new Rectangle(
                alignX + (i * width),
                (int)pos.Y,
                width,
                28);

            spriteBatch.Draw(
                Game1.staminaRect,
                bounds,
                selector.Saturation[i]);

            switch (this.currentHeld)
            {
                case Held.None when pressed && bounds.Contains(mouseX + originX, mouseY + originY):
                    this.currentHeld = Held.Saturation;
                    Game1.playSound("smallSelect");
                    break;
                case Held.Saturation when held &&
                    (i == 0 || mouseX + originX >= bounds.Left) &&
                    (i == selector.Saturation.Length - 1 || mouseX + originX < bounds.Right):
                    selector.S = i / (double)selector.Saturation.Length;
                    break;
            }
        }

        spriteBatch.Draw(
            Game1.mouseCursors,
            new Rectangle(alignX + (int)(selector.S * selector.Saturation.Length * width) - 10, (int)pos.Y + 28, 20,
                16),
            new Rectangle(412, 495, 5, 4),
            Color.White);

        pos.Y += lineHeight;

        (textWidth, _) = Game1.smallFont.MeasureString(I18n.ConfigOption_Lightness_Name());
        spriteBatch.DrawString(
            Game1.smallFont,
            I18n.ConfigOption_Lightness_Name(),
            new Vector2(alignX - textWidth - 8, pos.Y),
            SpriteText.color_Gray);

        width = 720 / selector.Lightness.Length;
        for (var i = 0; i < selector.Lightness.Length; i++)
        {
            var bounds = new Rectangle(
                alignX + (i * width),
                (int)pos.Y,
                width,
                28);

            spriteBatch.Draw(
                Game1.staminaRect,
                bounds,
                selector.Lightness[i]);

            switch (this.currentHeld)
            {
                case Held.None when pressed && bounds.Contains(mouseX + originX, mouseY + originY):
                    this.currentHeld = Held.Lightness;
                    Game1.playSound("smallSelect");
                    break;
                case Held.Lightness when held &&
                    (i == 0 || mouseX + originX >= bounds.Left) &&
                    (i == selector.Lightness.Length - 1 || mouseX + originX < bounds.Right):
                    selector.L = i / (double)selector.Lightness.Length;
                    break;
            }
        }

        spriteBatch.Draw(
            Game1.mouseCursors,
            new Rectangle(alignX + (int)(selector.L * selector.Lightness.Length * width) - 10, (int)pos.Y + 28, 20, 16),
            new Rectangle(412, 495, 5, 4),
            Color.White);

        pos.Y += lineHeight + 16;

        if (this.currentHeld is not Held.None)
        {
            Utility.HSLtoRGB(selector.H, selector.S, selector.L, out var r, out var g, out var b);
            if (selector.Color.R != r || selector.Color.G != g || selector.Color.B != b)
            {
                selector.Color = new Color(r, g, b);
            }
        }

        (textWidth, _) = Game1.dialogueFont.MeasureString(I18n.ConfigOption_EnablePalette_Name());
        Utility.drawTextWithShadow(
            spriteBatch,
            I18n.ConfigOption_EnablePalette_Name(),
            Game1.dialogueFont,
            pos,
            SpriteText.color_Gray);

        foreach (var chest in this.samples)
        {
            if (ModState.ConfigHelper.Temp.EnabledIds.Contains(chest.ItemId))
            {
                chest.playerChoiceColor.Value = selector.Color is { R: 0, G: 0, B: 0 }
                    ? Utility.GetPrismaticColor(0, 2f)
                    : selector.Color;
            }
            else
            {
                chest.playerChoiceColor.Value = Color.Black;
            }

            chest.draw(
                spriteBatch,
                (int)(alignX + chest.TileLocation.X),
                (int)(pos.Y + chest.TileLocation.Y) + Game1.tileSize,
                1f,
                true);

            if (mouseX < alignX - originX + chest.TileLocation.X ||
                mouseX > alignX - originX + chest.TileLocation.X + Game1.tileSize ||
                mouseY < pos.Y - originY + chest.TileLocation.Y ||
                mouseY > pos.Y - originY + chest.TileLocation.Y + (Game1.tileSize * 2))
            {
                continue;
            }

            hoverText ??= chest.DisplayName;
            if (!pressed || ModState.ConfigHelper.Temp.EnabledIds.Add(chest.ItemId))
            {
                continue;
            }

            ModState.ConfigHelper.Temp.EnabledIds.Remove(chest.ItemId);
        }

        if (!string.IsNullOrWhiteSpace(hoverText))
        {
            IClickableMenu.drawToolTip(spriteBatch, hoverText, null, null);
        }
        else if (mouseX >= 0 && mouseX <= textWidth && mouseY >= pos.Y - originY &&
            mouseY <= pos.Y - originY + lineHeight)
        {
            IClickableMenu.drawToolTip(
                spriteBatch,
                I18n.ConfigOption_EnablePalette_Description(),
                I18n.ConfigOption_EnablePalette_Name(),
                null);
        }

        pos.Y += Game1.tileSize * 2;
        this.height = (int)(pos.Y - originY);
    }

    private sealed class Selector
    {
        private readonly Func<Color> getColor;
        private readonly Action<Color> setColor;
        private double h;
        private double s;
        private double l;

        public Selector(int index, Func<Color> getColor, Action<Color> setColor)
        {
            this.getColor = getColor;
            this.setColor = setColor;

            this.Component = new ClickableComponent(
                new Rectangle(index * 36, 4, 28, 28),
                index.ToString(CultureInfo.InvariantCulture));

            var color = getColor();
            Utility.RGBtoHSL(color.R, color.G, color.B, out var initH, out var initS, out var initL);
            this.H = initH;
            this.S = initS;
            this.L = initL;
        }

        public Color[] Hue { get; } = new Color[360];

        public Color[] Saturation { get; } = new Color[180];

        public Color[] Lightness { get; } = new Color[180];

        public ClickableComponent Component { get; }

        public Color Color
        {
            get => this.getColor();
            set => this.setColor(value);
        }

        public double H
        {
            get => this.h;
            set
            {
                this.h = value;

                for (var i = 0; i < this.Saturation.Length; i++)
                {
                    Utility.HSLtoRGB(this.h, i / (double)this.Saturation.Length, this.l, out var r, out var g,
                        out var b);
                    this.Saturation[i] = new Color(r, g, b);
                }

                for (var i = 0; i < this.Lightness.Length; i++)
                {
                    Utility.HSLtoRGB(this.h, this.s, i / (double)this.Lightness.Length, out var r, out var g,
                        out var b);
                    this.Lightness[i] = new Color(r, g, b);
                }
            }
        }

        public double S
        {
            get => this.s;
            set
            {
                this.s = value;

                for (var i = 0; i < this.Hue.Length; i++)
                {
                    Utility.HSLtoRGB(i, this.s, this.l, out var r, out var g, out var b);
                    this.Hue[i] = new Color(r, g, b);
                }

                for (var i = 0; i < this.Lightness.Length; i++)
                {
                    Utility.HSLtoRGB(this.h, this.s, i / (double)this.Lightness.Length, out var r, out var g,
                        out var b);
                    this.Lightness[i] = new Color(r, g, b);
                }
            }
        }

        public double L
        {
            get => this.l;
            set
            {
                this.l = value;

                for (var i = 0; i < this.Hue.Length; i++)
                {
                    Utility.HSLtoRGB(i, this.s, this.l, out var r, out var g, out var b);
                    this.Hue[i] = new Color(r, g, b);
                }

                for (var i = 0; i < this.Saturation.Length; i++)
                {
                    Utility.HSLtoRGB(this.h, i / (double)this.Saturation.Length, this.l, out var r, out var g,
                        out var b);
                    this.Saturation[i] = new Color(r, g, b);
                }
            }
        }
    }
}