using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
// RollerAgent
public class ufm_agent_nolearning : Agent
{
    [Header("アタッチする必要あり9つ")]
    public GameObject top_object;//prefabの頂点にあるオブジェクト。これでリセット。
    public GameObject create_prefab;
    public GameObject rs_caribrate_obj;//realsenseのcaribrateを使うため。
    public GameObject click_point_go;//clickしたときのgoを保存。
    public GameObject ros_pubsub_ai_go;//実機からの値を得る
    public GameObject realbody_trigger_go;//現実パラメータ反映したロボットとcpとの判定に使用
    public GameObject top_point_origin4real;
    public GameObject top_point_dir4real;
    public GameObject all_button_control_go;//現在の自律か遠隔かのモードを取得
    [Header("ここまで")]
    [HideInInspector]
    private imax_flipper_control_fl_ML[] flipper_FL_ml = new imax_flipper_control_fl_ML[1];
    [HideInInspector]
    private imax_flipper_control_rl_ML[] flipper_RL_ml = new imax_flipper_control_rl_ML[1];
    [HideInInspector]
    private imax_flipper_control_fr_ML[] flipper_FR_ml = new imax_flipper_control_fr_ML[1];
    [HideInInspector]
    private imax_flipper_control_rr_ML[] flipper_RR_ml = new imax_flipper_control_rr_ML[1];
    [HideInInspector]
    private shock_detect_new shock;
    [HideInInspector]
    public float max_step=10000;
    [HideInInspector]
    private rs_origin_ufm[] realsense = new rs_origin_ufm[1];
    [HideInInspector]
    public nolearning_cp_trigger[] cp_trigger=new nolearning_cp_trigger[1];
    [HideInInspector]
    public cp_trigger4real cp_trigger4real;
    [HideInInspector]
    private touch_wall[] touch_wall=new touch_wall[1];
    [HideInInspector]
    public rs_caribrate rs_caribrate;
    [HideInInspector]
    private ros_pubsub_ai ros_pubsub_ai;
    [HideInInspector]
    public Unity.Robotics.UrdfImporter.Control.Controller_imax_ML urdf_control;
    private float[] realsense_val;
    private all_button_controll all_button_controll;
    
    //学習を始める最初の角度
    [HideInInspector]
    public GameObject body_state;//状態量を得るのに使用
    [HideInInspector]
    public GameObject body;//cpにふれたかの判定に使用
    [HideInInspector]
    public GameObject flipper_RL;
    [HideInInspector]
    public GameObject flipper_FL;
    [HideInInspector]
    public GameObject flipper_FR;
    [HideInInspector]
    public GameObject flipper_RR;
    [HideInInspector]
    public GameObject top_point_origin;
    [HideInInspector]
    public GameObject top_point_dir;
    [HideInInspector]
    public GameObject ray;
    [HideInInspector]
    public GameObject start;
    [HideInInspector]
    public GameObject prefab;
    [HideInInspector]
    private float distanceToTarget_past=1000.0f;
    // [HideInInspector]
    public float distanceTocp=0.0f;//cpとの距離
    [HideInInspector]
    private float angleTocp=0.0f;//cpとの間の角度
    public float distanceTocp4real=0.0f;//real
    public float angleTocp4real=0.0f;//real
    [HideInInspector]
    private float angleToTarget_past=180.0f;
    [HideInInspector]
    private float max_distance = 0.01245f;//actionGoが１で進める最大距離
    [HideInInspector]
    private float dis2target_past = 0.0f;//targetとの2次元距離過去
    [HideInInspector]
    private Vector3 cp_dis_vector=Vector3.zero;//cpとの二次元距離算出に使用
    private Vector3 cp_dis_vector4real=Vector3.zero;//real用

    //AIにより決定した値
    [HideInInspector]
    public float action_Go_Left=0.0f;//imax_body_control_lefが取得する値
    // [HideInInspector]
    public float action_Go_Right=0.0f;//imax_body_control_rightが取得する値
    // [HideInInspector]
    public float action_FrontL=0.0f;//imax_flipper_control_LF_MLに渡す値
    // [HideInInspector]
    public float action_FrontR=0.0f;//imax_flipper_control_RF_MLに渡す値
    // [HideInInspector]
    public float action_RearL=0.0f;//imax_flipper_control_LR_MLに渡す値
    // [HideInInspector]
    public float action_RearR=0.0f;//imax_flipper_control_RR_MLに渡す値
    [HideInInspector]
    private float distanceToTarget_start;//初期ポジとtargetの距離、正規化するのに使用
    [HideInInspector]
    private float body_pitch_x=0.0f;
    private float seconds=0.0f;

    // brainに使用
    StreamWriter file;
    [HideInInspector]
    [Header("データを取るときにtrue")]
    public string file_name;
    [HideInInspector]
    public bool brain_on;
    private float time_brain=0;//
    private float time_memo_brain=0;//
    private float time_past_brain=0;//
    [HideInInspector]
    public Vector3 initial_pos_dt;//最初のpositionを保存
    private float[] past_xyzrpy4vel = new float[6];
    // [HideInInspector]
    public float[] now_xyzrpy4vel = new float[6];
    public float[] vel_xyzrpy = new float[6];
    [HideInInspector]
    public Vector3 initial_pos_dt4real;//最初のpositionを保存
    private float[] past_xyzrpy4vel4real = new float[6];
    // [HideInInspector]
    public float[] now_xyzrpy4vel4real = new float[6];
    private float[] vel_xyzrpy4real = new float[6];
    
    private int file_num=1;

    //報酬、ステップ量、接触回数、失敗（転倒、カウント超過を分ける）のデータをリストに保存
    private int count_episode=0;

    [Header("フリッパーのみ推論")]
    public bool semi_auto_flipper = false;//フリッパーのみ推論するときは、falseにする。
    private Vector3 prefab_pos = new Vector3(0.0f,0.0f,0.0f);//prefab生成場所
    //クリックして作成したcp方向へいく。↓
    [Header("クリック作成したcpのほうに行きたいとき")]
    public bool create_cp_target=false;
    [HideInInspector]
    public int click_tf_index=0;//clicktarget時にインデックス指定に使用
    private create_toggle4checkpoint create_toggle4checkpoint;
    [Header("simのみをcsvに出力するとき,1のとき出力せず")]
    public string file_name_input="1";
    [Header("実機、simどちらも出力。")]
    public bool real_and_sim_input=false;//同時に出力する場合
    [Header("実機使うとき、実機の値をcsvに出力、名前はfile_name_inputである")]
    public bool real_world = false;//状態の入力を実機からの生データにする。
    private bool output_nninput=false;
    StreamWriter file_input;
    StreamWriter file_input_realsense;
    [Header("フリッパの動かし方")]
    public bool non_sync;
    public bool all_sync;
    public bool LR_sync;
    public bool FB_sync;
    public bool Tui_sync;
    //↑
    // 初期化時に呼ばれる
    private float past_y = 0.0f;
    //robotが通過したところに線を引く。
    public bool robot_trajctory_on = true;
    private Vector3 rt_past_pos=new Vector3(0.0f,0.0f,0.0f);
    private Vector3 rt_now_pos = new Vector3(0.0f,0.0f,0.0f);
    [SerializeField] Material lineMaterial;
    [SerializeField] Color lineColor;
    [Range(0.1f, 0.5f)]
    [SerializeField] float lineWidth;
    public List<LineRenderer> lineRenderers;
    //robotが通過したところのチェックポイントを保存
    [HideInInspector]
    public bool robot_trajctory_save = false;
    [HideInInspector]
    public List<Vector3> rt_tf_list = new List<Vector3>();//robotのtrajectoryのtfを保存する
    [HideInInspector]
    public List<Quaternion> rt_rot_list = new List<Quaternion>();//robotのrotation

    private float epos_rate =0.0f;
    private float epos_rate_ =0.0f;
    private float[] before_target = {0.0f,0.0f,0.0f,0.0f};

