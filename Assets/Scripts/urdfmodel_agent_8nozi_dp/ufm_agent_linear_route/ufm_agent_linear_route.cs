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
// RollerAgent
public class ufm_agent_linear_route : Agent
{
    private float sum_reward=0.0f;
    public Transform target; // TargetのTransform
    public Transform[] all_children_tf;
    public Vector3 body_tf;
    public List<Vector3> all_children_pos=new List<Vector3>();
    public List<Vector3> all_children_rot=new List<Vector3>();
    private imax_flipper_control_fl_ML[] flipper_FL_ml = new imax_flipper_control_fl_ML[1];
    private imax_flipper_control_rl_ML[] flipper_RL_ml = new imax_flipper_control_rl_ML[1];
    private imax_flipper_control_fr_ML[] flipper_FR_ml = new imax_flipper_control_fr_ML[1];
    private imax_flipper_control_rr_ML[] flipper_RR_ml = new imax_flipper_control_rr_ML[1];
    private shock_detect_new shock;
    public float max_step=10000;
    private rs_origin_ufm[] realsense = new rs_origin_ufm[1];
    public linear_cp_trigger[] cp_trigger=new linear_cp_trigger[1];
    private touch_wall[] touch_wall=new touch_wall[1];
    private rs_caribrate rs_caribrate;
    private ros_pubsub_ai ros_pubsub_ai;
    public Unity.Robotics.UrdfImporter.Control.Controller_imax_ML urdf_control;
    private float[] realsense_val;

    //学習を始める最初の位置
    private Vector3 target_initial_pos;
    private Vector3 body_initial_pos;
    //学習を始める最初の角度

    [SerializeField]
    public GameObject top_object;//prefabの頂点にあるオブジェクト。これでリセット。
    public GameObject body_state;//状態量を得るのに使用
    public GameObject body;//cpにふれたかの判定に使用
    public GameObject flipper_RL;
    public GameObject flipper_FL;
    public GameObject flipper_FR;
    public GameObject flipper_RR;
    public GameObject top_point_origin;
    public GameObject top_point_dir;
    public GameObject step;
    public GameObject ray;
    public GameObject start;
    public GameObject create_prefab;
    public GameObject prefab;
    public GameObject ros_pubsub_ai_go;//実機からの値を得る
    public GameObject rs_caribrate_obj;//realsenseのcaribrateを使うため。
    public GameObject click_point_go;//clickしたときのgoを保存。
    private float distanceToTarget_past=1000.0f;
    private float distanceTocp=0.0f;//cpとの距離
    private float angleTocp=0.0f;//cpとの間の角度
    private float angleToTarget_past=180.0f;
    private float max_distance = 0.01245f;//actionGoが１で進める最大距離
    private float dis2target_past = 0.0f;//targetとの2次元距離過去
    private float dis2target = 0.0f;//targetとの2次元距離

    private int count_disatance_near=0;
    private int count_disatance_far=0;
    private float count_dir_near=0.1f;
    private int count_dir_far=0;
    public int count_allstep=0;//前エピソードのステップ数
    private int count_step=0;//１エピソードのステップ数
    private int count_goal=0;//ゴールした回数
    private int count_goalstep=0;//ゴールした時のステップ数
    public int count_goal_continuous=0;//連続ゴール数を記録
    private int count_check_point;//チェックポイントの数
    private int now_cp_index=0;//現在のcp
    private int count_episode_reset=0;//エピソードの回数、リセットのために使用


    private float step_heihgt_flipper100;//フリッパの高さと釣り合う、stepのスケール
    private float step_height;
    public float step_height_persent=0.1f;
    private float config_shp=0.0f;
    private float step_height_persent_past;
    private Vector3 cp_dis_vector=Vector3.zero;//cpとの二次元距離算出に使用


    //AIにより決定した値
    public float action_Go_Left=0.0f;//imax_body_control_lefが取得する値
    public float action_Go_Right=0.0f;//imax_body_control_rightが取得する値
    public float action_FrontL=0.0f;//imax_flipper_control_LF_MLに渡す値
    public float action_FrontR=0.0f;//imax_flipper_control_RF_MLに渡す値
    public float action_RearL=0.0f;//imax_flipper_control_LR_MLに渡す値
    public float action_RearR=0.0f;//imax_flipper_control_RR_MLに渡す値
    private float approach_reward;//最初の100000ステップだけ与える報酬

    private float distanceToTarget_start;//初期ポジとtargetの距離、正規化するのに使用

    public float start_step_height=0.0f;//brainで最初の高さを設定

    private bool find_object=false;
    private int count_ray_distance_true=0;
    private float body_pitch_x=0.0f;
    public int[] goal_percent=new int[20];
    private int goal_percent_index=0;
    public int goal_percent_sum=0;
    private float seconds=0.0f;
    public bool fault_over=false;//brain中の失敗　カウントオーバー
    public bool fault_roll=false;//brin中の失敗　転倒
    public bool onoff_8wall_fault=false;//8wall間違えたら失敗   

