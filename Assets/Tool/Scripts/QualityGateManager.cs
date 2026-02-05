using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class QualityGateManager : MonoBehaviour
{
    public float maxSpeed = 1.2f;        // m/s
    public float maxJumpDistance = 0.5f; // meters
    public float maxRotationalSpeed = 60.0f; // degrees/sec

    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private bool hasLastPose = false;

    public bool IsFrameValid(Transform cameraTransform)
    {
        if (ARSession.state != ARSessionState.SessionTracking)
            return false;

        if (hasLastPose)
        {
            float jump = Vector3.Distance(cameraTransform.position, lastPosition);
            if (jump > maxJumpDistance)
                return false;

            float rotJump = Quaternion.Angle(cameraTransform.rotation, lastRotation);
            float rotSpeed = rotJump / Time.deltaTime;
            if (rotSpeed > maxRotationalSpeed)
                return false;
        }

        lastPosition = cameraTransform.position;
        lastRotation = cameraTransform.rotation;
        hasLastPose = true;

        return true;
    }

    public void ResetTracking()
    {
        hasLastPose = false;
    }
}
