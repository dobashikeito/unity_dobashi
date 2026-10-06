using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class imax_ps5 : MonoBehaviour
{
    public int L2L1;
    public int R2R1;
    public int Lstick;
    public int Rstick;
    public int Lstick_r;
    public int Rstick_r;
    public int squ_circle;
    public int tri_cross;
    public int cross_updown;
    public int cross_rl;
    public bool flipper_mode=true;
    private int count_time = 0;
    // Start is called before the first frame update
    void Start(){

    }
//ps5コントローラーの場合
// button
// 0:square,1:cross,2:circle,3:triangle,4:L1,5:R1,6:L2,7:R2,8:左シェア,9:右シェア,10:L3,11:R3,12:PS,13:center,14:PSの下のクリアーボタン
// Axis
// 1:LStick横、左negative,2;LStick上下、上negative,3;RStick横、左negative,4;L2,5;R2,6;Rstick上下、上negative,7;cross横　左negative,8:cross上下　下negative,
    // Update is called once per frame
    void Update()
    {
        float a=Input.GetAxis("Axis 2");
        float b=Input.GetAxis("Axis 1");
        a=Input.GetAxis("Horizontal");
        b=Input.GetAxis("Vertical");

        float c=Input.GetAxis("Axis 6");
        float d=Input.GetAxis("Axis 3");

        
        //×
        if(Input.GetKey(KeyCode.JoystickButton1)){
            tri_cross=1;
        }
        //△
        else if(Input.GetKey(KeyCode.JoystickButton3)){
            tri_cross=-1;
        }
        else{
            tri_cross=0;
        }
        //〇
        if(Input.GetKey(KeyCode.JoystickButton2)){
            squ_circle=1;
        }
        //□
        else if(Input.GetKey(KeyCode.JoystickButton0)){
            squ_circle=-1;
        }
        else{
            squ_circle=0;
        }
        //L1
        if(Input.GetKey(KeyCode.JoystickButton4)){
            L2L1=-1;
            if(!flipper_mode){
                L2L1=0;
            }
        }
        //L2
        else if(Input.GetKey(KeyCode.JoystickButton6)){
            L2L1=1;
            if(!flipper_mode){
                L2L1=0;
            }
        }
        else{
            L2L1=0;
        }
        //R1
        if(Input.GetKey(KeyCode.JoystickButton5)){
            R2R1=1;
            if(!flipper_mode){
                R2R1=0;
            }
        }
        //R2
        else if(Input.GetKey(KeyCode.JoystickButton7)){
            R2R1=-1;
            if(!flipper_mode){
                R2R1=0;
            }
        }
        else{
            R2R1=0;
        }
        //十字↑↓
        if (Input.GetAxis("Axis 8") > 0f){
            cross_updown=1;
        }
        else if (Input.GetAxis("Axis 8") < 0f){
            cross_updown=-1;
        }
        else{
            cross_updown=0;
        }
        //Lstick↑↓
        if(Mathf.Abs(a)>Mathf.Abs(b)){
            if (a > 0f){
                Lstick=-1;
                Rstick=0;
            }
            else if (a < 0f){
                Lstick=1;
                Rstick=0;
            }
        }
        else if(Mathf.Abs(a)<Mathf.Abs(b)){
            //Lstick→←
            if (b > 0f){
                Lstick=0;
                Rstick=1;
            }
            else if (b < 0f){
                Lstick=0;
                Rstick=-1;
            }
            else{
                Lstick=0;
                Rstick=0;
            }
        }
        else{
            Lstick=0;
            Rstick=0;
        }

        //Rstick↑↓
        if(Mathf.Abs(c)>Mathf.Abs(d)){
            if (c > 0f){
                Lstick_r=-1;
                Rstick_r=0;
            }
            else if (c < 0f){
                Lstick_r=1;
                Rstick_r=0;
            }
        }
        else if(Mathf.Abs(c)<Mathf.Abs(d)){
            //Rstick→←
            if (d > 0f){
                Lstick_r=0;
                Rstick_r=1;
            }
            else if (d < 0f){
                Lstick_r=0;
                Rstick_r=-1;
            }
            else{
                Lstick_r=0;
                Rstick_r=0;
            }
        }
        else{
            Lstick_r=0;
            Rstick_r=0;
        }

        if(Input.GetKey(KeyCode.Joystick1Button13)){
            
        }
        if(Input.GetKey(KeyCode.Joystick1Button9)){
            count_time++;
            if(flipper_mode){
                if(count_time>20){
                    flipper_mode=false;
                    count_time=0;
                    Debug.LogError("OFF");
                }
            }
            else if(!flipper_mode){
                if(count_time>20){
                    flipper_mode=true;
                    count_time=0;
                    Debug.LogError("ON");
                }
            }
            
        }
        
    }
    
}
