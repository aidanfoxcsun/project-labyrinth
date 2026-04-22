using UnityEngine;
using System.Collections.Generic;

public class RockSprite : MonoBehaviour
{
    [SerializeField] private List<Sprite> rockSprites;

    private void Start()
    {
        int chosenIndex = Random.Range(0, rockSprites.Count);

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = rockSprites[chosenIndex];
        }
    }
}
