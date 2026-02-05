using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneSetupHelper : MonoBehaviour
{
    public int floors = 3;
    public int roomsPerFloor = 5;
    public int staircasesPerFloor = 2;

    public float floorHeight = 4.0f;
    public Vector3 roomSize = new Vector3(8, 4, 8);
    public Vector3 stairSize = new Vector3(4, 4, 10);

    [ContextMenu("Generate Building Hierarchy")]
    public void GenerateHierarchy()
    {
        for (int f = 1; f <= floors; f++)
        {
            GameObject floorRoot = new GameObject($"Floor_{f}");
            floorRoot.transform.parent = this.transform;
            floorRoot.transform.localPosition = new Vector3(0, (f - 1) * floorHeight, 0);

            // Create Floor Anchor
            GameObject anchor = new GameObject($"Anchor_F{f}");
            anchor.transform.parent = floorRoot.transform;
            anchor.transform.localPosition = Vector3.zero;

            // Create Rooms
            for (int r = 1; r <= roomsPerFloor; r++)
            {
                CreateRegion(floorRoot.transform, $"Room_F{f}_R{r}", new Vector3(r * 10, 0, 0), roomSize, f);
            }

            // Create Staircases
            for (int s = 1; s <= staircasesPerFloor; s++)
            {
                char stairId = (char)('A' + s - 1);
                CreateRegion(floorRoot.transform, $"Stair_F{f}_{stairId}", new Vector3(0, 0, s * 12), stairSize, f);
            }
        }
        Debug.Log("Building hierarchy generated. Please adjust collider positions manually.");
    }

    private void CreateRegion(Transform parent, string id, Vector3 pos, Vector3 size, int floor)
    {
        GameObject go = new GameObject(id);
        go.transform.parent = parent;
        go.transform.localPosition = pos;

        BoxCollider col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = size;
        col.center = new Vector3(0, size.y / 2, 0);

        RegionTrigger rt = go.AddComponent<RegionTrigger>();
        rt.regionId = id;
        rt.setFloorTo = floor; // Ensure floor is set when entering any region on that floor
    }
}
