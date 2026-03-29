# FAQ and Exit Button Setup Guide

This document explains how to create an FAQ button and an Exit button with confirmation dialog in your Unity UI.

## Overview

Two components are provided:

1. **FAQPanelController** - Manages a help/FAQ panel that displays frequently asked questions
2. **ExitButtonController** - Manages exit/quit functionality with a confirmation dialog

## Features

### FAQ Panel
- Open/close FAQ panel with buttons
- Scrollable FAQ content
- Default FAQ content included
- Custom FAQ content support
- Close on Escape key

### Exit Button
- Confirmation dialog before exiting
- Multiple exit actions (Quit, Load Scene, Return to Menu)
- Customizable dialog text
- Cancel option
- Close on Escape key

---

## Part 1: FAQ Button Setup

### Step 1: Create the FAQ Panel UI

1. **Create the FAQ Panel:**
   - In the Hierarchy, right-click on your Canvas
   - Select: UI → Panel
   - Name it "FAQ Panel"

2. **Configure the Panel:**
   - Set RectTransform to cover most of the screen
   - Set Anchor: Stretch (full screen)
   - Add a close button inside the panel:
     - Right-click on FAQ Panel → UI → Button - TextMeshPro
     - Name it "CloseButton"
     - Position in top-right corner
     - Set text to "✕" or "Close"

3. **Add FAQ Content:**
   - Right-click on FAQ Panel → UI → Scroll View
   - Name it "FAQ Scroll View"
   - Position in the center of the panel
   - Inside the Scroll View's Viewport → Content:
     - Add a TextMeshProUGUI object
     - Name it "FAQ Content"
     - This will display the FAQ text

### Step 2: Create the FAQ Button

1. **Create the Button:**
   - Find your main menu or settings panel
   - Right-click → UI → Button - TextMeshPro
   - Name it "FAQ Button"
   - Set text to "FAQ" or "Help"

### Step 3: Add the FAQPanelController Component

1. **Create a GameObject:**
   - In the Hierarchy, create an empty GameObject
   - Name it "FAQ Manager"

2. **Add the Component:**
   - Select "FAQ Manager"
   - Add Component → `FAQPanelController`

3. **Configure the Component:**

```
Panel References:
├─ FAQ Panel: [Drag "FAQ Panel" here]
├─ Open FAQ Button: [Drag "FAQ Button" here]
└─ Close FAQ Button: [Drag "CloseButton" here]

FAQ Content (Optional):
├─ FAQ Content Text: [Drag "FAQ Content" TextMeshProUGUI here]
└─ FAQ ScrollRect: [Drag "FAQ Scroll View" ScrollRect here]
```

### Step 4: Test the FAQ Panel

1. Enter Play Mode
2. Click the FAQ button
3. The FAQ panel should open with default content
4. Click the close button or press Escape to close

### Step 5: Customize FAQ Content (Optional)

**Option A: In the Inspector**
- Select the FAQ Manager GameObject
- Find the FAQPanelController component
- Modify the FAQ Content Text directly

**Option B: Via Code**
```csharp
FAQPanelController faq = FindObjectOfType<FAQPanelController>();
faq.SetFAQContent("Your custom FAQ content here...");
```

**Option C: Edit the Default Content**
- Open `FAQPanelController.cs`
- Find the `PopulateDefaultFAQ()` method
- Modify the FAQ text string

---

## Part 2: Exit Button with Confirmation Setup

### Step 1: Create the Exit Button

1. **Create the Button:**
   - In your main menu or settings panel
   - Right-click → UI → Button - TextMeshPro
   - Name it "Exit Button"
   - Set text to "Exit" or "Quit"

### Step 2: Create the Confirmation Dialog

1. **Create the Dialog Panel:**
   - Right-click on Canvas → UI → Panel
   - Name it "Exit Confirm Dialog"
   - Set RectTransform to be a small centered box (e.g., 400x200)

2. **Add Dialog Background:**
   - Select the dialog panel
   - In the Image component:
     - Color: Dark semi-transparent (e.g., rgba(0, 0, 0, 200))
     - Or add a solid color background

