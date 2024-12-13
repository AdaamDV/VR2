using UnityEngine;

public class LookAtMe : MonoBehaviour
{
    [SerializeField] private LookAtMe lookAtMe;
    void LateUpdate()
    {
        // Get the position of the main camera
        Vector3 cameraPosition = Camera.main.transform.position;

        // Constrain the camera position to the same y-coordinate as the object
        Vector3 lookAtPosition = new Vector3(cameraPosition.x, transform.position.y, cameraPosition.z);

        // Make the object look at the camera
        transform.LookAt(lookAtPosition);
    }
}
