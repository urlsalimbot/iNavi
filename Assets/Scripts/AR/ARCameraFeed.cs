using UnityEngine;

public class ARCameraFeed : MonoBehaviour
{
    public float[] LatestFrame { get; private set; }
    public bool HasFrame => LatestFrame != null;

    private int targetWidth = 224;
    private int targetHeight = 224;

    public void SetTargetResolution(int w, int h)
    {
        targetWidth = w;
        targetHeight = h;
    }

    // Call this after CPU image acquisition & preprocessing
    public void UpdateFrame(float[] normalizedTensor)
    {
        LatestFrame = normalizedTensor;
    }
}
