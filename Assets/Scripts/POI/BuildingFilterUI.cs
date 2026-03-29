using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages building filter buttons in the destination select UI.
/// Attach this to the Destination Select UI GameObject.
/// </summary>
public class BuildingFilterUI : MonoBehaviour
{
    [Header("Filter Settings")]
    [Tooltip("The BuildingGroupedSelectList component to interact with")]
    public BuildingGroupedSelectList groupedSelectList;

    [Header("Filter Buttons")]
    [Tooltip("Button to show all buildings (no filter)")]
    public Button showAllButton;
    
    [Tooltip("Button to filter New Building rooms")]
    public Button nbFilterButton;
    
    [Tooltip("Button to filter Computer Science Building rooms")]
    public Button csFilterButton;
    
    [Tooltip("Button to filter Admin Building rooms")]
    public Button abFilterButton;
    
    [Tooltip("Button to filter Main Building rooms")]
    public Button mbFilterButton;

    [Header("Button Visuals")]
    [Tooltip("Color when button is selected/active")]
    public Color selectedColor = new Color(0.2f, 0.6f, 1f, 1f);
    
    [Tooltip("Color when button is not selected")]
    public Color normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    [Header("Button Text References")]
    public TextMeshProUGUI showAllText;
    public TextMeshProUGUI nbButtonText;
    public TextMeshProUGUI csButtonText;
    public TextMeshProUGUI abButtonText;
    public TextMeshProUGUI mbButtonText;

    [Header("Button Background References")]
    public Image showAllButtonImage;
    public Image nbButtonImage;
    public Image csButtonImage;
    public Image abButtonImage;
    public Image mbButtonImage;

    // Current filter state (null = show all)
    private string currentFilter = null;

    // Available building filters
    private static readonly string[] BuildingFilters = { "NB", "CS", "AB", "MB" };

    void Start()
    {
        InitializeButtons();
    }

    /// <summary>
    /// Initializes all filter buttons with click listeners and visual state.
    /// </summary>
    public void InitializeButtons()
    {
        // Set up Show All button
        if (showAllButton != null)
        {
            showAllButton.onClick.AddListener(() => SetFilter(null));
            UpdateButtonVisual(showAllButton, showAllButtonImage, true);
        }

        // Set up NB button
        if (nbFilterButton != null)
        {
            nbFilterButton.onClick.AddListener(() => SetFilter("NB"));
            UpdateButtonVisual(nbFilterButton, nbButtonImage, false);
            if (nbButtonText != null) nbButtonText.text = "New Building";
        }

        // Set up CS button
        if (csFilterButton != null)
        {
            csFilterButton.onClick.AddListener(() => SetFilter("CS"));
            UpdateButtonVisual(csFilterButton, csButtonImage, false);
            if (csButtonText != null) csButtonText.text = "Computer Sci";
        }

        // Set up AB button
        if (abFilterButton != null)
        {
            abFilterButton.onClick.AddListener(() => SetFilter("AB"));
            UpdateButtonVisual(abFilterButton, abButtonImage, false);
            if (abButtonText != null) abButtonText.text = "Admin";
        }

        // Set up MB button
        if (mbFilterButton != null)
        {
            mbFilterButton.onClick.AddListener(() => SetFilter("MB"));
            UpdateButtonVisual(mbFilterButton, mbButtonImage, false);
            if (mbButtonText != null) mbButtonText.text = "Main";
        }

        if (showAllText != null) showAllText.text = "All";
    }

    /// <summary>
    /// Sets the current building filter and updates the UI.
    /// </summary>
    /// <param name="buildingCode">Building code to filter by, or null to show all</param>
    public void SetFilter(string buildingCode)
    {
        currentFilter = buildingCode;

        // Update button visuals
        UpdateButtonVisual(showAllButton, showAllButtonImage, buildingCode == null);
        UpdateButtonVisual(nbFilterButton, nbButtonImage, buildingCode == "NB");
        UpdateButtonVisual(csFilterButton, csButtonImage, buildingCode == "CS");
        UpdateButtonVisual(abFilterButton, abButtonImage, buildingCode == "AB");
        UpdateButtonVisual(mbFilterButton, mbButtonImage, buildingCode == "MB");

        // Apply filter to the grouped select list
        if (groupedSelectList != null)
        {
            groupedSelectList.ApplyBuildingFilter(buildingCode);
        }
    }

    /// <summary>
    /// Gets the current building filter.
    /// </summary>
    /// <returns>Building code or null if showing all</returns>
    public string GetCurrentFilter()
    {
        return currentFilter;
    }

    /// <summary>
    /// Checks if a building code passes the current filter.
    /// </summary>
    /// <param name="buildingCode">The building code to check</param>
    /// <returns>True if the building should be shown</returns>
    public bool PassesFilter(string buildingCode)
    {
        // No filter = show all
        if (string.IsNullOrEmpty(currentFilter))
            return true;

        // Check if building matches filter
        return buildingCode == currentFilter;
    }

    /// <summary>
    /// Gets all active building filters.
    /// </summary>
    /// <returns>Array of building codes to show, or null for all</returns>
    public string[] GetActiveFilters()
    {
        if (string.IsNullOrEmpty(currentFilter))
            return BuildingFilters;

        return new[] { currentFilter };
    }

    /// <summary>
    /// Resets the filter to show all buildings.
    /// </summary>
    public void ResetFilter()
    {
        SetFilter(null);
    }

    /// <summary>
    /// Updates the visual appearance of a filter button.
    /// </summary>
    private void UpdateButtonVisual(Button button, Image buttonImage, bool isSelected)
    {
        if (buttonImage != null)
        {
            buttonImage.color = isSelected ? selectedColor : normalColor;
        }
    }

    /// <summary>
    /// Called when the destination UI is shown to reset filter state.
    /// </summary>
    public void OnDestinationUIOpened()
    {
        ResetFilter();
    }
}
