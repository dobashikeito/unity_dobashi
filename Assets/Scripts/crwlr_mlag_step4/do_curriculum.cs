using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class do_curriculum : MonoBehaviour
{
    private capsule_step4[] step4=new capsule_step4[16];
    public GameObject[] Learn=new GameObject[16];
    // private GameObject[] Learn_child=new GameObject[16];

    private int num_agent = 0;

    


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
            step4[i]=Learn[i].GetComponent<capsule_step4>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        num_agent=0;
        for(int i=0;i<step4.Length;i++){
            if(step4[i].count_goal_continuous>=10){
                num_agent++;
            }
        }

        if(num_agent>=8){
            for(int i=0;i<step4.Length;i++){
                step4[i].step_height_persent+=0.1f;
                step4[i].count_goal_continuous=0;
            }
            Debug.Log("現在の高さは "+step4[0].step_height_persent+" です。");

        }
    }
}