    void Start(){
        all_button_controll = all_button_control_go.GetComponent<all_button_controll>(); 
        //初期のpositionとrotationを格納
        // body_initial_pos = body.transform.position;
        if(robot_trajctory_on){//robot_trajectory用
            lineRenderers = new List<LineRenderer>();
            //空のゲームオブジェクト作成
            GameObject lineObj = new GameObject();
            //オブジェクトの名前をStrokeに変更
            lineObj.name = "Stroke";
            //lineObjにLineRendereコンポーネント追加
            lineObj.AddComponent<LineRenderer>();
            //lineRendererリストにlineObjを追加
            lineRenderers.Add(lineObj.GetComponent<LineRenderer>());
            //lineObjを自身の子要素に設定
            lineObj.transform.SetParent(transform);
            lineObj.GetComponent<LineRenderer>().numCapVertices = 10;
            //線をつなぐ点を0に初期化
            lineRenderers.Last().positionCount = 0;
            //マテリアルを初期化
            lineRenderers.Last().material = lineMaterial;
            //色の初期化
            lineRenderers.Last().material.color = lineColor;
            //太さの初期化
            lineRenderers.Last().startWidth = lineWidth;
            lineRenderers.Last().endWidth = lineWidth;
        }
        
        if(real_world){
            ros_pubsub_ai = ros_pubsub_ai_go.GetComponent<ros_pubsub_ai>();
        }
        if(create_cp_target){
            create_toggle4checkpoint = click_point_go.GetComponent<create_toggle4checkpoint>();
        }
        if(file_name_input!="1"){
            output_nninput=true;
            time_memo_brain = Time.realtimeSinceStartup;
            if(real_and_sim_input){
                    file_input=new StreamWriter("Assets/csv_file/input_check/0129/"+file_name_input+"_simandreal.csv",false);
                    file_input_realsense=new StreamWriter("Assets/csv_file/input_check/0129/"+file_name_input+"_rs_simandreal.csv",false);
                    file_input.WriteLine("time(s)"+","+"rpitch(deg)"+","+"spitch(deg)"+","+"ryaw(deg)"+","+"syaw(deg)"+","+"rroll(deg)"+","+"sroll(deg)"+","+"rLF"+","+"sLF"+","+"rLR"+","+"sLR"+","+"rRF"+","+"sRF"+","+"rRR"+","+"sRR"+","+"rx"+","+"sx"+","+"ry"+","+"sy"+","+"rz"+","+"sz"+","+"rdis2target"+","+"sdis2target"+","+"rvel_x"+","+"svel_x"+","+"rvel_y"+","+"svel_y"+","+"rvel_z"+","+"svel_z"+","+"rrot_x"+","+"srot_x"+","+"rrot_y"+","+"srot_y"+","+"rrot_z"+","+"srot_z");
                    string sentense="";
                    for(int i=0;i<25;i++){
                        sentense = sentense + "r"+i.ToString()+","+"s"+i.ToString()+",";
                    }
                    file_input_realsense.WriteLine(sentense);
            
                    return;
                }
                else if(real_world){
                    file_input=new StreamWriter("Assets/csv_file/input_check/0129/"+file_name_input+"_real.csv",false);
                    file_input_realsense=new StreamWriter("Assets/csv_file/input_check/0129/"+file_name_input+"_rs_real.csv",false);
                    file_input.WriteLine("time(s)"+","+"rpitch(deg)"+","+"spitch(deg)"+","+"ryaw(deg)"+","+"syaw(deg)"+","+"rroll(deg)"+","+"sroll(deg)"+","+"rLF"+","+"sLF"+","+"rLR"+","+"sLR"+","+"rRF"+","+"sRF"+","+"rRR"+","+"sRR"+","+"rx"+","+"sx"+","+"ry"+","+"sy"+","+"rz"+","+"sz"+","+"rdis2target"+","+"sdis2target"+","+"rvel_x"+","+"svel_x"+","+"rvel_y"+","+"svel_y"+","+"rvel_z"+","+"svel_z"+","+"rrot_x"+","+"srot_x"+","+"rrot_y"+","+"srot_y"+","+"rrot_z"+","+"srot_z");
                    string sentense="";
                    for(int i=0;i<25;i++){
                        sentense = sentense + "r"+i.ToString()+","+"s"+i.ToString()+",";
                    }
                    file_input_realsense.WriteLine(sentense);
                    // file_input_realsense.WriteLine("r0"+","+"s0"+","+"r1"+","+"s1"+","+"r2"+","+"s2"+","+"r3"+","+"s3"+","+"r4"+","+"s4"+","+"r5"+","+"s5"+","+"r6"+","+"s6"+","+"r7"+","+"s7"+","+"r8"+","+"s8"+","+"r9"+","+"s9"+","+"r10"+","+"s10"+","+"r11"+","+"s11"+","+"r12"+","+"s12"+","+"r13"+","+"s13"+","+"r14"+","+"s14");
            
                    return;
                } 
                
                else{
                    file_input=new StreamWriter("Assets/csv_file/input_check/0129/"+file_name_input+"_sim.csv",false);
                    file_input_realsense=new StreamWriter("Assets/csv_file/input_check/0129/"+file_name_input+"_rs_sim.csv",false);
                    file_input.WriteLine("time(s)"+","+"rpitch(deg)"+","+"spitch(deg)"+","+"ryaw(deg)"+","+"syaw(deg)"+","+"rroll(deg)"+","+"sroll(deg)"+","+"rLF"+","+"sLF"+","+"rLR"+","+"sLR"+","+"rRF"+","+"sRF"+","+"rRR"+","+"sRR"+","+"rx"+","+"sx"+","+"ry"+","+"sy"+","+"rz"+","+"sz"+","+"rdis2target"+","+"sdis2target"+","+"rvel_x"+","+"svel_x"+","+"rvel_y"+","+"svel_y"+","+"rvel_z"+","+"svel_z"+","+"rrot_x"+","+"srot_x"+","+"rrot_y"+","+"srot_y"+","+"rrot_z"+","+"srot_z");
                    string sentense="";
                    for(int i=0;i<25;i++){
                        sentense = sentense + "r"+i.ToString()+","+"s"+i.ToString()+",";
                    }
                    file_input_realsense.WriteLine(sentense);
                    return;
                }
                // file_input.WriteLine("time(s)"+","+"rpitch(deg)"+","+"spitch(deg)"+","+"ryaw(deg)"+","+"syaw(deg)"+","+"rroll(deg)"+","+"sroll(deg)"+","+"rLF"+","+"sLF"+","+"rLR"+","+"sLR"+","+"rRF"+","+"sRF"+","+"rRR"+","+"sRR"+","+"rx"+","+"sx"+","+"ry"+","+"sy"+","+"rz"+","+"sz"+","+"rdis2target"+","+"sdis2target");
                // string sentense="";
                // for(int i=0;i<25;i++){
                //     sentense = sentense + "r"+i.ToString()+","+"s"+i.ToString()+",";
                // }
                // file_input_realsense.WriteLine(sentense);
            }
        
        
    }

    void Curriculum_parameter(){
        
    }


    public override void Initialize()
    {
        rs_caribrate = rs_caribrate_obj.GetComponent<rs_caribrate>();
        //cpの一番最初のやつとの距離を初期値として保存
        if(brain_on){
            file=new StreamWriter("Assets/csv_file/brain/2024SICE/"+file_name+"_brain"+this.gameObject.name+".csv",false);
        }
        
    }

