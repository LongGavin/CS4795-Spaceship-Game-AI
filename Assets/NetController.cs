using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ScoringController;

public class NetController : MonoBehaviour
{
    public TestNet2 net;
    public Rigidbody rb;
    public Transform moonPos;

    public int layerCount = 2;
    public int nodeCount = 4;
    public int inputCount = 12;
    public int outputCount = 2;

    public bool isActive = true;

    public AISpaceshipController controller;

    // Start is called before the first frame update
    void Start()
    {
        net = new TestNet2();
        net.layers = layerCount;
        net.nodesPerLayer = nodeCount;
        net.inputSize = inputCount;
        net.outputSize = outputCount;
        net.Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.isEnabled)
        {
            isActive = true;
        }
        else
        {
            isActive = false;
        }
        if (isActive)
        {
            float[] newInputs = new float[inputCount];

            //position
            newInputs[0] = transform.position.x;
            newInputs[1] = transform.position.y;
            newInputs[2] = transform.position.z;

            //rotation
            newInputs[3] = transform.rotation.x;
            newInputs[4] = transform.rotation.y;
            newInputs[5] = transform.rotation.z;

            //velocity
            newInputs[6] = rb.velocity.x;
            newInputs[7] = rb.velocity.y;
            newInputs[8] = rb.velocity.z;

            //moon position
            newInputs[9] = moonPos.position.x;
            newInputs[10] = moonPos.position.y;
            newInputs[11] = moonPos.position.z;

            net.inputs = newInputs;
            float[] outputs = net.Evaluate();

            //Debug.Log(outputs[0] + ", " + outputs[1]);

            if (outputs[0] > 0.1f || outputs[0] < -0.1f)
            {
                controller.throttle = 1;
            }
            else
            {
                controller.throttle = 0;
            }

            if (outputs[1] > 0.1f || outputs[1] < -0.1f)
            {
                controller.xRotation = outputs[1];
            }
            else
            {
                controller.xRotation = 0;
            }
        }
    }

    public void MutateNetwork(NetData data, float mutationRate)
    {
        float[][][] weights;
        float[][] biases;
        float[] outputBiases;
        float[][] outputWeights;

        weights = new float[data.layers][][];
        biases = new float[data.layers][];

        for (int i = 0; i < data.layers; i++)
        {
            weights[i] = new float[data.nodesPerLayer][];
                       
            for (int j = 0; j < data.nodesPerLayer; j++)
            {
                if (i == 0)
                {
                    weights[i][j] = new float[data.inputSize];
                    //Debug.Log(weights[i][j].Length + " : " + data.weights[i][j].Length);
                    Array.Copy(data.weights[i][j], weights[i][j], data.inputSize);
                }
                else
                {
                    weights[i][j] = new float[data.nodesPerLayer];
                    Array.Copy(data.weights[i][j], weights[i][j], data.nodesPerLayer);
                }
                    
            }
            

            biases[i] = new float[data.nodesPerLayer];
            Array.Copy(data.biases[i], biases[i], data.nodesPerLayer);
        }

        outputBiases = new float[data.outputSize];
        outputWeights = new float[data.outputSize][];

        Array.Copy(data.outputBiases, outputBiases, data.outputSize);

        for (int i = 0; i < data.outputSize; i++)
        {
            outputWeights[i] = new float[data.nodesPerLayer];
            Array.Copy(data.outputWeights[i], outputWeights[i], data.nodesPerLayer);
        }



        


        

        for (int j = 0; j < data.layers; j++)
        {
            for (int k = 0; k < data.nodesPerLayer; k++)
            {
                if (j == 0)
                {
                    for (int w = 0; w < data.inputSize; w++)
                    {
                        weights[j][k][w] = weights[j][k][w] + UnityEngine.Random.Range(-mutationRate, mutationRate);
                    }
                }
                else
                {
                    for (int w = 0; w < data.nodesPerLayer; w++)
                    {
                        weights[j][k][w] = weights[j][k][w] + UnityEngine.Random.Range(-mutationRate, mutationRate);
                    }
                }
                

                biases[j][k] = biases[j][k] + UnityEngine.Random.Range(-mutationRate, mutationRate);
            }
        }

        for (int j = 0; j < data.outputSize; j++)
        {
            outputBiases[j] = outputBiases[j] + UnityEngine.Random.Range(-mutationRate, mutationRate);
        }

        for (int j = 0; j < data.outputSize; j++)
        {
            for (int k = 0; k < data.nodesPerLayer; k++)
            {
                outputWeights[j][k] = outputWeights[j][k] + UnityEngine.Random.Range(-mutationRate, mutationRate);
            }
        }
        
        net.InitializeWithValues(weights, biases, outputWeights, outputBiases);
    }
}
