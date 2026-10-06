using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ps5controller_arm : MonoBehaviour
{
    public bool button_cross=false;
    public bool button_triangle=false;
    public bool button_square=false;
    public bool button_circle=false;
    public bool button_L1=false;
    public bool button_L2=false;
    public bool button_R1=false;
    public bool button_R2=false;
    public bool button_cross_up=false;
    public float button_cross_up_f=0.0f;
    public bool button_cross_down=false;
    public float button_cross_down_f=0.0f;
    public bool button_cross_left=false;
    public bool button_cross_right=false;
    public float Lstick_lr=0.0f;
    public float Lstick_ud=0.0f;
    public float Rstick_lr=0.0f;
    public float Rstick_ud=0.0f;
    public bool button_touchpad=false;
    public bool button_share_r=false;
    public bool button_share_l=false;
    public bool button_ps=false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {//Lstick
        Lstick_ud=Input.GetAxis("Axis 2");//↑negative
        Lstick_lr=Input.GetAxis("Axis 1");//←negative
        //Rstick
        Rstick_ud=Input.GetAxis("Axis 5");//↑negative
        Rstick_lr=Input.GetAxis("Axis 4");//←negative
        
        //×
        if(Input.GetKey(KeyCode.JoystickButton0)){
            button_cross=true;
        }
        else{
            button_cross=false;
        }
        //△
        if(Input.GetKey(KeyCode.JoystickButton3)){
            button_triangle=true;
        }
        else{
            button_triangle=false;
        }
        //○
        if(Input.GetKey(KeyCode.JoystickButton1)){
            button_circle=true;
        }
        else{
            button_circle=false;
        }
        //□
        if(Input.GetKey(KeyCode.JoystickButton2)){
            button_square=true;
        }
        else{
            button_square=false;
        }
        //L1
        if(Input.GetKey(KeyCode.JoystickButton4)){
            button_L1=true;
        }
        else{
            button_L1=false;
        }
        //L2
        if(Input.GetAxis("Axis 3")>0.0f){
            button_L2=true;
        }
        else{
            button_L2=false;
        }
        //R1
        if(Input.GetKey(KeyCode.JoystickButton5)){
            button_R1=true;
        }
        else{
           button_R1=false;
        }
        //R2
        if(Input.GetAxis("Axis 6") > 0f){
            button_R2=true;
        }
        else{
           button_R2=false;
        }
        //十字↑↓
        if (Input.GetAxis("Axis 8") > 0f){//↓
            button_cross_down=true;
            button_cross_down_f=-1.0f;
        }
        else{
            button_cross_down=false;
            button_cross_down_f=0.0f;
        }
        if (Input.GetAxis("Axis 8") < 0f){//↑
            button_cross_up=true;
            button_cross_up_f=1.0f;
        }
        else{
            button_cross_up=false;
            button_cross_up_f=0.0f;
        }
        //十字→←
        if (Input.GetAxis("Axis 7") > 0f){//→
            button_cross_right=true;
        }
        else{
            button_cross_right=false;
        }
        if (Input.GetAxis("Axis 7") < 0f){//←
            button_cross_left=true;
        }
        else{
            button_cross_left=false;
        }
        //Lstick↑↓
        // if(Mathf.Abs(a)>Mathf.Abs(b)){
        //     if (a > 0f){

        //     }
        //     else if (a < 0f){

        //     }
        // }
        // else if(Mathf.Abs(a)<Mathf.Abs(b)){
        //     //Lstick→←
        //     if (b > 0f){
                
        //     }
        //     else if (b < 0f){
                
        //     }
        // }
        // else{
            
        // }

        if(Input.GetKey(KeyCode.Joystick1Button13)){//touchpad
            button_touchpad=true;
        }
        else{
            button_touchpad=false;
        }
        if(Input.GetKey(KeyCode.Joystick1Button7)){//右shareボタン
            button_share_r=true;
        }
        else{
            button_share_r=false;
        }
        if(Input.GetKey(KeyCode.Joystick1Button6)){//左shareボタン
            button_share_l=true;
        }
        else{
            button_share_l=false;
        }
        if(Input.GetKey(KeyCode.Joystick1Button8)){//PSボタン
            button_ps=true;
        }
        else{
            button_ps=false;
        }
        
    }
}
