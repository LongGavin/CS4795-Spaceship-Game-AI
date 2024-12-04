using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using static ScoringController;

public static class SaveNetwork 
{
    public static void SaveNetList(NetData[] net, string fileName)
    {
        NetworkList networkList = new NetworkList();
        networkList.nets = new Network[net.Length];

        for (int n = 0; n < net.Length; n++)
        {
            Network s = new Network();

            s.outputBiases = net[n].outputBiases;

            //Debug.Log(net[n].outputWeights[0][7]);

            s.outputWeights = new Node[net[n].outputWeights.Length];
            s.biasLayers = new BiasLayer[net[n].biases.Length];
            s.weightLayers = new Layer[net[n].weights.Length];

            for (int i = 0; i < net[n].outputWeights.Length; i++)
            {
                s.outputWeights[i] = new Node();
                s.outputWeights[i].weights = new float[net[n].weights[0].Length];

                Array.Copy(net[n].outputWeights[i], s.outputWeights[i].weights, net[n].weights[i].Length);
            }

            for (int i = 0; i < net[n].biases.Length; i++)
            {
                s.biasLayers[i] = new BiasLayer();
                s.biasLayers[i].biases = new float[net[n].biases[0].Length];

                Array.Copy(net[n].biases[i], s.biasLayers[i].biases, net[n].biases[0].Length);
            }

            for (int i = 0; i < net[n].weights.Length; i++)
            {
                s.weightLayers[i] = new Layer();
                s.weightLayers[i].nodes = new Node[net[n].weights[i].Length];
                for (int j = 0; j < net[n].weights[i].Length; j++)
                {
                    if (i == 0)
                    {
                        s.weightLayers[i].nodes[j] = new Node();
                        s.weightLayers[i].nodes[j].weights = new float[net[n].weights[i][j].Length];
                        Array.Copy(net[n].weights[i][j], s.weightLayers[i].nodes[j].weights, net[n].weights[i][j].Length);
                    }
                    else
                    {
                        s.weightLayers[i].nodes[j] = new Node();
                        s.weightLayers[i].nodes[j].weights = new float[net[n].weights[i][j].Length];
                        Array.Copy(net[n].weights[i][j], s.weightLayers[i].nodes[j].weights, net[n].weights[i][j].Length);
                    }
                    //Debug.Log(net[n].weights[i][j].Length);
                }
            }
            //Debug.Log(net[n].weights[1][1][14] + " : " + s.weightLayers[1].nodes[1].weights[14]);
            networkList.nets[n] = s;

        }

        string save = JsonUtility.ToJson(networkList);

        FileStream fStream = new FileStream("E:/Github/CS4795-Spaceship-Game-AI/Assets/Resources/" + fileName + ".json", FileMode.Create);

        using (StreamWriter writer = new StreamWriter(fStream))
        {
            writer.Write(save);
        }
        Debug.Log("E:/Github/CS4795-Spaceship-Game-AI/Assets/Resources/" + fileName + ".json");
    }

    public static NetworkList LoadNetList(string filename)
    {
        NetworkList data;

        TextAsset t = Resources.Load<TextAsset>(filename);
        

        data = JsonUtility.FromJson<NetworkList>(t.text);
        return data;
    }

    public static List<NetData> ConvertNetListToWinList(NetworkList netList)
    {
        List<NetData> winlist = new List<NetData>();

        for (int i = 0; i < netList.nets.Length; i++)
        {
            NetData w = new NetData();

            w.weights = new float[netList.nets[i].weightLayers.Length][][];
            w.biases = new float[netList.nets[i].biasLayers.Length][];
            w.outputWeights = new float[netList.nets[i].outputWeights.Length][];
            w.outputBiases = new float[netList.nets[i].outputBiases.Length];

            w.inputSize = netList.nets[i].weightLayers[0].nodes[0].weights.Length;
            w.layers = netList.nets[i].weightLayers.Length;
            w.nodesPerLayer = netList.nets[i].weightLayers[1].nodes.Length;
            w.outputSize = netList.nets[i].outputBiases.Length;



            for (int j = 0; j < netList.nets[i].weightLayers.Length; j++)
            {
                w.weights[j] = new float[netList.nets[i].weightLayers[j].nodes.Length][];

                for (int k = 0; k < netList.nets[i].weightLayers[j].nodes.Length; k++)
                {
                    w.weights[j][k] = new float[netList.nets[i].weightLayers[j].nodes[k].weights.Length];

                    Array.Copy(netList.nets[i].weightLayers[j].nodes[k].weights, w.weights[j][k], w.weights[j][k].Length);
                }

                w.biases[j] = new float[netList.nets[i].biasLayers[j].biases.Length];
                Array.Copy(netList.nets[i].biasLayers[j].biases, w.biases[j], w.biases[j].Length);
            }

            for (int j = 0; j < netList.nets[i].outputWeights.Length; j++)
            {
                w.outputWeights[j] = new float[netList.nets[i].outputWeights[j].weights.Length];
                Array.Copy(netList.nets[i].outputWeights[j].weights, w.outputWeights[j], w.outputWeights[j].Length);
            }

            Array.Copy(netList.nets[i].outputBiases, w.outputBiases, w.outputBiases.Length);

            winlist.Add(w);
        }

        return winlist;
    }
}
