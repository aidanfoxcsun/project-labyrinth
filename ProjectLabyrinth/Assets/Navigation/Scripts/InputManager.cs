using UnityEngine;

public class InputManager : MonoBehaviour
{
    public GameObject targetPoint;

    public NavigationAgent agent;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Get mouse position in screen space
            Vector3 mouseScreenPos = Input.mousePosition;

            // Convert to world space (with z depth)
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(
                new Vector3(mouseScreenPos.x, mouseScreenPos.y, -Camera.main.transform.position.z )
            );

            targetPoint.transform.position = worldPos;
            agent.SetDestination(worldPos);
        }
    }
}
