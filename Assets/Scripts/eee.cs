using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class eee : MonoBehaviour
{
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb=gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey("up")){rb.linearVelocity=new Vector3(1,1,1);}
        if(Input.GetKey("down")){}
        if(Input.GetKey(KeyCode.RightArrow)){rb.angularVelocity=new Vector3(0,10,0);}
        if(Input.GetKey(KeyCode.LeftArrow)){rb.angularVelocity=new Vector3(0,-10,0);}
    }
}
