using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class do_curriculum_new : MonoBehaviour
{
    private new_model_step4[] step4=new new_model_step4[20];
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

        for(int i=0;i<step4.Length;i++){
            step4[i]=Learn[i].GetComponent<new_model_step4>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        num_agent=0;
        for(int i=0;i<step4.Length;i++){
            int gp_sum = 0;
            for(int k=0;k<step4[i].goal_percent.Length;k++){
                gp_sum += step4[i].goal_percent[k];
            }
            if(gp_sum>=14){
                num_agent++;
            }
        }
        if(!random_step_switch){
            if(num_agent>=20){
                for(int i=0;i<step4.Length;i++){
                    step4[i].step_height_persent+=0.1f;
                    step4[i].count_goal_continuous=0;
                    for(int k=0;k<step4[i].goal_percent.Length;k++){
                        step4[i].goal_percent[k]=0;
                    }
                }
                Debug.Log("現在の高さは "+step4[0].step_height_persent+" です。現在のステップ数は、"+step4[0].count_allstep);
            }
            if(step4[0].step_height_persent>1.05f){
                random_step_switch=true;
            }
        }
        if(random_step_switch){
            if(num_agent>=20){
                for(int i=0;i<step4.Length;i++){
                    step4[i].step_height_persent=Random.Range(0.1f,0.89f);
                    step4[i].count_goal_continuous=0;
                }
                Debug.Log("現在の高さは "+step4[0].step_height_persent+" です。現在のステップ数は、"+step4[0].count_allstep);
            }
        }
    }
}