    // brainに使用
    StreamWriter file;
    [Header("brainするときにtrue")]
    public string file_name;
    public bool brain_on;
    private float time_brain=0;//
    private float time_memo_brain=0;//
    private float time_past_brain=0;//
    private Vector3 initial_pos_dt;//最初のpositionを保存
    private int file_num=1;

    //報酬、ステップ量、接触回数、失敗（転倒、カウント超過を分ける）のデータをリストに保存
    private int count_episode=0;
    [HideInInspector]
    public List<float> reward_list=new List<float>();//1エピソードの累計報酬を保存
    [HideInInspector]
    public List<float> step_list=new List<float>();//1エピソードのステップ数を保存
    [HideInInspector]
    public List<float> touch_list=new List<float>();//1エピソードの接触回数を保存
    [HideInInspector]
    public List<float> roll_list=new List<float>();//転倒による失敗を保存、しなかったら0
    [HideInInspector]
    public List<float> count_over_list=new List<float>();//カウント超過による失敗を保存、しなかったら0
    [HideInInspector]
    public List<float> succes_list=new List<float>();//成功を保存、しなかったら0
    [HideInInspector]
    public List<float> curriculum_list=new List<float>();//成功を保存、しなかったら0
    [Header("フリッパーのみ推論")]
    public bool semi_auto_flipper = false;//フリッパーのみ推論するときは、falseにする。
    [Header("実機使うとき")]
    public bool real_world = false;//状態の入力を実機からの生データにする。
    private Vector3 prefab_pos = new Vector3(0.0f,0.0f,0.0f);//prefab生成場所
    //クリックして作成したcp方向へいく。↓
    [Header("クリック作成したcpのほうに行きたいとき")]
    public bool create_cp_target=false;
    private int click_tf_index=0;
    private create_toggle4checkpoint create_toggle4checkpoint;
    [Header("Inputをcsvに出力するとき,1のとき出力せず")]
    public string file_name_input="1";
    public bool real_and_sim_input=false;//同時に出力する場合
    private bool output_nninput=false;
    StreamWriter file_input;
    StreamWriter file_input_realsense;
    public int input_noise = 0;//noiseの%
    private float input_noise_max;
    private float input_noise_min;
    

    //↑
    // 初期化時に呼ばれる
    void Start(){
        //初期のpositionとrotationを格納
        // body_initial_pos = body.transform.position;
    
        if(real_world){
            ros_pubsub_ai = ros_pubsub_ai_go.GetComponent<ros_pubsub_ai>();
        }
        if(file_name_input!="1"){
            output_nninput=true;
	    if(real_and_sim_input){
                file_input=new StreamWriter("Assets/csv_file/input_check/"+file_name_input+"_simandreal.csv",false);
                file_input_realsense=new StreamWriter("Assets/csv_file/input_check/"+file_name_input+"_rs_simandreal.csv",false);
            }
            else if(real_world){
                file_input=new StreamWriter("Assets/csv_file/input_check/"+file_name_input+"_real.csv",false);
                file_input_realsense=new StreamWriter("Assets/csv_file/input_check/"+file_name_input+"_rs_real.csv",false);
            } 
            
            else{
                file_input=new StreamWriter("Assets/csv_file/input_check/"+file_name_input+"_sim.csv",false);
                file_input_realsense=new StreamWriter("Assets/csv_file/input_check/"+file_name_input+"_rs_sim.csv",false);
            }
            file_input.WriteLine("time(s)"+","+"rpitch(deg)"+","+"spitch(deg)"+","+"ryaw(deg)"+","+"syaw(deg)"+","+"rroll(deg)"+","+"sroll(deg)"+","+"rLF"+","+"sLF"+","+"rLR"+","+"sLR"+","+"rRF"+","+"sRF"+","+"rRR"+","+"sRR");
            file_input_realsense.WriteLine("r0"+","+"s0"+","+"r1"+","+"s1"+","+"r2"+","+"s2"+","+"r3"+","+"s3"+","+"r4"+","+"s4"+","+"r5"+","+"s5"+","+"r6"+","+"s6"+","+"r7"+","+"s7"+","+"r8"+","+"s8"+","+"r9"+","+"s9"+","+"r10"+","+"s10"+","+"r11"+","+"s11"+","+"r12"+","+"s12"+","+"r13"+","+"s13"+","+"r14"+","+"s14");

        }
        // create_toggle4checkpoint = click_point_go.GetComponent<create_toggle4checkpoint>();
        // rs_caribrate = rs_caribrate_obj.GetComponent<rs_caribrate>();
    }

    void Curriculum_parameter(){
        EnvironmentParameters curri_para=Academy.Instance.EnvironmentParameters;
        // approach_reward=curri_para.GetWithDefault("approach_reward",0.1f);
        // config_shp=curri_para.GetWithDefault("step_height",start_step_height);
        // if(count_goal_continuous>10){
        //     // //Debug.Log("step_height_persent::"+step_height_persent+"::allstep::"+count_allstep);
        //     // step_height_persent+=0.1f;
        // }
        // //Debug.Log(step_heihgt_flipper100);
        input_noise_max=1.0f+input_noise/100.0f;
        input_noise_min=1.0f-input_noise/100.0f;
        step.transform.localScale=new Vector3(step.transform.localScale.x,step_heihgt_flipper100*step_height_persent,step.transform.localScale.z);
        // step.transform.localScale=new Vector3(step.transform.localScale.x,step_heihgt_flipper100*config_shp,step.transform.localScale.z);
    }


