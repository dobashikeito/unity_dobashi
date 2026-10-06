using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using StringMsg = RosMessageTypes.Std.StringMsg;
using Float32MultiArrayMsg = RosMessageTypes.Std.Float32MultiArrayMsg;
using JointStateMsg = RosMessageTypes.Sensor.JointStateMsg;
using motor_msg = RosMessageTypes.Motor.FlipperMotorStateMsg;
using EmptyMsg = RosMessageTypes.Std.EmptyMsg;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Nav;


public class ros_pubsub_ai : MonoBehaviour{
    ROSConnection ros;

    public GameObject agent;//agentファイル
    public GameObject button_process;//button処理ファイル
    public GameObject create_toggle4checkpoint_go;//cp作成ファイル

    private all_button_controll all_button_controll;
    // private urdfmodel_agent_dp agent_script;
    // private ufm_agent_several_route agent_script;
    // private ufm_agent_linear_route agent_script;
    // private ufm_agent_krail_route agent_script;
    // private nmodel4_cp_8nozi agent_script;
    private ufm_agent_nolearning agent_script;
    private create_toggle4checkpoint create_toggle4checkpoint;
    public float[] rs_data_list=new float[15];
    public float[] imu_data_list=new float[3];
    [HideInInspector]
    public float[] flipper_data_list = new float[4];
    public float[] button_data_list = new float[3];
    public float[] imu_caribrate_list = new float[3];
    public float[] flipper_data_list_new = new float[4];
    private float[] flipper_data_list_new_ = new float[4];//過去の値と変化したかを確認するため
    public Vector3 gazebo_pos;
    private float[] imu_pm=new float[3];
    private float[] imu_pm4lidar=new float[3];
    private float rad2deg = 57.3f; //180/pi
    private float time=0.0f;
    private float time_past = 0.0f;
    void Start(){

        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Float32MultiArrayMsg>("/unity_ai_param");
        ros.RegisterPublisher<Float32MultiArrayMsg>("/unity_ai_mode");
        ros.RegisterPublisher<EmptyMsg>("/unity_reset_cp");
        ros.Subscribe<Float32MultiArrayMsg>("/rs_depth", rs_cb);//realsenseのパラメータ。
        // ros.Subscribe<Float32MultiArrayMsg>("/imu_rpy", imu_cb);//imuのパラメータ。
        // ros.Subscribe<Float32MultiArrayMsg>("/imu_rpy_ai", imu_cb);
        // ros.Subscribe<Float32MultiArrayMsg>("/joint_states",flipper_cb_input);//
        // ros.Subscribe<JointStateMsg>("/joint_states",flipper_cb_new);//flipperの現在値を取得20247/23→多分gazebo
        ros.Subscribe<Float32MultiArrayMsg>("/flipper4unity",flipper_cb_s1);//20241018 flipperの現在値。float配列型に変更してunityへ
        ros.Subscribe<Float32MultiArrayMsg>("/mtabmap/odom4unity",odom_cb);//rtabmapによるodomパラメータ取得 rsのtf
        ros.Subscribe<motor_msg>("/flipper_motor_state", flipper_cb);//flipperのパラメータ。
        ros.Subscribe<OdometryMsg>("/robot_tf", gazebo_tf_cb);
        ros.Subscribe<EmptyMsg>("/reset_imu", reset_imu_cb);
        ros.Subscribe<EmptyMsg>("/reset_cp", reset_cp_cb);
        ros.Subscribe<Float32MultiArrayMsg>("/cp_pose4unity",cppose_cb);
        //rosのボタン処理
        ros.Subscribe<EmptyMsg>("/showcp_button", showcp_button_cb);
        ros.Subscribe<EmptyMsg>("/save_trajectory_button", save_trajectory_button_cb);
        ros.Subscribe<EmptyMsg>("/play_button", play_button_cb);
        ros.Subscribe<EmptyMsg>("/inplay_button", inplay_button_cb);




        all_button_controll=button_process.GetComponent<all_button_controll>();
        // agent_script=agent.GetComponent<urdfmodel_agent_dp>();
        // agent_script=agent.GetComponent<ufm_agent_linear_route>();
        // agent_script=agent.GetComponent<ufm_agent_krail_route>();
        agent_script=agent.GetComponent<ufm_agent_nolearning>();
        // agent_script=agent.GetComponent<ufm_agent_several_route>();
        // agent_script=agent.GetComponent<nmodel4_cp_8nozi>();
        create_toggle4checkpoint = create_toggle4checkpoint_go.GetComponent<create_toggle4checkpoint>();
        for(int i=0;i<rs_data_list.Length;i++){
            rs_data_list[i]=0.0f;
        }
        for(int i=0;i<imu_data_list.Length;i++){
            imu_data_list[i]=0.0f;
            imu_caribrate_list[i]=0.0f;
        }
        for(int i=0;i<flipper_data_list.Length;i++){
            flipper_data_list[i]=0.0f;
        }
        imu_pm[0]=-1.0f*rad2deg;
        imu_pm[1]=-1.0f*rad2deg;
        imu_pm[2]=rad2deg;
        imu_pm4lidar[0]=-1.0f;
        imu_pm4lidar[1]=1.0f;
        imu_pm4lidar[2]=-1.0f;
    }
    void Update(){
        button_data_list=all_button_controll.button_data_list;
        ai_param_pub();
    }

