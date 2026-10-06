using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class astm3_cp : MonoBehaviour
{
    public GameObject[] checkpoints=new GameObject[8];
    public Collider[] cp_collider=new Collider[8];
    public int now_cp=1;
    public bool gothrough_cp=false;
    public bool gothrough_cp_miss=false;
    public float miss_cp_reward=0.0f;

    // Start is called before the first frame update
    void Start()
    {
        for(int i=0;i<cp_collider.Length;i++){
            cp_collider[i]=checkpoints[i].GetComponent<BoxCollider>();
        }
        for(int i=0;i<checkpoints.Length;i++){//0~3まで、isTrigger on
            if(i<=3){
                cp_collider[i].isTrigger=true;
            }
            else{
                cp_collider[i].isTrigger=false;
            }
        }
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other){
        if(other.name=="check"+now_cp&&now_cp<9){
            Material mat = checkpoints[now_cp-1].GetComponent<Renderer>().material;
            mat.color = new Color(0.0f,1.0f,0.0f,1.0f);//通過したら緑になる。
            gothrough_cp=true;
            now_cp+=1;
            if(now_cp==2){//チェックポイントの位置が2と6は一緒のため。
                checkpoints[5].SetActive(false);
                checkpoints[1].SetActive(true);
            }
            if(now_cp>=3){
                cp_collider[now_cp-3].isTrigger=false;
            }
            if(now_cp==5){//チェックポイントの位置が2と6は一緒のため
                checkpoints[5].SetActive(true);
                checkpoints[1].SetActive(false);
                for(int i=4;i<cp_collider.Length;i++){//4から7までをisTriggerをonにする。
                    cp_collider[i].isTrigger=true;
                }
            }
        }
        else if(other.name!="check"+now_cp){
            gothrough_cp_miss=true;
        }
    }
    
    // void OnCollisionStay(Collision collision){
        
    //         miss_cp_reward=-0.001f;
    // }
    // void OnCollisionExit(Collision collision){
    //     miss_cp_reward=0.0f;
    // }
    // void Update(){
    //     if(now_cp>8){
    //         for(int i=0;i<checkpoints.Length;i++){
    //             checkpoints[i].SetActive(true);
    //         }
    //         now_cp=1;
    //         goal_line=true;
    //     }
    // }

}
