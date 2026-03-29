# Audio Navigation System Setup Guide

This document explains how to set up turn-by-turn audio navigation that announces "Turn Left", "Turn Right", etc. based on the navmesh path.

## Overview

The audio navigation system consists of two main components:

1. **AudioNavigationManager** - Monitors the navmesh path and plays audio cues at appropriate times
2. **NavigationAudioCues** - Utility class for managing audio cue assets

## Features

- **Turn Left/Right announcements** - Plays audio when approaching turns
- **U-turn detection** - Announces when a U-turn is required
- **Approaching destination** - Alerts user when near the destination
- **Destination reached** - Confirms arrival
- **Configurable thresholds** - Customize turn detection angles and distances
- **Anti-spam protection** - Prevents repeated announcements

## Setup Instructions

### Step 1: Prepare Audio Files

You need audio clips for each navigation cue. Recommended format:
- **Format**: WAV or MP3
- **Sample Rate**: 44.1kHz or 48kHz
- **Channels**: Mono or Stereo
- **Bit Depth**: 16-bit

**Required Audio Clips:**

| Clip Name | Description | Example Voice Line |
|-----------|-------------|-------------------|
| `turn_left` | Play when user should turn left | "Turn left" |
| `turn_right` | Play when user should turn right | "Turn right" |
| `continue_straight` | Play when continuing straight at junction | "Continue straight" |
| `approaching_destination` | Play when near destination | "Approaching destination" |
| `destination_reached` | Play when arrived | "You have arrived" |
| `u_turn` | Play when U-turn needed | "Make a U-turn" |

**Optional Audio Clips:**
- `recalculating` - "Recalculating route"
- `off_route` - "You are off route"

### Step 2: Import Audio Clips into Unity

1. Create a folder: `Assets/Resources/Audio/Navigation/`
2. Import your audio files into this folder
3. Name them according to the table above (e.g., `turn_left.wav`)
4. Select each audio file and configure import settings:
   - **Load Type**: Decompress On Load
   - **Quality**: High (or as needed)
   - **Compression Format**: PCM (for WAV) or Vorbis (for MP3)

### Step 3: Add AudioNavigationManager to Scene

1. **Find or create a GameObject** for navigation management:
   - Look for a GameObject with the `NavigationController` component
   - Or create a new empty GameObject named "NavigationManager"

2. **Add the AudioNavigationManager component**:
   - Select the GameObject
   - In the Inspector: Add Component → `AudioNavigationManager`

3. **Configure the component**:

**Audio Clips section:**
- `Turn Left Clip`: Drag your `turn_left` audio clip
- `Turn Right Clip`: Drag your `turn_right` audio clip
- `Continue Straight Clip`: Drag your `continue_straight` audio clip (optional)
- `Approaching Destination Clip`: Drag your `approaching_destination` audio clip
- `Destination Reached Clip`: Drag your `destination_reached` audio clip
- `U-turn Clip`: Drag your `u_turn` audio clip (optional)

**Audio Settings section:**
- `Volume`: Set audio volume (0-1), default is 1
- `Min Distance Between Cues`: Minimum distance between announcements (default: 5m)
- `Look Ahead Distance`: Distance before turn to announce (default: 10m)
- `Turn Angle Threshold`: Minimum angle to consider a turn (default: 45°)
- `U-turn Angle Threshold`: Minimum angle for U-turn (default: 135°)
- `Approaching Destination Distance`: Distance for approach announcement (default: 20m)

**References section:**
- `Navigation Controller`: Drag your NavigationController GameObject (auto-detected if not set)
- `Player Transform`: Drag the player/camera transform (auto-uses Main Camera if not set)

### Step 4: Test the System

1. Enter Play Mode
2. Start navigation to a destination
3. Walk along the path
4. You should hear audio cues when:
   - Approaching a left turn
   - Approaching a right turn
   - Getting close to the destination
   - Arriving at the destination

## Configuration Options

### Turn Detection Sensitivity

Adjust these values to fine-tune when turns are announced:

```csharp
// In AudioNavigationManager Inspector

// Lower = more turns detected (including slight turns)
// Higher = only sharp turns detected
Turn Angle Threshold: 30-60 degrees (default: 45)

// Distance before turn to play announcement
// Lower = announce closer to turn
// Higher = announce further in advance
Look Ahead Distance: 5-15 meters (default: 10)
```

### Audio Cue Frequency

Prevent too many announcements:

```csharp
// Minimum distance that must be traveled between cues
Min Distance Between Cues: 3-10 meters (default: 5)
```

### Volume Control

Control audio volume programmatically:

```csharp
// Get reference to AudioNavigationManager
AudioNavigationManager audioNav = FindObjectOfType<AudioNavigationManager>();

// Set volume (0-1)
audioNav.SetVolume(0.5f);

// Mute/unmute
audioNav.SetMuted(true);
```

## Advanced Usage

### Manual Cue Playback

You can play navigation cues manually:

