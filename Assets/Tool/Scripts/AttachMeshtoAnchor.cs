using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARAnchorManager))]
public class AttachMeshToAnchor : MonoBehaviour
{
    [Header("References")]
    public GameObject meshPrefab; // Prefab with MeshRenderer + MeshFilter
    public Camera arCamera;

    private ARAnchorManager anchorManager;

    void Awake()
    {
        anchorManager = GetComponent<ARAnchorManager>();

        if (meshPrefab == null)
            Debug.LogError("Mesh Prefab is not assigned.");
        if (arCamera == null)
            Debug.LogError("AR Camera is not assigned.");
    }

    public void AttachtoCam(ARAnchor anchor, Pose pose)
    {
        // Raycast against detected planes

        // Instantiate mesh as a child of the anchor
        GameObject meshObject = Instantiate(meshPrefab, pose.position, pose.rotation);
        meshObject.transform.SetParent(anchor.transform, worldPositionStays: true);

        Debug.Log("Mesh attached to anchor successfully.");
    }
}


