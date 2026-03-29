using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Manages audio cues for turn-by-turn navigation.
/// Monitors the navmesh path and plays audio prompts for turns.
/// Attach this to a GameObject in your scene (e.g., on the NavigationController GameObject).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioNavigationManager : MonoBehaviour
{
    [Header("Audio Clips")]
    [Tooltip("Audio clip played when user should turn left")]
    public AudioClip turnLeftClip;

    [Tooltip("Audio clip played when user should turn right")]
    public AudioClip turnRightClip;

    [Tooltip("Audio clip played when user should continue straight")]
    public AudioClip continueStraightClip;

    [Tooltip("Audio clip played when approaching destination")]
    public AudioClip approachingDestinationClip;

    [Tooltip("Audio clip played when destination is reached")]
    public AudioClip destinationReachedClip;

    [Tooltip("Audio clip played when user needs to make a U-turn")]
    public AudioClip uturnClip;

    [Header("Audio Settings")]
    [Tooltip("Volume for all audio cues (0-1)")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [Tooltip("Minimum distance between audio cues (in meters)")]
    public float minDistanceBetweenCues = 5f;

    [Tooltip("Distance ahead to check for turns (in meters)")]
    public float lookAheadDistance = 10f;

    [Tooltip("Angle threshold to consider a turn (in degrees)")]
    public float turnAngleThreshold = 45f;

    [Tooltip("Angle threshold to consider a U-turn (in degrees)")]
    public float uturnAngleThreshold = 135f;

    [Tooltip("Distance to destination for 'approaching' cue (in meters)")]
    public float approachingDestinationDistance = 20f;

    [Header("References")]
    [Tooltip("Reference to NavigationController. If null, will try to find automatically.")]
    public NavigationController navigationController;

    [Tooltip("Transform representing the user's position. If null, uses Main Camera.")]
    public Transform playerTransform;

    // Private references
    private AudioSource audioSource;
    private NavMeshPath currentPath;
    private List<Vector3> pathCorners = new List<Vector3>();

    // State tracking
    private float lastCueDistance = 0f;
    private int lastSpokenCornerIndex = -1;
    private bool hasPlayedApproaching = false;
    private bool hasPlayedDestination = false;

    void Awake()
    {
        // Get or add AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure AudioSource
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D audio
        audioSource.volume = volume;

        // Find NavigationController if not assigned
        if (navigationController == null)
        {
            navigationController = FindObjectOfType<NavigationController>();
            if (navigationController == null)
            {
                Debug.LogWarning("AudioNavigationManager: NavigationController not found in scene!");
            }
        }

        // Set player transform
        if (playerTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                playerTransform = mainCamera.transform;
            }
        }
    }

    void Update()
    {
        if (navigationController == null) return;
        if (!navigationController.IsCurrentlyNavigating()) return;
        if (audioSource == null) return;
        if (playerTransform == null) return;

        // Get current path from NavigationController
        UpdatePath();

        if (currentPath == null || currentPath.status != NavMeshPathStatus.PathComplete)
        {
            return;
        }

        // Check for audio cues
        CheckForAudioCues();
    }

    /// <summary>
    /// Updates the current navmesh path from the NavigationController.
    /// </summary>
    private void UpdatePath()
    {
        if (navigationController == null) return;

        // Try to get the current path from NavigationController
        // Note: This depends on how the SDK exposes the path
        currentPath = GetCurrentNavMeshPath();

        if (currentPath != null && currentPath.status == NavMeshPathStatus.PathComplete)
        {
            ExtractPathCorners(currentPath);
        }
    }

    /// <summary>
    /// Gets the current NavMeshPath from the NavigationController.
    /// This method may need adjustment based on how the SDK exposes path data.
    /// </summary>
    private NavMeshPath GetCurrentNavMeshPath()
    {
        if (navigationController == null) return null;

        // Try to get path via reflection or public properties
        // The SDK may expose this differently
        var augmentedSpaceProperty = navigationController.GetType().GetProperty("augmentedSpace");
        if (augmentedSpaceProperty != null)
        {
            var augmentedSpace = augmentedSpaceProperty.GetValue(navigationController);
            if (augmentedSpace != null)
            {
                // Try to get current path from augmented space
                var currentPathProperty = augmentedSpace.GetType().GetProperty("CurrentPath");
                if (currentPathProperty != null)
                {
                    return currentPathProperty.GetValue(augmentedSpace) as NavMeshPath;
                }

                var navMeshPathProperty = augmentedSpace.GetType().GetProperty("NavMeshPath");
                if (navMeshPathProperty != null)
                {
                    return navMeshPathProperty.GetValue(augmentedSpace) as NavMeshPath;
                }
            }
        }

        // Alternative: Calculate path from player to destination
        return CalculatePathFromPlayer();
    }

    /// <summary>
    /// Calculates a path from the player's current position to the destination.
    /// </summary>
    private NavMeshPath CalculatePathFromPlayer()
    {
        if (playerTransform == null) return null;

        NavMeshPath path = new NavMeshPath();
        Vector3 startPos = playerTransform.position;

        // Try to get destination from NavigationController
        Vector3 destination = GetNavigationDestination();

        if (destination != Vector3.zero)
        {
            NavMesh.CalculatePath(startPos, destination, NavMesh.AllAreas, path);
        }

        return path;
    }

    /// <summary>
    /// Gets the current navigation destination.
    /// </summary>
    private Vector3 GetNavigationDestination()
    {
        if (navigationController == null) return Vector3.zero;

        // Try to get destination via reflection
        var destinationProperty = navigationController.GetType().GetProperty("Destination");
        if (destinationProperty != null)
        {
            var dest = destinationProperty.GetValue(navigationController);
            if (dest is Vector3)
            {
                return (Vector3)dest;
            }
        }

        var targetProperty = navigationController.GetType().GetProperty("Target");
        if (targetProperty != null)
        {
            var target = targetProperty.GetValue(navigationController);
            if (target is Vector3)
            {
                return (Vector3)target;
            }
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Extracts corner points from the navmesh path.
    /// </summary>
    private void ExtractPathCorners(NavMeshPath path)
    {
        pathCorners.Clear();

        if (path == null) return;

        int cornerCount = path.corners.Length;
        for (int i = 0; i < cornerCount; i++)
        {
            pathCorners.Add(path.corners[i]);
        }
    }

    /// <summary>
    /// Checks if an audio cue should be played based on current position and path.
    /// </summary>
    private void CheckForAudioCues()
    {
        if (pathCorners.Count < 2) return;

        Vector3 playerPos = playerTransform.position;

        // Find the closest point on the path
        int currentCornerIndex = FindClosestCornerIndex(playerPos);

        // Check if we've reached the destination
        if (IsAtDestination(playerPos))
        {
            if (!hasPlayedDestination)
            {
                PlayCue(destinationReachedClip);
                hasPlayedDestination = true;
            }
            return;
        }
        else
        {
            hasPlayedDestination = false;
        }

        // Check for approaching destination
        float distanceToDestination = Vector3.Distance(playerPos, pathCorners[pathCorners.Count - 1]);
        if (distanceToDestination <= approachingDestinationDistance && !hasPlayedApproaching)
        {
            PlayCue(approachingDestinationClip);
            hasPlayedApproaching = true;
        }
        else if (distanceToDestination > approachingDestinationDistance)
        {
            hasPlayedApproaching = false;
        }

        // Find the next turn to announce
        int nextTurnCornerIndex = FindNextTurnCornerIndex(currentCornerIndex, playerPos);

        if (nextTurnCornerIndex > currentCornerIndex && nextTurnCornerIndex < pathCorners.Count)
        {
            // Check if we should announce this turn
            if (ShouldAnnounceTurn(nextTurnCornerIndex, playerPos))
            {
                Vector3 turnPosition = pathCorners[nextTurnCornerIndex];
                float distanceToTurn = Vector3.Distance(playerPos, turnPosition);

                // Determine turn direction
                TurnDirection turnDirection = GetTurnDirection(nextTurnCornerIndex);

                AudioClip cueClip = GetCueForTurn(turnDirection);
                if (cueClip != null)
                {
                    PlayCue(cueClip);
                    lastSpokenCornerIndex = nextTurnCornerIndex;
                    lastCueDistance = distanceToTurn;
                }
            }
        }
    }

    /// <summary>
    /// Finds the corner index closest to the player's position.
    /// </summary>
    private int FindClosestCornerIndex(Vector3 playerPos)
    {
        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < pathCorners.Count; i++)
        {
            float distance = Vector3.Distance(playerPos, pathCorners[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    /// <summary>
    /// Finds the next corner that represents a turn.
    /// </summary>
    private int FindNextTurnCornerIndex(int currentCornerIndex, Vector3 playerPos)
    {
        for (int i = currentCornerIndex + 1; i < pathCorners.Count - 1; i++)
        {
            if (IsSignificantTurn(i))
            {
                return i;
            }
        }

        return currentCornerIndex;
    }

    /// <summary>
    /// Checks if a corner represents a significant turn.
    /// </summary>
    private bool IsSignificantTurn(int cornerIndex)
    {
        if (cornerIndex <= 0 || cornerIndex >= pathCorners.Count - 1) return false;

        Vector3 incoming = (pathCorners[cornerIndex] - pathCorners[cornerIndex - 1]).normalized;
        Vector3 outgoing = (pathCorners[cornerIndex + 1] - pathCorners[cornerIndex]).normalized;

        float angle = Vector3.Angle(incoming, outgoing);

        return angle >= turnAngleThreshold;
    }

    /// <summary>
    /// Determines if we should announce a turn at the given corner.
    /// </summary>
    private bool ShouldAnnounceTurn(int cornerIndex, Vector3 playerPos)
    {
        // Don't announce the same corner twice
        if (cornerIndex <= lastSpokenCornerIndex) return false;

        Vector3 turnPosition = pathCorners[cornerIndex];
        float distanceToTurn = Vector3.Distance(playerPos, turnPosition);

        // Don't spam cues - enforce minimum distance
        if (distanceToTurn > lastCueDistance + minDistanceBetweenCues) return false;

        // Announce when approaching the turn (within look ahead distance)
        return distanceToTurn <= lookAheadDistance;
    }

    /// <summary>
    /// Determines the direction of a turn at the given corner.
    /// </summary>
    private TurnDirection GetTurnDirection(int cornerIndex)
    {
        if (cornerIndex <= 0 || cornerIndex >= pathCorners.Count - 1)
        {
            return TurnDirection.None;
        }

        Vector3 incoming = (pathCorners[cornerIndex] - pathCorners[cornerIndex - 1]).normalized;
        Vector3 outgoing = (pathCorners[cornerIndex + 1] - pathCorners[cornerIndex]).normalized;

        float angle = Vector3.Angle(incoming, outgoing);

        // Check for U-turn
        if (angle >= uturnAngleThreshold)
        {
            return TurnDirection.UTurn;
        }

        // Determine left or right using cross product
        Vector3 cross = Vector3.Cross(incoming, outgoing);
        float crossY = cross.y; // Assuming Y-up coordinate system

        if (Mathf.Abs(crossY) < 0.01f)
        {
            return TurnDirection.Straight;
        }

        return crossY > 0 ? TurnDirection.Left : TurnDirection.Right;
    }

    /// <summary>
    /// Gets the appropriate audio clip for a turn direction.
    /// </summary>
    private AudioClip GetCueForTurn(TurnDirection direction)
    {
        switch (direction)
        {
            case TurnDirection.Left:
                return turnLeftClip;
            case TurnDirection.Right:
                return turnRightClip;
            case TurnDirection.UTurn:
                return uturnClip;
            case TurnDirection.Straight:
                return continueStraightClip;
            default:
                return null;
        }
    }

    /// <summary>
    /// Checks if the player is at the destination.
    /// </summary>
    private bool IsAtDestination(Vector3 playerPos)
    {
        if (pathCorners.Count == 0) return false;

        Vector3 destination = pathCorners[pathCorners.Count - 1];
        float distanceToDestination = Vector3.Distance(playerPos, destination);

        return distanceToDestination < 2f; // Within 2 meters of destination
    }

    /// <summary>
    /// Plays an audio cue.
    /// </summary>
    private void PlayCue(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioNavigationManager: Attempted to play null clip!");
            return;
        }

        audioSource.PlayOneShot(clip);
        Debug.Log($"AudioNavigationManager: Playing cue '{clip.name}'");
    }

    /// <summary>
    /// Resets the audio navigation state. Call this when navigation starts.
    /// </summary>
    public void ResetState()
    {
        lastCueDistance = 0f;
        lastSpokenCornerIndex = -1;
        hasPlayedApproaching = false;
        hasPlayedDestination = false;
        pathCorners.Clear();
    }

    /// <summary>
    /// Sets the audio volume.
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    /// <summary>
    /// Mutes or unmutes audio cues.
    /// </summary>
    public void SetMuted(bool muted)
    {
        if (audioSource != null)
        {
            audioSource.mute = muted;
        }
    }

    /// <summary>
    /// Enum representing turn directions.
    /// </summary>
    public enum TurnDirection
    {
        None,
        Left,
        Right,
        Straight,
        UTurn
    }
}
