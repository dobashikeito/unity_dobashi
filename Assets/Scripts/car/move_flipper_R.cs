using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class move_flipper_R : MonoBehaviour
{   
    public Rigidbody rb1;
    public GameObject top_point_dir;
    public GameObject top_point_origin;    
    private Vector3 dir_vector;
    public GameObject body;
    private move_body move_body;

    // Start is called before the first frame update
    void Start()
    {
        rb1=GetComponent<Rigidbody>();
        move_body=body.GetComponent<move_body>();
        
    }

    // Update is called once per frame
    //  void FixedUpdate()
    // {
    //     Vector3 Rotation=gameObject.transform.localEulerAngles;
    //     float angle=Rotation[1];
    //     // Debug.LogWarning(Rotation[2]);
    //     // Debug.LogWarning(this.transform.localPosition.x);
    //     if(Input.GetKey("up")){
    //         rb1.AddForce(-20f*Mathf.Sin(angle*Mathf.PI/180f), 0f, -20f*Mathf.Cos(angle*Mathf.PI/180f), ForceMode.Acceleration);
    //         // rb1.AddForce(-20f*Mathf.Sin(angle*Mathf.PI/180f), 0f, -20f*Mathf.Cos(angle*Mathf.PI/180f), ForceMode.Force);
    //     }
    //     if(Input.GetKey("down")){
    //         rb1.AddForce(20f*Mathf.Sin(angle*Mathf.PI/180f), 0f, 20f*Mathf.Cos(angle*Mathf.PI/180f), ForceMode.Acceleration);
    //         // rb1.AddForce(-20f*Mathf.Sin(angle*Mathf.PI/180f), 0f, -20f*Mathf.Cos(angle*Mathf.PI/180f), ForceMode.Force);
    //     }
    //     if(Input.GetKey(KeyCode.RightArrow)){
    //         rb1.angularVelocity=new Vector3(0,1f,0);
    //     }
    //     if(Input.GetKey(KeyCode.LeftArrow)){
    //         rb1.angularVelocity=new Vector3(0,-1f,0);
    //     }

    // }
    void OnCollisionStay(Collision other){
        Vector3 Rotation=gameObject.transform.localEulerAngles;
        float angle=Rotation[1];
        Vector3 normal_vec=other.GetContact(0).normal.normalized;
        Vector3 vec=Vector3.zero;

        // Debug.Log(other.GetContact(0).point);
        Vector3 dir=top_point_dir.transform.position;
        Vector3 origin = top_point_origin.transform.position;
        Vector3 houkou=dir-origin;
        GameObject Cube=GameObject.CreatePrimitive(PrimitiveType.Cube);
        Cube.transform.position=other.GetContact(0).point;
        Cube.transform.localScale = new Vector3(1, 1, 1);
        if(normal_vec.y==1){
            houkou.y=0;
        }
        // Debug.Log(houkou.y);
        float senkai_speed=1.0f;
        float move_speed=10.0f;

        if(!move_body.body_touch){
            senkai_speed=5.5f;
            move_speed=30.0f;
        }
        Debug.Log(senkai_speed);
        // Debug.LogWarning(Rotation[2]);
        // Debug.LogWarning(this.transform.localPosition.x);
        if(Input.GetKey("up")){
            // rb1.AddForce(-20f*Mathf.Sin(angle*Mathf.PI/180f), 0f, -20f*Mathf.Cos(angle*Mathf.PI/180f), ForceMode.Acceleration);
            // rb1.AddForce(-20f*Mathf.Sin(angle*Mathf.PI/180f), 0f, -20f*Mathf.Cos(angle*Mathf.PI/180f), ForceMode.Force);
            // rb1.AddForceAtPosition(vec*-20.0f,other.GetContact(0).point,ForceMode.Acceleration);
            // Debug.Log(other.GetContact(0).point);
            rb1.AddForceAtPosition(houkou*move_speed,other.GetContact(0).point,ForceMode.Acceleration);
            // rb1.AddForceAtPosition(houkou*15.0f,gameObject.transform.position,ForceMode.Acceleration);
            // rb1.velocity=houkou*5.0f;
        }
        if(Input.GetKey("down")){
            // /rb1.AddForce(20f*Mathf.Sin(angle*Mathf.PI/180f), 0f, 20f*Mathf.Cos(angle*Mathf.PI/180f), ForceMode.Acceleration);
            // rb1.AddForce(-20f*Mathf.Sin(angle*Mathf.PI/180f), 0f, -20f*Mathf.Cos(angle*Mathf.PI/180f), ForceMode.Force);
            rb1.AddForceAtPosition(houkou*move_speed*-1.0f,other.GetContact(0).point,ForceMode.Acceleration);
            // rb1.AddForceAtPosition(houkou*-15.0f,gameObject.transform.position,ForceMode.Acceleration);
            // rb1.velocity=houkou*-5.0f;
        }
        
        if(Input.GetKey(KeyCode.RightArrow)){
            rb1.AddForceAtPosition(houkou*move_speed*-1.0f,other.GetContact(0).point,ForceMode.Acceleration);
            
        }
        if(Input.GetKey(KeyCode.LeftArrow)){
            rb1.AddForceAtPosition(houkou*move_speed*-1.0f,other.GetContact(0).point,ForceMode.Acceleration);
            
        }

        Destroy(Cube,0.2f);
    }
}

