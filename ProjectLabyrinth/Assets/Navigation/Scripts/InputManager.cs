using UnityEngine;

public class InputManager : MonoBehaviour
{
    // Placeholder class for testing navigation
    public GameObject targetPoint;

    public float tickRate = 999999999f;
    private float tickCounter;

    private void Update()
    {
        if (tickCounter >= tickRate)
        {
            //Get mouse position in screen space
            Vector3 mouseScreenPos = Input.mousePosition;

            // Convert to world space (with z depth)
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(
                new Vector3(mouseScreenPos.x, mouseScreenPos.y, -Camera.main.transform.position.z)
            );

            targetPoint.transform.position = worldPos;
            tickCounter = 0f;
        }

        tickCounter += Time.deltaTime;
    }
}
