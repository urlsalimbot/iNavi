using UnityEngine;
using System.IO;
using System.Diagnostics;

public class ModelManager : MonoBehaviour
{
    [Header("Dependencies")]
    public ARCameraFeed cameraFeed;
    public AndroidInferenceBridge inference;

    [Header("Model Configs")]
    public string int8ModelName;
    public string fp32ModelName;

    public ModelConfig CurrentConfig { get; private set; }
    public string ActiveModelName { get; private set; }

    private bool loggingEnabled;

    // ===== Model Lifecycle =====

    public void SwitchModel(bool useFP32)
    {
        string modelName = useFP32 ? fp32ModelName : int8ModelName;
        LoadModel(modelName);
    }

    private void LoadModel(string modelName)
    {
        string basePath = Path.Combine(Application.streamingAssetsPath, "models");
        string modelPath = Path.Combine(basePath, modelName + ".tflite");
        string jsonPath = Path.Combine(basePath, modelName + ".json");

        string json = File.ReadAllText(jsonPath);
        CurrentConfig = JsonUtility.FromJson<ModelConfig>(json);

        cameraFeed.SetTargetResolution(
            CurrentConfig.input_width,
            CurrentConfig.input_height
        );

        inference.LoadModel(modelPath);
        ActiveModelName = modelName;
    }

    // ===== Inference =====

    public InferenceResult RunInference(float[] input)
    {
        Stopwatch sw = Stopwatch.StartNew();

        float[] output = inference.Run(input);

        sw.Stop();

        var result = new InferenceResult
        {
            embedding = ExtractEmbedding(output),
            confidence = ExtractConfidence(output),
            latencyMs = sw.ElapsedMilliseconds
        };

        if (loggingEnabled)
            LogResult(result);

        return result;
    }

    // ===== Output Parsing =====

    private float[] ExtractEmbedding(float[] rawOutput)
    {
        int dim = CurrentConfig.embeddingDim;
        float[] embedding = new float[dim];
        System.Array.Copy(rawOutput, 0, embedding, 0, dim);
        return embedding;
    }

    private float ExtractConfidence(float[] rawOutput)
    {
        int offset = CurrentConfig.embeddingDim;
        float max = float.MinValue;
        float sum = 0f;

        for (int i = offset; i < rawOutput.Length; i++)
        {
            float v = Mathf.Exp(rawOutput[i]);
            sum += v;
            if (v > max) max = v;
        }

        return max / sum;
    }

    // ===== Logging =====

    public void EnableLogging(bool enabled)
    {
        loggingEnabled = enabled;
    }

    private void LogResult(InferenceResult result)
    {
        UnityEngine.Debug.Log(
            $"[Inference] Model={ActiveModelName} " +
            $"Latency={result.latencyMs:F1}ms " +
            $"Confidence={result.confidence:F2}"
        );
    }
}
