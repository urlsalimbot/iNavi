using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Status")]
    public TMP_Text modeText;
    public TMP_Text modelText;
    public TMP_Text latencyText;
    public TMP_Text confidenceText;

    [Header("Controls")]
    public Button startMappingButton;
    public Button startNavigationButton;
    public Button resetSessionButton;

    [Header("Research")]
    public Toggle precisionToggle; // FP32 vs INT8
    public Toggle loggingToggle;

    private AppController app;

    void Awake()
    {
        app = FindObjectOfType<AppController>();

        startMappingButton.onClick.AddListener(() => app.SetMode(AppMode.Mapping));
        startNavigationButton.onClick.AddListener(() => app.SetMode(AppMode.Navigation));
        resetSessionButton.onClick.AddListener(app.ResetSession);

        precisionToggle.onValueChanged.AddListener(OnPrecisionChanged);
        loggingToggle.onValueChanged.AddListener(app.SetLogging);
    }

    void OnPrecisionChanged(bool isFP32)
    {
        app.SetModelPrecision(isFP32);
    }

    // === Public update hooks ===

    public void UpdateMode(AppMode mode)
    {
        modeText.text = $"Mode: {mode}";
    }

    public void UpdateModel(string modelName)
    {
        modelText.text = $"Model: {modelName}";
    }

    public void UpdateLatency(float ms)
    {
        latencyText.text = $"Latency: {ms:F1} ms";
    }

    public void UpdateConfidence(float value)
    {
        confidenceText.text = $"Confidence: {value:F2}";
    }
}
