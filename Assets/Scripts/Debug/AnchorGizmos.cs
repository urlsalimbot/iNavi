using UnityEngine;

public class AnchorGizmos : MonoBehaviour
{
    public Color color = Color.cyan;
    public float radius = 0.15f;

    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, radius);
        Gizmos.DrawLine(
            transform.position,
            transform.position + transform.forward * 0.3f
        );
    }
}
