using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class do_brain_totani : MonoBehaviour
{
    private totani_agent[] totani=new totani_agent[1];
    public GameObject[] brain=new GameObject[1];
    private bool random_step_switch=false;
    // Start is called before the first frame update
    void Start()
    {
        totani[0]=brain[0].GetComponent<totani_agent>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!random_step_switch){
            if(totani[0].count_goal_continuous>=2){
                totani[0].step_height_persent+=0.1f;
                totani[0].count_goal_continuous=0;
                Debug.Log("現在の高さは "+totani[0].step_height_persent+" です。");
            }
            if(totani[0].step_height_persent>0.95f){
                random_step_switch=true;
            }
        }
        else{
            totani[0].step_height_persent=Random.Range(0.1f,0.9f);
            totani[0].count_goal_continuous=0;
            Debug.Log("現在の高さは "+totani[0].step_height_persent+" です。");
        }
        
    }
}
