using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameController;

public class ScoringController : MonoBehaviour
{
    public bool scoringV2 = true;

    public int netCount = 10;
    public int winnerCount = 3;
    public float mutationRate = 0.01f;

    public NetController[] nets;
    public Transform moon;
    public LineRenderer line;

    public float[] scores;

    public float[] lastDistance;

    public float anglePoints = 0.1f;
    public float distancePenalty = 1.1f;
    public float timePenalty = 1f;
    public float winReward = 10000f;
    public float crashPenalty = 10000f;
    public float timeoutPenalty = 10000f;

    public float distanceTolerance = 10f;
    public float distanceReward = 0.5f;
    public float maxDistPenalty = 100f;

    public float moonBonusDist = 500f;

    public bool saveOnClose;
    public bool loadSavedGroup;

    float pointTimer = 1;

    bool[] hasLeftPath;
    bool[] hasMoonBonus;

    bool[] hasReachedGoal;

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
        hasMoonBonus = new bool[netCount];
        hasReachedGoal = new bool[netCount];
        List<NetData> nDataList = new List<NetData>();
        if (loadSavedGroup)
        {
            NetworkList nList = SaveNetwork.LoadNetList("SaveData");
            nDataList = SaveNetwork.ConvertNetListToWinList(nList);
        }

        for (int i = 0; i < netCount; i++)
        {
            lastDistance[i] = Vector3.Distance(nets[i].transform.position, moon.position);
            hasMoonBonus[i] = true;
            if (loadSavedGroup)
            {
                nets[i].Initialize(nDataList[i]);
            }
            else
            {
                nets[i].Initialize();
            }
            
        }
        hasLeftPath = new bool[netCount];

        
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3[] path = new Vector3[line.positionCount];
        line.GetPositions(path);

        for (int i = 0; i < nets.Length; i++)
        {
            if (nets[i].controller.isEnabled)
            {
                if (scoringV2)
                {
                    List<float> distances = new List<float>();

                    float lastDist = Vector3.Distance(nets[i].transform.position, path[0]);
                    for (int j = 0; j < line.positionCount; j++)
                    {
                        float dist = Vector3.Distance(nets[i].transform.position, path[j]);

                        if (lastDist < dist)
                        {
                            break;
                        }
                        distances.Add(dist);

                        
                    }

                    distances.Sort();

                    if (!hasLeftPath[i] && distances[0] < distanceTolerance)
                    {
                        scores[i] += Mathf.Pow(Mathf.Lerp(distanceReward, 0, distances[0] / distanceTolerance), 2);

                        Vector3 moonDir = moon.position - nets[i].transform.position;
                        float angle = Vector3.Angle(nets[i].transform.forward, moonDir);
                        float angleMod = 0;
                        if (angle < 15)
                        {
                            angleMod = anglePoints;
                        }
                        scores[i] += angleMod;
                    }
                    else if (Vector3.Distance(nets[i].transform.position, moon.position) > lastDistance[i])
                    {
                        hasLeftPath[i] = true;
                        hasMoonBonus[i] = false;
                        scores[i] -= 0.05f;
                    }
                    else
                    {
                        hasLeftPath[i] = true;
                        scores[i] -= 0.05f;
                        //scores[i] -= Mathf.Lerp(0, 10, (distances[0] - distanceTolerance) / maxDistPenalty);
                    }

                    if (hasMoonBonus[i] && Vector3.Distance(nets[i].transform.position, moon.position) < moonBonusDist)
                    {
                        float d = Vector3.Distance(nets[i].transform.position, moon.position);
                        scores[i] += Mathf.Lerp(0.5f, 0f, (d / moonBonusDist));
                    }

                    lastDistance[i] = Vector3.Distance(nets[i].transform.position, moon.position);
                }
                else
                {
                    float newDistance = Vector3.Distance(nets[i].transform.position, moon.position);
                    float distanceDiff = lastDistance[i] - newDistance;
                    lastDistance[i] = newDistance;

                    if (distanceDiff <= 0)
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
            if (nets[i].controller.reachedGoal)
            {
                hasReachedGoal[i] = true;
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
            hasLeftPath[i] = false;
            hasMoonBonus[i] = true;
            
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

                Array.Copy(nets[id].net.biases[j], d.biases[j], nets[id].net.nodesPerLayer);
            }

            d.outputWeights = new float[nets[id].net.outputSize][];
            for (int j = 0; j < nets[id].net.outputSize; j++)
            {
                d.outputWeights[j] = new float[nets[id].net.nodesPerLayer];
                Array.Copy(nets[id].net.outputWeights[j], d.outputWeights[j], nets[id].net.nodesPerLayer);
            }

            d.outputBiases = new float[nets[id].net.outputSize];

            Array.Copy(nets[id].net.outputBiases, d.outputBiases, nets[id].net.outputSize);

            d.layers = nets[id].net.layers;
            d.nodesPerLayer = nets[id].net.nodesPerLayer;
            d.outputSize = nets[id].net.outputSize;
            d.inputSize = nets[id].net.inputSize;

            winners.Add(d);
        }
        float fitness = 0;
        float winRate = 0;
        for (int i = 0; i < netCount; i++)
        {
            int r = UnityEngine.Random.Range(0, winnerCount);

            nets[i].MutateNetwork(winners[r], mutationRate);

            lastDistance[i] = Vector3.Distance(nets[i].controller.initialPosition, moon.position);
            scores[i] = 0;

            fitness += fin[i].score;
            if (hasReachedGoal[i])
            {
                winRate++;
            }
            hasReachedGoal[i] = false;
        }
        fitness = fitness / fin.Count;
        winRate = winRate / fin.Count;

        Debug.Log("Average Fitness = " + fitness + ", Win Rate = " + winRate * 100 + "%");
    }

    private void OnDestroy()
    {
        if (saveOnClose)
        {
            SaveNet();
        }
    }

    void SaveNet()
    {
        List<NetData> netList = new List<NetData>();

        //copy network values into a list
        for (int i = 0; i < netCount; i++)
        {
            NetData d = new NetData();

            int id = i;

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

                Array.Copy(nets[id].net.biases[j], d.biases[j], nets[id].net.nodesPerLayer);
            }

            d.outputWeights = new float[nets[id].net.outputSize][];
            for (int j = 0; j < nets[id].net.outputSize; j++)
            {
                d.outputWeights[j] = new float[nets[id].net.nodesPerLayer];
                Array.Copy(nets[id].net.outputWeights[j], d.outputWeights[j], nets[id].net.nodesPerLayer);
            }

            d.outputBiases = new float[nets[id].net.outputSize];

            Array.Copy(nets[id].net.outputBiases, d.outputBiases, nets[id].net.outputSize);

            d.layers = nets[id].net.layers;
            d.nodesPerLayer = nets[id].net.nodesPerLayer;
            d.outputSize = nets[id].net.outputSize;
            d.inputSize = nets[id].net.inputSize;

            netList.Add(d);
        }

        SaveNetwork.SaveNetList(netList.ToArray(), "SaveData");
    }
}