    // エピソード開始時に呼ばれる
    public override void OnEpisodeBegin()
    {   
        if(count_episode>0){
            Destroy(prefab);
        }
        count_episode+=1;
        prefab = Instantiate(create_prefab, prefab_pos+this.transform.position+new Vector3(0.0f,1.0f,0.0f), Quaternion.identity);
        prefab.transform.parent = this.transform;
        flipper_FL = transform.Find("base_link_and_nolearning(Clone)/base_link_cp/fl").gameObject;
        flipper_FR = transform.Find("base_link_and_nolearning(Clone)/base_link_cp/fr").gameObject;
        flipper_RL = transform.Find("base_link_and_nolearning(Clone)/base_link_cp/rl").gameObject;
        flipper_RR = transform.Find("base_link_and_nolearning(Clone)/base_link_cp/rr").gameObject;
        top_point_dir = transform.Find("base_link_and_nolearning(Clone)/base_link_cp/top_point_dir").gameObject;
        top_point_origin = transform.Find("base_link_and_nolearning(Clone)/base_link_cp/top_point_origin").gameObject;
        ray = transform.Find("base_link_and_nolearning(Clone)/base_link_cp/ray_origin").gameObject;
        body =transform.Find("base_link_and_nolearning(Clone)/base_link_cp/detect_touch").gameObject;
        body_state = transform.Find("base_link_and_nolearning(Clone)/base_link_cp").gameObject;

        flipper_FL_ml[0]=flipper_FL.GetComponent<imax_flipper_control_fl_ML>();
        flipper_FL_ml[0].Start();
        flipper_FR_ml[0]=flipper_FR.GetComponent<imax_flipper_control_fr_ML>();
        flipper_FR_ml[0].Start();
        flipper_RL_ml[0]=flipper_RL.GetComponent<imax_flipper_control_rl_ML>();
        flipper_RL_ml[0].Start();
        flipper_RR_ml[0]=flipper_RR.GetComponent<imax_flipper_control_rr_ML>();
        flipper_RR_ml[0].Start();
        urdf_control=this.GetComponent<Unity.Robotics.UrdfImporter.Control.Controller_imax_ML>();
        urdf_control.Start();

        realsense[0]=ray.GetComponent<rs_origin_ufm>();
        realsense[0].Start();
        touch_wall[0]=body.GetComponent<touch_wall>();
        touch_wall[0].Start();

        shock=body.GetComponent<shock_detect_new>();
        shock.Start();
        cp_trigger[0]=body.GetComponent<nolearning_cp_trigger>();
        cp_trigger[0].make_cp();
        cp_trigger[0].click_point_index = click_tf_index+1;
        cp_trigger4real=realbody_trigger_go.GetComponent<cp_trigger4real>();
        cp_trigger4real.click_point_index = click_tf_index+1;

        cp_dis_vector = top_point_origin.transform.position - cp_trigger[0].cp_list[0].transform.position;
        distanceToTarget_start=Mathf.Sqrt(Mathf.Pow(cp_dis_vector[0],2)+Mathf.Pow(cp_dis_vector[2],2));

        // distanceToTarget_start=Vector3.Distance(top_point_origin.transform.position,cp_trigger[0].cp_list[0].transform.position);
        
        cp_trigger[0].now_cp=0;//indexを0にする
        distanceToTarget_past=distanceToTarget_start;
        distanceTocp=distanceToTarget_start;

        if(start!=null){
            start.SetActive(true);
        }

        // angleToTarget_past=180.0f;
        prefab.name="base_link_and_nolearning(Clone)"+count_episode+this.gameObject.name;
        dis2target_past=0;
        if(brain_on){
            export_brain_start();
        }
        initial_pos_dt=body_state.transform.position;//y加速度求める用
        now_xyzrpy4vel[0]=initial_pos_dt.x;
        now_xyzrpy4vel[1]=initial_pos_dt.y;
        now_xyzrpy4vel[2]=initial_pos_dt.z;
        now_xyzrpy4vel[3]=body_state.transform.localEulerAngles.x;
        now_xyzrpy4vel[4]=body_state.transform.localEulerAngles.y;
        now_xyzrpy4vel[5]=body_state.transform.localEulerAngles.z;
        
        initial_pos_dt4real=realbody_trigger_go.transform.position;//y加速度求める用
        now_xyzrpy4vel4real[0]=initial_pos_dt4real.x;
        now_xyzrpy4vel4real[1]=initial_pos_dt4real.y;
        now_xyzrpy4vel4real[2]=initial_pos_dt4real.z;
        now_xyzrpy4vel4real[3]=realbody_trigger_go.transform.eulerAngles.x;
        now_xyzrpy4vel4real[4]=realbody_trigger_go.transform.eulerAngles.y;
        now_xyzrpy4vel4real[5]=realbody_trigger_go.transform.eulerAngles.z;
        for(int i=0;i<6;i++){
            vel_xyzrpy[i]=0.0f;
            past_xyzrpy4vel[i]=now_xyzrpy4vel[i];

            vel_xyzrpy4real[i]=0.0f;
            past_xyzrpy4vel4real[i]=now_xyzrpy4vel4real[i];
        }
    }