    public override void Initialize()
    {
        input_noise_max=1.0f+input_noise/100.0f;
        input_noise_min=1.0f-input_noise/100.0f;
        rs_caribrate = rs_caribrate_obj.GetComponent<rs_caribrate>();
        //ゴールの確率をリストにまとめる
        for(int i=0;i<goal_percent.Length;i++){
            goal_percent[i]=0;
        }

        //stepの10割の高さを保存
        step_heihgt_flipper100=step.transform.localScale.y;
        step_height_persent_past=step_height_persent;
        Curriculum_parameter();
        //cpの一番最初のやつとの距離を初期値として保存
        if(brain_on){
            file=new StreamWriter("Assets/csv_file/brain/"+file_name+"_brain"+this.gameObject.name+".csv",false);
        }
        
        
    }

    // エピソード開始時に呼ばれる
    public override void OnEpisodeBegin()
    {   
        if(count_episode>0){
            Destroy(prefab);
            // flipper_FL=null;
        }
        // StartCoroutine(Time_stop());
        count_episode+=1;

        prefab = Instantiate(create_prefab, prefab_pos+this.transform.position, Quaternion.identity);
        //Debug.LogWarning("1");
        // StartCoroutine(Time_stop());
        prefab.transform.parent = this.transform;
        flipper_FL = transform.Find("base_link_and_linear_cp(Clone)/base_link_cp/fl").gameObject;
        flipper_FR = transform.Find("base_link_and_linear_cp(Clone)/base_link_cp/fr").gameObject;
        flipper_RL = transform.Find("base_link_and_linear_cp(Clone)/base_link_cp/rl").gameObject;
        flipper_RR = transform.Find("base_link_and_linear_cp(Clone)/base_link_cp/rr").gameObject;
        // step = transform.Find("astm3").gameObject;
        top_point_dir = transform.Find("base_link_and_linear_cp(Clone)/base_link_cp/top_point_dir").gameObject;
        top_point_origin = transform.Find("base_link_and_linear_cp(Clone)/base_link_cp/top_point_origin").gameObject;
        ray = transform.Find("base_link_and_linear_cp(Clone)/base_link_cp/ray_origin").gameObject;
        body =transform.Find("base_link_and_linear_cp(Clone)/base_link_cp/detect_touch").gameObject;
        body_state = transform.Find("base_link_and_linear_cp(Clone)/base_link_cp").gameObject;
        //Debug.LogWarning("2");
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
        //Debug.LogWarning("3");
        realsense[0]=ray.GetComponent<rs_origin_ufm>();
        realsense[0].Start();
        touch_wall[0]=body.GetComponent<touch_wall>();
        touch_wall[0].Start();
        //Debug.LogWarning("4");
        shock=body.GetComponent<shock_detect_new>();
        shock.Start();
        cp_trigger[0]=body.GetComponent<linear_cp_trigger>();
        cp_trigger[0].make_cp();
        //Debug.LogWarning("5");
        // Targetの位置のリセット
        // target.position = new Vector3(UnityEngine.Random.Range(-40,40), 0.5f, UnityEngine.Random.Range(-40,40));
        if(goal_percent_index>=20){
            goal_percent_index=0;
        }
        
        //cp関係のリセット
        // for(int i=0;i<cp_trigger[0].count_check_point;i++){//全部trueにする
        //     cp_trigger[0].cp_list[i].SetActive(true);
        // }
        
        cp_dis_vector = top_point_origin.transform.position - cp_trigger[0].cp_list[0].transform.position;
        distanceToTarget_start=Mathf.Sqrt(Mathf.Pow(cp_dis_vector[0],2)+Mathf.Pow(cp_dis_vector[2],2));
        // distanceToTarget_start=Vector3.Distance(top_point_origin.transform.position,cp_trigger[0].cp_list[0].transform.position);
        
        cp_trigger[0].now_cp=0;//indexを0にする
        distanceToTarget_past=distanceToTarget_start;
        distanceTocp=distanceToTarget_start;
        Curriculum_parameter();

        if(start!=null){
            start.SetActive(true);
        }
        //Debug.LogWarning("6");
        // angleToTarget_past=180.0f;
        prefab.name="base_link_and_linear_cp(Clone)"+count_episode+this.gameObject.name;
        target.transform.localPosition=new Vector3(UnityEngine.Random.Range(-4.5f,4.5f),0.43f,UnityEngine.Random.Range(-4.5f,4.5f)-12.0f);
        dis2target_past=Vector3.Distance(top_point_origin.transform.position,target.position);
        if(brain_on){
            export_brain_start();
        }
        body_tf=body.transform.position;
        
    }

    IEnumerator Time_stop(){//1秒止める、確実に着地してから力を与えるため
        yield return new WaitForSeconds(10);
    }

