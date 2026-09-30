using System;
using Godot;
using BearAdventure.Domain.Gameplay;

namespace BearAdventure.Rendering.PixelArt;

/// <summary>Paint/theme helpers only. No input, inventory, crafting or window state.</summary>
public static class PixelUi
{
    private static Theme? _theme;

    private static StyleBoxFlat Box(PxColor fill, PxColor edge)
    {
        return new StyleBoxFlat
        {
            BgColor = PixelAtlas.ToColor(fill),
            BorderColor = PixelAtlas.ToColor(edge),
            BorderWidthLeft = 2, BorderWidthTop = 2, BorderWidthRight = 2, BorderWidthBottom = 2,
            ContentMarginLeft = 12, ContentMarginRight = 12, ContentMarginTop = 4, ContentMarginBottom = 4,
            AntiAliasing = false,
        };
    }

    private static Theme GetTheme()
    {
        if (_theme is not null) return _theme;
        _theme = new Theme();
        _theme.SetStylebox("normal", "Button", Box(PxColor.Panel1, PxColor.PanelEdge));
        _theme.SetStylebox("hover", "Button", Box(PxColor.Panel2, PxColor.Gold2));
        _theme.SetStylebox("pressed", "Button", Box(PxColor.Green0, PxColor.Gold1));
        _theme.SetStylebox("disabled", "Button", Box(PxColor.Panel0, PxColor.Panel2));
        var focus = Box(PxColor.Panel1, PxColor.Gold3);
        focus.DrawCenter = false;
        _theme.SetStylebox("focus", "Button", focus);
        _theme.SetColor("font_color", "Button", PixelAtlas.ToColor(PxColor.Text));
        _theme.SetColor("font_hover_color", "Button", PixelAtlas.ToColor(PxColor.Gold3));
        _theme.SetColor("font_pressed_color", "Button", PixelAtlas.ToColor(PxColor.Text));
        _theme.SetColor("font_disabled_color", "Button", PixelAtlas.ToColor(PxColor.DimText));
        _theme.SetColor("font_color", "Label", PixelAtlas.ToColor(PxColor.Text));
        _theme.SetFontSize("font_size", "Button", 16);
        return _theme;
    }

    public static void StyleSimpleButton(Button button)
    {
        button.Theme = GetTheme();
        button.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
    }

    public static void StyleButton(Button button, Texture2D icon)
    {
        StyleSimpleButton(button);
        button.Icon = icon;
        button.ExpandIcon = true;
        button.CustomMinimumSize = new Vector2(button.CustomMinimumSize.X, Math.Max(56.0f, button.CustomMinimumSize.Y));
        button.IconAlignment = HorizontalAlignment.Left;
        button.Alignment = HorizontalAlignment.Left;
        button.AddThemeConstantOverride("icon_max_width", 48);
        button.AddThemeConstantOverride("h_separation", 12);
    }

    public static Texture2D StationIcon(string recipeId) => PixelAtlas.Icon(recipeId switch
    {
        "iron-bar" => ItemType.IronBar,
        "iron-axe" => ItemType.IronAxe,
        "iron-pickaxe" => ItemType.IronPickaxe,
        "juice" => ItemType.Juice,
        "soup" => ItemType.Soup,
        _ => ItemType.Cauldron,
    });

    public static void Decorate(ColorRect panel)
    {
        Color fill = PixelAtlas.ToColor(PxColor.Panel0);
        fill.A = 0.95f;
        panel.Color = fill;
        panel.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
        // Appended AFTER the title: workstation code still finds its Label at index 0.
        panel.AddChild(new PixelPanelSkin { Name = "PixelFrame", MouseFilter = Control.MouseFilterEnum.Ignore });
    }

    public static void StyleLabels(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is Label label)
            {
                label.AddThemeColorOverride("font_color", PixelAtlas.ToColor(PxColor.Text));
                label.AddThemeColorOverride("font_shadow_color", PixelAtlas.ToColor(PxColor.Ink));
                label.AddThemeConstantOverride("shadow_offset_x", 1);
                label.AddThemeConstantOverride("shadow_offset_y", 2);
            }
            StyleLabels(child);
        }
    }
}