    // 状態取得時に呼ばれる
    public override void CollectObservations(VectorSensor sensor){
        if(!real_world){
            if(float.IsNaN(body_state.transform.localEulerAngles.x)||float.IsNaN(flipper_FL_ml[0].currentDrive.target)||float.IsNaN(flipper_FR_ml[0].currentDrive.target)||float.IsNaN(flipper_RL_ml[0].currentDrive.target)||float.IsNaN(flipper_RR_ml[0].currentDrive.target)||float.IsNaN(realsense[0].ray_distance[0])){
                for(int i=0;i<23;i++){
                    sensor.AddObservation(0.0f);
                }
            }
            else{
                sensor.AddObservation(distanceTocp);//距離
                sensor.AddObservation(angleTocp);//角度
                sensor.AddObservation(body_state.transform.localEulerAngles.x);
                sensor.AddObservation(body_state.transform.localEulerAngles.y);
                sensor.AddObservation(body_state.transform.localEulerAngles.z); //Agentの向き//yaw角のみなので、y値のみ、本来はz値だが、クローラモデルの関係から90°ずれている
                epos_rate_+=Time.deltaTime;
                if(epos_rate_>-1.0f){
                    sensor.AddObservation(flipper_RR_ml[0].currentDrive.target);
                    sensor.AddObservation(flipper_FL_ml[0].currentDrive.target);
                    sensor.AddObservation(flipper_FR_ml[0].currentDrive.target);
                    sensor.AddObservation(flipper_RL_ml[0].currentDrive.target);
                    before_target[0] = flipper_RR_ml[0].currentDrive.target;
                    before_target[1] = flipper_FL_ml[0].currentDrive.target;
                    before_target[2] = flipper_FR_ml[0].currentDrive.target;
                    before_target[3] = flipper_RL_ml[0].currentDrive.target;
                    epos_rate_=0.0f;
                }
                else{
                    sensor.AddObservation(before_target[0]);
                    sensor.AddObservation(before_target[1]);
                    sensor.AddObservation(before_target[2]);
                    sensor.AddObservation(before_target[3]);
                }
                
                realsense_val = realsense[0].ray_distance;//realsenseの数、行列変更したら毎回変更する必要あり。
                for(int i=0;i<realsense_val.Length;i++){
                    if(float.IsNaN(realsense_val[i])){
                        realsense_val[i]=0.0f;
                    }
                    else{
                        realsense_val[i]=realsense[0].ray_distance[i]*rs_caribrate.ray_caribrate[i];
                    }
                }
                sensor.AddObservation(realsense_val);
                sensor.AddObservation(vel_xyzrpy);
            }
        }
        else{
            sensor.AddObservation(distanceTocp4real);
            sensor.AddObservation(angleTocp4real);
            //sensor→x,pitch y,roll, z,yaw  unity pitch yaw rollの順
            sensor.AddObservation(ros_pubsub_ai.imu_data_list[0]);
            sensor.AddObservation(ros_pubsub_ai.imu_data_list[1]);
            sensor.AddObservation(ros_pubsub_ai.imu_data_list[2]);
            //sensor→fl,fr,rl,rr //unity→rr,fl,fr,rl
            sensor.AddObservation(ros_pubsub_ai.flipper_data_list_new[3]);
            sensor.AddObservation(ros_pubsub_ai.flipper_data_list_new[0]);
            sensor.AddObservation(ros_pubsub_ai.flipper_data_list_new[1]);
            sensor.AddObservation(ros_pubsub_ai.flipper_data_list_new[2]);
            sensor.AddObservation(ros_pubsub_ai.rs_data_list);
            sensor.AddObservation(vel_xyzrpy4real);
        }
        if(output_nninput){
            time_brain = Time.realtimeSinceStartup - time_memo_brain;
            if(real_and_sim_input){
                // file_input.WriteLine(","+ros_pubsub_ai.imu_data_list[0]+","+body_state.transform.localEulerAngles.x+","+ros_pubsub_ai.imu_data_list[2]+","+body_state.transform.localEulerAngles.y+","+ros_pubsub_ai.imu_data_list[1]+","+body_state.transform.localEulerAngles.z+","+ros_pubsub_ai.flipper_data_list[0]+","+flipper_FL_ml[0].currentDrive.target+","+ros_pubsub_ai.flipper_data_list[2]+","+flipper_RL_ml[0].currentDrive.target+","+ros_pubsub_ai.flipper_data_list[1]+","+flipper_FR_ml[0].currentDrive.target+","+ros_pubsub_ai.flipper_data_list[3]+","+flipper_RR_ml[0].currentDrive.target+","+ros_pubsub_ai.gazebo_pos[0]+","+top_point_origin.transform.position.x+","+ros_pubsub_ai.gazebo_pos[1]+","+top_point_origin.transform.position.y+","+ros_pubsub_ai.gazebo_pos[2]+","+top_point_origin.transform.position.z);
                file_input.WriteLine(time_brain+","+ros_pubsub_ai.imu_data_list[0]+","+body_state.transform.localEulerAngles.x+","+ros_pubsub_ai.imu_data_list[1]+","+body_state.transform.localEulerAngles.y+","+ros_pubsub_ai.imu_data_list[2]+","+body_state.transform.localEulerAngles.z+","+ros_pubsub_ai.flipper_data_list_new[0]+","+flipper_FL_ml[0].currentDrive.target+","+ros_pubsub_ai.flipper_data_list_new[2]+","+flipper_RL_ml[0].currentDrive.target+","+ros_pubsub_ai.flipper_data_list_new[1]+","+flipper_FR_ml[0].currentDrive.target+","+ros_pubsub_ai.flipper_data_list_new[3]+","+flipper_RR_ml[0].currentDrive.target+","+ros_pubsub_ai.gazebo_pos[0]+","+top_point_origin.transform.position.x+","+ros_pubsub_ai.gazebo_pos[1]+","+top_point_origin.transform.position.y+","+ros_pubsub_ai.gazebo_pos[2]+","+top_point_origin.transform.position.z);

                file_input_realsense.WriteLine(ros_pubsub_ai.rs_data_list[0]+","+realsense_val[0]+","+ros_pubsub_ai.rs_data_list[1]+","+realsense_val[1]+","+ros_pubsub_ai.rs_data_list[2]+","+realsense_val[2]+","+ros_pubsub_ai.rs_data_list[3]+","+realsense_val[3]+","+ros_pubsub_ai.rs_data_list[4]+","+realsense_val[4]+","+ros_pubsub_ai.rs_data_list[5]+","+realsense_val[5]+","+ros_pubsub_ai.rs_data_list[6]+","+realsense_val[6]+","+ros_pubsub_ai.rs_data_list[7]+","+realsense_val[7]+","+ros_pubsub_ai.rs_data_list[8]+","+realsense_val[8]+","+ros_pubsub_ai.rs_data_list[9]+","+realsense_val[9]+","+ros_pubsub_ai.rs_data_list[10]+","+realsense_val[10]+","+ros_pubsub_ai.rs_data_list[11]+","+realsense_val[11]+","+ros_pubsub_ai.rs_data_list[12]+","+realsense_val[12]+","+ros_pubsub_ai.rs_data_list[13]+","+realsense_val[13]+","+ros_pubsub_ai.rs_data_list[14]+","+realsense_val[14]);
                return;
            }
            else if(real_world){//現実の値のみ
                float[] gazebo_pos_float=new float[3];
                gazebo_pos_float[0]=ros_pubsub_ai.gazebo_pos[0];
                gazebo_pos_float[1]=ros_pubsub_ai.gazebo_pos[1];
                gazebo_pos_float[2]=ros_pubsub_ai.gazebo_pos[2];
                // file_input.WriteLine(","+ros_pubsub_ai.imu_data_list[0]+","+","+ros_pubsub_ai.imu_data_list[2]+","+","+ros_pubsub_ai.imu_data_list[1]+","+","+ros_pubsub_ai.flipper_data_list[0]+","+","+ros_pubsub_ai.flipper_data_list[2]+","+","+ros_pubsub_ai.flipper_data_list[1]+","+","+ros_pubsub_ai.flipper_data_list[3]+","+","+ros_pubsub_ai.gazebo_pos[0]+","+""+","+ros_pubsub_ai.gazebo_pos[1]+","+""+","+ros_pubsub_ai.gazebo_pos[2]+","+"");
                file_input.WriteLine(time_brain+","+ros_pubsub_ai.imu_data_list[0]+","+","+ros_pubsub_ai.imu_data_list[1]+","+","+ros_pubsub_ai.imu_data_list[2]+","+","+ros_pubsub_ai.flipper_data_list_new[0]+","+","+ros_pubsub_ai.flipper_data_list_new[2]+","+","+ros_pubsub_ai.flipper_data_list_new[1]+","+","+ros_pubsub_ai.flipper_data_list_new[3]+","+","+now_xyzrpy4vel4real[0]+","+""+","+now_xyzrpy4vel4real[1]+","+""+","+now_xyzrpy4vel4real[2]+","+","+distanceTocp4real+","+","+vel_xyzrpy4real[0]+","+","+vel_xyzrpy4real[1]+","+","+vel_xyzrpy4real[2]+","+","+vel_xyzrpy4real[3]+","+","+vel_xyzrpy4real[4]+","+","+vel_xyzrpy4real[5]);
                // file_input_realsense.WriteLine(ros_pubsub_ai.rs_data_list[0]+","+","+ros_pubsub_ai.rs_data_list[1]+","+","+ros_pubsub_ai.rs_data_list[2]+","+","+ros_pubsub_ai.rs_data_list[3]+","+","+ros_pubsub_ai.rs_data_list[4]+","+","+ros_pubsub_ai.rs_data_list[5]+","+","+ros_pubsub_ai.rs_data_list[6]+","+","+ros_pubsub_ai.rs_data_list[7]+","+","+ros_pubsub_ai.rs_data_list[8]+","+","+ros_pubsub_ai.rs_data_list[9]+","+","+ros_pubsub_ai.rs_data_list[10]+","+","+ros_pubsub_ai.rs_data_list[11]+","+","+ros_pubsub_ai.rs_data_list[12]+","+","+ros_pubsub_ai.rs_data_list[13]+","+","+ros_pubsub_ai.rs_data_list[14]);
                string sentense = "";
                for(int i=0;i<25;i++){
                    sentense = sentense + ros_pubsub_ai.rs_data_list[i]+","+",";
                }
                file_input_realsense.WriteLine(sentense);
                return;
            }
            else{//simのみ
                file_input.WriteLine(time_brain+","+","+now_xyzrpy4vel[3]+","+","+now_xyzrpy4vel[4]+","+","+now_xyzrpy4vel[5]+","+","+flipper_FL_ml[0].currentDrive.target+","+","+flipper_RL_ml[0].currentDrive.target+","+","+flipper_FR_ml[0].currentDrive.target+","+","+flipper_RR_ml[0].currentDrive.target+","+""+","+now_xyzrpy4vel[0]+","+""+","+now_xyzrpy4vel[1]+","+""+","+now_xyzrpy4vel[2]+","+","+distanceTocp+","+","+vel_xyzrpy[0]+","+","+vel_xyzrpy[1]+","+","+vel_xyzrpy[2]+","+","+vel_xyzrpy[3]+","+","+vel_xyzrpy[4]+","+","+vel_xyzrpy[5]);
                string sentense = "";
                for(int i=0;i<25;i++){
                    sentense = sentense +","+realsense_val[i]+",";
                }
                file_input_realsense.WriteLine(sentense);
                
                return;
            }
        }
        
        

    }

