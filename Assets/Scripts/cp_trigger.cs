using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cp_trigger : MonoBehaviour
{
    public GameObject checkpoints;
    public GameObject eight_wall;
    public GameObject[] cp_list;
    public GameObject[] eight_wall_list;
    public int now_cp=0;
    public int count_check_point;
    public int count_eight_wall;
    public bool cp_through=false;
    public bool touch_eight_wall=false;
    public float cp_reward=1.0f;
    public float turn_reward=0.0f;

    // Start is called before the first frame update
    public void Start()
    {
        count_check_point=checkpoints.transform.childCount;
        count_eight_wall=eight_wall.transform.childCount;
        cp_list=new GameObject[count_check_point];
        eight_wall_list=new GameObject[count_eight_wall];
        for(int i=0;i<count_check_point;i++){
            cp_list[i]=checkpoints.transform.GetChild(i).gameObject;
        }
        cp_reward=cp_reward/count_check_point;
        for(int i=0;i<count_eight_wall;i++){
            eight_wall_list[i]=eight_wall.transform.GetChild(i).gameObject;
        }
        
        
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other){
        if(other.name=="check"+now_cp&&now_cp<count_check_point){
            cp_list[now_cp].SetActive(false);
            cp_through=true;
            turn_reward=0;
            if(now_cp==9){
                eight_wall_list[0].SetActive(false);
                eight_wall_list[1].SetActive(false);
                eight_wall_list[2].SetActive(true);
                eight_wall_list[3].SetActive(true);
                turn_reward=1.0f;
            }
            now_cp++;
        }
        if(other.name=="backR"||other.name=="backL"||other.name=="frontR"||other.name=="frontL"){
            touch_eight_wall=true;
        }
    }
    

}
