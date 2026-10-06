//hingejointを扱う
//RearFlipperRight用
//mlagentsで用いる
//フリッパをカプセルでやるとき用
//min maxが違うため
//hingeLFにスクリプトのコメントを記述
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class hingeRF : MonoBehaviour
{   
    public GameObject hps_obj;
    private hinge_power_select hps;
    public Rigidbody rb1;
    private Vector3 dir_vector;
    move_body angle_get;
    public float angle=0;
    public float action_RearR;
    private float maxdeg = 1;
    private float mindeg = -165;
    private float targetdeg = 0;
    private float addDeg = 15.0f;
    private float flipper_speed = 100;
    HingeJoint joint;
    JointSpring hingeSpring;
    JointLimits limits;

    void Start()
    {
        hps=hps_obj.GetComponent<hinge_power_select>();
        joint = GetComponent<HingeJoint>();
        hingeSpring = joint.spring;
        hingeSpring.spring = 100;  //力の大きさ
        hingeSpring.damper = 1;  // 大きいほど速度が落ちる（摩擦？））
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
        hingeSpring.spring = hps.hinge_spring;  //力の大きさ
        hingeSpring.damper = hps.hinge_damper;  // 大きいほど速度が落ちる（摩擦？））
        if(Input.GetKey(KeyCode.W)){
            // joint.useLimits=false;
            angle+=addDeg;
            if(angle>maxdeg){angle=maxdeg;}
        }
        if(Input.GetKey(KeyCode.R)){
            // joint.useLimits=false;
            angle-=addDeg;
            if(angle<mindeg){angle=mindeg;}
            
        }
        hingeSpring.targetPosition = angle;
        joint.spring=hingeSpring;
        joint.useSpring=true;

        // if(Mathf.Abs(joint.angle-angle)<2){
        //     limits=joint.limits;
        //     limits.min=angle-1;
        //     limits.max=angle+1;
        //     limits.bounceMinVelocity = 0;
        //     joint.limits=limits;
        //     joint.useLimits=true;
        // }
        
        // HingeJoint hinge = GetComponent<HingeJoint>();
        // // JointSpring spring = hinge.spring;
        // JointLimits limits = hinge.limits;
        // // limits.min = 90;
        // limits.bounciness = 0;//ジョイントが限界に達したときに跳ね返るサイズを決定
        // // limits.max = -90;
        // limits.bounceMinVelocity = 0;//跳ね返るときにジョイントが発生する最小速度
        // // hinge.limits = limits;//限界値を設定
        // hinge.useLimits = true;//ジョイントの制限を有効にする
        // // Make the hinge motor rotate with 90 degrees per second and a strong force.
        // var motor = hinge.motor;
        // motor.force=1000;
        // motor.targetVelocity=-100*action_RearR;
        // motor.freeSpin=false;
        // hinge.motor=motor;
        // hinge.useMotor=true;
        // angle=angle-action_RearR*addDeg;
        // if(angle>maxdeg){
        //     angle=maxdeg;
        // }
        // if(angle<mindeg){
        //     angle=mindeg;
        // }
        // limits.min=angle-1;
        // limits.max=angle+1;
        // hinge.limits=limits;

        // if(action_RearR==1){
        //     targetdeg+=addDeg;
        //     motor.targetVelocity=flipper_speed;
        // }
        // if(action_RearR==-1){
        //     targetdeg-=addDeg;
        //     motor.targetVelocity=-1.0f*flipper_speed;
        // }
        // if(targetdeg>maxdeg){
        //     targetdeg=maxdeg;
        // }
        // if(targetdeg<mindeg){
        //     targetdeg=mindeg;
        // }
        // spring.targetPosition=targetdeg;

        // if(hinge.useMotor){
        //     hinge.motor = motor;
        // }
        // if(hinge.useSpring){
        //     hinge.spring = spring;
        // }

        // angle_get = GameObject.Find("body").GetComponent<move_body>();
        // if(Input.GetKey("up")){
        //     rb1.AddForce(-10f*Mathf.Sin(angle_get.angle*Mathf.PI/180f), 0f, -10f*Mathf.Cos(angle_get.angle*Mathf.PI/180f), ForceMode.Acceleration);
        // }
        // if(Input.GetKey("down")){
        //     rb1.AddForce(10f*Mathf.Sin(angle_get.angle*Mathf.PI/180f),0f,10f*Mathf.Cos(angle_get.angle*Mathf.PI/180f),ForceMode.Acceleration);
        // }

        // HingeJoint hinge = GetComponent<HingeJoint>();
        // JointSpring spring = hinge.spring;
        // JointLimits limits = hinge.limits;
        // // limits.min = 90;
        // limits.bounciness = 0;//ジョイントが限界に達したときに跳ね返るサイズを決定
        // // limits.max = -90;
        // limits.bounceMinVelocity = 0;//跳ね返るときにジョイントが発生する最小速度
        // // hinge.limits = limits;//限界値を設定
        // hinge.useLimits = true;//ジョイントの制限を有効にする
        // // Make the hinge motor rotate with 90 degrees per second and a strong force.
        // var motor = hinge.motor;
        // if(Input.GetKey(KeyCode.Q)){
            
        //     motor.force = 1000;
        //     motor.targetVelocity = -100;
        //     motor.freeSpin = false;
        //     hinge.motor = motor;
        //     hinge.useMotor = true;
        //     angle++;
        //     if(angle>90){
        //         angle=90;
        //     }
        //     limits.min = angle;
        //     limits.max = angle-1;
        //     hinge.limits = limits;
        // }
        // if(Input.GetKey(KeyCode.W)){
        //     motor.force = 1000;
        //     motor.targetVelocity = 100;
        //     motor.freeSpin = false;
        //     hinge.motor = motor;
        //     hinge.useMotor = true;
        //     angle--;
        //     if(angle<-90){
        //         angle=-90;
        //     }
        //     limits.min = angle;
        //     limits.max = angle+1;
        //     hinge.limits = limits;
        // }

    }
}

