using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for displaying building group headers in the POI list.
/// </summary>
public class BuildingGroupHeader : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI buildingNameText;
    public GameObject background;
    public Button filterButton;

    [Header("Settings")]
    public Color headerBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
    public Color headerTextColor = Color.white;
    public float headerHeight = 40f;

    private string buildingName;
    private BuildingGroupedSelectList parentGroupedList;

    void Awake()
    {
        // Find the BuildingGroupedSelectList component in the scene
        parentGroupedList = Object.FindFirstObjectByType<BuildingGroupedSelectList>();
    }

    /// <summary>
    /// Sets the building name for this header.
    /// </summary>
    public void SetBuildingName(string name)
    {
        buildingName = name;
        if (buildingNameText != null)
        {
            buildingNameText.text = GetBuildingDisplayName(name);
            buildingNameText.color = headerTextColor;
        }

        if (background != null)
        {
            var image = background.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                image.color = headerBackgroundColor;
            }
        }

        // Set up filter button
        if (filterButton != null)
        {
            filterButton.onClick.RemoveAllListeners();
            filterButton.onClick.AddListener(OnFilterButtonClicked);
        }
    }

    /// <summary>
    /// Called when the filter button is clicked.
    /// </summary>
    private void OnFilterButtonClicked()
    {
        if (parentGroupedList != null && !string.IsNullOrEmpty(buildingName))
        {
            parentGroupedList.ApplyBuildingFilter(buildingName);
        }
    }
    
    /// <summary>
    /// Converts building code to a human-readable name.
    /// </summary>
    public static string GetBuildingDisplayName(string buildingCode)
    {
        switch (buildingCode.ToUpper())
        {
            case "CS":
                return "Computer Science Building";
            case "NB":
                return "New Building";
            case "AVR":
                return "Admin Building";
            case "COMMON":
                return "📍 Common Areas";
            default:
                return "📍 " + buildingCode;
        }
    }
}