    // 状態取得時に呼ばれる
    public override void CollectObservations(VectorSensor sensor)
    {
        // sensor.AddObservation(target.position.x); //TargetのX座標
        // sensor.AddObservation(target.position.z); //TargetのZ座標
        // sensor.AddObservation(body.transform.position.x); //AgentのX座標
        // sensor.AddObservation(body.transform.position.z); //AgentのZ座標
        // sensor.AddObservation(body.transform.position.y)
        if(!real_world){
            if(float.IsNaN(body_state.transform.localEulerAngles.x)||float.IsNaN(flipper_FL_ml[0].currentDrive.target)||float.IsNaN(flipper_FR_ml[0].currentDrive.target)||float.IsNaN(flipper_RL_ml[0].currentDrive.target)||float.IsNaN(flipper_RR_ml[0].currentDrive.target)||float.IsNaN(realsense[0].ray_distance[0])){
                for(int i=0;i<23;i++){
                    sensor.AddObservation(0.0f);
                }
                // Debug.Log("1");
            }
            else{
                // sensor.AddObservation(distanceTocp);//距離
                // sensor.AddObservation(angleTocp);//角度
                sensor.AddObservation(body_state.transform.localEulerAngles.x*UnityEngine.Random.Range(input_noise_min,input_noise_max));
		        // Debug.LogWarning(body_state.transform.localEulerAngles.x);
                sensor.AddObservation(body_state.transform.localEulerAngles.y*UnityEngine.Random.Range(input_noise_min,input_noise_max));
		//Debug.LogWarning(body_state.transform.localEulerAngles.y);
                sensor.AddObservation(body_state.transform.localEulerAngles.z*UnityEngine.Random.Range(input_noise_min,input_noise_max)); //Agentの向き//yaw角のみなので、y値のみ、本来はz値だが、クローラモデルの関係から90°ずれている
		//Debug.LogError(body_state.transform.localEulerAngles.z);
                sensor.AddObservation(flipper_RR_ml[0].currentDrive.target*UnityEngine.Random.Range(input_noise_min,input_noise_max));
                sensor.AddObservation(flipper_FL_ml[0].currentDrive.target*UnityEngine.Random.Range(input_noise_min,input_noise_max));
                sensor.AddObservation(flipper_FR_ml[0].currentDrive.target*UnityEngine.Random.Range(input_noise_min,input_noise_max));
                sensor.AddObservation(flipper_RL_ml[0].currentDrive.target*UnityEngine.Random.Range(input_noise_min,input_noise_max));
                realsense_val = realsense[0].ray_distance;//realsenseの数、行列変更したら毎回変更する必要あり。
                for(int i=0;i<realsense_val.Length;i++){
                    if(float.IsNaN(realsense_val[i])){
                        realsense_val[i]=0.0f;
                    }
                    else{
                        realsense_val[i]=realsense[0].ray_distance[i]*rs_caribrate.ray_caribrate[i]*UnityEngine.Random.Range(input_noise_min,input_noise_max);
                    }
                    
                }
                sensor.AddObservation(realsense_val);
            }
        }
        else{
            //sensor→x,pitch y,roll, z,yaw  unity pitch yaw rollの順
            sensor.AddObservation(ros_pubsub_ai.imu_data_list[0]);
            sensor.AddObservation(ros_pubsub_ai.imu_data_list[2]);
            sensor.AddObservation(ros_pubsub_ai.imu_data_list[1]);
            //sensor→fl,fr,rl,rr //unity→rr,fl,fr,rl
            sensor.AddObservation(ros_pubsub_ai.flipper_data_list[3]);
            sensor.AddObservation(ros_pubsub_ai.flipper_data_list[0]);
            sensor.AddObservation(ros_pubsub_ai.flipper_data_list[1]);
            sensor.AddObservation(ros_pubsub_ai.flipper_data_list[2]);
            sensor.AddObservation(ros_pubsub_ai.rs_data_list);
        }
        if(output_nninput){
            
            if(real_and_sim_input){
                file_input.WriteLine(","+ros_pubsub_ai.imu_data_list[0]+","+body_state.transform.localEulerAngles.x+","+ros_pubsub_ai.imu_data_list[2]+","+body_state.transform.localEulerAngles.y+","+ros_pubsub_ai.imu_data_list[1]+","+body_state.transform.localEulerAngles.z+","+ros_pubsub_ai.flipper_data_list[0]+","+flipper_FL_ml[0].currentDrive.target+","+ros_pubsub_ai.flipper_data_list[2]+","+flipper_RL_ml[0].currentDrive.target+","+ros_pubsub_ai.flipper_data_list[1]+","+flipper_FR_ml[0].currentDrive.target+","+ros_pubsub_ai.flipper_data_list[3]+","+flipper_RR_ml[0].currentDrive.target);
                file_input_realsense.WriteLine(ros_pubsub_ai.rs_data_list[0]+","+realsense_val[0]+","+ros_pubsub_ai.rs_data_list[1]+","+realsense_val[1]+","+ros_pubsub_ai.rs_data_list[2]+","+realsense_val[2]+","+ros_pubsub_ai.rs_data_list[3]+","+realsense_val[3]+","+ros_pubsub_ai.rs_data_list[4]+","+realsense_val[4]+","+ros_pubsub_ai.rs_data_list[5]+","+realsense_val[5]+","+ros_pubsub_ai.rs_data_list[6]+","+realsense_val[6]+","+ros_pubsub_ai.rs_data_list[7]+","+realsense_val[7]+","+ros_pubsub_ai.rs_data_list[8]+","+realsense_val[8]+","+ros_pubsub_ai.rs_data_list[9]+","+realsense_val[9]+","+ros_pubsub_ai.rs_data_list[10]+","+realsense_val[10]+","+ros_pubsub_ai.rs_data_list[11]+","+realsense_val[11]+","+ros_pubsub_ai.rs_data_list[12]+","+realsense_val[12]+","+ros_pubsub_ai.rs_data_list[13]+","+realsense_val[13]+","+ros_pubsub_ai.rs_data_list[14]+","+realsense_val[14]);
            }
else if(real_world){
                file_input.WriteLine(","+ros_pubsub_ai.imu_data_list[0]+","+","+ros_pubsub_ai.imu_data_list[2]+","+","+ros_pubsub_ai.imu_data_list[1]+","+","+ros_pubsub_ai.flipper_data_list[0]+","+","+ros_pubsub_ai.flipper_data_list[2]+","+","+ros_pubsub_ai.flipper_data_list[1]+","+","+ros_pubsub_ai.flipper_data_list[3]);
                file_input_realsense.WriteLine(ros_pubsub_ai.rs_data_list[0]+","+","+ros_pubsub_ai.rs_data_list[1]+","+","+ros_pubsub_ai.rs_data_list[2]+","+","+ros_pubsub_ai.rs_data_list[3]+","+","+ros_pubsub_ai.rs_data_list[4]+","+","+ros_pubsub_ai.rs_data_list[5]+","+","+ros_pubsub_ai.rs_data_list[6]+","+","+ros_pubsub_ai.rs_data_list[7]+","+","+ros_pubsub_ai.rs_data_list[8]+","+","+ros_pubsub_ai.rs_data_list[9]+","+","+ros_pubsub_ai.rs_data_list[10]+","+","+ros_pubsub_ai.rs_data_list[11]+","+","+ros_pubsub_ai.rs_data_list[12]+","+","+ros_pubsub_ai.rs_data_list[13]+","+","+ros_pubsub_ai.rs_data_list[14]);
            }
            else{
                file_input.WriteLine(","+","+body_state.transform.localEulerAngles.x+","+","+body_state.transform.localEulerAngles.y+","+","+body_state.transform.localEulerAngles.z+","+","+flipper_FL_ml[0].currentDrive.target+","+","+flipper_RL_ml[0].currentDrive.target+","+","+flipper_FR_ml[0].currentDrive.target+","+","+flipper_RR_ml[0].currentDrive.target);
                file_input_realsense.WriteLine(","+realsense_val[0]+","+","+realsense_val[1]+","+","+realsense_val[2]+","+","+realsense_val[3]+","+","+realsense_val[4]+","+","+realsense_val[5]+","+","+realsense_val[6]+","+","+realsense_val[7]+","+","+realsense_val[8]+","+","+realsense_val[9]+","+","+realsense_val[10]+","+","+realsense_val[11]+","+","+realsense_val[12]+","+","+realsense_val[13]+","+","+realsense_val[14]);
            }
        }
        
        

    }

