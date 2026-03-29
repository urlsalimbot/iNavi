using UnityEngine;

/// <summary>
/// Utility class for generating or managing navigation audio cues.
/// Provides helper methods for creating audio prompts programmatically.
/// </summary>
public static class NavigationAudioCues
{
    /// <summary>
    /// Available navigation cue types.
    /// </summary>
    public enum CueType
    {
        TurnLeft,
        TurnRight,
        ContinueStraight,
        ApproachingDestination,
        DestinationReached,
        UTurn,
        Recalculating,
        OffRoute
    }

    /// <summary>
    /// Default cue names for each cue type.
    /// Use these as filenames when importing audio clips.
    /// </summary>
    public static readonly string[] CueNames =
    {
        "turn_left",
        "turn_right",
        "continue_straight",
        "approaching_destination",
        "destination_reached",
        "u_turn",
        "recalculating",
        "off_route"
    };

    /// <summary>
    /// Gets the recommended filename for a cue type.
    /// </summary>
    public static string GetCueFileName(CueType cueType)
    {
        return CueNames[(int)cueType];
    }

    /// <summary>
    /// Gets the recommended spoken text for a cue type.
    /// Use this for text-to-speech implementations.
    /// </summary>
    public static string GetCueText(CueType cueType)
    {
        switch (cueType)
        {
            case CueType.TurnLeft:
                return "Turn left";
            case CueType.TurnRight:
                return "Turn right";
            case CueType.ContinueStraight:
                return "Continue straight";
            case CueType.ApproachingDestination:
                return "Approaching destination";
            case CueType.DestinationReached:
                return "You have arrived at your destination";
            case CueType.UTurn:
                return "Make a U-turn";
            case CueType.Recalculating:
                return "Recalculating route";
            case CueType.OffRoute:
                return "You are off route";
            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// Loads an audio clip from the Resources folder.
    /// Place your audio clips in Assets/Resources/Audio/Navigation/
    /// </summary>
    public static AudioClip LoadCue(CueType cueType)
    {
        string path = $"Audio/Navigation/{GetCueFileName(cueType)}";
        return Resources.Load<AudioClip>(path);
    }

    /// <summary>
    /// Loads all navigation audio cues from the Resources folder.
    /// </summary>
    public static AudioClip[] LoadAllCues()
    {
        AudioClip[] clips = new AudioClip[CueNames.Length];

        for (int i = 0; i < CueNames.Length; i++)
        {
            clips[i] = LoadCue((CueType)i);
            if (clips[i] == null)
            {
                Debug.LogWarning($"NavigationAudioCues: Failed to load clip '{CueNames[i]}'");
            }
        }

        return clips;
    }
}
