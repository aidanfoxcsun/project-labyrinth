using UnityEngine;

[CreateAssetMenu(menuName = "2D/Sprite Animation Clip")]
public class SpriteAnimationClip : ScriptableObject
{
    public Texture2D texture;
    public int frameWidth;
    public int frameHeight;
    public int row;
    public int frameCount;
    public float secondsPerFrame = 0.1f;
}