//hingejointを扱う
//FlontFlipperLeft用
//mlagentsで用いる
//フリッパをカプセルでやるとき用
//min maxが違うため
//新しいモデルのパラメータを適応11/10
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class flipperLF_ML_new : MonoBehaviour
{   
    public Rigidbody rb1;
    private Vector3 dir_vector;
    move_body angle_get;
    public float angle=0;
    public float action_FrontL;
    private float maxdeg = 1;
    private float mindeg = -165;
    private float targetdeg = 0;
    public float addDeg = 5.0f;
    // private float addDeg = 1.5f;
    private float flipper_speed = 100;
    HingeJoint joint;
    JointSpring hingeSpring;
    JointLimits limits;

    void Start()
    {
        joint = GetComponent<HingeJoint>();
        hingeSpring = joint.spring;
        hingeSpring.spring = 400;  //力の大きさ
        hingeSpring.damper = 50;  // 大きいほど速度が落ちる（摩擦？）
        hingeSpring.targetPosition = 0;  // 目的の角度
        joint.spring = hingeSpring;  // 上記三つをjointに渡す
        joint.useSpring = true;

        limits=joint.limits;
        limits.min=mindeg;
        limits.max=maxdeg;
        limits.bounceMinVelocity = 0;
        joint.limits=limits;
        joint.useLimits=true;

    }

    // Update is called once per frame
    void Update(){
        // angle+=addDeg*action_FrontL;
        if(action_FrontL>0){
            angle+=addDeg;
            if(angle>maxdeg){angle=maxdeg;}
        }
        else if(action_FrontL<0){
            angle-=addDeg;
            if(angle<mindeg){angle=mindeg;}
        }
        hingeSpring.targetPosition = angle;
        joint.spring=hingeSpring;
        joint.useSpring=true;

    }
}

