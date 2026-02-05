using UnityEngine;

public class RegionTrigger : MonoBehaviour
{
    [Tooltip("Example: Room_F2_R3 or Stair_F1_A")]
    public string regionId;

    [Tooltip("If > 0, entering this region will update the FloorManager's current floor.")]
    public int setFloorTo = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("MainCamera")) return;
        
        if (DataCaptureManager.IsManualOverride) return;

        DataCaptureManager.CurrentRegion = regionId;
        
        if (setFloorTo > 0)
        {
            var fm = Object.FindAnyObjectByType<FloorManager>();
            if (fm != null) fm.SetFloor(setFloorTo);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("MainCamera")) return;

        if (DataCaptureManager.IsManualOverride) return;

        DataCaptureManager.CurrentRegion = "Unknown";
    }
}