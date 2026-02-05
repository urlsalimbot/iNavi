using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARAnchorController : MonoBehaviour
{
    public ARAnchorManager anchorManager;
    public GameObject navigationArrowPrefab;

    private ARAnchor currentAnchor;

    public void PlaceNavigationArrow(Vector3 worldPosition)
    {
        if (currentAnchor != null)
            Destroy(currentAnchor.gameObject);

        GameObject go = new GameObject("NavAnchor");
        go.transform.position = worldPosition;

        currentAnchor = go.AddComponent<ARAnchor>();

        Instantiate(
            navigationArrowPrefab,
            currentAnchor.transform.position,
            Quaternion.identity,
            currentAnchor.transform
        );
    }
}
