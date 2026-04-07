using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the FAQ (Frequently Asked Questions) panel UI.
/// Attach this to a GameObject that manages the FAQ panel.
/// </summary>
public class FAQPanelController : MonoBehaviour
{
    [Header("Panel References")]
    [Tooltip("The FAQ panel GameObject to show/hide")]
    public GameObject faqPanel;

    [Tooltip("The button that opens the FAQ panel")]
    public Button openFAQButton;

    [Tooltip("The button that closes the FAQ panel")]
    public Button closeFAQButton;

    [Header("FAQ Content (Optional)")]
    [Tooltip("Text component to display FAQ content")]
    public TextMeshProUGUI faqContentText;

    [Tooltip("ScrollRect for scrolling through FAQ content")]
    public ScrollRect faqScrollRect;

    // State tracking
    private bool isPanelOpen = false;

    void Start()
    {
        // Initialize panel state
        if (faqPanel != null)
        {
            faqPanel.SetActive(false);
        }

        // Set up button listeners
        if (openFAQButton != null)
        {
            openFAQButton.onClick.AddListener(OpenFAQ);
        }

        if (closeFAQButton != null)
        {
            closeFAQButton.onClick.AddListener(CloseFAQ);
        }

        // Populate default FAQ content if text component exists
        if (faqContentText != null && string.IsNullOrEmpty(faqContentText.text))
        {
            PopulateDefaultFAQ();
        }
    }

    /// <summary>
    /// Opens the FAQ panel.
    /// </summary>
    public void OpenFAQ()
    {
        if (faqPanel != null)
        {
            faqPanel.SetActive(true);
            isPanelOpen = true;

            // Reset scroll position
            if (faqScrollRect != null)
            {
                faqScrollRect.verticalNormalizedPosition = 1f;
            }

            Debug.Log("FAQPanelController: FAQ panel opened");
        }
    }

    /// <summary>
    /// Closes the FAQ panel.
    /// </summary>
    public void CloseFAQ()
    {
        if (faqPanel != null)
        {
            faqPanel.SetActive(false);
            isPanelOpen = false;

            Debug.Log("FAQPanelController: FAQ panel closed");
        }
    }

    /// <summary>
    /// Toggles the FAQ panel open/closed.
    /// </summary>
    public void ToggleFAQ()
    {
        if (isPanelOpen)
        {
            CloseFAQ();
        }
        else
        {
            OpenFAQ();
        }
    }

    /// <summary>
    /// Checks if the FAQ panel is currently open.
    /// </summary>
    public bool IsFAQOpen()
    {
        return isPanelOpen;
    }

    /// <summary>
    /// Populates the FAQ content with default text.
    /// Override this method or set faqContentText directly to customize.
    /// </summary>
    private void PopulateDefaultFAQ()
    {
        string faqText = @"
<color=#4DA6FF><size=76><b>FREQUENTLY ASKED QUESTIONS</b></size></color>

<color=#FFD700><b>→ How do I navigate to a destination?</b></color>
Tap the destination button, select a building, then choose your room. Navigation will start automatically.

<color=#FFD700><b>→ How do I enable audio navigation?</b></color>
Audio navigation is automatic when navigating. Ensure your device volume is on.

<color=#FFD700><b>→ What do the building codes mean?</b></color>
• NB = New Building
• CS = Computer Science Building
• AB = Admin Building
• MB = Main Building

<color=#FFD700><b>→ The navigation is inaccurate, what should I do?</b></color>
Try recalibrating by walking to a known landmark. Ensure you have good GPS signal.

<color=#FFD700><b>→ Who do I contact for support?</b></color>
Contact IT Support at juhniarl@gmail.com.
";

        if (faqContentText != null)
        {
            faqContentText.text = faqText;
        }
    }

    /// <summary>
    /// Sets custom FAQ content.
    /// </summary>
    public void SetFAQContent(string content)
    {
        if (faqContentText != null)
        {
            faqContentText.text = content;
        }
    }

    void Update()
    {
        // Close FAQ on Escape key
        if (isPanelOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseFAQ();
        }
    }

    void OnDestroy()
    {
        // Clean up button listeners
        if (openFAQButton != null)
        {
            openFAQButton.onClick.RemoveListener(OpenFAQ);
        }

        if (closeFAQButton != null)
        {
            closeFAQButton.onClick.RemoveListener(CloseFAQ);
        }
    }
}
