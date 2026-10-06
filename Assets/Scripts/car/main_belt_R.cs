using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class main_belt_R : MonoBehaviour
{   
    public Rigidbody rb1;
    public static move_body script_mb;
    public GameObject top_point_dir;
    public GameObject top_point_origin;
    private Vector3 dir_vector;
    // private float angle = 0;
    // Start is called before the first frame update
    void Start()
    {
        rb1=GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    // void FixedUpdate()
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
        // Vector3 normal_vec=other.GetContact(0).normal.normalized;
        // Vector3 vec=Vector3.zero;
        // vec[0]=normal_vec[0];
        // vec[1]=normal_vec[2];
        // vec[2]=normal_vec[1];
        // Debug.Log(other.GetContact(0).point);
        Vector3 dir=top_point_dir.transform.position;
        Vector3 origin = top_point_origin.transform.position;
        Vector3 houkou=dir-origin;
        // Debug.LogWarning(Rotation[2]);
        // Debug.LogWarning(this.transform.localPosition.x);
        if(Input.GetKey("up")){
            // rb1.AddForce(-20f*Mathf.Sin(angle*Mathf.PI/180f), 0f, -20f*Mathf.Cos(angle*Mathf.PI/180f), ForceMode.Acceleration);
            // rb1.AddForce(-20f*Mathf.Sin(angle*Mathf.PI/180f), 0f, -20f*Mathf.Cos(angle*Mathf.PI/180f), ForceMode.Force);
            // rb1.AddForceAtPosition(vec*-20.0f,other.GetContact(0).point,ForceMode.Acceleration);
            // rb1.AddForceAtPosition(houkou*10.0f,other.GetContact(0).point,ForceMode.Acceleration);
            rb1.linearVelocity=new Vector3(100,0,0);
        }
        if(Input.GetKey("down")){
            // rb1.AddForce(20f*Mathf.Sin(angle*Mathf.PI/180f), 0f, 20f*Mathf.Cos(angle*Mathf.PI/180f), ForceMode.Acceleration);
            // rb1.AddForce(-20f*Mathf.Sin(angle*Mathf.PI/180f), 0f, -20f*Mathf.Cos(angle*Mathf.PI/180f), ForceMode.Force);
            rb1.AddForceAtPosition(houkou*-10.0f,other.GetContact(0).point,ForceMode.Acceleration);
        }
        if(Input.GetKey(KeyCode.RightArrow)){
            rb1.AddForceAtPosition(houkou*-10.0f,other.GetContact(0).point,ForceMode.Acceleration);
        }
        if(Input.GetKey(KeyCode.LeftArrow)){
            rb1.AddForceAtPosition(houkou*10.0f,other.GetContact(0).point,ForceMode.Acceleration);
        }
    }
}
