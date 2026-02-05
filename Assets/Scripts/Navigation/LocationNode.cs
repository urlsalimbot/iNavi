using System;
using UnityEngine;

[Serializable]
public class LocationNode
{
    public string roomId;
    public int floor;
    public Vector3 anchorPosition;
    public float[] embedding;
}
