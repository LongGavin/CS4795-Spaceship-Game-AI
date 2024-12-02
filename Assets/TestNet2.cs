using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNet2
{
    public int layers;
    public int nodesPerLayer;
    public int outputSize = 3;
    public int inputSize = 3;
    public float[][][] weights;
    public float[][] biases;
    public float[] outputBiases;
    public float[] inputs;
    public float[] outputs;
    public float[][] outputWeights;

    public float defaultWeight = 0.5f;
    public float defaultBias = 0f;

    public bool randomizeWeights = true;

    public void Initialize()
    {
        outputs = new float[outputSize];
        inputs = new float[inputSize];
        weights = new float[layers][][];
        biases = new float[layers][];
        outputWeights = new float[outputSize][];
        outputBiases = new float[outputSize];

        for (int i = 0; i < layers; i++)
        {
            weights[i] = new float[nodesPerLayer][];
            biases[i] = new float[nodesPerLayer];
            for (int j = 0; j < nodesPerLayer; j++)
            {
                if (i == 0)
                {
                    weights[i][j] = new float[inputSize];
                    for (int k = 0; k < inputSize; k++)
                    {
                        weights[i][j][k] = UnityEngine.Random.Range(-0.5f, 0.5f);
                    }
                }
                else
                {
                    weights[i][j] = new float[nodesPerLayer];
                    for (int k = 0; k < nodesPerLayer; k++)
                    {
                        weights[i][j][k] = UnityEngine.Random.Range(-0.5f, 0.5f);
                    }
                }
                            
                biases[i][j] = 0;
            }
        }
        for (int i = 0; i < outputSize; i++)
        {
            outputWeights[i] = new float[nodesPerLayer];
            for (int j = 0; j < nodesPerLayer; j++)
            {
                outputWeights[i][j] = UnityEngine.Random.Range(-0.5f, 0.5f);
            }
            
            outputBiases[i] = 0;
        }
    }

    public void InitializeWithValues(float[][][] w, float[][] b, float[][] oW, float[] oB)
    {
        for (int i = 0; i < layers; i++)
        {
            if (i == 0)
            {
                for (int j = 0; j < inputSize; j++)
                {
                    Array.Copy(w[i][j], weights[i][j], inputSize);
                }
            }
            else
            {
                for (int j = 0; j < nodesPerLayer; j++)
                {
                    Array.Copy(w[i][j], weights[i][j], nodesPerLayer);
                }
            }
            
            
            Array.Copy(b[i], biases[i], nodesPerLayer);
        }

        
        Array.Copy(oB, outputBiases, outputSize);

        for (int i = 0; i < outputSize; i++)
        {
            Array.Copy(oW[i], outputWeights[i], nodesPerLayer);
        }
    }

    public float[] Evaluate()
    {
        outputs = new float[outputSize];
        float[] prevlayer = new float[nodesPerLayer];

        

        //loop over first layer (input * hidden layer 1)
        for (int i = 0; i < nodesPerLayer; i++)
        {
            float inSum = 0;
            for (int j = 0; j < inputSize; j++)
            {
                inSum += inputs[j] * weights[0][i][j];
            }

            prevlayer[i] = inSum + biases[0][i];
            prevlayer[i] = ActivationFunction(prevlayer[i]);
        }
        
        //loop over all hidden layers excluding first
        for (int i = 1; i < layers; i++)
        {
            float[] nextLayer = new float[nodesPerLayer];
            for (int j = 0; j < nodesPerLayer; j++)
            {
                float layerSum = 0;
                
                for (int k = 0; k < nodesPerLayer; k++)
                {
                    layerSum += prevlayer[k] * weights[i][j][k];
                }

                nextLayer[j] = layerSum + biases[i][j];
                nextLayer[j] = ActivationFunction(nextLayer[j]);
            }

            prevlayer = nextLayer;
        }

        

        //loop over output layer
        for (int i = 0; i < outputSize; i++)
        {
            float outSum = 0;
            for (int j = 0; j < nodesPerLayer; j++)
            {
                outSum += prevlayer[j] * outputWeights[i][j];
            }

            outputs[i] = outSum + outputBiases[i];
            outputs[i] = ActivationFunction(outputs[i]);
        }

        return outputs;
    }


    float ActivationFunction(float x)
    {
        return x / (Mathf.Sqrt(1 + x * x));
    }    
}
