using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class FloorManager : MonoBehaviour
{
    [Header("Floor Configuration")]
    public int currentFloor = 1;
    public int maxFloors = 10;

    public Transform[] floorAnchors;
    private ARAnchorManager anchorManager;

    void Awake()
    {
        floorAnchors = new Transform[maxFloors];
        anchorManager = FindObjectOfType<ARAnchorManager>();

        if (anchorManager == null)
            Debug.LogError("[FloorManager] ARAnchorManager not found");
    }

    public void SetFloor(int floor)
    {
        currentFloor = Mathf.Clamp(floor, 1, maxFloors);
        Debug.Log($"[FloorManager] Active floor set to {currentFloor}");
    }

    public bool HasAnchorForCurrentFloor()
    {
        return GetCurrentAnchor() != null;
    }

    public Transform GetCurrentAnchor()
    {
        int idx = currentFloor - 1;
        if (idx < 0 || idx >= floorAnchors.Length)
            return null;

        return floorAnchors[idx];
    }

    // 🔑 THIS IS THE IMPORTANT PART
    public void CreateAnchorAtCamera()
    {
        if (anchorManager == null)
            return;

        int idx = currentFloor - 1;

        // Remove existing anchor if present
        if (floorAnchors[idx] != null)
        {
            Destroy(floorAnchors[idx].gameObject);
            floorAnchors[idx] = null;
        }

        Transform cam = Camera.main.transform;

        Pose pose = new Pose(cam.position, cam.rotation);

        GameObject anchorGO = new GameObject($"Anchor_Floor_{currentFloor}");
        anchorGO.transform.SetPositionAndRotation(pose.position, pose.rotation);
        anchorGO.transform.parent = anchorManager.transform;
        ARAnchor anchor = anchorGO.AddComponent<ARAnchor>();

        if (anchor == null)
        {
            Debug.LogError("[FloorManager] Failed to create anchor");
            Destroy(anchorGO);
            return;
        }

        floorAnchors[idx] = anchor.transform;

        Debug.Log($"[FloorManager] Anchor created for floor {currentFloor}");
    }
}
