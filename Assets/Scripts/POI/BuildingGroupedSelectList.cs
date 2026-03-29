using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Component that groups POIs by building in the SelectList UI.
/// Attach this to the same GameObject as SelectList.
/// This component uses method patching to intercept SelectList calls.
/// </summary>
[RequireComponent(typeof(SelectList))]
public class BuildingGroupedSelectList : MonoBehaviour
{
    [Header("Building Group Settings")]
    [Tooltip("Prefab for building group headers")]
    public GameObject buildingHeaderPrefab;
    
    [Tooltip("Height of building header in pixels")]
    public int headerHeight = 40;
    
    [Tooltip("Enable building grouping")]
    public bool groupByBuilding = true;
    
    [Header("UI References")]
    [Tooltip("The destination select UI GameObject to toggle")]
    public GameObject destinationSelectUI;

    [Header("Filter Settings")]
    [Tooltip("Optional: Building filter UI component")]
    public BuildingFilterUI buildingFilterUI;

    [Header("Optional: Custom building assignments")]
    [Tooltip("Override automatic building detection for specific POIs")]
    public List<BuildingAssignment> customBuildingAssignments = new List<BuildingAssignment>();

    // Current building filter (null = show all)
    private string currentBuildingFilter = null;
    
    private SelectList selectList;
    private List<ListItemData> currentItemsTotal;
    private MethodInfo originalRenderPOIsMethod;
    
    void Awake()
    {
        selectList = GetComponent<SelectList>();
        
        if (selectList == null)
        {
            Debug.LogError("BuildingGroupedSelectList: SelectList component not found!");
            enabled = false;
            return;
        }
        
        // Disable the original SelectList component to prevent it from rendering
        // We'll handle all rendering ourselves
        selectList.enabled = false;
    }
    
    void Start()
    {
        if (groupByBuilding)
        {
            PrepareAllData();
            // Render immediately if grouping is enabled
            RenderPOIsGroupedByBuilding();
        }
    }
    
    /// <summary>
    /// Toggles the POI list UI and renders POIs grouped by building.
    /// Use this as a replacement for NavigationUIController.ToggleDestinationSelectUI().
    /// </summary>
    public void TogglePOIList()
    {
        if (destinationSelectUI == null)
        {
            Debug.LogError("BuildingGroupedSelectList: destinationSelectUI not assigned! Cannot toggle POI list.");
            return;
        }

        Debug.Log($"BuildingGroupedSelectList: TogglePOIList called. Current active: {destinationSelectUI.activeSelf}");

        destinationSelectUI.SetActive(!destinationSelectUI.activeSelf);

        if (!destinationSelectUI.activeSelf)
        {
            Debug.Log("BuildingGroupedSelectList: Hiding destination select UI");
            ResetPOISearch();
            return;
        }

        Debug.Log("BuildingGroupedSelectList: Showing destination select UI and rendering POIs");
        
        // Reset filter when opening
        if (buildingFilterUI != null)
        {
            buildingFilterUI.OnDestinationUIOpened();
        }
        
        RenderPOIs();
    }
    
    /// <summary>
    /// Call this to render POIs grouped by building.
    /// This is called instead of SelectList.RenderPOIs().
    /// </summary>
    public void RenderPOIs()
    {
        if (selectList == null) return;
        
        if (groupByBuilding && buildingHeaderPrefab != null)
        {
            RenderPOIsGroupedByBuilding();
        }
        else
        {
            // Fall back to original behavior
            selectList.RenderList(selectList.pois);
        }
        currentItemsTotal = new List<ListItemData>(selectList.pois);
    }
    