    // 行動実行時に呼ばれる
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        body_tf=body.transform.position;
        if(brain_on){
            export_excel();
        }
       //Debug.LogWarning("7");
        int now_cp_past = cp_trigger[0].now_cp;//進む前のnow_cp
        //Debug.LogWarning("8");
        float body_past_z=body.transform.position.z;//
        //旋回と進むを同時にしないため、値は-1～1;
        //Debug.LogWarning("9");
        if(!real_world){
            robot_Action(actionBuffers);
        }
        else{
            realrobot_action(actionBuffers);
        }
        
        
        //体の角度が横に傾いている量に応じて、ガウス分布的な報酬を与える。-0.95~0.05(90deg~0deg)
        //roll_z角を保存、0がうまく反映されないので、if文を用いて調整
        float roll_z=0.0f;
        if(body_state.transform.localEulerAngles.z>180.0f){
            roll_z=body_state.transform.localEulerAngles.z-360.0f;
        }
        else{
            roll_z=body_state.transform.localEulerAngles.z;
        }
        // AddReward(Mathf.Exp(-5*Mathf.Pow((roll_z/180.0f*Mathf.PI),2))-1.0f);
        // sum_reward+=Mathf.Exp(-5*Mathf.Pow((roll_z/180.0f*Mathf.PI),2))-1.0f;
        //
        //Debug.LogWarning("9");

        //常時与える
        AddReward(-0.005f);
        sum_reward+=-0.005f;
        // //Debug.Log(action_rotate);
        // rtrace[0].AddForce(controlSignal * 150, ForceMode.Acceleration);

