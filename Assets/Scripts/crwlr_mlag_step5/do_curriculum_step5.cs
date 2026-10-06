using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class do_curriculum_step5 : MonoBehaviour
{
    private new_model_step5[] step5;
    public GameObject[] Learn=new GameObject[20];
    // private GameObject[] Learn_child=new GameObject[16];
    private int num_agent = 0;
    public int count_field = 0;
    private bool random_step_switch=false;

    


    // Start is called before the first frame update
    void Start()
    {
        step5=new new_model_step5[Learn.Length];

        for(int i=0;i<step5.Length;i++){
            step5[i]=Learn[i].GetComponent<new_model_step5>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        num_agent=0;
        for(int i=0;i<step5.Length;i++){
            int gp_sum = 0;
            for(int k=0;k<step5[i].goal_percent.Length;k++){
                gp_sum += step5[i].goal_percent[k];
            }
            if(gp_sum>=14){
                num_agent++;
            }
        }
        if(!random_step_switch){
            if(num_agent>=20){
                for(int i=0;i<step5.Length;i++){
                    step5[i].step_height_persent+=0.1f;
                    step5[i].count_goal_continuous=0;
                    for(int k=0;k<step5[i].goal_percent.Length;k++){
                        step5[i].goal_percent[k]=0;
                    }
                }
                Debug.Log("現在の高さは "+step5[0].step_height_persent+" です。現在のステップ数は、"+step5[0].count_allstep);
            }
            if(step5[0].step_height_persent>1.05f){
                random_step_switch=true;
            }
        }
        if(random_step_switch){
            if(num_agent>=20){
                for(int i=0;i<step5.Length;i++){
                    step5[i].step_height_persent=Random.Range(0.1f,0.89f);
                    step5[i].count_goal_continuous=0;
                }
                Debug.Log("現在の高さは "+step5[0].step_height_persent+" です。現在のステップ数は、"+step5[0].count_allstep);
            }
        }
    }
}
