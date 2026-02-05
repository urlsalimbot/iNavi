using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LocationDatabase", menuName = "IndoorNav/LocationDatabase")]
public class LocationDatabase : ScriptableObject
{
    public List<LocationNode> locations = new();

    public void Add(LocationNode node)
    {
        locations.Add(node);
    }

    public LocationNode FindBestMatch(float[] queryEmbedding, out float similarity)
    {
        LocationNode best = null;
        similarity = -1f;

        foreach (var loc in locations)
        {
            float sim = CosineSimilarity(queryEmbedding, loc.embedding);
            if (sim > similarity)
            {
                similarity = sim;
                best = loc;
            }
        }
        return best;
    }

    private float CosineSimilarity(float[] a, float[] b)
    {
        float dot = 0, magA = 0, magB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }
        return dot / (Mathf.Sqrt(magA) * Mathf.Sqrt(magB) + 1e-6f);
    }
}
