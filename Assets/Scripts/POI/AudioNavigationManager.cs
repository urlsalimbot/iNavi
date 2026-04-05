using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MultiSet;

/// <summary>
/// Manages audio cues for turn-by-turn navigation.
/// Uses ShowPath.instance to get the actual navmesh path corners.
/// Works with MultiSet SDK's ShowPath and NavigationController.
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

    [Tooltip("Distance ahead to check for turns (in meters)")]
    public float lookAheadDistance = 20f;

    [Tooltip("Angle threshold to consider a turn (in degrees)")]
    public float turnAngleThreshold = 15f;

    [Tooltip("Angle threshold to consider a U-turn (in degrees)")]
    public float uturnAngleThreshold = 120f;

    [Tooltip("Distance to destination for 'approaching' cue (in meters)")]
    public float approachingDestinationDistance = 50f;

    [Tooltip("Hysteresis buffer - must exit this far beyond approaching distance before reset")]
    public float approachingResetBuffer = 20f;

    [Header("Path Settings")]
    [Tooltip("Distance threshold to consider reached destination")]
    public float destinationThreshold = 5f;

    [Tooltip("Minimum distance that must be traveled between turn cues")]
    public float minTravelDistanceBetweenCues = 1f;

    [Tooltip("Enable debug logging")]
    public bool enableDebugLogs = true;

    [Tooltip("Show path corners in Scene view")]
    public bool showPathGizmos = false;

    [Header("Cue Settings")]
    [Tooltip("Play 'continue straight' cue at intersections")]
    public bool playContinueStraightCue = true;

    [Tooltip("Angle range to consider 'continue straight' (degrees)")]
    public float straightAngleThreshold = 5f;

    [Header("Debug")]
    [Tooltip("If true, will invert left/right detection (use if directions are swapped)")]
    public bool invertLeftRight = false;

    [Header("Path Update Settings")]
    [Tooltip("How often to check for audio cues (seconds)")]
    public float audioCueCheckInterval = 0.05f;

    // Private references
    private AudioSource audioSource;
    private List<Vector3> pathCorners = new List<Vector3>();
    private Vector3 lastPlayerPosition = Vector3.zero;
    private float totalDistanceTraveled = 0f;

    // State tracking
    private float lastCueCheckTime = 0f;
    private int lastAnnouncedCornerIndex = -1;
    private bool hasPlayedApproaching = false;
    private bool hasPlayedDestination = false;
    private bool wasNavigating = false;

    // Static singleton
    private static AudioNavigationManager instance;
    public static AudioNavigationManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AudioNavigationManager>();
            }
            return instance;
        }
    }

    void Awake()
    {
        // Singleton check
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Get or add AudioSource
        if (!TryGetComponent(out audioSource))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = volume;
        }
    }

    void Start()
    {
        LogMessage("AudioNavigationManager initialized");

        // Verify ShowPath exists
        if (ShowPath.instance == null)
        {
            LogWarning("ShowPath.instance not found! Audio cues will not work until path visualization is active.");
        }
        else
        {
            LogMessage("ShowPath.instance found ✓");
        }
    }

    void Update()
    {
        if (this == null || audioSource == null) return;

        // Check navigation state
        bool isNavigating = IsCurrentlyNavigating();

        // Handle navigation state changes
        if (isNavigating != wasNavigating)
        {
            wasNavigating = isNavigating;

            if (isNavigating)
            {
                LogMessage("Navigation started - resetting state");
                ResetState();
            }
            else
            {
                LogMessage("Navigation stopped");
                ClearState();
            }
        }

        // Only process when navigating
        if (!isNavigating) return;

        // Check for audio cues at controlled intervals
        lastCueCheckTime += Time.deltaTime;
        if (lastCueCheckTime < audioCueCheckInterval) return;

        lastCueCheckTime = 0f;

        // Get path corners from ShowPath
        if (!GetPathCornersFromShowPath())
        {
            return;
        }

        if (pathCorners.Count < 2)
        {
            return;
        }

        // Check for audio cues
        Vector3 playerPos = GetPlayerPosition();
        if (playerPos == Vector3.zero) return;

        // Update distance traveled
        UpdateDistanceTraveled(playerPos);

        CheckForAudioCues(playerPos);
    }

    /// <summary>
    /// Updates the total distance traveled since last cue.
    /// </summary>
    private void UpdateDistanceTraveled(Vector3 playerPos)
    {
        if (lastPlayerPosition == Vector3.zero)
        {
            lastPlayerPosition = playerPos;
            return;
        }

        float distanceSinceLastCheck = Vector3.Distance(playerPos, lastPlayerPosition);
        totalDistanceTraveled += distanceSinceLastCheck;
        lastPlayerPosition = playerPos;
    }

    /// <summary>
    /// Checks if we're currently navigating.
    /// </summary>
    private bool IsCurrentlyNavigating()
    {
        try
        {
            if (NavigationController.instance == null)
            {
                return false;
            }

            return NavigationController.instance.IsCurrentlyNavigating();
        }
        catch (System.Exception ex)
        {
            LogWarning($"Error checking navigation: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets path corners from ShowPath.instance.
    /// </summary>
    private bool GetPathCornersFromShowPath()
    {
        if (ShowPath.instance == null)
        {
            return false;
        }

        // Try to access the 'path' field via reflection
        var showPathInstance = ShowPath.instance;
        var pathField = showPathInstance.GetType().GetField("path",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (pathField == null)
        {
            return false;
        }

        NavMeshPath path = pathField.GetValue(showPathInstance) as NavMeshPath;

        if (path == null || path.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }

        if (path.corners == null || path.corners.Length < 2)
        {
            return false;
        }

        // Update corners list
        pathCorners.Clear();
        for (int i = 0; i < path.corners.Length; i++)
        {
            pathCorners.Add(path.corners[i]);
        }

        return true;
    }

    /// <summary>
    /// Gets the player/camera position.
    /// </summary>
    private Vector3 GetPlayerPosition()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            return mainCamera.transform.position;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Checks if an audio cue should be played.
    /// </summary>
    private void CheckForAudioCues(Vector3 playerPos)
    {
        if (pathCorners.Count < 2) return;

        // Check if we've reached the destination (with larger threshold)
        if (IsAtDestination(playerPos))
        {
            if (!hasPlayedDestination)
            {
                PlayCue(destinationReachedClip, "Destination Reached");
                hasPlayedDestination = true;
                LogMessage($"✓ Destination reached! Distance: {Vector3.Distance(playerPos, pathCorners[pathCorners.Count - 1]):F1}m");
            }
            return;
        }
        else
        {
            hasPlayedDestination = false;
        }

        // Check for approaching destination (with hysteresis to prevent spam)
        float distanceToDestination = DistanceXZ(playerPos, pathCorners[pathCorners.Count - 1]);

        if (!hasPlayedApproaching && distanceToDestination <= approachingDestinationDistance)
        {
            PlayCue(approachingDestinationClip, "Approaching Destination");
            hasPlayedApproaching = true;
            LogMessage($"Approaching cue played. Distance: {distanceToDestination:F1}m");
        }

        // Hysteresis: only reset when WELL outside the approaching zone
        float resetDistance = approachingDestinationDistance + approachingResetBuffer;
        if (hasPlayedApproaching && distanceToDestination > resetDistance)
        {
            hasPlayedApproaching = false;
            LogMessage($"Approaching reset. Distance: {distanceToDestination:F1}m (threshold: {resetDistance:F1}m)");
        }

        // Find our position on the path
        int closestCornerIndex = FindClosestCornerIndex(playerPos);
        float distanceToClosestCorner = DistanceXZ(playerPos, pathCorners[closestCornerIndex]);

        // Find the next corner ahead (including winding paths)
        int nextCornerIndex = FindNextCornerToAnnounce(closestCornerIndex);

        LogMessage($"Position: closest={closestCornerIndex}, next={nextCornerIndex}, total corners={pathCorners.Count}");

        if (nextCornerIndex <= closestCornerIndex || nextCornerIndex >= pathCorners.Count)
        {
            return;
        }

        // Check if we should announce this corner
        if (ShouldAnnounceTurn(nextCornerIndex, closestCornerIndex, distanceToClosestCorner))
        {
            TurnDirection turnDirection = GetTurnDirection(nextCornerIndex);
            AudioClip cueClip = GetCueForTurn(turnDirection);
            string turnName = GetTurnDirectionName(turnDirection);

            if (cueClip != null)
            {
                PlayCue(cueClip, turnName);

                // Update state
                lastAnnouncedCornerIndex = nextCornerIndex;
                totalDistanceTraveled = 0f;
            }
            else
            {
                LogWarning($"No audio clip assigned for {turnName}");
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
    /// Finds the next corner to announce (including straight, left, right, u-turn).
    /// For winding paths (like staircases), announces the next corner ahead regardless of angle.
    /// </summary>
    private int FindNextCornerToAnnounce(int currentCornerIndex)
    {
        // Look ahead from the next corner
        for (int i = currentCornerIndex + 1; i < pathCorners.Count - 1; i++)
        {
            // For winding paths, announce the next corner that's ahead
            Vector3 cornerPos = pathCorners[i];
            Vector3 playerPos = GetPlayerPosition();
            float distanceToCorner = DistanceXZ(playerPos, cornerPos);

            // Announce the first corner that's at least 0.5m ahead
            // This catches even tight winding staircases
            if (distanceToCorner > 0.5f)
            {
                return i;
            }
        }

        return currentCornerIndex;
    }

    /// <summary>
    /// Checks if a corner is significant enough to announce.
    /// Now includes straight paths for 'continue straight' cues.
    /// </summary>
    private bool IsSignificantCorner(int cornerIndex)
    {
        if (cornerIndex <= 0 || cornerIndex >= pathCorners.Count - 1) return false;

        // Get incoming and outgoing directions (projected to XZ plane)
        Vector3 incoming = ProjectToXZ(pathCorners[cornerIndex] - pathCorners[cornerIndex - 1]);
        Vector3 outgoing = ProjectToXZ(pathCorners[cornerIndex + 1] - pathCorners[cornerIndex]);

        if (incoming == Vector3.zero || outgoing == Vector3.zero) return false;

        float angle = Vector3.Angle(incoming, outgoing);

        // Check if it's a significant turn OR u-turn OR straight path
        if (angle >= turnAngleThreshold) return true;
        if (angle >= uturnAngleThreshold) return true;
        if (playContinueStraightCue && angle <= straightAngleThreshold) return true;

        return false;
    }

    /// <summary>
    /// Projects a vector to XZ plane (ignores Y for height-independent detection).
    /// </summary>
    private Vector3 ProjectToXZ(Vector3 vector)
    {
        return new Vector3(vector.x, 0, vector.z).normalized;
    }

    /// <summary>
    /// Calculates distance ignoring height (XZ plane only).
    /// </summary>
    private float DistanceXZ(Vector3 a, Vector3 b)
    {
        Vector3 aXZ = new Vector3(a.x, 0, a.z);
        Vector3 bXZ = new Vector3(b.x, 0, b.z);
        return Vector3.Distance(aXZ, bXZ);
    }

    /// <summary>
    /// Determines if we should announce a turn.
    /// </summary>
    private bool ShouldAnnounceTurn(int turnCornerIndex, int currentCornerIndex, float distanceToCurrentCorner)
    {
        // Don't announce the same corner twice
        if (turnCornerIndex <= lastAnnouncedCornerIndex) return false;

        // Calculate distance to the turn (ignoring height)
        Vector3 turnPosition = pathCorners[turnCornerIndex];
        Vector3 playerPos = GetPlayerPosition();
        float distanceToTurn = DistanceXZ(playerPos, turnPosition);

        // Check if we're within the announcement range
        bool withinRange = distanceToTurn <= lookAheadDistance;

        // Also check if we've traveled enough distance since last announcement
        bool traveledEnough = totalDistanceTraveled >= minTravelDistanceBetweenCues;

        return withinRange && traveledEnough;
    }

    /// <summary>
    /// Determines the direction of a turn.
    /// </summary>
    private TurnDirection GetTurnDirection(int cornerIndex)
    {
        if (cornerIndex <= 0 || cornerIndex >= pathCorners.Count - 1)
        {
            return TurnDirection.None;
        }

        // Get incoming and outgoing directions (projected to XZ plane)
        Vector3 incoming = ProjectToXZ(pathCorners[cornerIndex] - pathCorners[cornerIndex - 1]);
        Vector3 outgoing = ProjectToXZ(pathCorners[cornerIndex + 1] - pathCorners[cornerIndex]);

        if (incoming == Vector3.zero || outgoing == Vector3.zero)
        {
            return TurnDirection.None;
        }

        float angle = Vector3.Angle(incoming, outgoing);

        // Check for U-turn first
        if (angle >= uturnAngleThreshold)
        {
            return TurnDirection.UTurn;
        }

        // Check for straight path
        if (playContinueStraightCue && angle <= straightAngleThreshold)
        {
            return TurnDirection.Straight;
        }

        // Determine left or right using cross product
        Vector3 cross = Vector3.Cross(incoming, outgoing);
        float crossY = cross.y;

        LogMessage($"Turn: angle={angle:F1}°, crossY={crossY:F3}, in=({incoming.x:F2},{incoming.z:F2}), out=({outgoing.x:F2},{outgoing.z:F2})");

        if (Mathf.Abs(crossY) < 0.01f)
        {
            return TurnDirection.Straight;
        }

        // Determine direction - can be inverted via inspector if needed
        bool isRight = crossY > 0;

        if (invertLeftRight)
        {
            isRight = !isRight;
        }

        return isRight ? TurnDirection.Right : TurnDirection.Left;
    }

    /// <summary>
    /// Gets a readable name for a turn direction.
    /// </summary>
    private string GetTurnDirectionName(TurnDirection direction)
    {
        switch (direction)
        {
            case TurnDirection.Left: return "Turn Left";
            case TurnDirection.Right: return "Turn Right";
            case TurnDirection.UTurn: return "U-Turn";
            case TurnDirection.Straight: return "Continue Straight";
            default: return "Unknown";
        }
    }

    /// <summary>
    /// Gets the appropriate audio clip for a turn direction.
    /// </summary>
    private AudioClip GetCueForTurn(TurnDirection direction)
    {
        switch (direction)
        {
            case TurnDirection.Left: return turnLeftClip;
            case TurnDirection.Right: return turnRightClip;
            case TurnDirection.UTurn: return uturnClip;
            case TurnDirection.Straight: return continueStraightClip;
            default: return null;
        }
    }

    /// <summary>
    /// Checks if the player is at the destination (ignoring height).
    /// </summary>
    private bool IsAtDestination(Vector3 playerPos)
    {
        if (pathCorners.Count == 0) return false;

        Vector3 destination = pathCorners[pathCorners.Count - 1];
        float distanceToDestination = DistanceXZ(playerPos, destination);

        return distanceToDestination < destinationThreshold;
    }

    /// <summary>
    /// Plays an audio cue.
    /// </summary>
    private void PlayCue(AudioClip clip, string cueName)
    {
        if (this == null || audioSource == null) return;

        if (clip == null)
        {
            LogWarning($"Null clip: {cueName}");
            return;
        }

        // Check if already playing
        if (audioSource.isPlaying)
        {
            LogMessage($"Queue skipped: '{cueName}' (audio busy)");
            return;
        }

        try
        {
            audioSource.PlayOneShot(clip);
            LogMessage($"▶ Playing: '{cueName}' [{clip.name}]");
        }
        catch (System.Exception ex)
        {
            LogWarning($"Error playing audio: {ex.Message}");
        }
    }

    /// <summary>
    /// Resets the audio navigation state.
    /// </summary>
    public void ResetState()
    {
        lastCueCheckTime = 0f;
        lastAnnouncedCornerIndex = -1;
        hasPlayedApproaching = false;
        hasPlayedDestination = false;
        totalDistanceTraveled = 0f;
        lastPlayerPosition = Vector3.zero;
    }

    /// <summary>
    /// Clears state when navigation stops.
    /// </summary>
    private void ClearState()
    {
        ResetState();
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
    /// Logs a message if debug is enabled.
    /// </summary>
    private void LogMessage(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[AudioNav] {message}");
        }
    }

    /// <summary>
    /// Logs a warning.
    /// </summary>
    private void LogWarning(string message)
    {
        Debug.LogWarning($"[AudioNav] {message}");
    }

    void OnDrawGizmos()
    {
        if (!showPathGizmos) return;
        if (pathCorners == null || pathCorners.Count < 2) return;

        // Draw path corners
        Gizmos.color = Color.yellow;
        for (int i = 0; i < pathCorners.Count; i++)
        {
            Gizmos.DrawWireSphere(pathCorners[i], 0.5f);
        }

        // Draw path lines
        Gizmos.color = Color.green;
        for (int i = 0; i < pathCorners.Count - 1; i++)
        {
            Gizmos.DrawLine(pathCorners[i], pathCorners[i + 1]);
        }

        // Draw destination
        if (pathCorners.Count > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pathCorners[pathCorners.Count - 1], 1f);
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
