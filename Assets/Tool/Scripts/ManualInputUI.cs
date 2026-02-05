using UnityEngine;

public class ManualInputUI : MonoBehaviour
{
    public FloorManager floorManager;
    private bool showUI = true;

    void Start()
    {
        if (!floorManager) floorManager = FindObjectOfType<FloorManager>();
    }

    private void OnGUI()
    {
        if (!showUI)
        {
            if (GUI.Button(new Rect(10, 10, 100, 30), "Show Tool")) showUI = true;
            return;
        }

        GUI.Box(new Rect(10, 10, 240, 600), "Manual Labeling Tool");

        if (GUI.Button(new Rect(20, 40, 90, 30), "Hide")) showUI = false;

        // Mode Toggle
        string modeBtn = DataCaptureManager.IsManualOverride ? "MODE: MANUAL" : "MODE: AUTO";
        if (GUI.Button(new Rect(120, 40, 120, 30), modeBtn))
        {
            DataCaptureManager.IsManualOverride = !DataCaptureManager.IsManualOverride;
            if (!DataCaptureManager.IsManualOverride)
            {
                DataCaptureManager.CurrentRegion = "Unknown";
            }
        }

        if (!DataCaptureManager.IsManualOverride)
        {
            GUI.Label(new Rect(20, 80, 200, 30), "Walk into zones to label.");
            GUI.Label(new Rect(20, 110, 200, 30), $"Region: {DataCaptureManager.CurrentRegion}");
            return;
        }

        // Floors
        if (floorManager != null)
        {
            GUI.Label(new Rect(20, 80, 200, 20), $"Current Floor: {floorManager.currentFloor}");
            if (GUI.Button(new Rect(20, 100, 60, 30), "F1")) floorManager.SetFloor(1);
            if (GUI.Button(new Rect(85, 100, 60, 30), "F2")) floorManager.SetFloor(2);
            if (GUI.Button(new Rect(150, 100, 60, 30), "F3")) floorManager.SetFloor(3);
        }
        else
        {
            GUI.Label(new Rect(20, 80, 200, 20), "FloorManager not found!");
        }

        // Rooms
        GUI.Label(new Rect(20, 140, 200, 20), "Set Region:");

        int y = 160;
        int currentF = floorManager ? floorManager.currentFloor : 1;

        for (int i = 1; i <= 5; i++)
        {
            if (GUI.Button(new Rect(20, y, 200, 30), $"Room {i}"))
            {
                DataCaptureManager.CurrentRegion = $"Room_F{currentF}_R{i}";
            }
            y += 35;
        }

        y += 10;
        if (GUI.Button(new Rect(20, y, 95, 30), "Stair A"))
            DataCaptureManager.CurrentRegion = $"Stair_F{currentF}_A";

        if (GUI.Button(new Rect(125, y, 95, 30), "Stair B"))
            DataCaptureManager.CurrentRegion = $"Stair_F{currentF}_B";

        y += 40;
        if (GUI.Button(new Rect(20, y, 200, 30), "Hallway"))
            DataCaptureManager.CurrentRegion = $"Hallway_F{currentF}";

        GUI.Label(new Rect(20, y + 40, 220, 30), $"Current: {DataCaptureManager.CurrentRegion}");

        y += 80;
        if (GUI.Button(new Rect(20, y, 200, 30), "Anchor at Camera"))
        {
            
            DataCaptureManager.CurrentRegion = $"Hallway_F{currentF}";
            var dcm = FindObjectOfType<DataCaptureManager>();
            if (dcm != null) dcm.AnchorNow();
            else Debug.LogWarning("DataCaptureManager not found to call AnchorNow.");
        }
    }
}
