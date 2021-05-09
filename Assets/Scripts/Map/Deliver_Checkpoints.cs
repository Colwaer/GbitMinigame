﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deliver_Checkpoints : MonoBehaviour
{
    private void Start() 
    {
        GameManager.Instance.Checkpoints = GetComponentsInChildren<CCheckpoint>();
        GameManager.Instance.Checkpoints[0].Spawn();
    }
}
