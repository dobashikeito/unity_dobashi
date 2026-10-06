//mlagantで使う
//ボディの前進、後退、旋回
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class move_body_noac : MonoBehaviour
{   
    public Rigidbody rb1;
    public static move_body script_mb;
    public float angle;
    public GameObject top_point_dir;
    public GameObject top_point_origin;
    private Vector3 dir_vector;
    public float action_Go;
    public float action_rotate;
    // [SerializeField]
    public bool body_touch=false;
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
    //     angle=Rotation[1];//進行方向取得
    //     // Debug.LogWarning(angle);
    //     float Go = action_Go;//Goを-1~1にするため5で割る
    //     rb1.AddForce(-20f*Mathf.Sin(angle*Mathf.PI/180f)*Go, 0f, -20f*Mathf.Cos(angle*Mathf.PI/180f)*Go, ForceMode.Acceleration);
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
        // if(Input.GetKey(KeyCode.RightArrow)){
        //     rb1.angularVelocity=new Vector3(0,1f,0);
        // }
        // if(Input.GetKey(KeyCode.LeftArrow)){
        //     rb1.angularVelocity=new Vector3(0,-1f,0);
        // }

    // }
    void OnCollisionExit(Collision other)
    {
        body_touch=false;
    }

    void OnCollisionStay(Collision other){
        body_touch=true;

        Vector3 normal_vec=other.GetContact(0).normal.normalized;


        Vector3 dir=top_point_dir.transform.position;
        Vector3 origin = top_point_origin.transform.position;
        Vector3 houkou=dir-origin;
        
        if(normal_vec.y==1){
            houkou.y=0;
        }
        rb1.AddForceAtPosition(houkou*10.0f*action_Go,other.GetContact(0).point);
        rb1.angularVelocity=new Vector3(0,1f*action_rotate,0);

        // Destroy(Cube,0.2f);

    }
}
