using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float transitionSpeed = 5.0f;

    private Vector3 destination;

    public void SetTargetDestination(Vector2 target)
    {
        destination = new Vector3(target.x, target.y, -10);
    }

    private void Start()
    {
        destination = new Vector3(0, 0, -10);
    }

    public void Update()
    {
        if(Vector3.Distance(transform.position, destination) < 0.1f)
        {
            return;
        }

        transform.position = Vector3.Lerp(transform.position, destination, transitionSpeed * Time.deltaTime);
    }
}
