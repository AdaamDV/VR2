using UnityEngine;

public class camZoom : MonoBehaviour
{
    public Camera cam;
    public float zoomSpeed = 0.1f; // Time to transition between zoom levels
    private float targetFOV; // Target field of view
    private float currentFOV; // Smoothly adjust the current field of view

    void Start()
    {
        targetFOV = cam.fieldOfView; // Start with the current camera FoV
        currentFOV = targetFOV;
    }

    void Update()
    {
        // Set targetFOV based on key press
        if (Input.GetMouseButton(1))
        {
            targetFOV = 35; // Zoom in
        }
        else
        {
            targetFOV = 65; // Zoom out
        }

        // Smoothly interpolate current FoV towards the target FoV
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime / zoomSpeed);
        cam.fieldOfView = currentFOV;
    }
}
