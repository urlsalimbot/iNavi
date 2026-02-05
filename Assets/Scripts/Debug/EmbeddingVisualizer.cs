using UnityEngine;

public class EmbeddingVisualizer : MonoBehaviour
{
    public LineRenderer line;
    public int maxDims = 64;

    public void Render(float[] embedding)
    {
        int dims = Mathf.Min(maxDims, embedding.Length);
        line.positionCount = dims;

        for (int i = 0; i < dims; i++)
        {
            line.SetPosition(i, new Vector3(i * 0.02f, embedding[i], 0));
        }
    }
}