3. **Add Dialog Title:**
   - Right-click on dialog → UI → Text - TextMeshPro
   - Name it "Dialog Title"
   - Position at top
   - Set text to "Exit Application"
   - Font size: 18-24, Bold

4. **Add Dialog Message:**
   - Right-click on dialog → UI → Text - TextMeshPro
   - Name it "Dialog Message"
   - Position in center
   - Set text to "Are you sure you want to exit?"
   - Font size: 14

5. **Add Confirm Button:**
   - Right-click on dialog → UI → Button - TextMeshPro
   - Name it "Confirm Button"
   - Position bottom-left
   - Set text to "Yes" or "Exit"
   - Color: Red or warning color

6. **Add Cancel Button:**
   - Right-click on dialog → UI → Button - TextMeshPro
   - Name it "Cancel Button"
   - Position bottom-right
   - Set text to "No" or "Cancel"
   - Color: Gray or neutral

### Step 3: Add the ExitButtonController Component

1. **Create a GameObject:**
   - In the Hierarchy, create an empty GameObject
   - Name it "Exit Manager"

2. **Add the Component:**
   - Select "Exit Manager"
   - Add Component → `ExitButtonController`

3. **Configure the Component:**

```
Button References:
├─ Exit Button: [Drag "Exit Button" here]

Confirmation Dialog:
├─ Confirm Dialog: [Drag "Exit Confirm Dialog" here]
├─ Confirm Button: [Drag "Confirm Button" here]
└─ Cancel Button: [Drag "Cancel Button" here]

Dialog Text (Optional):
├─ Dialog Title Text: [Drag "Dialog Title" TextMeshProUGUI here]
└─ Dialog Message Text: [Drag "Dialog Message" TextMeshProUGUI here]

Settings:
├─ Confirmation Message: "Are you sure you want to exit?"
├─ Dialog Title: "Exit Application"
├─ Show Confirmation: ✓ (checked)
└─ Exit Action: Quit Application

Scene Loading (if using scene transition):
└─ Scene To Load: [Leave empty for Quit, or enter scene name]
```

### Step 4: Configure Exit Action

Choose what happens when the user confirms exit:

**Option A: Quit Application**
```
Exit Action: Quit Application
```
- Quits the app (works in builds)
- In Editor, stops play mode

**Option B: Load Specific Scene**
```
Exit Action: Load Scene
Scene To Load: "MainMenu"
```
- Loads the specified scene

**Option C: Return to Main Menu**
```
Exit Action: Return To Menu
```
- Loads scene at index 0

**Option D: Custom Action**
```
Exit Action: Custom
```
- Create a subclass and override `OnCustomExit()`

### Step 5: Test the Exit Button

1. Enter Play Mode
2. Click the Exit button
3. Confirmation dialog should appear
4. Click "Yes" to exit or "No" to cancel
5. Press Escape to cancel

---

## Complete Example Hierarchy

```
Canvas
├── Main Menu Panel
│   ├── FAQ Button
│   ├── Exit Button
│   └── [Other menu buttons...]
│
├── FAQ Panel (initially inactive)
│   ├── CloseButton
│   └── FAQ Scroll View
│       └── Viewport
│           └── Content
│               └── FAQ Content (TextMeshProUGUI)
│
├── Exit Confirm Dialog (initially inactive)
│   ├── Dialog Title (TextMeshProUGUI)
│   ├── Dialog Message (TextMeshProUGUI)
│   ├── Confirm Button
│   └── Cancel Button
│
├── FAQ Manager (Empty GameObject)
│   └── FAQPanelController (component)
│
└── Exit Manager (Empty GameObject)
    └── ExitButtonController (component)
```

---

## Customization

### FAQ Content Format

The FAQ content supports Rich Text formatting:

```csharp
string faqContent = @"
<color=#4DA6FF><size=18><b>TITLE</b></size></color>

<color=#FFD700><b>❓ Question?</b></color>
Answer text here.

<color=#FFD700><b>❓ Another Question?</b></color>
Another answer.
";
```

