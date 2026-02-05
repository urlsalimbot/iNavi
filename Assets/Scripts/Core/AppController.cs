using UnityEngine;

public enum AppMode
{
    Mapping,
    Navigation,
    Evaluation
}

public class AppController : MonoBehaviour
{
    [Header("Systems")]
    public ARCameraFeed cameraFeed;
    public ModelManager modelManager;
    public IndoorNavigator navigator;
    public ConfidenceGate confidenceGate;
    public UIController ui;

    [Header("Inference")]
    public int inferEveryNFrames = 6;

    private AppMode currentMode = AppMode.Navigation;
    private int frameCounter;

    void Update()
    {
        if (currentMode != AppMode.Navigation)
            return;

        if (!cameraFeed.HasFrame)
            return;

        frameCounter++;
        if (frameCounter % inferEveryNFrames != 0)
            return;

        RunNavigationInference();
    }

    private void RunNavigationInference()
    {
        var input = cameraFeed.LatestFrame;
        var result = modelManager.RunInference(input);

        ui.UpdateLatency(result.latencyMs);
        ui.UpdateConfidence(result.confidence);

        if (!confidenceGate.IsConfident(result.confidence))
        {
            navigator.HaltNavigation();
            return;
        }

        navigator.Navigate(result.embedding);
    }

    // ===== UI hooks =====

    public void SetMode(AppMode mode)
    {
        currentMode = mode;
        ui.UpdateMode(mode);
    }

    public void ResetSession()
    {
        frameCounter = 0;
        navigator.ResetNavigation();
    }

    public void SetModelPrecision(bool useFP32)
    {
        modelManager.SwitchModel(useFP32);
        ui.UpdateModel(modelManager.ActiveModelName);
    }

    public void SetLogging(bool enabled)
    {
        modelManager.EnableLogging(enabled);
    }
}
