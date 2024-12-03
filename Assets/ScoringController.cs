using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameController;

public class ScoringController : MonoBehaviour
{
    public int netCount = 10;
    public int winnerCount = 3;
    public float mutationRate = 0.01f;

    public NetController[] nets;
    public Transform moon;

    public float[] scores;

    public float[] lastDistance;

    public float anglePoints = 0.1f;
    public float distancePenalty = 1.1f;
    public float timePenalty = 1f;
    public float winReward = 10000f;
    public float crashPenalty = 10000f;
    public float timeoutPenalty = 10000f;

    float pointTimer = 1;

    struct FinalScores
    {
        public float score;
        public int index;
    }

    public struct NetData
    {
        public float[][][] weights;
        public float[][] biases;
        public float[] outputBiases;
        public float[][] outputWeights;

        public int layers;
        public int nodesPerLayer;
        public int inputSize;
        public int outputSize;
    }


    private void OnEnable()
    {
        OnResetLevelEvent += FinishScoring;
    }

    private void OnDisable()
    {
        OnResetLevelEvent -= FinishScoring;
    }

    // Start is called before the first frame update
    void Start()
    {
        scores = new float[netCount];
        lastDistance = new float[netCount];

        for (int i = 0; i < netCount; i++)
        {
            lastDistance[i] = Vector3.Distance(nets[i].transform.position, moon.position);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for (int i = 0; i < nets.Length; i++)
        {
            if (nets[i].controller.isEnabled)
            {
                float newDistance = Vector3.Distance(nets[i].transform.position, moon.position);
                float distanceDiff = lastDistance[i] - newDistance;
                lastDistance[i] = newDistance;

                if ( distanceDiff <= 0)
                {
                    distanceDiff *= distancePenalty;
                }

                Vector3 moonDir = moon.position - nets[i].transform.position;
                float angle = Vector3.Angle(nets[i].transform.forward, moonDir);
                float angleMod = 0;
                if (angle < 15 && nets[i].controller.throttle != 0)
                {
                    angleMod = anglePoints;
                }

                float timeMod = 0;
                if (pointTimer <= 0)
                {
                    timeMod = timePenalty;
                    pointTimer = 1f;
                }
                else
                {
                    pointTimer -= Time.fixedDeltaTime;
                }

                scores[i] += distanceDiff + angleMod - timeMod;
            }
            
        }
    }

    void FinishScoring(object sender, EventArgs args)
    {
        List<FinalScores> fin = new List<FinalScores>(netCount);

        //apply final scores and add to a list
        for (int i = 0; i < netCount; i++)
        {
            //Debug.Log(scores[i]);
            if (nets[i].controller.reachedGoal)
            {
                nets[i].controller.reachedGoal = false;
                scores[i] += winReward;
            }
            else if (nets[i].controller.shipCrashed)
            {
                nets[i].controller.shipCrashed = false;
                scores[i] += -crashPenalty;
                //Debug.Log(scores[i]);
            }
            else
            {
                nets[i].controller.shipCrashed = false;
                scores[i] += -timeoutPenalty;
            }
            FinalScores f = new FinalScores();

            f.score = scores[i];
            f.index = i;

            fin.Add(f);
            scores[i] = 0;
        }


        //sort the scores in decending order
        fin.Sort((s1, s2) => s1.score.CompareTo(s2.score));
        fin.Reverse();

        List<NetData> winners = new List<NetData>();

        //copy network values into a list
        for (int i = 0; i < winnerCount; i++)
        {
            NetData d = new NetData();

            int id = fin[i].index;

            d.weights = new float[nets[id].net.layers][][];
            d.biases = new float[nets[id].net.layers][];

            for (int j = 0; j < nets[id].net.layers; j++)
            {
                d.weights[j] = new float[nets[id].net.nodesPerLayer][];
                for (int k = 0; k < nets[id].net.nodesPerLayer; k++)
                {
                    if (j == 0)
                    {
                        d.weights[j][k] = new float[nets[id].net.inputSize];
                        Array.Copy(nets[id].net.weights[j][k], d.weights[j][k], nets[id].net.inputSize);
                    }
                    else
                    {
                        d.weights[j][k] = new float[nets[id].net.nodesPerLayer];
                        Array.Copy(nets[id].net.weights[j][k], d.weights[j][k], nets[id].net.nodesPerLayer);
                    }
                }
                d.biases[j] = new float[nets[id].net.nodesPerLayer];

                Array.Copy(nets[id].net.biases[j], d.biases[j], nets[id].net.layers);
            }

            d.outputWeights = new float[nets[id].net.outputSize][];
            for (int j = 0; j < nets[id].net.outputSize; j++)
            {
                d.outputWeights[j] = new float[nets[id].net.nodesPerLayer];
                Array.Copy(nets[id].net.outputWeights[j], d.outputWeights[j], nets[id].net.outputSize);
            }

            d.outputBiases = new float[nets[id].net.outputSize];

            Array.Copy(nets[id].net.outputBiases, d.outputBiases, nets[id].net.outputSize);

            d.layers = nets[id].net.layers;
            d.nodesPerLayer = nets[id].net.nodesPerLayer;
            d.outputSize = nets[id].net.outputSize;
            d.inputSize = nets[id].net.inputSize;

            winners.Add(d);
        }

        for (int i = 0; i < netCount; i++)
        {
            int r = UnityEngine.Random.Range(0, winnerCount);

            nets[i].MutateNetwork(winners[r], mutationRate);

            lastDistance[i] = Vector3.Distance(nets[i].controller.initialPosition, moon.position);
            scores[i] = 0;
        }
    }
}
