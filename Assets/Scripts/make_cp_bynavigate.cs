using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class make_cp_bynavigate : MonoBehaviour{
    public int num_cp=0;
    public GameObject origin;
    public GameObject dir;
    public GameObject check_point;
    private Vector3 originTodir;
    private Vector3 originTodir_past;
    private int count_num=12;
    private float time=0.0f;
    private float time_threshold=0.2f;

    // Start is called before the first frame update
    void Start()
    {
        originTodir=dir.transform.position-origin.transform.position;
        originTodir_past=originTodir;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        time+=Time.deltaTime;
        if(time>time_threshold){
            Debug.Log(time);
            originTodir=dir.transform.position-origin.transform.position;
            float cos_theta=Vector3.Dot(originTodir,originTodir_past)/originTodir.magnitude/originTodir_past.magnitude;
            // Debug.Log(cos_theta);
            // if(cos_theta<Mathf.Cos(20.0f*Mathf.PI/180.0f)){
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position=origin.transform.position;
                cube.transform.localEulerAngles=this.transform.localEulerAngles;
                cube.transform.localScale=new Vector3(1.5f,1,0.01f);
                cube.transform.parent=check_point.transform;
                
                Collider cb_bc=cube.GetComponent<BoxCollider>();
                cb_bc.isTrigger=true;
                cube.name="check"+count_num;
                count_num++;
            // }
            originTodir_past=originTodir;
            time=0.0f;
        }
        
    }
}
