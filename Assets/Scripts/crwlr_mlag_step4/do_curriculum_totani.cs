using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class do_curriculum_totani : MonoBehaviour
{
    private totani_agent[] totani=new totani_agent[20];
    public GameObject[] Learn=new GameObject[20];
    // private GameObject[] Learn_child=new GameObject[16];

    private int num_agent = 0;
    private bool random_step_switch=false;

    


    // Start is called before the first frame update
    void Start()
    {
        // for(int i=0;i<Learn.Length;i++){
        //     Learn[i]=transform.GetChild(i).gameObject;
        // }
        // for(int i=0;i<Learn.Length;i++){
        //     Learn_child[i]=Learn[i].GetComponent<Transform>().transform.GetChild(0).gameObject;
        // }

        for(int i=0;i<totani.Length;i++){
            totani[i]=Learn[i].GetComponent<totani_agent>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        num_agent=0;
        for(int i=0;i<totani.Length;i++){
            if(totani[i].count_goal_continuous>=20){
                num_agent++;
            }
        }
        if(!random_step_switch){
            if(num_agent>=20){
                for(int i=0;i<totani.Length;i++){
                    totani[i].step_height_persent+=0.1f;
                    totani[i].count_goal_continuous=0;
                }
                Debug.Log("現在の高さは "+totani[0].step_height_persent+" です。現在のステップ数は、"+totani[0].count_allstep);
            }
            if(totani[0].step_height_persent>0.95f){
                random_step_switch=true;
            }
        }
        if(random_step_switch){
            if(num_agent>=20){
                for(int i=0;i<totani.Length;i++){
                    totani[i].step_height_persent=Random.Range(0.1f,0.9f);
                    totani[i].count_goal_continuous=0;
                }
                Debug.Log("現在の高さは "+totani[0].step_height_persent+" です。現在のステップ数は、"+totani[0].count_allstep);
            }
        }
    }
}