        //rayで物体認識
        for(int i=0;i<realsense[0].ray_distance.Length;i++){
            if(realsense[0].ray_distance[i]<=0.1f){
                count_ray_distance_true+=1;
            }
        }
        // //Debug.LogWarning(count_ray_distance_true);
        if(count_ray_distance_true>0){
            find_object=true;//物体あるよ
        }
        else{
            find_object=false;
        }
        body_pitch_x=body_state.transform.localEulerAngles.x;
        if(body_state.transform.localEulerAngles.x>180){
                body_pitch_x=body_state.transform.localEulerAngles.x-360.0f;
            }
        if(find_object){//物体認識した場合bodyのpitch角に応じて報酬を与える(max 0.1)
            // AddReward(0.001f);
            // AddReward(body_pitch_x*0.0005f);
            // AddReward(-0.01f);
            // sum_reward+=-0.01f;
            count_ray_distance_true=0;
        }
        // else{//姿勢制御の報酬
        //     // AddReward(Mathf.Exp(-1/3.0f*Mathf.Pow((roll_z/180.0f*Mathf.PI),2))-0.995f);//体の角度が横に傾いている量に応じて、ガウス分布的な報酬を与える。-0.95~0.05(90deg~0deg)
        // }

        
        
        if(!create_cp_target){
                // //Debug.Log(cos_theta_goal);
            int now_cp=cp_trigger[0].now_cp;//進んだ後のnow_cp
            // //Debug.LogWarning(now_cp);
            // float distanceToTarget=Vector3.Distance(top_point_origin.transform.position,target.position);//targetとの距離が初期値
            // dis2target=Vector3.Distance(top_point_origin.transform.position,target.position);//targetとの距離が初期値
            
            // if(now_cp!=cp_trigger[0].count_check_point){
            //     goal_dir= cp_trigger[0].cp_list[now_cp_past].transform.position - top_point_origin.transform.position;
            //     distanceToTarget=Vector3.Distance(top_point_origin.transform.position,cp_trigger[0].cp_list[now_cp_past].transform.position);//現在のcpとの距離。
            // }
            //ゴールの方向に向いてたら、追加報酬
            if(now_cp!=cp_trigger[0].count_check_point){//ゴール時にゴール判定の前にこの関数が実行されてしまうので、indexのエラーが起きる。→そのための回避。
                cp_dis_vector=cp_trigger[0].cp_list[now_cp].transform.position - top_point_origin.transform.position;
            }

            Vector3 body_dir= top_point_dir.transform.position - top_point_origin.transform.position;
            Vector3 plane_normal = Vector3.up;
            var plane_cp_dis_vector=Vector3.ProjectOnPlane(cp_dis_vector,plane_normal);
            var plane_body_dir = Vector3.ProjectOnPlane(body_dir,plane_normal);
            angleTocp = Vector3.SignedAngle(plane_body_dir,plane_cp_dis_vector,plane_normal);
            float cos_theta_goal = Mathf.Cos(angleTocp/180.0f*3.14f);
            // angleTocp = Mathf.Acos(cos_theta_goal); 
            // Debug.LogWarning(cos_theta_goal);
            // Debug.LogError(angleTocp);
            if(cos_theta_goal>0.0f){
                AddReward(cos_theta_goal*0.001f);//cpに対する角度による報酬
                // Debug.Log(cos_theta_goal);
                sum_reward+=cos_theta_goal*0.001f;
            }

            // //Debug.Log(cos_theta_goal);
            float body_now_z = body.transform.position.z;
            //近づいたときに報酬
            if(now_cp!=cp_trigger[0].count_check_point){//ゴール時にゴール判定の前にこの関数が実行されてしまうので、indexのエラーが起きる。→そのための回避。
            //二次元処理
                // distanceTocp=Vector3.Distance(top_point_origin.transform.position,cp_trigger[0].cp_list[now_cp].transform.position);
                distanceTocp=Mathf.Sqrt(Mathf.Pow(cp_dis_vector[0],2)+Mathf.Pow(cp_dis_vector[2],2));
                // Debug.Log(distanceTocp);
            }
            float dis_reward=distanceToTarget_past - distanceTocp;
            if(dis_reward>0){
                AddReward(dis_reward*10.0f);
                sum_reward+=dis_reward*10.0f;
                distanceToTarget_past=distanceTocp;
            }
            // AddReward(dis_reward*10.0f);
            // sum_reward+=dis_reward*10.0f;
            //過去の距離更新
            // Debug.Log(dis_reward*10.0f);
                // //Debug.Log(dis_reward*10.0f);
                // AddReward(0.1f);
            // sum_reward+=distanceToTarget_past - distanceToTarget;
                // //Debug.Log(distanceToTarget_past);
            // distanceToTarget_past=distanceToTarget;
            // //Debug.Log(distanceTocp);
            // if(distanceToTarget>distanceTocp){
            //     AddReward(distanceTocp-distanceToTarget);
            //     //Debug.Log(distanceTocp-distanceToTarget);
            // }
            // seconds+=Time.deltaTime;
            // if (shock.now_acc.y >= 5.0f && seconds > 0.5f){
            //     AddReward(-1f / 600.0f * ((shock.now_acc.y + 10) * (shock.now_acc.y + 10) - 25));
            //     seconds=0.0f;
            // } 

            if(cp_trigger[0].cp_through){//距離が近づいたら、distanceToTarget_startを次のcpとの距離にし、falseにする、indexを増やす。
                cp_trigger[0].cp_through=false;
                if(now_cp!=cp_trigger[0].count_check_point){//indexを超えてしまったら、次はゴールとの設定にする
                    // distanceToTarget_past=Vector3.Distance(top_point_origin.transform.position,cp_trigger[0].cp_list[now_cp].transform.position);
                    distanceToTarget_past=Mathf.Sqrt(Mathf.Pow(cp_dis_vector[0],2)+Mathf.Pow(cp_dis_vector[2],2));
                    distanceTocp=distanceToTarget_past;
                    AddReward(cp_trigger[0].turn_reward);
                    sum_reward+=cp_trigger[0].turn_reward;
                    // //Debug.Log(cp_trigger[0].turn_reward);
                    // sum_reward+=cp_trigger[0].cp_reward;
                    // //Debug.LogError("cpこえた2");
                    
                }            
            }
            
            count_allstep++;
            count_step++;
            if(now_cp==cp_trigger[0].count_check_point){//ゴール

                AddReward(1.0f+3.0f*(max_step-count_step)/max_step);
                sum_reward+=1.0f+3.0f*(max_step-count_step)/max_step;
                count_goal_continuous++;
                // if(count_goal_continuous>=20){
                //     Debug.LogError(step_height_persent+"の高さで、"+count_goal_continuous);
                // }
                goal_percent[goal_percent_index]=1;
                goal_percent_index++;
                // Debug.LogWarning(sum_reward);
                keep_value_list(1.0f,0.0f,0.0f);//成功、転倒、カウント超過
                if(brain_on){
                    export_brain_goal("clear");
                }
                EndEpisode();
            }
        }
        else {//clickした箇所に行くときに使用
            if(cp_trigger[0].touch_clickpoint){
                cp_trigger[0].touch_clickpoint=false;
                create_toggle4checkpoint.go_list[click_tf_index].SetActive(false);
                click_tf_index++;
            }
            if(create_toggle4checkpoint.tf_list[click_tf_index]!=null||create_toggle4checkpoint.tf_list.Count!=0){
                cp_dis_vector=create_toggle4checkpoint.tf_list[click_tf_index] - top_point_origin.transform.position;
                Vector3 body_dir= top_point_dir.transform.position - top_point_origin.transform.position;
                Vector3 plane_normal = Vector3.up;
                var plane_cp_dis_vector=Vector3.ProjectOnPlane(cp_dis_vector,plane_normal);
                var plane_body_dir = Vector3.ProjectOnPlane(body_dir,plane_normal);
                angleTocp = Vector3.SignedAngle(plane_body_dir,plane_cp_dis_vector,plane_normal);
                distanceTocp=Mathf.Sqrt(Mathf.Pow(cp_dis_vector[0],2)+Mathf.Pow(cp_dis_vector[2],2));
            }
            else{
                angleTocp=0.0f;
                distanceTocp=0.0f;
            }
            // Debug.Log(distanceTocp);
            
        }
        

