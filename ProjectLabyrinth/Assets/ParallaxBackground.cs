using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public Transform transform;
        [Range(0f, 1f)] public float speed = 0.1f;
    }

    public Layer[] layers;
    public Vector2 scrollDir = new Vector2(-1f, 0f);
    public float amplitude = 0.1f; // subtle vertical drift
    public float driftSpeed = 0.2f;

    Vector2[] basePos;
    float t;

    void Awake()
    {
        basePos = new Vector2[layers.Length];
        for (int i = 0; i < layers.Length; i++)
            if (layers[i].transform) basePos[i] = layers[i].transform.position;
    }

    void Update()
    {
        t += Time.deltaTime * driftSpeed;
        for (int i = 0; i < layers.Length; i++)
        {
            if (!layers[i].transform) continue;
            var p = basePos[i];
            p += scrollDir * layers[i].speed * Time.deltaTime;
            p.y += Mathf.Sin(t + i * 0.7f) * amplitude * layers[i].speed;
            layers[i].transform.position = p;
        }
    }
}
