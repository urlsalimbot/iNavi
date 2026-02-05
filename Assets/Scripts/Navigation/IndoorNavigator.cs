using UnityEngine;

public class IndoorNavigator : MonoBehaviour
{
    [Header("Data")]
    public LocationDatabase database;

    [Header("Visuals")]
    public NavigationArrowController arrow;

    public LocationNode ResolveLocation(float[] embedding)
    {
        if (database == null || database.locations.Count == 0)
            return null;

        database.FindBestMatch(embedding, out _);
        return database.FindBestMatch(embedding, out _);
    }

    public void Navigate(float[] embedding)
    {
        var node = ResolveLocation(embedding);
        if (node == null)
        {
            HaltNavigation();
            return;
        }

        arrow.Show();
        arrow.PointTo(node.anchorPosition);
    }

    public void HaltNavigation()
    {
        if (arrow != null)
            arrow.Hide();
    }

    public void ResetNavigation()
    {
        HaltNavigation();
    }
}
