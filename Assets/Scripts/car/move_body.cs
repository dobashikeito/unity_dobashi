using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class move_body : MonoBehaviour
{   
    public GameObject hps_obj;
    private hinge_power_select hps;
    public Rigidbody rb1;
    public static move_body script_mb;
    public GameObject top_point_dir;
    public GameObject top_point_origin;
    private Vector3 dir_vector;
    private Vector3 other_past=Vector3.zero;
    [SerializeField]
    public bool body_touch=false;
    // private float angle = 0;
    // Start is called before the first frame update
    void Start()
    {
        hps=hps_obj.GetComponent<hinge_power_select>();
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
    void OnCollisionExit(Collision other)
    {
        body_touch=false;

    }

    void OnCollisionStay(Collision other){
        body_touch=true;
        Vector3 Rotation=gameObject.transform.localEulerAngles;
        float angle=Rotation[1];
        Vector3 normal_vec=other.GetContact(0).normal.normalized;
        Vector3 vec=Vector3.zero;

        // Debug.Log("接地店::"+other.GetContact(0).point);
        // Debug.Log("法線::"+normal_vec);
        Vector3 dir=top_point_dir.transform.position;
        Vector3 origin = top_point_origin.transform.position;
        Vector3 houkou=(dir-origin).normalized;
        // GameObject Cube=GameObject.CreatePrimitive(PrimitiveType.Cube);
        // Cube.transform.position=other.GetContact(0).point;
        // Cube.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
        if(normal_vec.y==1){
            houkou.y=0;
        }
        // Debug.Log(houkou.y);
        
        // Debug.LogWarning(Rotation[2]);
        // Debug.LogWarning(this.transform.localPosition.x);
        if(Input.GetKey("up")){
            // rb1.AddForce(-20f*Mathf.Sin(angle*Mathf.PI/180f), 0f, -20f*Mathf.Cos(angle*Mathf.PI/180f), ForceMode.Acceleration);
            // rb1.AddForce(-20f*Mathf.Sin(angle*Mathf.PI/180f), 0f, -20f*Mathf.Cos(angle*Mathf.PI/180f), ForceMode.Force);
            // rb1.AddForceAtPosition(vec*-20.0f,other.GetContact(0).point,ForceMode.Acceleration);
            if(rb1.linearVelocity.magnitude<=2.0f){
                rb1.AddForceAtPosition(houkou*hps.move_speed,other.GetContact(0).point,ForceMode.Acceleration);
            }
            
            // rb1.velocity=houkou*1.0f;
        }
        if(Input.GetKey("down")){
            // rb1.AddForce(20f*Mathf.Sin(angle*Mathf.PI/180f), 0f, 20f*Mathf.Cos(angle*Mathf.PI/180f), ForceMode.Acceleration);
            // rb1.AddForce(-20f*Mathf.Sin(angle*Mathf.PI/180f), 0f, -20f*Mathf.Cos(angle*Mathf.PI/180f), ForceMode.Force);
            if(rb1.linearVelocity.magnitude>=-2.0f){
                rb1.AddForceAtPosition(houkou*-1.0f*hps.move_speed,other.GetContact(0).point,ForceMode.Acceleration);
            }
            // rb1.velocity=houkou*-1.0f;
        }
        if(Input.GetKey(KeyCode.RightArrow)){
            rb1.angularVelocity=new Vector3(0,hps.rotate_speed,0);
        }
        if(Input.GetKey(KeyCode.LeftArrow)){
            rb1.angularVelocity=new Vector3(0,-1.0f*hps.rotate_speed,0);
        }

        // Destroy(Cube,0.2f);

    }
}
