using UnityEngine;

public static class SpriteRendererExtensions
{
    public static void SetFrame(this SpriteRenderer renderer,
                                SpriteAnimationClip clip, int frameIndex)
    {
        int x = frameIndex * clip.frameWidth;
        int y = clip.texture.height - (clip.row + 1) * clip.frameHeight;

        var rect = new Rect(x, y, clip.frameWidth, clip.frameHeight);
        var pivot = new Vector2(0.5f, 0.5f);
        float ppu = clip.frameWidth; // 1 unit = 1 frame wide; adjust as needed

        renderer.sprite = Sprite.Create(clip.texture, rect, pivot, ppu);
    }
}