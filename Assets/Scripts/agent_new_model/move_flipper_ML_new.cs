//mlagantで使う
//フリッパの前進、後退、旋回は無し
//新しいモデルのパラメータを適応11/10
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class move_flipper_ML_new : MonoBehaviour
{   
    public Rigidbody rb1;
    public Rigidbody rb2;
    public GameObject top_point_dir;
    public GameObject top_point_origin;
    public float angle;
    private Vector3 dir_vector;
    public GameObject body;
    private move_body_ML_new move_body_ml;
    public float action_Go;
    public float action_rotate;
    // private float max_speed=1.0f;
    // private float angle = 0;
    // Start is called before the first frame update
    void Start()
    {
        rb1=GetComponent<Rigidbody>();
        rb2=body.GetComponent<Rigidbody>();
        move_body_ml=body.GetComponent<move_body_ML_new>();
        
    }

    // Update is called once per frame
    // void FixedUpdate()
    // {
    //     Vector3 Rotation=gameObject.transform.localEulerAngles;
    //     angle=Rotation[1];
    //     float Go = action_Go;//Goを-1~1にするため5で割る
    //     rb1.AddForce(-20f*Mathf.Sin(angle*Mathf.PI/180f)*Go, 0f, -20f*Mathf.Cos(angle*Mathf.PI/180f)*Go ,ForceMode.Acceleration);
    //     rb1.angularVelocity=new Vector3(0,action_rotate,0);
    //     // if(action_Go==0){
    //     //     rb1.velocity=Vector3.zero;
    //     // }

        //操作
        // Vector3 Rotation=gameObject.transform.localEulerAngles;
        // angle=Rotation[2];
        // Debug.LogWarning(Rotation[2]);
        // if(Input.GetKey("up")){
        //     rb1.AddForce(-20f*Mathf.Sin(angle*Mathf.PI/180f), 0f, -20f*Mathf.Cos(angle*Mathf.PI/180f), ForceMode.Acceleration);
        // }
        // if(Input.GetKey("down")){
        //     rb1.AddForce(20f*Mathf.Sin(angle*Mathf.PI/180f),0f,20f*Mathf.Cos(angle*Mathf.PI/180f),ForceMode.Acceleration);
        // }
     void OnCollisionStay(Collision other){

        Vector3 normal_vec=other.GetContact(0).normal.normalized;

        // Debug.Log(other.GetContact(0).point);
        Vector3 dir=top_point_dir.transform.position;
        Vector3 origin = top_point_origin.transform.position;
        Vector3 houkou=(dir-origin).normalized;

        if(normal_vec.y==1){
            houkou.y=0;
        }
        // Debug.Log(houkou.y);
        float senkai_speed=5.0f;
        float move_speed=15.0f;

        if(!move_body_ml.body_touch){
            senkai_speed=30.0f;
            move_speed=150.0f;
        }
        // Debug.LogWarning(senkai_speed);

        if(rb2.linearVelocity.magnitude<=move_body_ml.max_speed){
            // rb1.AddForceAtPosition(houkou*move_speed*action_Go,other.GetContact(0).point,ForceMode.Acceleration);
            rb1.AddForceAtPosition(houkou*move_speed*action_Go*(move_body_ml.max_speed-Mathf.Abs(rb2.linearVelocity.magnitude))/move_body_ml.max_speed,other.GetContact(0).point,ForceMode.Acceleration);
        }

        rb1.angularVelocity=new Vector3(0,senkai_speed*action_rotate,0);
        if(action_Go==0){
            rb1.linearVelocity=new Vector3(0,0,0);
        }
        if(action_rotate==0){
            rb1.angularVelocity=new Vector3(0,0,0);
        }

    }
}