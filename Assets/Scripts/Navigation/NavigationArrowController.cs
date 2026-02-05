using UnityEngine;

public class NavigationArrowController : MonoBehaviour
{
    public Transform arrowVisual;
    public float rotationSpeed = 5f;

    public void PointTo(Vector3 worldTarget)
    {
        Vector3 dir = worldTarget - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        arrowVisual.rotation = Quaternion.Slerp(
            arrowVisual.rotation,
            targetRot,
            Time.deltaTime * rotationSpeed
        );
    }

    public void Hide() => arrowVisual.gameObject.SetActive(false);
    public void Show() => arrowVisual.gameObject.SetActive(true);
}
