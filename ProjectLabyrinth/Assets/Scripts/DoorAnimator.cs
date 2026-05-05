using System.Collections;
using UnityEngine;

// Plays frame-by-frame door animations by cycling through sprite arrays.
// Attach this to the same GameObject as Door.cs.
// Assign the sliced spritesheet frames in the Inspector:
//   openFrames  = frames 0-19  (closed -> open)
//   closeFrames = frames 20-39 (open -> closed)
[RequireComponent(typeof(SpriteRenderer))]
public class DoorAnimator : MonoBehaviour
{
    [Header("Animation Frames")]
    public Sprite[] openFrames;
    public Sprite[] closeFrames;

    [Header("Timing")]
    // Seconds per frame
    public float frameRate = 0.05f;

    private SpriteRenderer sr;
    private Coroutine current;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Called by Door.cs when the locked state changes
    public void PlayOpen()
    {
        if (openFrames == null || openFrames.Length == 0) return;
        if (current != null) StopCoroutine(current);
        current = StartCoroutine(PlayFrames(openFrames));
    }

    public void PlayClose()
    {
        if (closeFrames == null || closeFrames.Length == 0) return;
        if (current != null) StopCoroutine(current);
        current = StartCoroutine(PlayFrames(closeFrames));
    }

    private IEnumerator PlayFrames(Sprite[] frames)
    {
        foreach (Sprite frame in frames)
        {
            sr.sprite = frame;
            yield return new WaitForSeconds(frameRate);
        }
    }
}
