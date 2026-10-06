using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cp_trigger4real : MonoBehaviour
{
    public bool touch_clickpoint=false;
    public int click_point_index=0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other){
        // if(other.name=="click_point"+click_point_index){
        //     click_point_index++;
        //     touch_clickpoint=true;
        // }
    }
}
