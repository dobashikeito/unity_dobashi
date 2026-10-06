//hingejointを扱う
//RearFlipper用
//mlagentsで用いる
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class flipper_R_ML : MonoBehaviour
{
    public Rigidbody rb1;
    private Vector3 dir_vector;
    move_body angle_get;
    private float angle=0;
    public float action_Rear;
    private float maxdeg = 75;
    private float mindeg = -75;
    private float targetdeg = 0;
    private float addDeg = 10.0f;
    private float flipper_speed = 100;
    HingeJoint hinge;

    void Start()
    {
        rb1=GetComponent<Rigidbody>();
        hinge = GetComponent<HingeJoint>();
        JointLimits limits = hinge.limits;
        limits.min = mindeg;
        limits.max = maxdeg;
        hinge.limits = limits;


        
    }

    void Update(){
        
        HingeJoint hinge = GetComponent<HingeJoint>();
        JointSpring spring = hinge.spring;
        JointLimits limits = hinge.limits;
        // limits.min = 90;
        limits.bounciness = 0;//ジョイントが限界に達したときに跳ね返るサイズを決定
        // limits.max = -90;
        limits.bounceMinVelocity = 0;//跳ね返るときにジョイントが発生する最小速度
        // hinge.limits = limits;//限界値を設定
        hinge.useLimits = true;//ジョイントの制限を有効にする
        // Make the hinge motor rotate with 90 degrees per second and a strong force.
        var motor = hinge.motor;
        if(action_Rear==1){
            targetdeg+=addDeg;
            motor.targetVelocity=flipper_speed;
        }
        if(action_Rear==-1){
            targetdeg-=addDeg;
            motor.targetVelocity=-1.0f*flipper_speed;
        }
        if(targetdeg>maxdeg){
            targetdeg=maxdeg;
        }
        if(targetdeg<mindeg){
            targetdeg=mindeg;
        }
        spring.targetPosition=targetdeg;

        if(hinge.useMotor){
            hinge.motor = motor;
        }
        if(hinge.useSpring){
            hinge.spring = spring;
        }

        // if(Input.GetKey(KeyCode.A)){
            
        //     motor.force = 1000;
        //     motor.targetVelocity = 100;
        //     motor.freeSpin = false;
        //     hinge.motor = motor;
        //     hinge.useMotor = true;
        //     angle++;    
        //     if(angle>90){
        //         angle=90;
        //     }        
        //     limits.max = angle;
        //     limits.min = angle-1;
        //     hinge.limits = limits;
        // }
        // if(Input.GetKey(KeyCode.S)){

        //     motor.force = 1000;
        //     motor.targetVelocity = -100;
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