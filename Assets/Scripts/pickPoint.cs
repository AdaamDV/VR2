using Unity.Hierarchy;
using UnityEngine;

public class pickPoint : MonoBehaviour
{
    public Camera cam;
    public float rayLength = 10f;
    public GameObject targetObjectPos; // Assigned to the positive x court
    public GameObject targetObjectNeg; // Assigned to the negative x court
    public GameObject screenCubePos; // The screen that will pop up
    public GameObject screenCubeNeg; // The screen that will pop up
    public LayerMask layerMask; // Set this to include all layers except "PlayerLayer"
    // Reference to the TriggerPointProjection script
    public TriggerPointProjection triggerPointProjectionPos;
    public TriggerPointProjection triggerPointProjectionNeg;
    public NumberDisplayManager numberDisplayManagerPos;
    public NumberDisplayManager numberDisplayManagerNeg;

    void Start()
    {
        // Exclude the PlayerLayer from the layer mask
        int playerLayer = LayerMask.NameToLayer("PlayerLayer");
        layerMask = ~LayerMask.GetMask(LayerMask.LayerToName(playerLayer));
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Visualize the ray for debugging
            Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.red, 1f);

            // Perform the raycast, ignoring the PlayerLayer
            if (Physics.Raycast(ray, out hit, rayLength, layerMask))
            {
                Debug.Log($"Hit: {hit.collider.gameObject.name}");
                if (hit.collider.gameObject == targetObjectPos)
                {
                    Debug.Log("Selected Object target Positive");

                    // Step 1: Get the hit point in world space
                    Vector3 worldHitPoint = hit.point;
                    Vector2 binHitPoint = new Vector2((worldHitPoint.z+25),(47 - worldHitPoint.x));
                    int k = Mathf.FloorToInt(binHitPoint.y)*50+Mathf.FloorToInt(binHitPoint.x); //index in data
                    Debug.Log(binHitPoint);
                    Debug.Log(k);
                    // Check if the reference to TriggerPointProjection is set
                    if (triggerPointProjectionPos != null)
                    {
                        float shotPercent = triggerPointProjectionPos.GetDetails(k, "Ratio");
                        float shotAttempts = triggerPointProjectionPos.GetDetails(k, "Total");
                        Debug.Log("Grid Sum Ratio at index " + k + ": " + shotPercent);
                        //let's put the number on the texture
                        numberDisplayManagerPos.UpdateGraphic(shotPercent*100, shotAttempts);
                        //now we make the screen appear at coordinates:
                        Vector3 targetPosition = new Vector3(worldHitPoint.x, worldHitPoint.y + 5.7f, worldHitPoint.z);
                        screenCubePos.transform.position = targetPosition;
                    }
                    else
                    {
                        Debug.LogError("TriggerPointProjection reference is not set.");
                    }
                }
                else if (hit.collider.gameObject == targetObjectNeg)
                {
                    Debug.Log("Selected Object target Negative");

                    // Step 1: Get the hit point in world space
                    Vector3 worldHitPoint = hit.point;
                    Vector2 binHitPoint = new Vector2((-worldHitPoint.z+25),(47 + worldHitPoint.x));
                    int k = Mathf.FloorToInt(binHitPoint.y)*50+Mathf.FloorToInt(binHitPoint.x); //index in data
                    Debug.Log(binHitPoint);
                    Debug.Log(k);
                    // Check if the reference to TriggerPointProjection is set
                    if (triggerPointProjectionNeg != null)
                    {
                        float shotPercent = triggerPointProjectionNeg.GetDetails(k, "Ratio");
                        float shotAttempts = triggerPointProjectionPos.GetDetails(k, "Total");
                        Debug.Log("Grid Sum Ratio at index " + k + ": " + shotPercent);
                        //let's put the number on the texture
                        numberDisplayManagerNeg.UpdateGraphic(shotPercent*100, shotAttempts);
                        //now we make the screen appear at coordinates:
                        Vector3 targetPosition = new Vector3(worldHitPoint.x, worldHitPoint.y + 5.7f, worldHitPoint.z);
                        screenCubeNeg.transform.position = targetPosition;
                    }
                    else
                    {
                        Debug.LogError("TriggerPointProjection reference is not set.");
                    }
                }
                else if(hit.collider.gameObject.name == "Pos_D_All_Shots"){
                    triggerPointProjectionPos.HeatMapOnButtonClick(true, true);
                }else if(hit.collider.gameObject.name == "Pos_D_Made_Shots"){
                    triggerPointProjectionPos.HeatMapOnButtonClick(false, true);
                }
                else if(hit.collider.gameObject.name == "Pos_D_Missed_Shots"){
                    triggerPointProjectionPos.HeatMapOnButtonClick(false, false);
                }
                else if(hit.collider.gameObject.name == "Neg_D_All_Shots"){
                    triggerPointProjectionNeg.HeatMapOnButtonClick(true, true);
                }else if(hit.collider.gameObject.name == "Neg_D_Made_Shots"){
                    triggerPointProjectionNeg.HeatMapOnButtonClick(false, true);
                }
                else if(hit.collider.gameObject.name == "Neg_D_Missed_Shots"){
                    triggerPointProjectionNeg.HeatMapOnButtonClick(false, false);
                }
                else
                {
                    Debug.Log("Wrong Object or No Selection");
                }
                
            }
            else
            {
                Debug.Log("No Object Hit");
            }
        }
    }
}
