using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ch_transform : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetAxis("Vertical")>0.0f){
            this.transform.position=new Vector3(0,1.0f,0);
        }
        else if(Input.GetAxis("Vertical")<0.0f){
            this.transform.position=new Vector3(0,-1.0f,0);
        }
        
    }
}