        if(body.transform.position.y<-2.0f){//落下
            AddReward(-5.0f);
            sum_reward+=-5.0f;
            count_goal_continuous=0;
            goal_percent[goal_percent_index]=0;
            goal_percent_index++;
            // fault_brain=true;
            if(brain_on){
                export_brain_goal("fall");
            }
            EndEpisode();
        }

        if(Mathf.Abs(body_pitch_x)>80.0f||Mathf.Abs(roll_z)>80.0f){
            AddReward(-1.0f);
            sum_reward+=-1.0f;
            count_goal_continuous=0;
            goal_percent[goal_percent_index]=0;
            goal_percent_index++;
            keep_value_list(0.0f,1.0f,0.0f);//成功、転倒、カウント超過
            fault_roll=true;
            if(brain_on){
                export_brain_goal("tumble");
            }
            EndEpisode();
        }
        if(count_step>=max_step-1){
            AddReward(-5.0f);
            sum_reward+=-5.0f;
            keep_value_list(0.0f,0.0f,1.0f);//成功、転倒、カウント超過
            fault_over=true;
            count_goal_continuous=0;
            goal_percent[goal_percent_index]=0;
            goal_percent_index++;
            if(brain_on){
                export_brain_goal("timeover");
            }
            EndEpisode();
        }
        
        
    }

    // ヒューリスティックモードの行動決定時に呼ばれる
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // var continuousActionsOut = actionsOut.ContinuousActions;
        // continuousActionsOut[0] = Input.GetAxis("move");
        // continuousActionsOut[1] = Input.GetAxis("flipperLF");
        // continuousActionsOut[2] = Input.GetAxis("flipperLR");
        // continuousActionsOut[3] = Input.GetAxis("flipperRF");
        // continuousActionsOut[4] = Input.GetAxis("flipperRR");

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
        if(Input.GetKey(KeyCode.Q)){//左前脚上げる
            discreteActionsOut[1]=0;
        }
        else if(Input.GetKey(KeyCode.E)){//左前脚下げる
            discreteActionsOut[1]=2;
        }
        else{
            discreteActionsOut[1]=1;
        }
        if(Input.GetKey(KeyCode.W)){//右前脚上げる
            discreteActionsOut[2]=0;
        }
        else if(Input.GetKey(KeyCode.R)){//右前脚下げる
            discreteActionsOut[2]=2;
        }
        else{
            discreteActionsOut[2]=1;
        }
        if(Input.GetKey(KeyCode.Z)){//左後ろ脚上げる
            discreteActionsOut[3]=0;
        }
        else if(Input.GetKey(KeyCode.C)){//左後ろ脚下げる
            discreteActionsOut[3]=2;
        }
        else{
            discreteActionsOut[3]=1;
        }
        if(Input.GetKey(KeyCode.X)){//右後ろ脚上げる
            discreteActionsOut[4]=0;
        }
        else if(Input.GetKey(KeyCode.V)){//右後ろ下げる
            discreteActionsOut[4]=2;
        }
        else{
            discreteActionsOut[4]=1;
        }

    }


    void keep_value_list(float success,float roll,float count_over){
        reward_list.Add(sum_reward);
        step_list.Add(count_step);
        curriculum_list.Add(step_height_persent);
        float touch=0.0f;
        for(int i=0;i<touch_wall.Length;i++){
            touch+=touch_wall[i].touch_obstacle_reward;
            touch_wall[i].touch_obstacle_reward=0.0f;
        }
        touch_list.Add(touch);
        roll_list.Add(roll);
        count_over_list.Add(count_over);
        succes_list.Add(success);
        //1エピソードのステップをリセット
        count_step=0;
        //1エピソードの報酬をリセット
        sum_reward=0.0f;

    }

    void export_excel(){//brainへ書き込み
               
        time_brain=Time.realtimeSinceStartup-time_memo_brain;
        if(time_brain-time_past_brain>0.2f||time_brain==0){
            float pitch=body.transform.localEulerAngles.x;
            float roll=body.transform.localEulerAngles.z;
            if(pitch>180.0f){
                pitch=pitch-360.0f;
            }
            if(roll>300){
                roll=roll-360.0f;
            }
            float x=body.transform.position.x-initial_pos_dt.x;
            float y=body.transform.position.y-initial_pos_dt.y;
            float z=body.transform.position.z-initial_pos_dt.z;
            file.WriteLine(time_brain+","+pitch+","+roll+","+y+","+x+","+z+","+shock.now_acc.y+","+flipper_FL_ml[0].currentDrive.target+","+flipper_RL_ml[0].currentDrive.target+","+flipper_FR_ml[0].currentDrive.target+","+flipper_RR_ml[0].currentDrive.target);
            file.Flush();
            time_past_brain=time_brain;
        }

    }
    void export_brain_start(){
        initial_pos_dt=body.transform.position;
        file.WriteLine("time(s)"+","+"pitch(deg)"+","+"roll(deg)"+","+"y(m)"+","+"x(m)"+","+"z(m)"+","+"y_acc"+","+"LF"+","+"LR"+","+"RF"+","+"RR"+","+file_num);
        time_memo_brain=Time.realtimeSinceStartup;
    }
    void export_brain_goal(string fault){
        file.WriteLine(fault);        
        time_brain=0;
        time_past_brain=0;
        Debug.LogError(file_num);
        file_num++;
        if(file_num==101){
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
                action_Go_Left=-10;
                action_Go_Right=10;
            }
            else if(actionBuffers.DiscreteActions[0]==1){//front
                action_Go_Left=10;
                action_Go_Right=-10;
            }
            else if(actionBuffers.DiscreteActions[0]==2){//nothing
                action_Go_Left=0;
                action_Go_Right=0;
            }
            else if(actionBuffers.DiscreteActions[0]==3){//right
                action_Go_Left=-4;
                action_Go_Right=-4;
            }
            else if(actionBuffers.DiscreteActions[0]==4){//left
                action_Go_Left=4;
                action_Go_Right=4;
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
        //Debug.LogWarning("9");
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

            flipper_FL_ml[0].action_FrontL=action_FrontL;
            flipper_RL_ml[0].action_RearL=action_RearL;
            flipper_FR_ml[0].action_FrontR=action_FrontR;
            flipper_RR_ml[0].action_RearR=action_RearR;

            }

        else if(ros_pubsub_ai.button_data_list[2]>0.5f){//自律走行
            if(actionBuffers.DiscreteActions[0]==0){//back
                action_Go_Left=-1;
                action_Go_Right=1.0f;
            }
            else if(actionBuffers.DiscreteActions[0]==1){//front
                action_Go_Left=10;
                action_Go_Right=-10;
            }
            else if(actionBuffers.DiscreteActions[0]==2){//nothing
                action_Go_Left=0;
                action_Go_Right=0;
            }
            else if(actionBuffers.DiscreteActions[0]==3){//right
                action_Go_Left=-4;
                action_Go_Right=-4;
            }
            else if(actionBuffers.DiscreteActions[0]==4){//left
                action_Go_Left=4;
                action_Go_Right=4;
            }
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
            flipper_FL_ml[0].action_FrontL=action_FrontL;
            flipper_RL_ml[0].action_RearL=action_RearL;
            flipper_FR_ml[0].action_FrontR=action_FrontR;
            flipper_RR_ml[0].action_RearR=action_RearR;
        }
    }
}




