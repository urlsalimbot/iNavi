# POI Building Grouping Setup Guide

This guide explains how to organize POIs by building in the UI.

## Overview

The solution adds building group headers to the POI list, organizing POIs into:
- **Computer Science Building** (CS201, CS301, etc.)
- **North Building** (NB201, NB202, etc.)
- **AVR Building**
- **Common Areas** (Comfort Rooms, Offices, etc.)

## Files Created

```
Assets/Scripts/POI/
├── BuildingGroupHeader.cs       # Building header UI component
├── BuildingGroupedSelectList.cs # Component that groups POIs by building
├── NavigationUIHelper.cs        # Helper for navigation UI visibility
└── Prefabs/
    └── BuildingGroupHeader.prefab # Header prefab
```

## Current Scene Configuration (Production.unity)

The scene has been pre-configured with:
- `BuildingGroupedSelectList` component on the SelectList GameObject
- `NavigationUIHelper` component on the NavigationUI GameObject
- All button handlers updated to call `BuildingGroupedSelectList` methods
- All UI references properly assigned

## How It Works

### BuildingGroupedSelectList
1. Automatically finds the `SelectList` component on the same GameObject
2. Disables the original `SelectList` to prevent duplicate rendering
3. Loads POIs from `NavigationController.instance.augmentedSpace.GetPOIs()`
4. Groups POIs by building based on naming conventions
5. Renders building headers and POIs in groups

### NavigationUIHelper
1. Monitors `NavigationController.instance.IsCurrentlyNavigating()`
2. Automatically shows/hides Progress Slider and Stop Button
3. Ensures UI state matches navigation state

## Building Detection Logic

The system automatically detects buildings from POI names:

| Prefix/Pattern | Building |
|---------------|----------|
| CS* | Computer Science Building |
| NB* | North Building |
| AVR* | AVR Building |
| Contains "Comfort Room" | Common Areas |
| Contains "Teacher's Lounge" | Common Areas |
| Contains "Office of Student Affairs" | Common Areas |
| Contains "Testing Room" | Common Areas |
| Other | Uses first word or "OTHER" |

## Your POI Organization

Based on your scene, POIs will be grouped as follows:

### 🏢 Computer Science Building
- CS201, CS301, CS302
- Computer Lab A, Computer Lab B, Computer Lab C
- ICONS Office, NCSC Office

### 🏢 North Building
- NB201, NB202, NB203
- NB301, NB302, NB303
- NB401, NB402, NB403

### 🏢 AVR Building
- AVR

### 📍 Common Areas
- Comfort Rooms 1F, Comfort Rooms 3F
- Office of Student Affairs
- Teacher's Lounge
- Testing Room

## Setup Steps (For New Scenes)

### Step 1: Add Components

1. Find the GameObject with the `SelectList` component
2. Add `BuildingGroupedSelectList` component to the same GameObject
3. Add `NavigationUIHelper` component to the NavigationUI GameObject

### Step 2: Configure BuildingGroupedSelectList

In the `BuildingGroupedSelectList` component:
1. **Select List**: Auto-assigned from same GameObject
2. **Building Header Prefab**: Drag `BuildingGroupHeader.prefab`
3. **Header Height**: Set to `40`
4. **Group By Building**: Check this
5. **Destination Select UI**: Drag the POI list panel GameObject

### Step 3: Configure NavigationUIHelper

In the `NavigationUIHelper` component:
1. **Progress Slider**: Drag the Progress Slider GameObject
2. **Stop Button**: Drag the Stop Button GameObject
3. **Destination Select UI**: Drag the POI list panel GameObject

### Step 4: Update Button Handlers

Update OnClick handlers to call `BuildingGroupedSelectList`:

| Button | Method |
|--------|--------|
| Show POI List | `TogglePOIList()` |
| Reset Search | `ResetPOISearch()` |
| Search Field | `SearchPOIOnSearchChanged(string)` |

### Step 5: Test

1. Enter Play Mode
2. Click the POI list button - POIs should be grouped by building
3. Click "Go" on a POI - Progress Slider and Stop Button should appear
4. Click Stop Button - Navigation UI should disappear

## Customization

### Change Building Display Names

Edit `BuildingGroupHeader.GetBuildingDisplayName()`:

```csharp
public static string GetBuildingDisplayName(string buildingCode)
{
    switch (buildingCode.ToUpper())
    {
        case "CS":
            return "🏢 Computer Science Building";
        // ...
    }
}
```

### Change Header Colors

In `BuildingGroupHeader` component:
- **Header Background Color**: Default (0.15, 0.15, 0.15)
- **Header Text Color**: Default white

### Custom Building Assignments

To override automatic detection:
1. Expand "Custom Building Assignments" in `BuildingGroupedSelectList`
2. Add entries with POI Name and Building code

## Troubleshooting

### POIs not grouped
- Ensure `Group By Building` is checked
- Verify `Building Header Prefab` is assigned
- Check console for errors

### Headers not showing
- Verify prefab has `BuildingGroupHeader` component
- Check `headerHeight` is set correctly
- Re-drag the prefab in Unity to refresh GUID

### Progress Slider / Stop Button not showing
- Verify `NavigationUIHelper` component is added
- Check all UI references are assigned
- Look for console logs: "NavigationUIHelper: Navigation started"

### No POIs rendered
- Check that `NavigationController.instance` and `augmentedSpace` are set up
- Verify POIs exist in the scene

## Console Debug Output

When working correctly, you should see:
```
BuildingGroupedSelectList: TogglePOIList called. Current active: false
BuildingGroupedSelectList: Showing destination select UI and rendering POIs
BuildingGroupedSelectList: Rendering X POIs grouped by building
BuildingGroupedSelectList: Grouped into Y buildings
BuildingGroupedSelectList: Rendering building 'CS' with N POIs
...
NavigationUIHelper: Navigation started, showing UI elements
```

## Reverting to Original

To disable building grouping:
1. Uncheck `Group By Building` in `BuildingGroupedSelectList`
2. Or remove `BuildingGroupedSelectList` and `NavigationUIHelper` components
3. Update button handlers to call original `SelectList` methods
