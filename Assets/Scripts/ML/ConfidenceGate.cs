using UnityEngine;

public class ConfidenceGate : MonoBehaviour
{
    [Range(0f, 1f)]
    public float threshold = 0.65f;

    public bool IsConfident(float confidence)
    {
        return confidence >= threshold;
    }
}
