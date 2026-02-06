using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.IO;
using System.Text;
using System;
using Unity.Collections;

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
    private string sessionDir;
    void Start()

    {
        IsManualOverride = false; // Reset on start
        string date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        makeSession(date);
    }

    void makeSession(string date)
    {
        sessionDir = Path.Combine(Application.persistentDataPath, $"Session_{date}");

        imageDir = Path.Combine(sessionDir, "images");
        labelDir = Path.Combine(sessionDir, "labels");

        Directory.CreateDirectory(imageDir);
        Directory.CreateDirectory(labelDir);

        Debug.Log("[DataCaptureManager] Dataset path: " + sessionDir);
    }

    void Update()
    {
        if (Time.time - lastCaptureTime < captureInterval)
            return;

        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            Debug.LogWarning("[DataCapture] Failed to acquire CPU image");
            return;
        }


        Transform cam = Camera.main.transform;

        // if (!qualityGate.IsFrameValid(cam))
        // {
        //     image.Dispose();
        //     return;
        // }

        SaveFrame(image, cam);
        image.Dispose();

        lastCaptureTime = Time.time;
    }

    void SaveFrame(XRCpuImage image, Transform cam)
    {

        Transform anchor = floorManager.GetCurrentAnchor();
        if (anchor == null)
        {
            Debug.LogError("[Label] Anchor is NULL");
            return;
        }
        
        Texture2D tex = ConvertAndDownscaleImage(image);
        string imgName = $"frame_{frameIndex:D6}.jpg";
        string imgPath = Path.Combine(imageDir, imgName);

        byte[] jpg = tex.EncodeToJPG(90);
        if (jpg == null || jpg.Length == 0)
        {
            Debug.LogError("JPG encode failed");
            return;
        }
        File.WriteAllBytes(imgPath, jpg);
        Destroy(tex);

        SaveLabel(imgName, cam);
        frameIndex++;
    }

    void SaveLabel(string imgName, Transform cam)
    {
        if (floorManager == null)
        {
            Debug.LogError("[Label] FloorManager is NULL");
            return;
        }

        Transform anchor = floorManager.GetCurrentAnchor();
        if (anchor == null)
        {
            Debug.LogError("[Label] Anchor is NULL");
            return;
        }

        Debug.Log("[Label] Writing label for " + imgName);

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
        sb.AppendFormat("\"tracking_state\":\"{0}\",", ARSession.state.ToString());
        sb.AppendFormat("\"timestamp\":{0}", System.DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        sb.Append("}");

        string labelPath = Path.Combine(
            labelDir,
            imgName.Replace(".jpg", ".json")
        );

        File.WriteAllText(labelPath, sb.ToString());
        Debug.Log("[Label] Wrote: " + labelPath);
    }

    Texture2D ConvertAndDownscaleImage(XRCpuImage image)
    {
        const int TARGET_WIDTH = 640;
        const int TARGET_HEIGHT = 480;

        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(TARGET_WIDTH, TARGET_HEIGHT),
            outputFormat = TextureFormat.RGB24,
            transformation = XRCpuImage.Transformation.MirrorY
        };

        int bufferSize = image.GetConvertedDataSize(conversionParams);
        var buffer = new NativeArray<byte>(bufferSize, Allocator.Temp);

        image.Convert(conversionParams, new NativeSlice<byte>(buffer));

        Texture2D tex = new Texture2D(
            TARGET_WIDTH,
            TARGET_HEIGHT,
            TextureFormat.RGB24,
            false
        );

        tex.LoadRawTextureData(buffer);
        tex.Apply();

        buffer.Dispose();

        return tex;
    }

    void OnApplicationQuit()
    {
        Debug.Log("[DataCaptureManager] Application quitting, cleaning up anchors.");

        if (floorManager != null)
        {
            for (int i = 0; i < floorManager.floorAnchors.Length; i++)
            {
                if (floorManager.floorAnchors[i] != null)
                {
                    Destroy(floorManager.floorAnchors[i].gameObject);
                    floorManager.floorAnchors[i] = null;
                }
            }
        }
    }

    public void AnchorNow()
    {
        if (floorManager != null)
        {
            floorManager.CreateAnchorAtCamera();
        }
    }
}