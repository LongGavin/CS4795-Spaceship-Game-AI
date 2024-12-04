using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Network
{
    public Layer[] weightLayers;
    public BiasLayer[] biasLayers;
    public float[] outputBiases;
    public Node[] outputWeights;
}

[System.Serializable]
public class Layer
{
    public Node[] nodes;
}

[System.Serializable]
public class Node
{
    public float[] weights;
}

[System.Serializable]
public class BiasLayer
{
    public float[] biases;
}

[System.Serializable]
public class NetworkList
{
    public Network[] nets;
}
