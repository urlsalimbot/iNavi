# Building Filter UI Setup Guide

This document explains how to set up the building filter buttons for the destination list UI.

## Overview

The building filter system allows users to filter POIs (Points of Interest) by building:
- **NB** = New Building
- **CS** = Computer Science Building
- **AB** = Admin Building
- **MB** = Main Building

## Components

### 1. BuildingFilterUI

Attach this to the "Destination Select" GameObject.

**Setup Steps:**

1. Select the "Destination Select" GameObject in the hierarchy
2. Add Component → `BuildingFilterUI`
3. Configure the following fields:

**Filter Settings:**
- `Grouped Select List`: Drag the GameObject that has `BuildingGroupedSelectList` component

**Filter Buttons** (create 5 buttons as children of "Destination Select" → "DestinationList" → "Header"):
- `Show All Button`: Button to show all buildings
- `NB Filter Button`: Button for New Building
- `CS Filter Button`: Button for Computer Science Building
- `AB Filter Button`: Button for Admin Building
- `MB Filter Button`: Button for Main Building

**Button Text References** (optional, for button labels):
- `Show All Text`: TextMeshProUGUI on Show All Button
- `NB Button Text`: TextMeshProUGUI on NB Button
- `CS Button Text`: TextMeshProUGUI on CS Button
- `AB Button Text`: TextMeshProUGUI on AB Button
- `MB Button Text`: TextMeshProUGUI on MB Button

**Button Background References** (optional, for button colors):
- `Show All Button Image`: Image component on Show All Button
- `NB Button Image`: Image component on NB Button
- `CS Button Image`: Image component on CS Button
- `AB Button Image`: Image component on AB Button
- `MB Button Image`: Image component on MB Button

**Visual Settings:**
- `Selected Color`: Color when button is active (default: blue)
- `Normal Color`: Color when button is inactive (default: dark gray)

### 2. BuildingGroupedSelectList

This component already exists. Just connect the new field:

1. Select the GameObject with `BuildingGroupedSelectList` component
2. Find the `Filter Settings` section
3. `Building Filter UI`: Drag the "Destination Select" GameObject (has BuildingFilterUI)

### 3. BuildingGroupHeader Prefab

The prefab has been updated to include a filter button. To update it in your scene:

1. Navigate to `Assets/Scripts/POI/Prefabs/BuildingGroupHeader.prefab`
2. The prefab now includes:
   - A "Filter" button on the right side of the header
   - The button automatically calls `ApplyBuildingFilter()` when clicked

**If you need to re-apply the prefab:**
1. Select all existing BuildingGroupHeader instances in your scene
2. In the Inspector, click "Select" next to the prefab reference
3. Choose the updated `BuildingGroupHeader.prefab`

## Creating the Filter Buttons UI

### Step 1: Create Button Container

1. In the hierarchy, navigate to: `Destination Select` → `DestinationList` → `Header`
2. Right-click → UI → Panel
3. Name it "FilterContainer"
4. Set RectTransform:
   - Anchor: Top Stretch
   - Position: (0, -40, 0)
   - Size: (0, 40)

### Step 2: Create Individual Buttons

For each building filter button (5 total + 1 "All" button):

1. Right-click on "FilterContainer" → UI → Button - TextMeshPro
2. Name it appropriately (e.g., "FilterButton_All", "FilterButton_NB", etc.)
3. Set RectTransform for each button (horizontal layout recommended)
4. Configure button colors in the Button component

### Step 3: Recommended Layout

For a horizontal button layout:

1. Add a `Horizontal Layout Group` component to "FilterContainer"
   - Spacing: 5
   - Child Alignment: Middle Center
   - Child Force Expand: Width = false, Height = false
2. Add `Content Size Fitter` to "FilterContainer"
   - Horizontal Fit: Preferred Size
   - Vertical Fit: Preferred Size
3. Set each button's RectTransform:
   - Width: 100
   - Height: 35

### Step 4: Button Text Labels

Set the text on each button:
- "All" - Shows all buildings
- "New" - Filters to New Building only
- "Comp Sci" - Filters to Computer Science Building only
- "Admin" - Filters to Admin Building only
- "Main" - Filters to Main Building only

## Updating NavigationUIHelper

If you're using `NavigationUIHelper.cs`, update the `ToggleDestinationSelectUI` method call:

```csharp
// Old code:
NavigationUIController.Instance.ToggleDestinationSelectUI();

// New code (if using BuildingGroupedSelectList):
var groupedSelectList = FindObjectOfType<BuildingGroupedSelectList>();
if (groupedSelectList != null)
{
    groupedSelectList.TogglePOIList();
}
```

## Testing

1. Enter Play Mode
2. Click the POI/Destination button to open the destination list
3. You should see:
   - POIs grouped by building (NB, CS, AB, MB, COMMON, OTHER)
   - Each building header has a "Filter" button
   - Top-level filter buttons (if created) to filter by building
4. Click a filter button - only POIs from that building should show
5. Click "All" to reset the filter

## Troubleshooting

### Filter buttons not working
- Ensure `BuildingFilterUI.groupedSelectList` is assigned
- Check console for errors
- Verify `BuildingGroupedSelectList.buildingFilterUI` is assigned

### POIs not grouped correctly
- Check POI naming convention (should start with NB, CS, AB, MB, etc.)
- Use `customBuildingAssignments` in `BuildingGroupedSelectList` for overrides

### Filter button on header not working
- Ensure `BuildingGroupHeader.filterButton` is assigned in the prefab
- Check that `BuildingGroupedSelectList` is in the scene

## Files Modified/Created

- `Assets/Scripts/POI/BuildingFilterUI.cs` (NEW)
- `Assets/Scripts/POI/BuildingGroupedSelectList.cs` (UPDATED)
- `Assets/Scripts/POI/BuildingGroupHeader.cs` (UPDATED)
- `Assets/Scripts/POI/Prefabs/BuildingGroupHeader.prefab` (UPDATED)
