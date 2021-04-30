﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Trigger_CameraEffect : MonoBehaviour
{
    private CameraController Controller;    //相机控制器的脚本
    [SerializeField] private float t_Effect = 0.8f;     //实现相机效果需要的时间            
    private void Start() 
    {
        Controller = Camera.main.GetComponentInChildren<CameraController>();
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (collision.CompareTag("Player"))
        {
            Controller.StartChangeScale(t_Effect, transform);
        }    
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Controller.FollowPlayer();
        }
    }
}