**Supported Tags:**
- `<b>bold</b>`
- `<i>italic</i>`
- `<size=18>larger text</size>`
- `<color=#FF0000>red text</color>`
- `<align=center>centered</align>`

### Dialog Styling

Customize the confirmation dialog appearance:

```csharp
// In ExitButtonController
public void SetDialogText(string title, string message)
{
    dialogTitle = "Custom Title";
    confirmationMessage = "Custom message with rich text: <color=red>Warning!</color>";
}
```

### Programmatic Control

```csharp
// Open FAQ from code
FAQPanelController faq = FindObjectOfType<FAQPanelController>();
faq.OpenFAQ();

// Close FAQ
faq.CloseFAQ();

// Show exit dialog
ExitButtonController exit = FindObjectOfType<ExitButtonController>();
exit.ShowConfirmDialog();

// Exit without confirmation
exit.PerformExitAction();

// Check states
bool isFAQOpen = faq.IsFAQOpen();
bool isDialogOpen = exit.IsDialogOpen();
```

---

## Troubleshooting

### FAQ Panel Not Opening

1. **Check References:** Ensure all button references are assigned in the Inspector
2. **Check Panel Active State:** FAQ Panel should be inactive at start
3. **Check Console:** Look for null reference errors
4. **Verify Button Click Events:** Ensure buttons have onClick listeners

### Exit Button Not Working

1. **Check Dialog Assignment:** Confirm Dialog must be assigned
2. **Check Button Listeners:** Verify exitButton.onClick has listener
3. **Check Exit Action:** Ensure correct action is selected
4. **Scene Build Settings:** If loading scene, ensure it's in Build Settings

### Dialog Not Closing

1. **Check HideConfirmDialog():** Ensure dialog is being hidden after action
2. **Check Button References:** Confirm cancelButton is assigned
3. **Check Escape Key:** Ensure no other input is blocking Escape

### Text Not Displaying

1. **Check Font Asset:** Ensure TextMeshPro has valid font asset
2. **Check Text Component:** Verify TextMeshProUGUI component exists
3. **Check Content:** Ensure text is not empty or white on white

---

## Best Practices

1. **Accessibility:**
   - Use clear, readable fonts
   - Ensure sufficient color contrast
   - Make buttons large enough to tap

2. **User Experience:**
   - Keep FAQ content concise
   - Use common icons (✕ for close, ? for help)
   - Provide clear confirm/cancel options

3. **Performance:**
   - Keep FAQ panel inactive when not in use
   - Use object pooling for frequently shown dialogs

4. **Mobile Considerations:**
   - Make buttons at least 44x44 pixels
   - Ensure dialogs don't extend off-screen
   - Test on actual device screen sizes

---

## API Reference

### FAQPanelController

**Public Methods:**
- `OpenFAQ()` - Opens the FAQ panel
- `CloseFAQ()` - Closes the FAQ panel
- `ToggleFAQ()` - Toggles panel open/closed
- `IsFAQOpen()` - Returns true if panel is open
- `SetFAQContent(string)` - Sets custom FAQ text

### ExitButtonController

**Public Methods:**
- `OnExitButtonClicked()` - Called when exit button clicked
- `ShowConfirmDialog()` - Shows confirmation dialog
- `HideConfirmDialog()` - Hides confirmation dialog
- `ConfirmExit()` - Confirms and performs exit
- `CancelExit()` - Cancels exit action
- `PerformExitAction()` - Executes configured exit action
- `IsDialogOpen()` - Returns true if dialog is open
- `SetDialogText(string, string)` - Sets custom dialog text

**ExitAction Enum:**
- `QuitApplication` - Quit the app
- `LoadScene` - Load specified scene
- `ReturnToMenu` - Load scene at index 0
- `Custom` - Call OnCustomExit() override

---

## Files Created

- `FAQPanelController.cs` - FAQ panel management
- `ExitButtonController.cs` - Exit button with confirmation
- `FAQ_EXIT_BUTTON_SETUP.md` - This documentation

See also:
- `AUDIO_NAVIGATION_SETUP.md` - Audio navigation setup
- `BUILDING_FILTER_SETUP.md` - Building filter setup
