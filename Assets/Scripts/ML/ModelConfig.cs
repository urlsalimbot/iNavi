using System;

[Serializable]
public class ModelConfig
{
    public int input_width;
    public int input_height;
    public string input_type;
    public float mean;
    public float std;
    public string output_type;
    
    public int embeddingDim;
    public int outputDim;

}
