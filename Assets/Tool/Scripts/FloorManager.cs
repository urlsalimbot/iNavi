using UnityEngine;

public class FloorManager : MonoBehaviour
{
    [Header("Floor Configuration")]
    public int currentFloor = 1;

    [Tooltip("Index 0 = Floor 1, Index 1 = Floor 2, etc.")]
    public Transform[] floorAnchors;

    [ContextMenu("Auto-populate Anchors")]
    public void AutoPopulateAnchors()
    {
        var anchors = new System.Collections.Generic.List<Transform>();
        for (int i = 1; i <= 10; i++) // Support up to 10 floors
        {
            GameObject go = GameObject.Find($"Anchor_F{i}");
            if (go != null) anchors.Add(go.transform);
            else break;
        }
        floorAnchors = anchors.ToArray();
        Debug.Log($"[FloorManager] Populated {floorAnchors.Length} anchors.");
    }

    public void SetFloor(int floor)
    {
        currentFloor = Mathf.Clamp(floor, 1, floorAnchors.Length);
        Debug.Log($"[FloorManager] Active floor set to {currentFloor}");
    }

    public Transform GetCurrentAnchor()
    {
        if (currentFloor - 1 >= floorAnchors.Length)
        {
            Debug.LogError("[FloorManager] No anchor for current floor!");
            return null;
        }
        return floorAnchors[currentFloor - 1];
    }
}