    // 行動実行時に呼ばれる
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int count_active = 0;

        for(int i=0;i<create_toggle4checkpoint.go_list.Count;i++){
            if(create_toggle4checkpoint.go_list[i].activeSelf==true){
                count_active++;
                break;
            }
           
        }

        if(count_active==0||create_toggle4checkpoint.go_list.Count==0){
            action_FrontL=0;
            action_FrontR=0;
            action_Go_Left=0;
            action_Go_Right=0;
            action_RearL=0;
            action_RearR=0;
            flipper_FL_ml[0].action_FrontL=action_FrontL;
            flipper_RL_ml[0].action_RearL=action_RearL;
            flipper_FR_ml[0].action_FrontR=action_FrontR;
            flipper_RR_ml[0].action_RearR=action_RearR;
            // if(all_button_controll.button_data_list[1]>0.5f||all_button_controll.button_data_list[2]>0.5f){
            //     return;
            // }
            return;
        }
        if(brain_on){
            export_excel();
        }
        //旋回と進むを同時にしないため、値は-1～1;
        if(!real_world){
            epos_rate+=Time.deltaTime;
            if(epos_rate>-1.0f){
                robot_Action(actionBuffers);
                epos_rate=0.0f;
            }
        }
        else{
            realrobot_action(actionBuffers);

        }
        
        //roll_z角を保存、0がうまく反映されないので、if文を用いて調整
        //速度計算sim用
        float roll_z=0.0f;
        if(!real_world){
            if(body_state.transform.localEulerAngles.z>180.0f){
            roll_z=body_state.transform.localEulerAngles.z-360.0f;
            }
            else{
                roll_z=body_state.transform.localEulerAngles.z;
            }        
            if(robot_trajctory_on){
                create_robo_trajectory();
            }
            if(body_state.transform.localEulerAngles.x>180.0f){
                now_xyzrpy4vel[3]=body_state.transform.localEulerAngles.x-360.0f;
            }
            else{
                now_xyzrpy4vel[3]=body_state.transform.localEulerAngles.x;
            }
            if(body_state.transform.localEulerAngles.x>180.0f){
                now_xyzrpy4vel[4]=body_state.transform.localEulerAngles.y-360.0f;
            }
            else{
                now_xyzrpy4vel[4]=body_state.transform.localEulerAngles.y;
            }
            if(body_state.transform.localEulerAngles.x>180.0f){
                now_xyzrpy4vel[5]=body_state.transform.localEulerAngles.z-360.0f;
            }
            else{
                now_xyzrpy4vel[5]=body_state.transform.localEulerAngles.z;
            }
            now_xyzrpy4vel[0]=body_state.transform.position.x-initial_pos_dt.x;
            now_xyzrpy4vel[1]=body_state.transform.position.y-initial_pos_dt.y;
            now_xyzrpy4vel[2]=body_state.transform.position.z-initial_pos_dt.z;
        }
        
        //速度計算real用
        else{
            if(realbody_trigger_go.transform.eulerAngles.z>180.0f){
            roll_z=realbody_trigger_go.transform.eulerAngles.z-360.0f;
            }
            else{
                roll_z=realbody_trigger_go.transform.eulerAngles.z;
            }        
            // if(robot_trajctory_on){
            //     create_robo_trajectory();
            // }
            if(realbody_trigger_go.transform.eulerAngles.x>180.0f){
                now_xyzrpy4vel4real[5]=realbody_trigger_go.transform.eulerAngles.x-360.0f;
            }
            else{
                now_xyzrpy4vel4real[5]=realbody_trigger_go.transform.eulerAngles.x;
            }
            float yaw_y = realbody_trigger_go.transform.eulerAngles.y-90.0f;
            if(yaw_y < 0.0f){
                yaw_y = 270.0f  + realbody_trigger_go.transform.eulerAngles.y;
            }
            if(yaw_y>180.0f){
                now_xyzrpy4vel4real[4]=yaw_y-360.0f;
            }
            else{
                now_xyzrpy4vel4real[4]=yaw_y;
            }
            if(realbody_trigger_go.transform.eulerAngles.x>180.0f){
                now_xyzrpy4vel4real[3]=-1.0f*(realbody_trigger_go.transform.eulerAngles.z-360.0f);
            }
            else{
                now_xyzrpy4vel4real[3]=-1.0f*(realbody_trigger_go.transform.eulerAngles.z);
            }
            now_xyzrpy4vel4real[0]=realbody_trigger_go.transform.position.x-initial_pos_dt4real.x;
            now_xyzrpy4vel4real[1]=realbody_trigger_go.transform.position.y-initial_pos_dt4real.y;
            now_xyzrpy4vel4real[2]=realbody_trigger_go.transform.position.z-initial_pos_dt4real.z;
        }
        seconds+=Time.deltaTime;
        if (seconds > 0.5f){//直線速度、角速度を求める
                for(int i=0;i<6;i++){
                    vel_xyzrpy[i]=(now_xyzrpy4vel[i]-past_xyzrpy4vel[i])/seconds;
                    past_xyzrpy4vel[i] = now_xyzrpy4vel[i];

                    vel_xyzrpy4real[i]=(now_xyzrpy4vel4real[i]-past_xyzrpy4vel4real[i])/seconds;
                    past_xyzrpy4vel4real[i] = now_xyzrpy4vel4real[i];
                }
                seconds=0.0f;
            }
        if(create_cp_target){//clickした箇所に行くときに使用
            if(cp_trigger[0].touch_clickpoint){
                cp_trigger[0].touch_clickpoint=false;
                create_toggle4checkpoint.go_list[click_tf_index].SetActive(false);
                click_tf_index++;
            }
            if(cp_trigger4real.touch_clickpoint){
                cp_trigger4real.touch_clickpoint=false;
                create_toggle4checkpoint.go_list[click_tf_index].SetActive(false);
                click_tf_index++;
            }
            if(create_toggle4checkpoint.tf_list.Count<=click_tf_index||create_toggle4checkpoint.tf_list[click_tf_index]!=null||create_toggle4checkpoint.tf_list.Count!=0){
                if(!create_toggle4checkpoint.go_list[click_tf_index].activeSelf){
                    click_tf_index++;
                    cp_trigger4real.click_point_index++;
                    cp_trigger[0].click_point_index++;
                }
                else{
                    //sim用
                    cp_dis_vector=create_toggle4checkpoint.tf_list[click_tf_index] - top_point_origin.transform.position;
                    Vector3 body_dir= top_point_dir.transform.position - top_point_origin.transform.position;
                    Vector3 plane_normal = Vector3.up;
                    var plane_cp_dis_vector=Vector3.ProjectOnPlane(cp_dis_vector,plane_normal);
                    var plane_body_dir = Vector3.ProjectOnPlane(body_dir,plane_normal);
                    angleTocp = Vector3.SignedAngle(plane_body_dir,plane_cp_dis_vector,plane_normal);
                    distanceTocp=Mathf.Sqrt(Mathf.Pow(cp_dis_vector[0],2)+Mathf.Pow(cp_dis_vector[2],2));
                    //real用
                    cp_dis_vector4real=create_toggle4checkpoint.tf_list[click_tf_index] - top_point_origin4real.transform.position;
                    Vector3 body_dir4real= top_point_dir4real.transform.position - top_point_origin4real.transform.position;
                    Vector3 plane_normal4real = Vector3.up;
                    var plane_cp_dis_vector4real=Vector3.ProjectOnPlane(cp_dis_vector4real,plane_normal4real);
                    var plane_body_dir4real = Vector3.ProjectOnPlane(body_dir4real,plane_normal4real);
                    angleTocp4real = Vector3.SignedAngle(plane_body_dir4real,plane_cp_dis_vector4real,plane_normal4real);
                    distanceTocp4real=Mathf.Sqrt(Mathf.Pow(cp_dis_vector4real[0],2)+Mathf.Pow(cp_dis_vector4real[2],2));
                }
                
            }
            else{
                //sim用
                angleTocp=0.0f;
                distanceTocp=0.0f;
                //real用
                angleTocp4real=0.0f;
                distanceTocp4real=0.0f;
            }
            if(distanceTocp<0.5f&&!real_world){
                cp_trigger[0].touch_clickpoint=true;
            }
            if(distanceTocp4real<0.5f&&real_world){
                cp_trigger4real.touch_clickpoint=true;
            }
            
        }
        

