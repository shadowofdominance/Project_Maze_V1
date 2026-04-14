using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class PlayerController : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Camera mainCamera;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        
        // Optional: Make the line look nice if material isn't set
        if (lineRenderer.sharedMaterial == null) {
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Mouse.current == null) return;

        // Start a new path when mouse is clicked
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            lineRenderer.positionCount = 0;
        }

        // Add points while dragging
        if (Mouse.current.leftButton.isPressed)
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos.z = 10f; // Distance from camera (adjust as needed for your maze)
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

            // Add the new point if it's the first one, or if we moved far enough from the last one
            if (lineRenderer.positionCount == 0 || Vector3.Distance(worldPos, lineRenderer.GetPosition(lineRenderer.positionCount - 1)) > 0.1f)
            {
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, worldPos);
            }
        }
    }
}
