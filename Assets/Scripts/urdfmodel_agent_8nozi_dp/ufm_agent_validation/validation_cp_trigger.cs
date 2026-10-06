using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class validation_cp_trigger : MonoBehaviour
{
    public GameObject[] checkpoints;
    public GameObject[] cp_list;
    public int now_cp=0;
    public int cp_index=0;
    [Header("経路を固定したいときに使用、使わないときは10にする")]
    public int cp_index_fixed;
    public int count_check_point;
    public int count_eight_wall;
    public bool cp_through=false;
    public bool touch_eight_wall=false;
    public bool touch_clickpoint=false;
    public float cp_reward=1.0f;
    public float turn_reward=0.0f;
    private int click_point_index=1;

    // Start is called before the first frame update
    public void make_cp(int field_number)
    {   
        cp_index = field_number;
        checkpoints[field_number].SetActive(true);
        count_check_point=checkpoints[field_number].transform.childCount;
        cp_list=new GameObject[count_check_point];
        for(int i=0;i<count_check_point;i++){
            cp_list[i]=checkpoints[field_number].transform.GetChild(i).gameObject;//checkpointのgbをここに補完
        }
        cp_reward=cp_reward/count_check_point;
        
        
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other){
        if(other.name=="check"+now_cp&&now_cp<count_check_point){
            cp_list[now_cp].SetActive(false);
            cp_through=true;
            turn_reward=0;
            if(now_cp==9){
                turn_reward=1.0f;
            }
            else if(now_cp==count_check_point){//cpをリセットする。
                for(int i=0;i<count_check_point;i++){//cpのリセット
                    cp_list[i].SetActive(true);
                }
                checkpoints[cp_index].SetActive(false);
            }
            now_cp++;
        }
        if(other.name=="click_point"+click_point_index){
            click_point_index++;
            touch_clickpoint=true;
        }
    }
    

}
