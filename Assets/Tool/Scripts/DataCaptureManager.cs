using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.IO;
using System.Text;

public class DataCaptureManager : MonoBehaviour
{
    public ARCameraManager cameraManager;
    public FloorManager floorManager;
    public QualityGateManager qualityGate;

    public float captureInterval = 0.8f;

    private float lastCaptureTime;
    private int frameIndex;

    public static string CurrentRegion = "Unknown";
    public static bool IsManualOverride = false;

    private string imageDir;
    private string labelDir;

    void Start()
    {
        IsManualOverride = false; // Reset on start
        imageDir = Path.Combine(Application.persistentDataPath, "images");
        labelDir = Path.Combine(Application.persistentDataPath, "labels");

        Directory.CreateDirectory(imageDir);
        Directory.CreateDirectory(labelDir);

        Debug.Log("[DataCaptureManager] Dataset path: " + Application.persistentDataPath);
    }

    void Update()
    {
        if (Time.time - lastCaptureTime < captureInterval)
            return;

        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            return;

        Transform cam = Camera.main.transform;

        if (!qualityGate.IsFrameValid(cam))
        {
            image.Dispose();
            return;
        }

        SaveFrame(image, cam);
        image.Dispose();

        lastCaptureTime = Time.time;
    }

    void SaveFrame(XRCpuImage image, Transform cam)
    {
        Texture2D tex = ConvertImage(image);
        string imgName = $"frame_{{frameIndex:D6}}.jpg";
        string imgPath = Path.Combine(imageDir, imgName);

        File.WriteAllBytes(imgPath, tex.EncodeToJPG(90));

        SaveLabel(imgName, cam);
        frameIndex++;
    }

    void SaveLabel(string imgName, Transform cam)
    {
        if (floorManager == null) return;
        Transform anchor = floorManager.GetCurrentAnchor();
        if (anchor == null) return;

        Vector3 rel = cam.position - anchor.position;
        Vector3 rot = cam.eulerAngles;

        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        sb.AppendFormat("\"image\":\"{0}\",", imgName);
        sb.AppendFormat("\"x\":{0:F3},", rel.x);
        sb.AppendFormat("\"y\":{0:F3},", rel.z);
        sb.AppendFormat("\"z\":{0:F3},", rel.y);
        sb.AppendFormat("\"yaw\":{0:F1},", rot.y);
        sb.AppendFormat("\"pitch\":{0:F1},", rot.x);
        sb.AppendFormat("\"roll\":{0:F1},", rot.z);
        sb.AppendFormat("\"floor\":{0},", floorManager.currentFloor);
        sb.AppendFormat("\"region\":\"{0}\",", CurrentRegion);
        sb.AppendFormat("\"manual\":{0},", IsManualOverride ? "true" : "false");
        sb.AppendFormat("\"tracking_state\":\"{0}\",", ARSession.state.ToString());
        sb.AppendFormat("\"timestamp\":{0}", System.DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        sb.Append("}");

        string labelPath = Path.Combine(
            labelDir,
            imgName.Replace(".jpg", ".json")
        );

        File.WriteAllText(labelPath, sb.ToString());
    }

    Texture2D ConvertImage(XRCpuImage image)
    {
        Texture2D tex = new Texture2D(image.width, image.height, TextureFormat.RGB24, false);

        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(image.width, image.height),
            outputFormat = TextureFormat.RGB24,
            transformation = XRCpuImage.Transformation.MirrorY
        };

        var raw = tex.GetRawTextureData<byte>();
        image.Convert(conversionParams, raw);
        tex.Apply();

        return tex;
    }
}