    void rs_cb(Float32MultiArrayMsg msg){
        for(int i=0;i<rs_data_list.Length;i++){
            if((float)msg.data[i]>6.0f&&(float)msg.data[i]<10.0f){
                rs_data_list[i]=6.0f;
            }
            else if((float)msg.data[i]>10.0f){
                rs_data_list[i]=0.0f;
            }
            else{
                rs_data_list[i]=(float)msg.data[i];
            }
            
        }
        // Debug.Log(rs_data_list[0]);
    }

    void imu_cb(Float32MultiArrayMsg msg){
        //imuのキャリブラーションも同時に行う。imuはマイクロBがimaxのお尻方向に伸びていることを想定、ここから
        // if(imu_caribrate_list[0]==0.0f){ //初期値をキャリブレーション値に  
        //     for(int i=0;i<imu_caribrate_list.Length;i++){
        //         float data_raw = (float)msg.data[i]*180.0f/3.14f;
        //         if(data_raw<0.0f){//imu値が-180~180なので、unityの0~360に変更
        //             data_raw=data_raw+360.0f;
        //         }
        //         imu_caribrate_list[i]=data_raw;
        //     }
        // }

        // for(int i=0;i<imu_data_list.Length;i++){//常に更新
        //     float data = (float)msg.data[i]*180.0f/3.14f;//rad→deg
        //     if(data<0.0f){//imu値が-180~180なので、unityの0~360に変更
        //         data=data+360.0f;
        //     }
        //     data = data - imu_caribrate_list[i];
        //     if(data<0.0f){//キャリブレーション後の調整
        //         data=data+360.0f;
        //     }
        //     imu_data_list[i]=360.0f-data;//回転方向全部逆なので
        // }
        //ここまで

        //imuのキャリブラーションも同時に行う。imuはマイクロBがimaxの前方向に伸びていることを想定、ここから
        if(imu_caribrate_list[0]==0.0f){ //初期値をキャリブレーション値に  
            for(int i=0;i<imu_caribrate_list.Length;i++){
                float data_raw = (float)msg.data[i]*imu_pm[i];
                if(data_raw<0.0f){//imu値が-180~180なので、unityの0~360に変更
                    data_raw=data_raw+360.0f;
                }
                imu_caribrate_list[i]=data_raw;
            }
        }

        for(int i=0;i<imu_data_list.Length;i++){//常に更新
            float data = (float)msg.data[i]*imu_pm[i];//rad→deg
            if(data<0.0f){//imu値が-180~180なので、unityの0~360に変更
                data=data+360.0f;
            }
            data = data - imu_caribrate_list[i];
            if(data<0.0f){//キャリブレーション後の調整
                data=data+360.0f;
            }
            imu_data_list[i]=360.0f-data;//回転方向全部逆なので
        }
        //ここまで
    }

    void flipper_cb(motor_msg msg){

        flipper_data_list[0]=(float)msg.front_left_deg*-1.0f;
        flipper_data_list[1]=(float)msg.front_right_deg;
        flipper_data_list[2]=(float)msg.rear_left_deg*-1.0f;
        flipper_data_list[3]=(float)msg.rear_right_deg;

        // Debug.LogWarning(flipper_data_list[0]);
    }

    void flipper_cb_new(JointStateMsg msg){//このままagentに渡す
        flipper_data_list_new[0]=((float)msg.position[4]+1.57f)*rad2deg;//fl
        flipper_data_list_new[1]=((float)msg.position[6]+1.57f)*-rad2deg;//fr
        flipper_data_list_new[2]=((float)msg.position[5]+1.57f)*-rad2deg;//rl
        flipper_data_list_new[3]=((float)msg.position[7]+1.57f)*rad2deg;//rr
    }

    void flipper_cb_s1(Float32MultiArrayMsg msg){
        flipper_data_list_new[0]=((float)msg.data[0]+1.57f)*rad2deg;//fl
        flipper_data_list_new[1]=((float)msg.data[1]+1.57f)*-1.0f*rad2deg;//fr
        flipper_data_list_new[2]=((float)msg.data[2]+1.57f)*-1.0f*rad2deg;//rl
        flipper_data_list_new[3]=((float)msg.data[3]+1.57f)*rad2deg;//rr
        // time = Time.time;
        // if(flipper_data_list_new_[0]!=flipper_data_list_new[0]){
        //     flipper_data_list_new_[0]=((float)msg.data[0]+1.57f)*rad2deg;//fl
            
        //     Debug.LogWarning(time-time_past);
        //     time_past=time;
        // }
    }

