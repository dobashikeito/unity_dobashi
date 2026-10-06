using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class do_brain_discrete : MonoBehaviour
{
    private nmodel_step4_discrete[] step4=new nmodel_step4_discrete[1];
    public GameObject[] brain=new GameObject[1];
    private bool random_step_switch=false;
    // Start is called before the first frame update
    void Start()
    {
        step4[0]=brain[0].GetComponent<nmodel_step4_discrete>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!random_step_switch){
            if(step4[0].count_goal_continuous>=2){
                step4[0].step_height_persent+=0.1f;
                step4[0].count_goal_continuous=0;
                Debug.Log("現在の高さは "+step4[0].step_height_persent+" です。");
            }
            if(step4[0].step_height_persent>1.25f){
                random_step_switch=true;
            }
        }
        else{
            step4[0].step_height_persent=Random.Range(0.1f,1.0f);
            step4[0].count_goal_continuous=0;
            Debug.Log("現在の高さは "+step4[0].step_height_persent+" です。");
        }
        
    }
}
