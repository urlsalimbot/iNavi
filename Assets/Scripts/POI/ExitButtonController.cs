using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages exit button functionality with confirmation dialog.
/// Attach this to a GameObject that manages the exit/quit functionality.
/// </summary>
public class ExitButtonController : MonoBehaviour
{
    [Header("Button References")]
    [Tooltip("The button that triggers the exit action")]
    public Button exitButton;

    [Header("Confirmation Dialog")]
    [Tooltip("The confirmation dialog panel GameObject")]
    public GameObject confirmDialog;

    [Tooltip("The confirm/yes button in the dialog")]
    public Button confirmButton;

    [Tooltip("The cancel/no button in the dialog")]
    public Button cancelButton;

    [Header("Dialog Text (Optional)")]
    [Tooltip("Title text for the confirmation dialog")]
    public TextMeshProUGUI dialogTitleText;

    [Tooltip("Message text for the confirmation dialog")]
    public TextMeshProUGUI dialogMessageText;

    [Header("Settings")]
    [Tooltip("Custom confirmation message")]
    public string confirmationMessage = "Are you sure you want to exit?";

    [Tooltip("Custom dialog title")]
    public string dialogTitle = "Exit Application";

    [Tooltip("Should the dialog show on exit button click?")]
    public bool showConfirmation = true;

    [Tooltip("What action to perform on confirm")]
    public ExitAction exitAction = ExitAction.QuitApplication;

    [Header("Scene Loading (if using scene transition)")]
    [Tooltip("Scene to load when exiting (if ExitAction is LoadScene)")]
    public string sceneToLoad;

    // State tracking
    private bool isDialogOpen = false;

    void Start()
    {
        // Initialize dialog state
        if (confirmDialog != null)
        {
            confirmDialog.SetActive(false);
        }

        // Set up button listeners
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(ConfirmExit);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CancelExit);
        }

        // Set default dialog text
        SetDialogText();
    }

    /// <summary>
    /// Called when the exit button is clicked.
    /// </summary>
    public void OnExitButtonClicked()
    {
        if (showConfirmation)
        {
            ShowConfirmDialog();
        }
        else
        {
            ConfirmExit();
        }
    }

    /// <summary>
    /// Shows the confirmation dialog.
    /// </summary>
    public void ShowConfirmDialog()
    {
        if (confirmDialog != null)
        {
            confirmDialog.SetActive(true);
            isDialogOpen = true;
            SetDialogText();

            Debug.Log("ExitButtonController: Confirmation dialog shown");
        }
        else
        {
            Debug.LogWarning("ExitButtonController: Confirm dialog not assigned!");
            ConfirmExit(); // Proceed without confirmation
        }
    }

    /// <summary>
    /// Hides the confirmation dialog.
    /// </summary>
    public void HideConfirmDialog()
    {
        if (confirmDialog != null)
        {
            confirmDialog.SetActive(false);
            isDialogOpen = false;

            Debug.Log("ExitButtonController: Confirmation dialog hidden");
        }
    }

    /// <summary>
    /// Confirms and performs the exit action.
    /// </summary>
    public void ConfirmExit()
    {
        Debug.Log("ExitButtonController: Exit confirmed");

        HideConfirmDialog();
        PerformExitAction();
    }

    /// <summary>
    /// Cancels the exit action.
    /// </summary>
    public void CancelExit()
    {
        Debug.Log("ExitButtonController: Exit cancelled");
        HideConfirmDialog();
    }

    /// <summary>
    /// Performs the configured exit action.
    /// </summary>
    public void PerformExitAction()
    {
        switch (exitAction)
        {
            case ExitAction.QuitApplication:
                QuitApplication();
                break;

            case ExitAction.LoadScene:
                LoadScene();
                break;

            case ExitAction.ReturnToMenu:
                ReturnToMenu();
                break;

            case ExitAction.Custom:
                OnCustomExit();
                break;
        }
    }

    /// <summary>
    /// Quits the application.
    /// Note: Does not work in Unity Editor without special handling.
    /// </summary>
    public void QuitApplication()
    {
        Debug.Log("ExitButtonController: Quitting application");

#if UNITY_EDITOR
        // In Editor, stop play mode
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // In build, quit the application
        Application.Quit();
#endif
    }

    /// <summary>
    /// Loads the specified scene.
    /// </summary>
    public void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("ExitButtonController: No scene specified to load!");
            return;
        }

        Debug.Log($"ExitButtonController: Loading scene '{sceneToLoad}'");
        SceneManager.LoadScene(sceneToLoad);
    }

    /// <summary>
    /// Returns to the main menu (scene index 0).
    /// </summary>
    public void ReturnToMenu()
    {
        Debug.Log("ExitButtonController: Returning to main menu");
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Override this method for custom exit behavior.
    /// </summary>
    protected virtual void OnCustomExit()
    {
        Debug.Log("ExitButtonController: Custom exit action");
        // Override this method in a subclass for custom behavior
    }

    /// <summary>
    /// Sets the dialog text.
    /// </summary>
    private void SetDialogText()
    {
        if (dialogTitleText != null)
        {
            dialogTitleText.text = dialogTitle;
        }

        if (dialogMessageText != null)
        {
            dialogMessageText.text = confirmationMessage;
        }
    }

    /// <summary>
    /// Sets custom dialog text.
    /// </summary>
    public void SetDialogText(string title, string message)
    {
        dialogTitle = title;
        confirmationMessage = message;
        SetDialogText();
    }

    /// <summary>
    /// Checks if the confirmation dialog is currently open.
    /// </summary>
    public bool IsDialogOpen()
    {
        return isDialogOpen;
    }

    void Update()
    {
        // Close dialog on Escape key
        if (isDialogOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CancelExit();
        }
    }

    void OnDestroy()
    {
        // Clean up button listeners
        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(OnExitButtonClicked);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(ConfirmExit);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(CancelExit);
        }
    }

    /// <summary>
    /// Enum defining the type of exit action to perform.
    /// </summary>
    public enum ExitAction
    {
        QuitApplication,    // Quit the app
        LoadScene,          // Load a specific scene
        ReturnToMenu,       // Return to main menu (scene 0)
        Custom              // Custom action (override OnCustomExit)
    }
}