    /// <summary>
    /// Renders POIs grouped by their building.
    /// </summary>
    public void RenderPOIsGroupedByBuilding()
    {
        if (selectList == null)
        {
            Debug.LogError("BuildingGroupedSelectList: selectList is null!");
            return;
        }
        if (selectList.content == null)
        {
            Debug.LogError("BuildingGroupedSelectList: selectList.content is null!");
            return;
        }
        if (selectList.SpawnPoint == null)
        {
            Debug.LogError("BuildingGroupedSelectList: selectList.SpawnPoint is null!");
            return;
        }
        if (buildingHeaderPrefab == null)
        {
            Debug.LogError("BuildingGroupedSelectList: buildingHeaderPrefab is null!");
            return;
        }
        
        // Ensure we have POIs
        if (selectList.pois == null || selectList.pois.Count == 0)
        {
            PrepareAllData();
        }
        
        if (selectList.pois == null || selectList.pois.Count == 0)
        {
            Debug.LogWarning("BuildingGroupedSelectList: No POIs found to render!");
            return;
        }
        
        Debug.Log($"BuildingGroupedSelectList: Rendering {selectList.pois.Count} POIs grouped by building");
        
        // Group POIs by building
        var groupedPOIs = GroupPOIsByBuilding(selectList.pois);
        Debug.Log($"BuildingGroupedSelectList: Grouped into {groupedPOIs.Count} buildings");
        
        // Remove previous items first
        foreach (Transform child in selectList.SpawnPoint.transform)
        {
            Destroy(child.gameObject);
        }
        
        int totalHeight = 0;
        
        // Render each building group
        foreach (var buildingGroup in groupedPOIs)
        {
            Debug.Log($"BuildingGroupedSelectList: Rendering building '{buildingGroup.Key}' with {buildingGroup.Value.Count} POIs");
            
            // Create building header
            if (buildingHeaderPrefab != null)
            {
                Vector3 headerPos = new Vector3(selectList.SpawnPoint.localPosition.x, -totalHeight, selectList.SpawnPoint.localPosition.z);
                GameObject headerObj = Instantiate(buildingHeaderPrefab, headerPos, selectList.SpawnPoint.rotation);
                headerObj.transform.SetParent(selectList.SpawnPoint, false);
                
                BuildingGroupHeader header = headerObj.GetComponent<BuildingGroupHeader>();
                if (header != null)
                {
                    header.SetBuildingName(buildingGroup.Key);
                    Debug.Log($"BuildingGroupedSelectList: Set header name to '{buildingGroup.Key}'");
                }
                else
                {
                    Debug.LogError($"BuildingGroupedSelectList: Header prefab doesn't have BuildingGroupHeader component! GameObject: {headerObj.name}");
                }
                
                totalHeight += headerHeight;
            }
            
            // Sort POIs within the group alphabetically
            var sortedPOIs = buildingGroup.Value.OrderBy(p => p.listTitle).ToList();
            
            // Render POIs in this group
            foreach (ListItemData item in sortedPOIs)
            {
                float spawnY = totalHeight;
                Vector3 pos = new Vector3(selectList.SpawnPoint.localPosition.x, -spawnY, selectList.SpawnPoint.localPosition.z);
                
                GameObject spawnedItem = Instantiate(selectList.spawnItem, pos, selectList.SpawnPoint.rotation);
                spawnedItem.transform.SetParent(selectList.SpawnPoint, false);
                
                ListItemUI itemUI = spawnedItem.GetComponent<ListItemUI>();
                if (itemUI != null)
                {
                    itemUI.SetListItemData(item);
                }
                
                totalHeight += selectList.heightOfPrefab;
            }
        }
        
        // Set content holder height
        selectList.content.sizeDelta = new Vector2(0, totalHeight);
        
        Debug.Log($"BuildingGroupedSelectList: Rendered {selectList.pois.Count} POIs in {groupedPOIs.Count} building groups. Total height: {totalHeight}");
    }
    
    /// <summary>
    /// Groups POIs by their building based on naming convention.
    /// </summary>
    private Dictionary<string, List<ListItemData>> GroupPOIsByBuilding(List<ListItemData> pois)
    {
        var grouped = new Dictionary<string, List<ListItemData>>();

        foreach (ListItemData poi in pois)
        {
            string building = GetBuildingFromPOI(poi);

            // Apply building filter - skip POIs that don't match the filter
            if (!string.IsNullOrEmpty(currentBuildingFilter) && building != currentBuildingFilter)
            {
                continue;
            }

            if (!grouped.ContainsKey(building))
            {
                grouped[building] = new List<ListItemData>();
            }

            grouped[building].Add(poi);
        }

        // Sort buildings by priority (NB, CS, AB, MB, then others)
        var sortedGrouped = new Dictionary<string, List<ListItemData>>();
        string[] priorityOrder = { "NB", "CS", "AB", "MB", "COMMON" };

        foreach (string building in priorityOrder)
        {
            if (grouped.ContainsKey(building))
            {
                sortedGrouped[building] = grouped[building];
                grouped.Remove(building);
            }
        }

        // Add remaining buildings
        foreach (var kvp in grouped.OrderBy(k => k.Key))
        {
            sortedGrouped[kvp.Key] = kvp.Value;
        }

        return sortedGrouped;
    }
    