    void gazebo_tf_cb(OdometryMsg msg){
        gazebo_pos = msg.pose.pose.position.From<FLU>();
        float gazebo_pos_cab_x = gazebo_pos[0] * -1.0f;
        float gazebo_pos_cab = gazebo_pos[2] * -1.0f;
        gazebo_pos[0] = gazebo_pos_cab_x;
        gazebo_pos[2] = gazebo_pos_cab;
        Quaternion rot = msg.pose.pose.orientation.From<FLU>();
    }

    void odom_cb(Float32MultiArrayMsg msg){//0,1,2→xyz(unity)→-yzx
        gazebo_pos[0] = msg.data[1];//
        gazebo_pos[1] = msg.data[2];//
        gazebo_pos[2] = -1.0f*msg.data[0];//
        //lidarのimu使用時
        if(imu_caribrate_list[0]==0.0f){ //初期値をキャリブレーション値に  
            for(int i=0;i<imu_caribrate_list.Length;i++){
                float data_raw = (float)msg.data[i+3]*imu_pm4lidar[i];
                if(data_raw<0.0f){//imu値が-180~180なので、unityの0~360に変更
                    data_raw=data_raw+360.0f;
                }
                imu_caribrate_list[i]=data_raw;
            }
        }

        for(int i=0;i<imu_data_list.Length;i++){//常に更新
            float data = (float)msg.data[i+3]*imu_pm4lidar[i];//rad→deg
            if(data<0.0f){//imu値が-180~180なので、unityの0~360に変更
                data=data+360.0f;
            }
            data = data - imu_caribrate_list[i];
            if(data<0.0f){//キャリブレーション後の調整
                data=data+360.0f;
            }
            imu_data_list[i]=360.0f-data;//回転方向全部逆なので
        }
    }
    void cppose_cb(Float32MultiArrayMsg msg){
        create_toggle4checkpoint.create_cp_byros(msg.data);
    }
    void reset_imu_cb(EmptyMsg msg){//imuのcaribrateを更新するため
        for(int i=0;i<imu_caribrate_list.Length;i++){
            imu_caribrate_list[i]=0.0f;
        }
    }
    void reset_cp_cb(EmptyMsg msg){//imuのcaribrateを更新するため
        create_toggle4checkpoint.delete_toggle_onclick();
        
    }
    public void reset_cp_send(){
        EmptyMsg msg=new EmptyMsg();
        ros.Publish("/unity_reset_cp",msg);
    }

    public void change_mode(int mode){//0remote,1semi,2auto
        Float32MultiArrayMsg ai_mode=new Float32MultiArrayMsg();
        ai_mode.data = new float[3];
        if(mode==0){
            ai_mode.data[0]=1.0f;
            ai_mode.data[1]=0.0f;
            ai_mode.data[2]=0.0f;
        }
        else if(mode==1){
            ai_mode.data[0]=0.0f;
            ai_mode.data[1]=1.0f;
            ai_mode.data[2]=0.0f;
        }
        else if(mode==2){
            ai_mode.data[0]=0.0f;
            ai_mode.data[1]=0.0f;
            ai_mode.data[2]=1.0f;
        }
        ros.Publish("/unity_ai_mode",ai_mode);
    }
    //rosのボタン処理コールバック
    void showcp_button_cb(EmptyMsg msg){
        create_toggle4checkpoint.show_toggle_onclick();
    }
    void save_trajectory_button_cb(EmptyMsg msg){
        create_toggle4checkpoint.robot_trajctory_save_start();
    }
    void play_button_cb(EmptyMsg msg){
        create_toggle4checkpoint.robot_trajctory_trace_play();
    }
    void inplay_button_cb(EmptyMsg msg){
        create_toggle4checkpoint.robot_trajctory_trace_inverse_play();
    }
    //
    void ai_param_pub(){//ros側、fl,fr,rl,rr 
        time=Time.time;
        Float32MultiArrayMsg ai_param=new Float32MultiArrayMsg();
        ai_param.data = new float[6];
        // if(time-time_past>0.28f)           
            
        ai_param.data[0]=agent_script.action_Go_Left;
        ai_param.data[1]=agent_script.action_Go_Right*-1.0f;
        ai_param.data[2]=agent_script.action_FrontL*1.0f;
        ai_param.data[3]=agent_script.action_FrontR;
        ai_param.data[4]=agent_script.action_RearL;
        ai_param.data[5]=agent_script.action_RearR*1.0f;

        ros.Publish("/unity_ai_param",ai_param);
        //     time_past=time;
        // }
        
        
    }

}
