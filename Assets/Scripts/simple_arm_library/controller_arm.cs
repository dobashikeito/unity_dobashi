using System;
using Unity.Robotics;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class controller_arm : MonoBehaviour
    {
        //変数
        　//座標、z前、y上、x横
        public float[] xyz_ee=new float[3]{0,0,0};
        public float[] xyz4=new float[3]{0,0,0};//アームの先端座標
        public float[] xyz3=new float[3]{0,0,0};//
        public float[] xyz2=new float[3]{0,0,0};
          //角度
        private float[] angle=new float[8]{0.0f,1.57f,-1.57f,0.0f,0.0f,0.0f,0.0f,0.0f};
        public float[] publish_angle=new float[8]{0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f};
          //先端のベクトル方向
        public float[] v43_now = new float[3];
        public float[] v43_now_rotate = new float[3];
          //ハンドの向きを変化する。
        private float ch_angle0 = 0.0f;
        private float ch_angle3 =0.0f;
        private float ch_angle4=0.0f;
        private float ch_angle5 =0.0f;
          //変化するモードのbool
        private bool flipper_mode=false;
        private bool back_mode=false;
        public bool camera_sync_sw=false;
        //定数
          //アームの長さ
        private float L1=0.653f;
        private float L2=0.677f;
        private float L3=0.2f;
        private float arm_max;
        private float joy_mass=0.005f;
        private float arm_min;
        public GameObject ps5controller;
        private ps5controller_arm ps5controller_arm;

        void Start(){
            ps5controller_arm=ps5controller.GetComponent<ps5controller_arm>();
            arm_max=L1+L2;
            // arm_min=(L1+L2)/100.0f;
            arm_min=0.0f;
            kinetic();
            for(int i=0;i<xyz_ee.Length;i++){
                xyz_ee[i]=xyz3[i];
            }
            inverse_kinetic();
            eedir();
            angle_publish();
        }

        void Update(){//
            // if(Input.GetKey(KeyCode.Q)){//縦移動
            //     xyz_ee[2]=xyz_ee[2]+0.001f;
            // }
            // if(Input.GetKey(KeyCode.W)){//縦移動
            //     xyz_ee[2]=xyz_ee[2]-+0.001f;
            // }
            // kinetic();
            // inverse_kinetic();
            // angle_publish();
            if(ps5controller_arm.button_R1){
                handroll(-1.0f);
            }
            if(ps5controller_arm.button_L1){
                handroll(1.0f);
            }
            joy2xyz();
        }
        void kinetic(){//運動学
            xyz2[0]=L1*Mathf.Cos(angle[1])*Mathf.Sin(angle[0]);
            xyz2[1]=L1*Mathf.Sin(angle[1]);
            xyz2[2]=L1*Mathf.Cos(angle[1])*Mathf.Cos(angle[0]);

            xyz3[0]=xyz2[0]+L2*Mathf.Cos(angle[1]+angle[2])*Mathf.Sin(angle[0]);
            xyz3[1]=xyz2[1]+L2*Mathf.Sin(angle[1]+angle[2]);
            xyz3[2]=xyz2[2]+L2*Mathf.Cos(angle[1]+angle[2])*Mathf.Cos(angle[0]);

            xyz4[0] = xyz3[0] + L3 * Mathf.Sin(angle[5] + angle[0]) * Mathf.Cos(angle[1] + angle[2] + angle[3]);
            xyz4[1] = xyz3[1] + L3 * Mathf.Sin(angle[1] + angle[2] + angle[3]);
            xyz4[2] = xyz3[2] + L3 * Mathf.Cos(angle[5] + angle[0]) * Mathf.Cos(angle[1] + angle[2] + angle[3]);

        }
        void inverse_kinetic(){//逆運動学
            float xyz=Mathf.Pow(xyz_ee[0],2.0f)+Mathf.Pow(xyz_ee[1],2.0f)+Mathf.Pow(xyz_ee[2],2.0f);
            float s=Mathf.Pow(xyz_ee[0],2.0f)+Mathf.Pow(xyz_ee[2],2.0f);

            angle[0]=Mathf.Atan(xyz_ee[0]/xyz_ee[2]);
            angle[1]=Mathf.Acos((xyz-Mathf.Pow(L2,2)+Mathf.Pow(L1,2))/(2*L1*Mathf.Pow(xyz,0.5f)))+Mathf.Atan((xyz_ee[1])/(s));
            angle[2]=Mathf.Atan((xyz_ee[1]-L1*Mathf.Sin(angle[1]))/(Mathf.Pow(s,0.5f)-L1*Mathf.Cos(angle[1])))-angle[1];

        }


        bool ik_check(int num){//逆運動学求められるかをチェック
            bool check = false;

            if (num == 0){ // x プラス方向
            //Debug.LogWarning(Mathf.Sqrt(Mathf.Pow(xyz_ee[2] + joy_mass, 2) + Mathf.Pow(xyz_ee[0], 2) + Mathf.Pow(xyz_ee[1], 2)));
                if ((arm_max > Mathf.Sqrt(Mathf.Pow(xyz_ee[2] + joy_mass, 2) + Mathf.Pow(xyz_ee[0], 2) + Mathf.Pow(xyz_ee[1], 2))) && xyz_ee[2] + joy_mass > arm_min){check = true;}
            }
            else if (num == 1){ // x マイナス方向
                if ((arm_max > Mathf.Sqrt(Mathf.Pow(xyz_ee[2] - joy_mass, 2) + Mathf.Pow(xyz_ee[0], 2) + Mathf.Pow(xyz_ee[1], 2))) && xyz_ee[2] - joy_mass > arm_min){check = true;}
            }
            else if (num == 2){ // y プラス方向
                if (arm_max > Mathf.Sqrt(Mathf.Pow(xyz_ee[0] + joy_mass, 2) + Mathf.Pow(xyz_ee[2], 2) + Mathf.Pow(xyz_ee[1], 2))){check = true;}
            }
            else if (num == 3){ // y マイナス方向
                if (arm_max > Mathf.Sqrt(Mathf.Pow(xyz_ee[0] - joy_mass, 2) + Mathf.Pow(xyz_ee[2], 2) + Mathf.Pow(xyz_ee[1], 2))){check = true;}
            }
            else if (num == 4){// z プラス方向
                if (arm_max > Mathf.Sqrt(Mathf.Pow(xyz_ee[1] + joy_mass, 2) + Mathf.Pow(xyz_ee[2], 2) + Mathf.Pow(xyz_ee[0], 2))){check = true;}
            }
            else if (num == 5){ // z マイナス方向
                if (arm_max > Mathf.Sqrt(Mathf.Pow(xyz_ee[1] - joy_mass, 2) + Mathf.Pow(xyz_ee[2], 2) + Mathf.Pow(xyz_ee[0], 2))){check = true;}
            }

            return check;
        }

        void vectorrotatexy(float theta1, float theta2, float theta3){
            float theta1_ = theta1 + 1.57f;
            if (Mathf.Abs(theta1) < 0.01f){
                theta1_ = theta1;
            }
            else if (theta1 > 0.01f){
                theta1_ = theta1 - 1.57f;
            }

            float theta2_ = theta2 - ch_angle3;
            float theta3_ = theta3;

            float vxy = v43_now[0] * Mathf.Cos(theta2_) + v43_now[2] * Mathf.Sin(theta2_);
            float vyy = v43_now[1];
            float vzy = -1.0f * v43_now[0] * Mathf.Sin(theta2_) + v43_now[2] * Mathf.Cos(theta2_);

            float vxz = vxy * Mathf.Cos(theta3_) - vyy * Mathf.Sin(theta3_);
            float vyz = vxy * Mathf.Sin(theta3_) + vyy * Mathf.Cos(theta3_);
            float vzz = vzy;

            v43_now_rotate[0] = vxz / L3;
            v43_now_rotate[1] = (vyz * Mathf.Cos(theta1_) - vzz * Mathf.Sin(theta1_)) / L3;
            v43_now_rotate[2] = (vyz * Mathf.Sin(theta1_) + vzz * Mathf.Cos(theta1_)) / L3;

        }

        public void vectorrotatexz(float theta1, float theta2, float theta3){
            float theta1_ = theta1 + 1.57f;
            if (Mathf.Abs(theta1) < 0.01f){
                theta1_ = theta1;
            }
            else if (theta1 > 0.01f){
                theta1_ = theta1 - 1.57f;
            }

            float theta2_ = theta2;
            float theta3_ = theta3 - ch_angle5;

            float vxy = v43_now[0] * Mathf.Cos(theta3_) - v43_now[1] * Mathf.Sin(theta3_);
            float vyy = v43_now[0] * Mathf.Sin(theta3_) + v43_now[1] * Mathf.Cos(theta3_);
            float vzy = v43_now[2];

            float vxz = vxy * Mathf.Cos(theta2_) + vzy * Mathf.Sin(theta2_);
            float vyz = vyy;
            float vzz = -1.0f * vxy * Mathf.Sin(theta2_) + vzy * Mathf.Cos(theta2_);

            v43_now_rotate[0] = vxz / L3;
            v43_now_rotate[1] = (vyz * Mathf.Cos(theta1_) - vzz * Mathf.Sin(theta1_)) / L3;
            v43_now_rotate[2] = (vyz * Mathf.Sin(theta1_) + vzz * Mathf.Cos(theta1_)) / L3;

        }

        void eedir(){
            v43_now[0] = xyz4[2] - xyz3[2];
            v43_now[1] = xyz4[0] - xyz3[0];
            v43_now[2] = xyz4[1] - xyz3[1];
        }

        void handud(float linZ){//ハンドを上げ下げする//pitch
            float pastValue = angle[3];
            angle[3] = pastValue - joy_mass * linZ;
            ch_angle3 += - joy_mass * linZ;
            kinetic();
        }

        void handlr(float angX){//ハンドを左右する//yaw
            float pastValue = angle[5];
            angle[5] = pastValue + joy_mass * angX;
            ch_angle5 +=  joy_mass * angX;
            kinetic();
        }

        void armupdown(bool up, float value, float origin){
            float pastXyzEe0 = xyz_ee[2];
            float pastXyzEe1 = xyz_ee[0];
            float pastXyzEe2 = xyz_ee[1];
            float joyControl = Mathf.Abs(origin - 1.0f) / 2.0f;

            if (up)
            {
                if (ik_check(4))
                {
                    // xyz_ee[2] = past_z + joy_mass;
                    vectorrotatexz(angle[4], 1.57f, 0.0f); // ベクトルを回転させる
                    xyz_ee[2] = pastXyzEe0 - v43_now_rotate[0] * value * joyControl;
                    xyz_ee[0] = pastXyzEe1 - v43_now_rotate[1] * value * joyControl;
                    xyz_ee[1] = pastXyzEe2 - v43_now_rotate[2] * value * joyControl;
                }
            }
            else if (!up)
            {
                if (ik_check(5))
                {
                    // xyz_ee[2] = past_z - joy_mass;
                    vectorrotatexz(angle[4], 1.57f, 0.0f); // ベクトルを回転させる
                    xyz_ee[2] = pastXyzEe0 + v43_now_rotate[0] * value * joyControl;
                    xyz_ee[0] = pastXyzEe1 + v43_now_rotate[1] * value * joyControl;
                    xyz_ee[1] = pastXyzEe2 + v43_now_rotate[2] * value * joyControl;
                }
            }

            kinetic();
            eedir();
            inverse_kinetic();
            angle_publish();
        }

        void handroll(float roll){
            float pastRoll = angle[4];
            angle[4] = pastRoll + roll * joy_mass / 180.0f * 3.14f;
            ch_angle4 = pastRoll + roll * joy_mass / 180.0f * 3.14f;
        }


        void joy2xyz(){
            // msg から各種変数を取得
            float angY = ps5controller_arm.Rstick_lr*1.0f; //RStickの左右の倒し具合、角度関係:右-1.0~1.0左
            float angZ = ps5controller_arm.Rstick_ud*-1.0f; // msg.angular.z;//RStickの上下の倒し具合、角度関係:下-1.0~1.0上
            float angX = 0.0f; // msg.angular.x;//十字上下　上正
            if(ps5controller_arm.button_cross_down){angX=1.0f;}
            else if(ps5controller_arm.button_cross_up){angX=-1.0f;}
            float linX = 1.0f; // msg.linear.x;//R2 デフォルト1、押すと-1
            if(ps5controller_arm.button_R2){linX=-1.0f;}
            float linY = 1.0f; // msg.linear.y;//L2 デフォルト1、押すと-1
            if(ps5controller_arm.button_L2){linY=-1.0f;}
            float linZ = 0.0f; // msg.linear.z;//十字左右　左正
            if(ps5controller_arm.button_cross_left){linZ=1.0f;}
            else if(ps5controller_arm.button_cross_right){linZ=-1.0f;}

            float pastXyzEe0 = xyz_ee[2];
            float pastXyzEe1 = xyz_ee[0];
            float pastXyzEe2 = xyz_ee[1];

            if (Mathf.Abs(angY) < 0.01f && Mathf.Abs(angZ) < 0.01f){//微小な値を無視する
                angY = 0;
                angZ = 0;
            }

            if (Mathf.Abs(angY) > Mathf.Abs(angZ)){//左右のほうが大きいとき
                if (back_mode){
                    turntable(angY);
                }
                else{
                    if (angY > 0.01f && ik_check(2)){
                        float joy1 = joy_mass * angY;
                        //Debug.LogError("3");
                        if (camera_sync_sw){
                            vectorrotatexy(angle[4], 0.0f, 1.57f);
                            xyz_ee[2] = pastXyzEe0 + angY * joy1 * v43_now_rotate[0];
                            xyz_ee[0] = pastXyzEe1 + angY * joy1 * v43_now_rotate[1];
                            xyz_ee[1] = pastXyzEe2 + angY * joy1 * v43_now_rotate[2];
                        }
                        else{
                            xyz_ee[0] = pastXyzEe1 + joy1;
                        }
                    }
                    else if (angY < -0.01f && ik_check(3)){
                        float joy2 = joy_mass * angY;
                        //Debug.LogError("4");
                        if (camera_sync_sw){
                            vectorrotatexy(angle[4], 0.0f, 1.57f);
                            xyz_ee[2] = pastXyzEe0 + angY * joy2 * v43_now_rotate[0];
                            xyz_ee[0] = pastXyzEe1 + angY * joy2 * v43_now_rotate[1];
                            xyz_ee[1] = pastXyzEe2 + angY * joy2 * v43_now_rotate[2];
                        }
                        else
                        {
                            xyz_ee[0] = pastXyzEe1 + joy2;
                        }
                    }
                }
            }
            else
            {//Debug.LogError(ik_check(0));
                if (angZ > 0.01f && ik_check(0))
                {
                    //Debug.LogError("1");
                    float joy3 = joy_mass * angZ;

                    if (camera_sync_sw)
                    {
                        xyz_ee[2] = pastXyzEe0 + angZ * joy3 * v43_now[0];
                        xyz_ee[0] = pastXyzEe1 + angZ * joy3 * v43_now[1];
                        xyz_ee[1] = pastXyzEe2 + angZ * joy3 * v43_now[2];
                    }
                    else
                    {
                        xyz_ee[2] = pastXyzEe0 + joy3;
                    }
                }
                else if (angZ < -0.01f && ik_check(1))
                {
                    //Debug.LogError("2");
                    float joy4 = joy_mass * angZ;

                    if (camera_sync_sw)
                    {
                        xyz_ee[2] = pastXyzEe0 + angZ * joy4 * v43_now[0];
                        xyz_ee[0] = pastXyzEe1 + angZ * joy4 * v43_now[1];
                        xyz_ee[1] = pastXyzEe2 + angZ * joy4 * v43_now[2];
                    }
                    else
                    {
                        xyz_ee[2] = pastXyzEe0 + joy4;
                    }
                }
            }

            if (Mathf.Abs(angX) > 0.5f){
                handud(angX);
            }
            else if (Mathf.Abs(linZ) > 0.5f){
                handlr(linZ);
            }

            if (!flipper_mode){
                if (linX < 0.9f)
                {
                    float joy5 = joy_mass * Mathf.Abs(linX - 1.0f) / 2.0f;
                    armupdown(true, joy5, linX);
                }
                else if (linY < 0.9f)
                {
                    float joy6 = joy_mass * Mathf.Abs(linY - 1.0f) / 2.0f;
                    armupdown(false, joy6, linY);
                }
            }

            kinetic();
            eedir();
            inverse_kinetic();

            if (float.IsNaN(angle[2])){
                //Debug.LogError("Dead Position");
            }
            else{
                if (camera_sync_sw){
                    angle_publish_camera();
                }
                else{
                    angle_publish();//カメラ用に作成する必要ある。
                }
            }
        }

        void turntable(float angY){

        }
        void angle_publish_camera(){
            publish_angle[0]=(angle[0]+ch_angle0)*Mathf.Rad2Deg;
            publish_angle[1]=angle[1]*Mathf.Rad2Deg;
            publish_angle[2]=-1.0f*angle[2]*Mathf.Rad2Deg;
            // publish_angle[3]=angle[3];
            angle[3]=3.14f-(3.14f-Mathf.Abs(angle[1]))-Mathf.Abs(angle[2])+ch_angle3;
            angle[3]=Mathf.Abs(angle[1])-Mathf.Abs(angle[2])-ch_angle3;
            // publish_angle[3]=(3.14f-(3.14f-Mathf.Abs(angle[1]))-Mathf.Abs(angle[2])+ch_angle3);
            publish_angle[3]=angle[3]*Mathf.Rad2Deg*-1.0f;
            // publish_angle[4]=angle[4]-theta4_diff_past;
            // publish_angle[4]=angle[4];
            publish_angle[4]=ch_angle4*Mathf.Rad2Deg;
            if(!back_mode){
            // publish_angle[5]=-1.0f*(-1.0f*angle[0]+ch_angle5);
            publish_angle[5]=(-1.0f*controll_theta5()*Mathf.Rad2Deg);
            angle[5]=1.0f*angle[0]+ch_angle5;
            
            }
            else{
                publish_angle[5]=ch_angle5*Mathf.Rad2Deg;
            }
            publish_angle[6]=angle[6]*Mathf.Rad2Deg;
            publish_angle[7]=angle[7]*Mathf.Rad2Deg;
        }
        void angle_publish(){//jc_arm_controlに渡す値。
            publish_angle[0]=angle[0]*Mathf.Rad2Deg;
            publish_angle[1]=angle[1]*Mathf.Rad2Deg;
            publish_angle[2]=angle[2]*Mathf.Rad2Deg*-1.0f;
            publish_angle[3]=angle[3]*Mathf.Rad2Deg;
            publish_angle[4]=angle[4]*Mathf.Rad2Deg;
            publish_angle[5]=angle[5]*Mathf.Rad2Deg;
            publish_angle[6]=angle[6]*Mathf.Rad2Deg;
            publish_angle[7]=angle[7]*Mathf.Rad2Deg;
        }
        float controll_theta5(){

            // float theta3 = angle[3];
            float theta3 = ch_angle3;
            float theta4 = -1.0f * angle[0] + ch_angle5;
            float l1 = Mathf.Abs(L3 * Mathf.Sin(theta3));
            float l2 = Mathf.Abs(L3 * Mathf.Cos(theta3));
            float l3 = Mathf.Abs(l2 * Mathf.Sin(theta4) / Mathf.Cos(theta4));
            float l4 = Mathf.Sqrt(Mathf.Pow(l2, 2) + Mathf.Pow(l3, 2));
            float l5 = Mathf.Sqrt(Mathf.Pow(l1, 2) + Mathf.Pow(l4, 2));

            float res = Mathf.Acos((Mathf.Pow(L3, 2) + Mathf.Pow(l5, 2) - Mathf.Pow(l3, 2)) / (2 * L3 * l5)) * theta4 / Mathf.Abs(theta4);

            if (float.IsNaN(res))
            {
                res = 0.0f;
            }
            return res;
        }


    
}



