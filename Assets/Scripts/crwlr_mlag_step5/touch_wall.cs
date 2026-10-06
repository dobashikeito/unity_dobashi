using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class touch_wall : MonoBehaviour
{
    // public GameObject[] walls=new GameObject[8];
    public float touch_obstacle_reward = 0.0f;//x 松尾さんが使っていたらしくcoするとエラー起きるので放置
    public float touch_reward_once = -0.5f;
    public int count_touch = 0;


    // Start is called before the first frame update
    public void Start()
    {
        // Debug.Log(now_cp);
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "obstacle")
        {
            count_touch++;
            // Debug.Log($"touched obstacle!,{count_touch}");
        }

    }
    

    // void OnCollisionExit(Collision other)
    // {
    //     touch_obstacle_reward=0;
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
