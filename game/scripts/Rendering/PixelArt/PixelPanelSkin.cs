using Godot;

namespace BearAdventure.Rendering.PixelArt;

public partial class PixelPanelSkin : Control
{
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        TextureFilter = TextureFilterEnum.Nearest;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        Resized += QueueRedraw;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (Size.X < 12 || Size.Y < 12) return;
        Color edge = PixelAtlas.ToColor(PxColor.PanelEdge);
        Color light = PixelAtlas.ToColor(PxColor.Gold1);
        // Hard-edged double frame and square corner rivets; no rounded vector chrome.
        DrawRect(new Rect2(Vector2.Zero, Size), edge, false, 2);
        DrawRect(new Rect2(new Vector2(4, 4), Size - new Vector2(8, 8)), PixelAtlas.ToColor(PxColor.Panel2), false, 1);
        DrawLine(new Vector2(2, 1), new Vector2(Size.X - 2, 1), light, 1);
        foreach (Vector2 corner in new[] { new Vector2(1, 1), new Vector2(Size.X - 6, 1),
            new Vector2(1, Size.Y - 6), Size - new Vector2(6, 6) })
        {
            DrawRect(new Rect2(corner, new Vector2(5, 5)), PixelAtlas.ToColor(PxColor.Gold0));
            DrawRect(new Rect2(corner + Vector2.One, new Vector2(2, 2)), PixelAtlas.ToColor(PxColor.Gold2));
        }
    }
}