```csharp
using UnityEngine;

public class CustomNavigationAudio : MonoBehaviour
{
    public AudioClip customCue;
    private AudioNavigationManager audioNav;

    void Start()
    {
        audioNav = FindObjectOfType<AudioNavigationManager>();
    }

    void PlayCustomCue()
    {
        // This requires exposing a public method in AudioNavigationManager
        // You can add: public void PlayCue(AudioClip clip) { audioSource.PlayOneShot(clip); }
    }
}
```

### Text-to-Speech Integration

For dynamic voice prompts without pre-recorded audio:

```csharp
// Example using Unity's built-in or a TTS plugin
public class TTSNavigationAudio : MonoBehaviour
{
    public void SpeakTurnDirection(string direction)
    {
        // Example: "Turn left in 10 meters"
        // Implement using your preferred TTS solution
        Debug.Log($"TTS: Turn {direction}");
    }
}
```

### Loading Audio Clips from Resources

Use the NavigationAudioCues utility:

```csharp
using UnityEngine;

public class AudioLoader : MonoBehaviour
{
    void Start()
    {
        // Load a specific cue
        AudioClip turnLeft = NavigationAudioCues.LoadCue(
            NavigationAudioCues.CueType.TurnLeft
        );

        // Load all cues
        AudioClip[] allCues = NavigationAudioCues.LoadAllCues();
    }
}
```

## Troubleshooting

### No Audio Playing

1. **Check AudioSource**: Ensure the GameObject has an AudioSource component
2. **Check Audio Clips**: Verify clips are assigned in the Inspector
3. **Check Volume**: Ensure volume is not muted (check AudioSettings and system volume)
4. **Check Navigation**: Verify NavigationController.IsCurrentlyNavigating() returns true
5. **Check Console**: Look for warnings about null clips or missing references

### Audio Cues Not Triggering

1. **Path Issues**: Verify navmesh path is being calculated correctly
   - Check that `currentPath.status == NavMeshPathStatus.PathComplete`
   
2. **Turn Detection**: Adjust turn angle threshold
   - Lower the `Turn Angle Threshold` if turns aren't being detected
   - Check path corners in debug view

3. **Distance Issues**: Check player position tracking
   - Ensure `Player Transform` is assigned and updating
   - Verify `Look Ahead Distance` is appropriate for your scale

4. **Spam Prevention**: Cues may be blocked by minimum distance
   - Check `Min Distance Between Cues` setting
   - Review console logs for cue attempts

### Incorrect Turn Direction

1. **Coordinate System**: Verify your scene uses Y-up (Unity standard)
2. **Path Winding**: Check if path corners are in correct order
3. **Angle Threshold**: Adjust `Turn Angle Threshold` for your path granularity

### U-turn Not Detected

1. Increase `U-turn Angle Threshold` if false positives occur
2. Decrease it if U-turns aren't being detected
3. Verify the path actually contains a ~180° turn

## File Structure

```
Assets/
├── Scripts/
│   └── POI/
│       ├── AudioNavigationManager.cs    (Main audio navigation component)
│       └── NavigationAudioCues.cs       (Utility for loading cues)
└── Resources/
    └── Audio/
        └── Navigation/
            ├── turn_left.wav
            ├── turn_right.wav
            ├── continue_straight.wav
            ├── approaching_destination.wav
            ├── destination_reached.wav
            └── u_turn.wav
```

## API Reference

### AudioNavigationManager

**Public Methods:**
- `ResetState()` - Reset navigation state (call when navigation starts)
- `SetVolume(float)` - Set audio volume (0-1)
- `SetMuted(bool)` - Mute/unmute audio cues

**Public Properties:**
- `volume` - Current volume setting
- `minDistanceBetweenCues` - Minimum distance between announcements
- `lookAheadDistance` - Distance before turn to announce
- `turnAngleThreshold` - Minimum angle for turn detection
- `uturnAngleThreshold` - Minimum angle for U-turn detection
- `approachingDestinationDistance` - Distance for approach announcement

### NavigationAudioCues

**Static Methods:**
- `GetCueFileName(CueType)` - Get filename for cue type
- `GetCueText(CueType)` - Get spoken text for cue type
- `LoadCue(CueType)` - Load clip from Resources
- `LoadAllCues()` - Load all navigation clips

**CueType Enum:**
- `TurnLeft`, `TurnRight`, `ContinueStraight`
- `ApproachingDestination`, `DestinationReached`
- `UTurn`, `Recalculating`, `OffRoute`

## Performance Considerations

- **Audio Loading**: Use "Decompress On Load" for short cues to reduce latency
- **Path Updates**: Path is checked every frame during navigation
- **Memory**: Audio clips are cached after first load
- **CPU**: Turn detection uses simple vector math (~0.01ms per frame)

## Customization Ideas

1. **Distance Announcements**: Add "In 100 meters, turn left" style cues
2. **Landmark Cues**: Announce near known POIs ("Turn left after the cafeteria")
3. **Multiple Languages**: Support different audio sets per language
4. **Voice Selection**: Allow users to choose voice type
5. **Haptic Feedback**: Add vibration cues alongside audio

## Credits

Created for iNavi navigation system.
Compatible with MultiSet SDK NavigationController.
