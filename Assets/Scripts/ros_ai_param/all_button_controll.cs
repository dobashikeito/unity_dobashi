using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class all_button_controll : MonoBehaviour {

    public float[] button_data_list=new float[3];//0→遠隔操作、1→半自律、2→自律
    public GameObject[] all_button_go=new GameObject[3];
    public Button[] all_buttons=new Button[3];
    public GameObject ros_pubsub_ai_go;
    public ros_pubsub_ai ros_pubsub_ai;

    // ボタンが押された場合、今回呼び出される関数
    private void Start() {
        ros_pubsub_ai = ros_pubsub_ai_go.GetComponent<ros_pubsub_ai>();
        for(int i=0;i<3;i++){
            all_buttons[i]=all_button_go[i].GetComponent<Button>();//0→遠隔操作、1→半自律、2→自律
        }
        all_buttons[0].image.color=Color.red;
        button_data_list[0]=1.0f;
        button_data_list[1]=0.0f;
        button_data_list[2]=0.0f;
    }
    public void remote_control_OnClick()
    {
        if(button_data_list[0]>0.5f){
            button_data_list[0]=0.0f;
            all_buttons[0].image.color=Color.white;
            Debug.LogError("終了");  // ログを出力
        }
        else{
            ros_pubsub_ai.change_mode(0);//mode chmsgを publish
            button_data_list[0]=1;
            all_buttons[0].image.color=Color.red;
            Debug.LogError("遠隔操作開始");  // ログを出力

            button_data_list[1]=0.0f;
            button_data_list[2]=0.0f;
            all_buttons[1].image.color=Color.white;
            all_buttons[2].image.color=Color.white;
        }
    }
    public void semi_autnomous_OnClick()
    {
        if(button_data_list[1]>0.5f){
            button_data_list[1]=0.0f;
            all_buttons[1].image.color=Color.white;
            Debug.LogError("終了");  // ログを出力
        }
        else{
            ros_pubsub_ai.change_mode(1);//mode chmsgを publish
            button_data_list[1]=1.0f;
            all_buttons[1].image.color=Color.red;
            Debug.LogError("半自律走行開始");  // ログを出力

            button_data_list[0]=0.0f;
            button_data_list[2]=0.0f;
            all_buttons[0].image.color=Color.white;
            all_buttons[2].image.color=Color.white;
        }
    }
    public void autnomous_OnClick()
    {
        if(button_data_list[2]>0.5f){
            button_data_list[2]=0.0f;
            all_buttons[2].image.color=Color.white;
            Debug.LogError("終了");  // ログを出力
        }
        else{
            ros_pubsub_ai.change_mode(2);//mode chmsgを publish
            button_data_list[2]=1.0f;
            all_buttons[2].image.color=Color.red;
            Debug.LogError("自律走行開始");  // ログを出力

            button_data_list[1]=0.0f;
            button_data_list[0]=0.0f;
            all_buttons[0].image.color=Color.white;
            all_buttons[1].image.color=Color.white;
        }
            
    }

}