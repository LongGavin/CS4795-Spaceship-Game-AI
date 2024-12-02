using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMoveToPoint1 : MonoBehaviour
{
    public int trainingCount = 10;
    public int winningCount = 3;
    public float trainingTime = 20;
    public float mutationRate = 0.05f;

    public int layerCount = 2;
    public int nodeCount = 4;
    public int inputCount = 3;
    public int outputCount = 3;

    public TestNet2[] net;
    public Rigidbody[] balls;
    public Vector3 startPoint;
    public Vector3 targetPoint;

    

    public float strength = 5;

    public float[] distances;

    bool running;

    public int generation = 0;


    public bool loadNetwork;
    public bool saveAfterRun;

    [Serializable]
    public struct win
    {
        public float score;
        public float[][][] weights;
        public float[][] biases;
        public float[] outputBiases;
        public float[][] outputWeights;
    }

    struct network
    {
        public float[][][] weights;
        public float[][] biases;
        public float[] outputBiases;
        public float[][] outputWeights;
    }

    public List<win> scores = new List<win>();

    // Start is called before the first frame update
    void Start()
    {
        net = new TestNet2[trainingCount];
        distances = new float[trainingCount];
        for (int i = 0; i < net.Length; i++)
        {
            net[i] = new TestNet2();
            net[i].layers = layerCount;
            net[i].nodesPerLayer = nodeCount;
            net[i].inputSize = inputCount;
            net[i].outputSize = outputCount;
            net[i].Initialize();
            
        }


        

        //SaveNetwork.SaveOneNet(scores[0], "test");
        



        for (int i = 0; i < trainingCount; i++)
        {
            win w = new win();

            w.weights = new float[layerCount][][];
            w.biases = new float[layerCount][];

            for (int j = 0; j < layerCount; j++)
            {
                w.weights[j] = new float[nodeCount][];
                for (int k = 0; k < nodeCount; k++)
                {
                    if (j == 0)
                    {
                        w.weights[j][k] = new float[nodeCount];
                        Array.Copy(net[i].weights[j][k], w.weights[j][k], inputCount);
                    }
                    else
                    {
                        w.weights[j][k] = new float[nodeCount];
                        Array.Copy(net[i].weights[j][k], w.weights[j][k], nodeCount);
                    }
                }
                w.biases[j] = new float[nodeCount];

                Array.Copy(net[i].biases[j], w.biases[j], layerCount);
            }

            w.outputWeights = new float[outputCount][];
            for (int j = 0; j < outputCount; j++)
            {
                w.outputWeights[j] = new float[nodeCount];
                Array.Copy(net[i].outputWeights[j], w.outputWeights[j], outputCount);
            }

            w.outputBiases = new float[outputCount];

            Array.Copy(net[i].outputBiases, w.outputBiases, outputCount);
            w.score = 0;
            scores.Add(w);
        }
        

        Debug.Log(scores[0].weights[1][1][14]);

        StartCoroutine(Timer());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (running)
        {
            for (int i = 0; i < trainingCount; i++)
            {
                float[] newInputs = new float[inputCount];
                newInputs[0] = balls[i].gameObject.transform.position.x;
                newInputs[1] = balls[i].gameObject.transform.position.y;
                newInputs[2] = balls[i].gameObject.transform.position.z;
                newInputs[3] = Vector3.Distance(balls[i].position, targetPoint);
                newInputs[4] = balls[i].velocity.magnitude;

                net[i].inputs = newInputs;
                float[] nOut = net[i].Evaluate();

                Vector3 force = new Vector3(Mathf.Clamp(nOut[0], -1, 1), 
                                            Mathf.Clamp(nOut[1], -1, 1), 
                                            Mathf.Clamp(nOut[2], -1, 1));               
 
                balls[i].AddForce(force * strength, ForceMode.Force);

                float newDist = Vector3.Distance(balls[i].transform.position, targetPoint);

                float diff = distances[i] - newDist;
                distances[i] = newDist;

                float bonus = 0;

                float sign = 1;
                if (diff < 0)
                {
                    diff *= 1.2f;
                    sign = -1;
                }
                else if (newDist < 0.5f)
                {
                    bonus = 0.5f;
                }


                win w = scores[i];
                w.score += sign * (diff * diff) + bonus;
                scores[i] = w;
            }
        }
        else
        {
            //SaveNetwork.SaveNetList(scores.ToArray(), "postRun");

            for (int i = 0; i < trainingCount; i++)
            {
                if (Vector3.Distance(balls[i].transform.position, targetPoint) < 0.01f)
                {
                    win w = scores[i];
                    w.score += 5;
                    scores[i] = w;
                }
            }
            
            
            scores.Sort((s1, s2) => s1.score.CompareTo(s2.score));

            scores.Reverse();

            Debug.Log(scores[0].score);
            Debug.Log(scores[1].score);
            Debug.Log(scores[2].score);


            for (int i = 0; i < trainingCount; i++)
            {
                int s = UnityEngine.Random.Range(0, winningCount);

                float[][][] weights = new float[layerCount][][];
                float[][] biases = new float[layerCount][];               
                float[][] outputWeights = new float[outputCount][];
                float[] outputBiases = new float[outputCount];

                for (int j = 0; j < layerCount; j++)
                {
                    weights[j] = new float[nodeCount][];
                    for (int k = 0; k < nodeCount; k++)
                    {
                        weights[j][k] = new float[nodeCount];
                        Array.Copy(scores[s].weights[j][k], weights[j][k], nodeCount);
                    }
                    biases[j] = new float[nodeCount];
                    
                    Array.Copy(scores[s].biases[j], biases[j], nodeCount);
                }

                for (int j = 0; j < outputCount; j++)
                {
                    outputWeights[j] = new float[nodeCount];
                    Array.Copy(scores[s].outputWeights[j], outputWeights[j], nodeCount);
                }

                
                Array.Copy(scores[s].outputBiases, outputBiases, outputCount);

                for (int j = 0; j < layerCount ; j++)
                {
                    for (int k = 0; k < nodeCount; k++)
                    {
                        for (int w = 0; w < nodeCount; w++)
                        {
                            weights[j][k][w] = weights[j][k][w] + UnityEngine.Random.Range(-mutationRate, mutationRate);
                        }
                        
                        biases[j][k] = biases[j][k] + UnityEngine.Random.Range(-mutationRate, mutationRate);
                    }
                }

                for (int j = 0; j < outputCount; j++)
                {
                    outputBiases[j] = outputBiases[j] + UnityEngine.Random.Range(-mutationRate, mutationRate);
                }

                for (int j = 0; j < outputCount; j++)
                {
                    for (int k = 0; k < nodeCount; k++)
                    {
                        outputWeights[j][k] = outputWeights[j][k] + UnityEngine.Random.Range(-mutationRate, mutationRate);
                    }       
                }
                //Debug.Log(biases.Length + " : " + winners[s].biases.Length);
                net[i].InitializeWithValues(weights, biases, outputWeights, outputBiases);
                balls[i].transform.position = startPoint;
            }

            for (int i = 0; i < trainingCount; i++)
            {
                win w = new win();

                w.weights = new float[layerCount][][];
                w.biases = new float[layerCount][];

                for (int j = 0; j < layerCount; j++)
                {
                    w.weights[j] = new float[nodeCount][];
                    for (int k = 0; k < nodeCount; k++)
                    {
                        if (j == 0)
                        {
                            w.weights[j][k] = new float[nodeCount];
                            Array.Copy(net[i].weights[j][k], w.weights[j][k], inputCount);
                        }
                        else
                        {
                            w.weights[j][k] = new float[nodeCount];
                            Array.Copy(net[i].weights[j][k], w.weights[j][k], nodeCount);
                        }
                    }
                    w.biases[j] = new float[nodeCount];
                    
                    Array.Copy(net[i].biases[j], w.biases[j], nodeCount);
                }

                w.outputWeights = new float[outputCount][];
                for (int j = 0; j < outputCount; j++)
                {
                    w.outputWeights[j] = new float[nodeCount];
                    Array.Copy(net[i].outputWeights[j], w.outputWeights[j], nodeCount);
                }
                w.outputBiases = new float[outputCount];

                
                Array.Copy(net[i].outputBiases, w.outputBiases, outputCount);
                w.score = 0;
                scores[i] = w;

                distances[i] = Vector3.Distance(balls[i].transform.position, targetPoint);
            }

            //SaveNetwork.SaveNetList(scores.ToArray(), "postRun");

            StartCoroutine(Timer());
        }
    }

    private void OnDestroy()
    {
        if (saveAfterRun)
        {
            //SaveNetwork.SaveNetList(scores.ToArray(), "postRun");
        }
        
    }

    IEnumerator Timer()
    {
        running = true;
        generation++;
        Debug.Log("Begin: Gen " + generation);
        yield return new WaitForSeconds(trainingTime);
        Debug.Log("End");
        running = false;
    }
}