    /// <summary>
    /// Extracts building code from POI name or uses custom assignment.
    /// </summary>
    private string GetBuildingFromPOI(ListItemData poi)
    {
        // Check custom assignments first
        var customAssignment = customBuildingAssignments.Find(a => a.poiName == poi.listTitle);
        if (customAssignment != null && !string.IsNullOrEmpty(customAssignment.building))
        {
            return customAssignment.building.ToUpper();
        }

        string name = poi.listTitle.ToUpper();

        // Check for building codes at the start of the name
        if (name.StartsWith("CS"))
            return "CS";
        if (name.StartsWith("NB"))
            return "NB";
        if (name.StartsWith("AB"))
            return "AB";
        if (name.StartsWith("MB"))
            return "MB";
        if (name.StartsWith("AVR"))
            return "AVR";

        // Check for common areas
        if (name.Contains("COMFORT ROOM") ||
            name.Contains("TEACHER'S LOUNGE") ||
            name.Contains("OFFICE OF STUDENT AFFAIRS") ||
            name.Contains("TESTING ROOM"))
        {
            return "COMMON";
        }

        // Default to the first word or "OTHER"
        string[] parts = name.Split(' ');
        if (parts.Length > 0 && parts[0].Length <= 5)
        {
            return parts[0];
        }

        return "OTHER";
    }

    /// <summary>
    /// Applies a building filter and re-renders the POI list.
    /// </summary>
    /// <param name="buildingCode">Building code to filter by, or null to show all</param>
    public void ApplyBuildingFilter(string buildingCode)
    {
        currentBuildingFilter = buildingCode;
        
        if (string.IsNullOrEmpty(buildingCode))
        {
            Debug.Log("BuildingGroupedSelectList: Filter cleared - showing all buildings");
        }
        else
        {
            Debug.Log($"BuildingGroupedSelectList: Filter set to building '{buildingCode}'");
        }
        
        // Re-render with the new filter
        RenderPOIsGroupedByBuilding();
    }

    /// <summary>
    /// Gets the current building filter.
    /// </summary>
    public string GetCurrentBuildingFilter()
    {
        return currentBuildingFilter;
    }

    /// <summary>
    /// Call when search string changed.
    /// </summary>
    public void SearchPOIOnSearchChanged(string search)
    {
        if (selectList == null) return;
        
        if (search == "")
        {
            if (selectList.resetButtonSearchField != null)
            {
                selectList.resetButtonSearchField.SetActive(false);
            }
            
            if (groupByBuilding && buildingHeaderPrefab != null)
            {
                RenderPOIsGroupedByBuilding();
            }
            else
            {
                selectList.RenderList(selectList.pois);
            }
            
            if (selectList.placeholder != null)
            {
                selectList.placeholder.SetActive(true);
            }
            return;
        }
        
        if (selectList.resetButtonSearchField != null)
        {
            selectList.resetButtonSearchField.SetActive(true);
        }
        
        // For search, show flat list without grouping
        selectList.RenderList(FilterByTitle(search));
    }
    
    /// <summary>
    /// Resets the POI search.
    /// </summary>
    public void ResetPOISearch()
    {
        if (selectList == null) return;
        
        if (selectList.searchField != null)
        {
            selectList.searchField.text = "";
        }
        
        if (selectList.resetButtonSearchField != null)
        {
            selectList.resetButtonSearchField.SetActive(false);
        }
        
        if (groupByBuilding && buildingHeaderPrefab != null)
        {
            RenderPOIsGroupedByBuilding();
        }
        else
        {
            selectList.RenderList(selectList.pois);
        }
        
        if (selectList.placeholder != null)
        {
            selectList.placeholder.SetActive(true);
        }
    }
    
    /// <summary>
    /// Filters poi list by title.
    /// </summary>
    List<ListItemData> FilterByTitle(string searchTerm)
    {
        string search = searchTerm.ToLower();
        List<ListItemData> filteredItems = currentItemsTotal.FindAll(x =>
        {
            return x.listTitle.ToLower().Contains(search);
        });
        return filteredItems;
    }
    
    /// <summary>
    /// Prepares all POI data from the NavigationController.
    /// </summary>
    void PrepareAllData()
    {
        if (selectList == null) return;
        
        selectList.pois = new List<ListItemData>();
        
        if (NavigationController.instance != null && NavigationController.instance.augmentedSpace != null)
        {
            foreach (var poi in NavigationController.instance.augmentedSpace.GetPOIs())
            {
                selectList.pois.Add(poi);
            }
        }
        
        Debug.Log($"BuildingGroupedSelectList: Loaded {selectList.pois.Count} POIs");
    }
}

[System.Serializable]
public class BuildingAssignment
{
    public string poiName;
    public string building;
}
