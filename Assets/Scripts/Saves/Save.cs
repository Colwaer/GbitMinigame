﻿using System.Collections;
using System;
using UnityEngine;
using System.IO;


[CreateAssetMenu]
public class Save : ScriptableObject
{
    public int SceneIndex;
    public int CheckPointIndex;
    public int Point;
    public const int STAR_NUM = 9;
    public bool[] Stars_Destroyed = new bool[STAR_NUM];    //true表示被摧毁了  

    public void SaveData(string savePath)
    {
        SceneIndex = GameManager.Instance.SceneIndex;
        CheckPointIndex = GameManager.Instance.ActiveCheckpointIndex;
        Point = GameManager.Instance.Point;
        using (BinaryWriter writer = new BinaryWriter(File.Open(savePath, FileMode.Create)))
        {
            writer.Write(SceneIndex);
            writer.Write(CheckPointIndex);
            writer.Write(Point);
            for (int i = 0; i < STAR_NUM; i++)
            {
                writer.Write(Stars_Destroyed[i]);
            }   
        }
    }
    
    public void LoadData(string savePath)
    {
        using (BinaryReader reader = new BinaryReader(File.Open(savePath, FileMode.Open)))
        {
            SceneIndex = reader.ReadInt32();
            CheckPointIndex = reader.ReadInt32();
            Point = reader.ReadInt32();
            for (int i = 0; i < STAR_NUM; i++)
            {
                Stars_Destroyed[i] = reader.ReadBoolean();
            }
        }
    }

    public void ResetGame()
    {
        Debug.LogWarning("游戏存档被重置");
        Array.Clear(Stars_Destroyed, 0, Stars_Destroyed.Length);
        SceneIndex = 0;
        CheckPointIndex = 0;
        Point = 0;
    }
}
