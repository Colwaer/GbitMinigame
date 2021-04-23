﻿using UnityEngine;
using Public;
using System.Collections;
using System;

public class CPlayer : MonoBehaviour,IDamagable_Friendly
{
    internal float Speed { get; private set; }
    internal float DashSpeed { get; private set; } 
   
    internal float JumpHeight { get; private set; }

    private float MaxFallSpeed;
    private float MaxRiseSpeed;

    private LayerMask GroundLayer;
    internal Rigidbody2D m_RigidBody;
    internal bool b_OnGround;
    internal bool b_IsMoving;
    internal bool b_isDashing;
    internal float m_DesiredDirection;      //移动方向
    private float m_RaycastLength = 1.2f;
    
    [SerializeField] 
    private int frame_Accelerate;           //地面上加速需要的固定帧帧数
    [SerializeField] 
    private int frame_SlowDown;             //地面上减速需要的固定帧帧数

    internal int MaxShootCount;           //最大射击次数

    [SerializeField]
    internal int _ShootCount;
    internal int ShootCount                 //可用射击次数
    { 
        get
        {
            return _ShootCount;
        }
        private set
        {
            if (value > MaxShootCount) return;
            CEventSystem.Instance.ShootCountChanged?.Invoke(value);
            _ShootCount = value;
        }
    }       

    private float t_Shoot;                  //射击冷却
    private bool b_CanShoot;                //射击冷却完毕

    private float t_Dash;                   //冲刺时间,应当小于射击冷却
    private IEnumerator ie_Dash;            //冲刺协程

    private Vector2 m_Velocity_LastFrame;   //上一固定帧中的速度
    private GameObject LastCloud;           //上一朵碰撞的云

    private void Awake()
    {
        Initialize();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - m_RaycastLength, 0));
    }

    public void Initialize()
    {
        frame_Accelerate = 20;
        frame_SlowDown = 10;
        MaxShootCount = 4;
        ShootCount = 0;
        b_CanShoot = true;
        t_Shoot = 0.3f;
        Speed = 10f;
        DashSpeed = 30f;
        MaxFallSpeed = 10f;
        MaxRiseSpeed = 20f;
        JumpHeight = 3f;
        t_Dash = 0.15f;
        m_RigidBody = GetComponent<Rigidbody2D>();
        GroundLayer = LayerMask.GetMask("Ground");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (b_isDashing && ie_Dash != null)
        {
            StopCoroutine(ie_Dash);
            b_isDashing = false;
            m_RigidBody.gravityScale = 1;
        }

        /*
        float normalSpeed;  //碰前法向速度大小
        Vector2 deltaVelocity;  //
        Debug.Log($"撞击时法向速度大小为{normalSpeed}");
        */

        //不能通过连续撞击同一朵云来获得云，只要接触了另一朵云，就解除这个限制
        if (LastCloud == null || collision.gameObject != LastCloud)
        {
            if (m_Velocity_LastFrame.magnitude > 15f)
            {
                ShootCount += 2;
                LastCloud = collision.gameObject;
                //不能通过向下冲锋获得云
                if (m_Velocity_LastFrame.y < -15f)
                    ShootCount --;
            }
            else
                LastCloud = null;
        }
    }

    public void PhysicsCheck()
    {
        m_Velocity_LastFrame = m_RigidBody.velocity;
        b_IsMoving = m_RigidBody.velocity.magnitude > 0.1f;
        b_OnGround = Physics2D.Raycast(transform.position, new Vector2(0, -1), m_RaycastLength, GroundLayer);
        if (b_OnGround && ShootCount <= 2) ShootCount = 2;
    }

    // 地面跑动：采用惯性跑动，有加速与减速过程，但是不需要太明显弄得地面太滑；存在速度阈值，
    // 加速到阈值速度的时间大概在0.4s左右。减速的需求：减速从阈值到0速的时间为0.2s左右，只要
    // 玩家停止原运动方向的输入即判定进入减速阶段，减速过程位移与时间固定，玩家选择反向输入或
    // 停止输入不影响减速过程。但是玩家若在减速过程中选择原方向输入，那就以当前的速度进入到加速过程。
    public void Move()
    {
        if (b_isDashing) 
            return;

        float v_x = Mathf.Abs(m_RigidBody.velocity.x);
        float v_y = Mathf.Abs(m_RigidBody.velocity.y);
        float sgn_x = Mathf.Sign(m_RigidBody.velocity.x);
        float sgn_y = Mathf.Sign(m_RigidBody.velocity.y);
        if (v_x < 0.01f) sgn_x = m_DesiredDirection;
        if (v_y < 0.01f) sgn_y = -1;
        if (b_OnGround)
        {
            m_RigidBody.velocity = new Vector2(sgn_x * Mathf.Min(Speed, v_x + Speed / frame_Accelerate * m_DesiredDirection * sgn_x), m_RigidBody.velocity.y);
            //地面上的水平速度不能超过Speed
            if (v_x > Speed)
                m_RigidBody.velocity = new Vector2(sgn_x * Speed, m_RigidBody.velocity.y);
            if (m_DesiredDirection * sgn_x <= 0)
                m_RigidBody.velocity = new Vector2(sgn_x * Mathf.Max(0, v_x - Speed / frame_SlowDown), m_RigidBody.velocity.y);
        }
        else
        {
            if (sgn_y < 0 && v_y > MaxFallSpeed)
                m_RigidBody.velocity = new Vector2(m_RigidBody.velocity.x, -MaxFallSpeed);
            else if (sgn_y > 0 && v_y > MaxRiseSpeed)
                m_RigidBody.velocity = new Vector2(m_RigidBody.velocity.x, MaxRiseSpeed);
            //空中水平速度超过Speed则不能再加速（但可以减速）
            if (v_x < Speed || sgn_x * m_DesiredDirection < 0)
                m_RigidBody.velocity = new Vector2(sgn_x * Mathf.Min(Speed, v_x + Speed / frame_Accelerate * m_DesiredDirection * sgn_x), m_RigidBody.velocity.y);
        }
    }

    public void Jump()
    {
        if (b_OnGround)
            m_RigidBody.velocity =  new Vector2(m_RigidBody.velocity.x, Mathf.Sqrt(JumpHeight * -Physics2D.gravity.y * 2));
    }

    public void Shoot(Vector2 direction)
    {
        ShootCount--;
        if (!b_CanShoot)
            return;
        ie_Dash = Dash(direction);
        StartCoroutine(ie_Dash);
        StartCoroutine(ShootCoolDown());
    }

    private IEnumerator ShootCoolDown()
    {
        b_CanShoot = false;
        yield return new WaitForSeconds(t_Shoot);
        b_CanShoot = true;
    }
    //冲刺时不能主动移动，也不受重力影响
    public IEnumerator Dash(Vector2 direction)
    {
        b_isDashing = true;
        m_RigidBody.gravityScale = 0;
        m_RigidBody.velocity = direction * DashSpeed;
        yield return CTool.Wait(t_Dash);
        //减速过程
        for(;m_RigidBody.velocity.magnitude>Speed ; )
        {
            m_RigidBody.velocity -= m_RigidBody.velocity.normalized* 2f;
            yield return new WaitForFixedUpdate();
        }
        b_isDashing = false;
        m_RigidBody.gravityScale = 1;
    }

    public void Die()
    {
        CEventSystem.Instance.PlayerDie?.Invoke();
    }

}
