using UnityEngine;
using MultiSet;

/// <summary>
/// Helper component that ensures navigation UI elements are properly shown/hidden.
/// This works around issues with the SDK's NavigationUIController.
/// Attach this to the same GameObject as NavigationUIController or anywhere in the scene.
/// </summary>
public class NavigationUIHelper : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The Progress Slider GameObject")]
    public GameObject progressSlider;
    
    [Tooltip("The Stop Button GameObject")]
    public GameObject stopButton;
    
    [Tooltip("The Destination Select UI GameObject")]
    public GameObject destinationSelectUI;
    
    private bool wasNavigating = false;
    
    void Start()
    {
        // Initialize UI state - hide navigation elements
        if (progressSlider != null) progressSlider.SetActive(false);
        if (stopButton != null) stopButton.SetActive(false);
        if (destinationSelectUI != null) destinationSelectUI.SetActive(false);
    }
    
    void Update()
    {
        // Check if we're currently navigating
        bool isNavigating = NavigationController.instance != null && 
                           NavigationController.instance.IsCurrentlyNavigating();
        
        // State changed?
        if (isNavigating != wasNavigating)
        {
            wasNavigating = isNavigating;
            
            if (isNavigating)
            {
                // Started navigating - show progress slider and stop button
                Debug.Log("NavigationUIHelper: Navigation started, showing UI elements");
                if (progressSlider != null) progressSlider.SetActive(true);
                if (stopButton != null) stopButton.SetActive(true);
                
                // Hide the destination select UI
                if (destinationSelectUI != null) destinationSelectUI.SetActive(false);
            }
            else
            {
                // Stopped navigating - hide progress slider and stop button
                Debug.Log("NavigationUIHelper: Navigation stopped, hiding UI elements");
                if (progressSlider != null) progressSlider.SetActive(false);
                if (stopButton != null) stopButton.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Call this to manually show navigation UI elements.
    /// </summary>
    public void ShowNavigationUI()
    {
        Debug.Log("NavigationUIHelper: ShowNavigationUI called");
        if (progressSlider != null) progressSlider.SetActive(true);
        if (stopButton != null) stopButton.SetActive(true);
        wasNavigating = true;
    }
    
    /// <summary>
    /// Call this to manually hide navigation UI elements.
    /// </summary>
    public void HideNavigationUI()
    {
        Debug.Log("NavigationUIHelper: HideNavigationUI called");
        if (progressSlider != null) progressSlider.SetActive(false);
        if (stopButton != null) stopButton.SetActive(false);
        wasNavigating = false;
    }
}