        if(body.transform.position.y<-2.0f){//落下
            if(brain_on){
                export_brain_goal("fall");
            }
            EndEpisode();
        }

        if(Mathf.Abs(body_pitch_x)>80.0f||Mathf.Abs(roll_z)>80.0f){
            if(brain_on){
                export_brain_goal("tumble");
            }
            EndEpisode();
        }       
        
    }
    public void index_reset_button(){//create_toggle4checkpointでdeleteボタンが押されたときに起動
        click_tf_index=0;
        cp_trigger[0].click_point_index=1;
        cp_trigger4real.click_point_index=1;
    }
    // ヒューリスティックモードの行動決定時に呼ばれる
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        float a=Input.GetAxis("Axis 2");
        float b=Input.GetAxis("Axis 1");
        float c=Input.GetAxis("Axis 6");
        float d=Input.GetAxis("Axis 3");

        var discreteActionsOut = actionsOut.DiscreteActions;
        if(Input.GetAxis("move")<0){
            discreteActionsOut[0]=0;
        }
        else if(Input.GetAxis("move")>0){
            discreteActionsOut[0]=1;
        }
        else if(Input.GetAxis("rotation")<0){
            discreteActionsOut[0]=3;
        }
        else if(Input.GetAxis("rotation")>0){
            discreteActionsOut[0]=4;
            // discreteActionsOut[0]=4;
        }
        else{
            discreteActionsOut[0]=2;
        }
        //Lstick↑↓
        if(Mathf.Abs(a)>Mathf.Abs(b)){
            if (a > 0f){
                discreteActionsOut[0]=0;
            }
            else if (a < 0f){
                discreteActionsOut[0]=1;
            }
        }
        else if(Mathf.Abs(a)<Mathf.Abs(b)){
            //Lstick→←
            if (b > 0f){
                 discreteActionsOut[0]=4;
            }
            else if (b < 0f){
                discreteActionsOut[0]=3;
            }
            else{
                discreteActionsOut[0]=2;
            }
        }
        else{
            discreteActionsOut[0]=2;
        }
        if(Input.GetKey(KeyCode.Q)||Input.GetKey(KeyCode.JoystickButton4)){//左前脚上げる
            discreteActionsOut[1]=0;
        }
        else if(Input.GetKey(KeyCode.E)||Input.GetKey(KeyCode.JoystickButton6)){//左前脚下げる
            discreteActionsOut[1]=2;
        }
        else{
            discreteActionsOut[1]=1;
        }
        if(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.JoystickButton5)){//右前脚上げる
            discreteActionsOut[2]=0;
        }
        else if(Input.GetKey(KeyCode.R)||Input.GetKey(KeyCode.JoystickButton7)){//右前脚下げる
            discreteActionsOut[2]=2;
        }
        else{
            discreteActionsOut[2]=1;
        }
        if(Input.GetKey(KeyCode.Z)||Input.GetAxis("Axis 8") > 0f){//左後ろ脚上げる
            discreteActionsOut[3]=0;
        }
        else if(Input.GetKey(KeyCode.C)||Input.GetAxis("Axis 8") < 0f){//左後ろ脚下げる
            discreteActionsOut[3]=2;
        }
        else{
            discreteActionsOut[3]=1;
        }
        if(Input.GetKey(KeyCode.X)||Input.GetKey(KeyCode.JoystickButton3)){//右後ろ脚上げる
            discreteActionsOut[4]=0;
        }
        else if(Input.GetKey(KeyCode.V)||Input.GetKey(KeyCode.JoystickButton1)){//右後ろ下げる
            discreteActionsOut[4]=2;
        }
        else{
            discreteActionsOut[4]=1;
        }

    }
    void export_excel(){//brainへ書き込み
               
        time_brain=Time.realtimeSinceStartup-time_memo_brain;
        if(time_brain-time_past_brain>0.2f||time_brain==0){
            float pitch=body_state.transform.localEulerAngles.x;
            float roll=body_state.transform.localEulerAngles.z;
            if(pitch>180.0f){
                pitch=pitch-360.0f;
            }
            if(roll>300){
                roll=roll-360.0f;
            }
            float x=body_state.transform.position.x-initial_pos_dt.x;
            float y=body_state.transform.position.y-initial_pos_dt.y;
            float z=body_state.transform.position.z-initial_pos_dt.z;
            float now_acc_y = (y-past_y)/(time_brain-time_past_brain)/(time_brain-time_past_brain);
            file.WriteLine(time_brain+","+pitch+","+roll+","+y+","+x+","+z+","+now_acc_y+","+flipper_FL_ml[0].currentDrive.target+","+flipper_RL_ml[0].currentDrive.target+","+flipper_FR_ml[0].currentDrive.target+","+flipper_RR_ml[0].currentDrive.target);
            file.Flush();
            time_past_brain=time_brain;
            past_y = y;
        }

    }
    void export_brain_start(){
        initial_pos_dt=body_state.transform.position;
        file.WriteLine("time(s)"+","+"pitch(deg)"+","+"roll(deg)"+","+"y(m)"+","+"x(m)"+","+"z(m)"+","+"y_acc"+","+"LF"+","+"LR"+","+"RF"+","+"RR"+","+file_num);
        time_memo_brain=Time.realtimeSinceStartup;
    }
    void export_brain_goal(string fault){
        file.WriteLine(fault);        
        time_brain=0;
        time_past_brain=0;
        Debug.LogError(file_num);
        file_num++;
        if(file_num==11){
            // Debug.LogError(file+"は30回行きました");
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }

    void robot_Action(ActionBuffers actionBuffers){
        if(!semi_auto_flipper){
            if(actionBuffers.DiscreteActions[0]==0){//back
                action_Go_Left=-10*0.22f/0.44f;
                action_Go_Right=10*0.22f/0.44f;
            }
            else if(actionBuffers.DiscreteActions[0]==1){//front
                action_Go_Left=10*0.22f/0.44f;
                action_Go_Right=-10*0.22f/0.44f;
            }
            else if(actionBuffers.DiscreteActions[0]==2){//nothing
                action_Go_Left=0;
                action_Go_Right=0;
            }
            else if(actionBuffers.DiscreteActions[0]==3){//right
                action_Go_Left= -4*80/56*2*70/90*75/71*0.5f;
                action_Go_Right= -4*80/56*2*70/90*75/71*0.5f;
            }
            else if(actionBuffers.DiscreteActions[0]==4){//left
                action_Go_Left=4*80/56*2*70/90*75/71*0.5f;
                action_Go_Right=4*80/56*2*70/90*75/71*0.5f;
            }
        }
        else{//フリッパーのみ推論時は十字キーで動かす
            if(Input.GetAxis("move")<0){
                action_Go_Left=-10.0f;
                action_Go_Right=-10.0f;
            }
            else if(Input.GetAxis("move")>0){
                action_Go_Left=10.0f;
                action_Go_Right=10.0f;
            }
            else if(Input.GetAxis("rotation")<0){
                action_Go_Left=-1.0f;
                action_Go_Right=1.0f;
            }
            else if(Input.GetAxis("rotation")>0){
                action_Go_Left=-1.0f;
                action_Go_Right=1.0f;
            }
            else{
                action_Go_Left=0.0f;
                action_Go_Right=0.0f;
            }
        }

        //non sync
        if(non_sync){
        if(actionBuffers.DiscreteActions[1]==0){action_FrontL=1;}
        else if(actionBuffers.DiscreteActions[1]==1){action_FrontL=0;}
        else if(actionBuffers.DiscreteActions[1]==2){action_FrontL=-1;}

        if(actionBuffers.DiscreteActions[2]==0){action_FrontR=1;}
        else if(actionBuffers.DiscreteActions[2]==1){action_FrontR=0;}
        else if(actionBuffers.DiscreteActions[2]==2){action_FrontR=-1;}

        if(actionBuffers.DiscreteActions[3]==0){action_RearL=1;}
        else if(actionBuffers.DiscreteActions[3]==1){action_RearL=0;}
        else if(actionBuffers.DiscreteActions[3]==2){action_RearL=-1;}

        if(actionBuffers.DiscreteActions[4]==0){action_RearR=1;}
        else if(actionBuffers.DiscreteActions[4]==1){action_RearR=0;}
        else if(actionBuffers.DiscreteActions[4]==2){action_RearR=-1;}
        }
        
        //all sync
        if(all_sync){
            if(actionBuffers.DiscreteActions[1]==0){
                action_FrontL=1;
                action_FrontR=1;
                action_RearL=1;
                action_RearR=1;
                }
            else if(actionBuffers.DiscreteActions[1]==1){
                action_FrontL=0;
                action_FrontR=0;
                action_RearL=0;
                action_RearR=0;
                }
            else if(actionBuffers.DiscreteActions[1]==2){
                action_FrontL=-1;
                action_FrontR=-1;
                action_RearL=-1;
                action_RearR=-1;
                }
        }
        
        if(FB_sync){
            if(actionBuffers.DiscreteActions[1]==0){
                action_FrontL=1;
                action_FrontR=1;
                }
            else if(actionBuffers.DiscreteActions[1]==1){
                action_FrontL=0;
                action_FrontR=0;
                }
            else if(actionBuffers.DiscreteActions[1]==2){
                action_FrontL=-1;
                action_FrontR=-1;
                }
            if(actionBuffers.DiscreteActions[2]==0){
                action_RearL=1;
                action_RearR=1;
                }
            else if(actionBuffers.DiscreteActions[2]==1){
                action_RearL=0;
                action_RearR=0;
                }
            else if(actionBuffers.DiscreteActions[2]==2){
                action_RearL=-1;
                action_RearR=-1;
                }
        }

        if(LR_sync){
            if(actionBuffers.DiscreteActions[1]==0){
                action_FrontL=1;
                action_RearL=1;
                }
            else if(actionBuffers.DiscreteActions[1]==1){
                action_FrontL=0;
                action_RearL=0;
                }
            else if(actionBuffers.DiscreteActions[1]==2){
                action_FrontL=-1;
                action_RearL=-1;
                }
            if(actionBuffers.DiscreteActions[2]==0){
                action_RearR=1;
                action_FrontR=1;
                }
            else if(actionBuffers.DiscreteActions[2]==1){
                action_FrontR=0;
                action_RearR=0;
                }
            else if(actionBuffers.DiscreteActions[2]==2){
                action_FrontR=-1;
                action_RearR=-1;
                }
        }
        if(Tui_sync){
            if(actionBuffers.DiscreteActions[1]==0){
                action_FrontL=1;
                action_RearR=1;
                }
            else if(actionBuffers.DiscreteActions[1]==1){
                action_FrontL=0;
                action_RearR=0;
                }
            else if(actionBuffers.DiscreteActions[1]==2){
                action_FrontL=-1;
                action_RearR=-1;
                }
            if(actionBuffers.DiscreteActions[2]==0){
                action_RearL=1;
                action_FrontR=1;
                }
            else if(actionBuffers.DiscreteActions[2]==1){
                action_FrontR=0;
                action_RearL=0;
                }
            else if(actionBuffers.DiscreteActions[2]==2){
                action_FrontR=-1;
                action_RearL=-1;
                }
        }
        
        flipper_FL_ml[0].action_FrontL=action_FrontL;
        flipper_RL_ml[0].action_RearL=action_RearL;
        flipper_FR_ml[0].action_FrontR=action_FrontR;
        flipper_RR_ml[0].action_RearR=action_RearR;
    }

    void realrobot_action(ActionBuffers actionBuffers){
        if(ros_pubsub_ai.button_data_list[0]>0.5f){//遠隔操作
            action_Go_Left=0.0f;
            action_Go_Right=0.0f;
            action_FrontL=0.0f;
            action_RearL=0.0f;
            action_FrontR=0.0f;
            action_RearR=0.0f;
            flipper_FL_ml[0].action_FrontL=action_FrontL;
            flipper_RL_ml[0].action_RearL=action_RearL;
            flipper_FR_ml[0].action_FrontR=action_FrontR;
            flipper_RR_ml[0].action_RearR=action_RearR;
        }
        else if(ros_pubsub_ai.button_data_list[1]>0.5f){//半自律
            action_Go_Left=0.0f;
            action_Go_Right=0.0f;
            if(non_sync){
                if(actionBuffers.DiscreteActions[1]==0){action_FrontL=1;}
                else if(actionBuffers.DiscreteActions[1]==1){action_FrontL=0;}
                else if(actionBuffers.DiscreteActions[1]==2){action_FrontL=-1;}

                if(actionBuffers.DiscreteActions[2]==0){action_FrontR=1;}
                else if(actionBuffers.DiscreteActions[2]==1){action_FrontR=0;}
                else if(actionBuffers.DiscreteActions[2]==2){action_FrontR=-1;}

                if(actionBuffers.DiscreteActions[3]==0){action_RearL=1;}
                else if(actionBuffers.DiscreteActions[3]==1){action_RearL=0;}
                else if(actionBuffers.DiscreteActions[3]==2){action_RearL=-1;}

                if(actionBuffers.DiscreteActions[4]==0){action_RearR=1;}
                else if(actionBuffers.DiscreteActions[4]==1){action_RearR=0;}
                else if(actionBuffers.DiscreteActions[4]==2){action_RearR=-1;}
            }
        
            //all sync
            if(all_sync){
                if(actionBuffers.DiscreteActions[1]==0){
                    action_FrontL=1;
                    action_FrontR=1;
                    action_RearL=1;
                    action_RearR=1;
                    }
                else if(actionBuffers.DiscreteActions[1]==1){
                    action_FrontL=0;
                    action_FrontR=0;
                    action_RearL=0;
                    action_RearR=0;
                    }
                else if(actionBuffers.DiscreteActions[1]==2){
                    action_FrontL=-1;
                    action_FrontR=-1;
                    action_RearL=-1;
                    action_RearR=-1;
                    }
            }
            
            if(FB_sync){
                if(actionBuffers.DiscreteActions[1]==0){
                    action_FrontL=1;
                    action_FrontR=1;
                    }
                else if(actionBuffers.DiscreteActions[1]==1){
                    action_FrontL=0;
                    action_FrontR=0;
                    }
                else if(actionBuffers.DiscreteActions[1]==2){
                    action_FrontL=-1;
                    action_FrontR=-1;
                    }
                if(actionBuffers.DiscreteActions[2]==0){
                    action_RearL=1;
                    action_RearR=1;
                    }
                else if(actionBuffers.DiscreteActions[2]==1){
                    action_RearL=0;
                    action_RearR=0;
                    }
                else if(actionBuffers.DiscreteActions[2]==2){
                    action_RearL=-1;
                    action_RearR=-1;
                    }
            }

            if(LR_sync){
                if(actionBuffers.DiscreteActions[1]==0){
                    action_FrontL=1;
                    action_RearL=1;
                    }
                else if(actionBuffers.DiscreteActions[1]==1){
                    action_FrontL=0;
                    action_RearL=0;
                    }
                else if(actionBuffers.DiscreteActions[1]==2){
                    action_FrontL=-1;
                    action_RearL=-1;
                    }
                if(actionBuffers.DiscreteActions[2]==0){
                    action_RearR=1;
                    action_FrontR=1;
                    }
                else if(actionBuffers.DiscreteActions[2]==1){
                    action_FrontR=0;
                    action_RearR=0;
                    }
                else if(actionBuffers.DiscreteActions[2]==2){
                    action_FrontR=-1;
                    action_RearR=-1;
                    }
            }
            if(Tui_sync){
                if(actionBuffers.DiscreteActions[1]==0){
                    action_FrontL=1;
                    action_RearR=1;
                    }
                else if(actionBuffers.DiscreteActions[1]==1){
                    action_FrontL=0;
                    action_RearR=0;
                    }
                else if(actionBuffers.DiscreteActions[1]==2){
                    action_FrontL=-1;
                    action_RearR=-1;
                    }
                if(actionBuffers.DiscreteActions[2]==0){
                    action_RearL=1;
                    action_FrontR=1;
                    }
                else if(actionBuffers.DiscreteActions[2]==1){
                    action_FrontR=0;
                    action_RearL=0;
                    }
                else if(actionBuffers.DiscreteActions[2]==2){
                    action_FrontR=-1;
                    action_RearL=-1;
                    }
            }

            flipper_FL_ml[0].action_FrontL=action_FrontL;
            flipper_RL_ml[0].action_RearL=action_RearL;
            flipper_FR_ml[0].action_FrontR=action_FrontR;
            flipper_RR_ml[0].action_RearR=action_RearR;

        }

        else if(ros_pubsub_ai.button_data_list[2]>0.5f){//自律走行
            if(actionBuffers.DiscreteActions[0]==0){//back
                action_Go_Left=-1;
                action_Go_Right=1;
                // Debug.LogError("back");
            }
            else if(actionBuffers.DiscreteActions[0]==1){//front
                action_Go_Left=1;
                action_Go_Right=-1;
                // Debug.LogError("front");
            }
            else if(actionBuffers.DiscreteActions[0]==2){//nothing
                action_Go_Left=0;
                action_Go_Right=0;
                // Debug.LogError("nothing");
            }
            else if(actionBuffers.DiscreteActions[0]==3){//right
                action_Go_Left=-1;
                action_Go_Right=-1;
                // Debug.LogError("right");
            }
            else if(actionBuffers.DiscreteActions[0]==4){//left
                action_Go_Left=1;
                action_Go_Right=1;
                // Debug.LogError("left");
            }
            if(non_sync){
                if(actionBuffers.DiscreteActions[1]==0){action_FrontL=1;}
                else if(actionBuffers.DiscreteActions[1]==1){action_FrontL=0;}
                else if(actionBuffers.DiscreteActions[1]==2){action_FrontL=-1;}

                if(actionBuffers.DiscreteActions[2]==0){action_FrontR=1;}
                else if(actionBuffers.DiscreteActions[2]==1){action_FrontR=0;}
                else if(actionBuffers.DiscreteActions[2]==2){action_FrontR=-1;}

                if(actionBuffers.DiscreteActions[3]==0){action_RearL=1;}
                else if(actionBuffers.DiscreteActions[3]==1){action_RearL=0;}
                else if(actionBuffers.DiscreteActions[3]==2){action_RearL=-1;}

                if(actionBuffers.DiscreteActions[4]==0){action_RearR=1;}
                else if(actionBuffers.DiscreteActions[4]==1){action_RearR=0;}
                else if(actionBuffers.DiscreteActions[4]==2){action_RearR=-1;}
            }
        
            //all sync
            if(all_sync){
                if(actionBuffers.DiscreteActions[1]==0){
                    action_FrontL=1;
                    action_FrontR=1;
                    action_RearL=1;
                    action_RearR=1;
                    }
                else if(actionBuffers.DiscreteActions[1]==1){
                    action_FrontL=0;
                    action_FrontR=0;
                    action_RearL=0;
                    action_RearR=0;
                    }
                else if(actionBuffers.DiscreteActions[1]==2){
                    action_FrontL=-1;
                    action_FrontR=-1;
                    action_RearL=-1;
                    action_RearR=-1;
                    }
            }
            
            if(FB_sync){
                if(actionBuffers.DiscreteActions[1]==0){
                    action_FrontL=1;
                    action_FrontR=1;
                    }
                else if(actionBuffers.DiscreteActions[1]==1){
                    action_FrontL=0;
                    action_FrontR=0;
                    }
                else if(actionBuffers.DiscreteActions[1]==2){
                    action_FrontL=-1;
                    action_FrontR=-1;
                    }
                if(actionBuffers.DiscreteActions[2]==0){
                    action_RearL=1;
                    action_RearR=1;
                    }
                else if(actionBuffers.DiscreteActions[2]==1){
                    action_RearL=0;
                    action_RearR=0;
                    }
                else if(actionBuffers.DiscreteActions[2]==2){
                    action_RearL=-1;
                    action_RearR=-1;
                    }
            }

            if(LR_sync){
                if(actionBuffers.DiscreteActions[1]==0){
                    action_FrontL=1;
                    action_RearL=1;
                    }
                else if(actionBuffers.DiscreteActions[1]==1){
                    action_FrontL=0;
                    action_RearL=0;
                    }
                else if(actionBuffers.DiscreteActions[1]==2){
                    action_FrontL=-1;
                    action_RearL=-1;
                    }
                if(actionBuffers.DiscreteActions[2]==0){
                    action_RearR=1;
                    action_FrontR=1;
                    }
                else if(actionBuffers.DiscreteActions[2]==1){
                    action_FrontR=0;
                    action_RearR=0;
                    }
                else if(actionBuffers.DiscreteActions[2]==2){
                    action_FrontR=-1;
                    action_RearR=-1;
                    }
            }
            if(Tui_sync){
                if(actionBuffers.DiscreteActions[1]==0){
                    action_FrontL=1;
                    action_RearR=1;
                    }
                else if(actionBuffers.DiscreteActions[1]==1){
                    action_FrontL=0;
                    action_RearR=0;
                    }
                else if(actionBuffers.DiscreteActions[1]==2){
                    action_FrontL=-1;
                    action_RearR=-1;
                    }
                if(actionBuffers.DiscreteActions[2]==0){
                    action_RearL=1;
                    action_FrontR=1;
                    }
                else if(actionBuffers.DiscreteActions[2]==1){
                    action_FrontR=0;
                    action_RearL=0;
                    }
                else if(actionBuffers.DiscreteActions[2]==2){
                    action_FrontR=-1;
                    action_RearL=-1;
                    }
                }
            flipper_FL_ml[0].action_FrontL=action_FrontL;
            flipper_RL_ml[0].action_RearL=action_RearL;
            flipper_FR_ml[0].action_FrontR=action_FrontR;
            flipper_RR_ml[0].action_RearR=action_RearR;
        }
    }
    void create_robo_trajectory(){
        rt_now_pos = body_state.transform.position;
        Vector3 worldPosition = new Vector3(body_state.transform.position.x,body_state.transform.position.y+0.02f,body_state.transform.position.z);
        Quaternion worldRotation = body_state.transform.rotation;
        if(real_world){
            rt_now_pos = realbody_trigger_go.transform.position;
            worldPosition = new Vector3(realbody_trigger_go.transform.position.x,realbody_trigger_go.transform.position.y+0.02f,realbody_trigger_go.transform.position.z);
            worldRotation = realbody_trigger_go.transform.rotation;
        } 
        float distance = Vector3.Distance(rt_now_pos, rt_past_pos);
        if(distance>0.3f){
            //スクリーン座標をワールド座標に変換
                       
            //ワールド座標をローカル座標に変換
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition.x, worldPosition.y, -1.0f);
            
            //lineRenderersの最後のlineObjのローカルポジションを上記のローカルポジションに設定
            lineRenderers.Last().transform.localPosition = localPosition;
            
            //lineObjの線と線をつなぐ点の数を更新
            lineRenderers.Last().positionCount += 1;
            
            //LineRendererコンポーネントリストを更新
            lineRenderers.Last().SetPosition(lineRenderers.Last().positionCount - 1, worldPosition);
            
            //あとから描いた線が上に来るように調整
            lineRenderers.Last().sortingOrder = lineRenderers.Count;
            rt_past_pos = rt_now_pos;
            if(robot_trajctory_save){
                rt_tf_list.Add(rt_now_pos);
                rt_rot_list.Add(worldRotation);
            }
        }
    }
}




