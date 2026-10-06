// transformで回転
//この回転はできない
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
            transform.Rotate(new Vector3(0.1f,0,0));
        if (Input.GetKey(KeyCode.RightArrow))
            transform.Rotate(new Vector3(-0.1f,0,0));
    }
}
