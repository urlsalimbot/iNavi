using UnityEngine;

public class AndroidInferenceBridge : MonoBehaviour
{
    private AndroidJavaObject engine;

    void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var unityPlayer =
            new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            engine = new AndroidJavaObject(
                "com.example.tflite.TFLiteEngine", activity);
        }
#endif
    }

    public void LoadModel(string path)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        engine.Call("loadModel", path);
#endif
    }

    public float[] Run(byte[] input)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return engine.Call<float[]>("run", input);
#else
        return new float[256];
#endif
    }

    public float[] Run(float[] input)
{
#if UNITY_ANDROID && !UNITY_EDITOR
    return engine.Call<float[]>("runInference", input);
#else
    return null;
#endif
}
}
