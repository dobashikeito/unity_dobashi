using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System;
using System.Collections;

// RollerAgent
public class urdfmodel_agent : Agent
{
    private float sum_reward=0.0f;
    public Transform target; // TargetのTransform
    public Transform[] all_children_tf;
    public List<Vector3> all_children_pos=new List<Vector3>();
    public List<Vector3> all_children_rot=new List<Vector3>();
    private imax_flipper_control_fl_ML[] flipper_FL_ml = new imax_flipper_control_fl_ML[1];
    private imax_flipper_control_rl_ML[] flipper_RL_ml = new imax_flipper_control_rl_ML[1];
    private imax_flipper_control_fr_ML[] flipper_FR_ml = new imax_flipper_control_fr_ML[1];
    private imax_flipper_control_rr_ML[] flipper_RR_ml = new imax_flipper_control_rr_ML[1];
    private shock_detect_new shock;
    public float max_step=10000;
    private realsensed455[] realsense = new realsensed455[1];
    public cp_trigger[] cp_trigger=new cp_trigger[1];
    private touch_wall[] touch_wall=new touch_wall[1];
    public Unity.Robotics.UrdfImporter.Control.Controller_imax_ML urdf_control;

    //学習を始める最初の位置
    private Vector3 target_initial_pos;
    private Vector3 body_initial_pos;
    //学習を始める最初の角度

    [SerializeField]
    public GameObject top_object;//prefabの頂点にあるオブジェクト。これでリセット。
    public GameObject body;//body_origin 触れているかの判定に使用
    public GameObject body_state;//状態量をとってくるのに使用、今回はbase_link
    public GameObject flipper_RL;//
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
    public GameObject poles;
    public GameObject[] poles_list;
    
    private float distanceToTarget_past=1000.0f;
    private float distanceTocp=0.0f;
    private float angleToTarget_past=180.0f;
    private float max_distance = 0.01245f;//actionGoが１で進める最大距離
    private float dis2target_past = 0.0f;//targetとの2次元距離過去
    private float dis2target = 0.0f;//targetとの2次元距離
    private float cos_theta_goal = 0.0f;//targetとの間の角度

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


    //AIにより決定した値
    public float action_Go_Left;//imax_body_control_lefが取得する値
    public float action_Go_Right;//imax_body_control_rightが取得する値
    public float action_FrontL;//imax_flipper_control_LF_MLに渡す値
    public float action_FrontR;//imax_flipper_control_RF_MLに渡す値
    public float action_RearL;//imax_flipper_control_LR_MLに渡す値
    public float action_RearR;//imax_flipper_control_RR_MLに渡す値
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
    private Vector3 prefab_pos = new Vector3(-3.61f,0,-8.24f);//prefab生成場所
    private float[] realsense_val = new float[15];//realsenseの数、行列変更したら毎回変更する必要あり。
    // 初期化時に呼ばれる
    void Start(){
        //初期のpositionとrotationを格納
        // body_initial_pos = body.transform.position;
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
        step.transform.localScale=new Vector3(step.transform.localScale.x,step_heihgt_flipper100*step_height_persent,step.transform.localScale.z);
        step.transform.localPosition=new Vector3(step.transform.localPosition.x,step_height_persent*-0.5f,step.transform.localPosition.z);
        // step.transform.localScale=new Vector3(step.transform.localScale.x,step_heihgt_flipper100*config_shp,step.transform.localScale.z);
    }


    public override void Initialize()
    {

        //ゴールの確率をリストにまとめる
        for(int i=0;i<goal_percent.Length;i++){
            goal_percent[i]=0;
        }

        poles_list = new GameObject[poles.transform.childCount];
        for(int i = 0;i<poles.transform.childCount;i++){
            poles_list[i]=poles.transform.GetChild(i).gameObject;
        }
        // flipper_FL = transform.Find("base_link/fl").gameObject;
        // flipper_FR = transform.Find("base_link/fr").gameObject;
        // flipper_RL = transform.Find("base_link/rl").gameObject;
        // flipper_RR = transform.Find("base_link/rr").gameObject;
        // step = transform.Find("astm3").gameObject;
        // top_point_dir = transform.Find("base_link/top_point_dir").gameObject;
        // top_point_origin = transform.Find("base_link/top_point_origin").gameObject;
        // ray = transform.Find("base_link/ray_origin").gameObject;
        // body =transform.Find("base_link/detect_touch").gameObject;
        

        // flipper_FL_ml[0]=flipper_FL.GetComponent<imax_flipper_control_fl_ML>();
        // flipper_FR_ml[0]=flipper_FR.GetComponent<imax_flipper_control_fr_ML>();

        // flipper_RL_ml[0]=flipper_RL.GetComponent<imax_flipper_control_rl_ML>();
        // flipper_RR_ml[0]=flipper_RR.GetComponent<imax_flipper_control_rr_ML>();

        // realsense[0]=ray.GetComponent<realsensed455>();
        
        // touch_wall[0]=body.GetComponent<touch_wall>();

        // shock=body.GetComponent<shock_detect_new>();
        // cp_trigger[0]=body.GetComponent<cp_trigger>();
        //stepの10割の高さを保存
        step_heihgt_flipper100=step.transform.localScale.y;
        step_height_persent_past=step_height_persent;
        Curriculum_parameter();
        //cpの一番最初のやつとの距離を初期値として保存
        
        
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
        prefab_pos=new Vector3(-3.61f,0,-8.24f)+this.transform.position;
        prefab = Instantiate(create_prefab, prefab_pos, Quaternion.identity);
        //Debug.LogWarning("1");
        // StartCoroutine(Time_stop());
        prefab.transform.parent = this.transform;
        flipper_FL = transform.Find("base_link(Clone)/fl").gameObject;
        flipper_FR = transform.Find("base_link(Clone)/fr").gameObject;
        flipper_RL = transform.Find("base_link(Clone)/rl").gameObject;
        flipper_RR = transform.Find("base_link(Clone)/rr").gameObject;
        // step = transform.Find("astm3").gameObject;
        top_point_dir = transform.Find("base_link(Clone)/top_point_dir").gameObject;
        top_point_origin = transform.Find("base_link(Clone)/top_point_origin").gameObject;
        ray = transform.Find("base_link(Clone)/ray_origin").gameObject;
        body =transform.Find("base_link(Clone)/detect_touch").gameObject;
        body_state = transform.Find("base_link(Clone)").gameObject;
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
        realsense[0]=ray.GetComponent<realsensed455>();
        realsense[0].Start();
        touch_wall[0]=body.GetComponent<touch_wall>();
        touch_wall[0].Start();
        //Debug.LogWarning("4");
        shock=body.GetComponent<shock_detect_new>();
        shock.Start();
        cp_trigger[0]=body.GetComponent<cp_trigger>();
        cp_trigger[0].Start();
        //Debug.LogWarning("5");
        // Targetの位置のリセット
        // target.position = new Vector3(UnityEngine.Random.Range(-40,40), 0.5f, UnityEngine.Random.Range(-40,40));
        if(goal_percent_index>=20){
            goal_percent_index=0;
        }
        
        //cp関係のリセット
        for(int i=0;i<cp_trigger[0].count_check_point;i++){//全部trueにする
            cp_trigger[0].cp_list[i].SetActive(true);
        }
        
        cp_trigger[0].eight_wall_list[0].SetActive(true);
        cp_trigger[0].eight_wall_list[1].SetActive(true);
        cp_trigger[0].eight_wall_list[2].SetActive(false);
        cp_trigger[0].eight_wall_list[3].SetActive(false);
        distanceToTarget_start=Vector3.Distance(top_point_origin.transform.position,cp_trigger[0].cp_list[0].transform.position);
        
        cp_trigger[0].now_cp=0;//indexを0にする
        distanceToTarget_past=distanceToTarget_start;
        distanceTocp=distanceToTarget_start;
        Curriculum_parameter();

        if(start!=null){
            start.SetActive(true);
        }
        //Debug.LogWarning("6");
        // angleToTarget_past=180.0f;
        prefab.name="base_link(Clone)"+count_episode+this.gameObject.name;
        target.transform.localPosition=new Vector3(UnityEngine.Random.Range(-4.5f,4.5f),0.43f,UnityEngine.Random.Range(-4.5f,4.5f)-12.0f);
        dis2target_past=Vector3.Distance(top_point_origin.transform.position,target.position);
        for(int i = 0;i<poles_list.Length;i++){
            poles_list[i].transform.localPosition=new Vector3(UnityEngine.Random.Range(-4.5f,4.5f),2.0f,UnityEngine.Random.Range(-4.5f,4.5f)-12.0f);;
        }
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
        if(float.IsNaN(body_state.transform.localEulerAngles.x)||float.IsNaN(flipper_FL_ml[0].currentDrive.target)||float.IsNaN(flipper_FR_ml[0].currentDrive.target)||float.IsNaN(flipper_RL_ml[0].currentDrive.target)||float.IsNaN(flipper_RR_ml[0].currentDrive.target)||float.IsNaN(realsense[0].ray_distance[0])){
            for(int i=0;i<24;i++){
                sensor.AddObservation(0.0f);
            }
            // Debug.Log("1");
        }
        else{
            // if(this.gameObject.name=="urdfmodel_agent")Debug.Log("1");
            sensor.AddObservation(dis2target);
            sensor.AddObservation(cos_theta_goal);
            // sensor.AddObservation(0);
            // sensor.AddObservation(0);
            // if(this.gameObject.name=="urdfmodel_agent")Debug.Log("2");
            //-180~180で与えたいため、if文で処理。
            if(body_state.transform.localEulerAngles.x>180.0f)sensor.AddObservation(body_state.transform.localEulerAngles.x-360);
            else sensor.AddObservation(body_state.transform.localEulerAngles.x);
            if(body_state.transform.localEulerAngles.y>180.0f)sensor.AddObservation(body_state.transform.localEulerAngles.y-360);
            else sensor.AddObservation(body_state.transform.localEulerAngles.y);
            if(body_state.transform.localEulerAngles.z>180.0f)sensor.AddObservation(body_state.transform.localEulerAngles.z-360);
            else sensor.AddObservation(body_state.transform.localEulerAngles.z);
            
            // if(this.gameObject.name=="urdfmodel_agent")Debug.Log("3");
            // sensor.AddObservation(count_step);
            // if(this.gameObject.name=="urdfmodel_agent")Debug.Log("4");
            // sensor.AddObservation(body_state.transform.localEulerAngles.y);
            // if(this.gameObject.name=="urdfmodel_agent")Debug.Log("5");
            // sensor.AddObservation(body_state.transform.localEulerAngles.z); //Agentの向き,yaw角のみなので、y値のみ、本来はz値だが、クローラモデルの関係から90°ずれている
            // if(this.gameObject.name=="urdfmodel_agent")Debug.Log("6");
            // sensor.AddObservation(body.transform.localPosition.x);
            // sensor.AddObservation(body.transform.localPosition.y);
            // sensor.AddObservation(body.transform.localPosition.z); //Agentの向き//yaw角のみなので、y値のみ、本来はz値だが、クローラモデルの関係から90°ずれている
            // sensor.AddObservation(rtrace[0].velocity.x); // AgentのX速度
            // sensor.AddObservation(rtrace[0].velocity.z); // AgentのZ速度
            sensor.AddObservation(flipper_RR_ml[0].currentDrive.target);
            sensor.AddObservation(flipper_FL_ml[0].currentDrive.target);
           //  if(this.gameObject.name=="urdfmodel_agent")Debug.Log("7");
            sensor.AddObservation(flipper_FR_ml[0].currentDrive.target);
            // if(this.gameObject.name=="urdfmodel_agent")Debug.Log("8");
            sensor.AddObservation(flipper_RL_ml[0].currentDrive.target);
            // if(this.gameObject.name=="urdfmodel_agent")Debug.Log("9");
            realsense_val=realsense[0].ray_distance;
            for(int i=0;i<realsense_val.Length;i++){
                if(float.IsNaN(realsense_val[i])){
                    realsense_val[i]=0.0f;
                }
                
                // Debug.Log(i+"_"+realsense[0].ray_distance[i]);
            }
            sensor.AddObservation(realsense_val);            
            // if(this.gameObject.name=="urdfmodel_agent")Debug.Log("10");
            // Debug.LogWarning(body.transform.localEulerAngles);
            // Debug.LogWarning(this.gameObject.name+realsense[0].ray_distance[0]);
            // Debug.LogWarning(flipper_FL_ml[0].currentDrive.target);
            // Debug.LogWarning(this.gameObject.name+dis2target);
        }
        

    }

    // 行動実行時に呼ばれる
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
       //Debug.LogWarning("7");
        int now_cp_past = cp_trigger[0].now_cp;//進む前のnow_cp
        //Debug.LogWarning("8");
        float body_past_z=body_state.transform.position.z;//
        //旋回と進むを同時にしないため、値は-1～1;
        //Debug.LogWarning("9");
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
                action_Go_Left=-10;
                action_Go_Right=10;
            }
            else if(Input.GetAxis("move")>0){
                action_Go_Left=10;
                action_Go_Right=-10;
            }
            else if(Input.GetAxis("rotation")<0){
                action_Go_Left=-4;
                action_Go_Right=-4;
            }
            else if(Input.GetAxis("rotation")>0){
                action_Go_Left=4;
                action_Go_Right=4;
            }
            else{
                action_Go_Left=0.0f;
                action_Go_Right=0.0f;
            }
        }
        //Debug.LogWarning("9");
        if(actionBuffers.DiscreteActions[1]==0){action_FrontL=2;}
        else if(actionBuffers.DiscreteActions[1]==1){action_FrontL=0;}
        else if(actionBuffers.DiscreteActions[1]==2){action_FrontL=-2;}

        if(actionBuffers.DiscreteActions[2]==0){action_FrontR=2;}
        else if(actionBuffers.DiscreteActions[2]==1){action_FrontR=0;}
        else if(actionBuffers.DiscreteActions[2]==2){action_FrontR=-2;}

        if(actionBuffers.DiscreteActions[3]==0){action_RearL=2;}
        else if(actionBuffers.DiscreteActions[3]==1){action_RearL=0;}
        else if(actionBuffers.DiscreteActions[3]==2){action_RearL=-2;}

        if(actionBuffers.DiscreteActions[4]==0){action_RearR=2;}
        else if(actionBuffers.DiscreteActions[4]==1){action_RearR=0;}
        else if(actionBuffers.DiscreteActions[4]==2){action_RearR=-2;}
        
        flipper_FL_ml[0].action_FrontL=action_FrontL;
        flipper_RL_ml[0].action_RearL=action_RearL;
        flipper_FR_ml[0].action_FrontR=action_FrontR;
        flipper_RR_ml[0].action_RearR=action_RearR;

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
        AddReward(-0.001f);
        sum_reward+=-0.001f;
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

        //ゴールの方向に向いてたら、追加報酬
        Vector3 goal_dir= target.transform.position - top_point_origin.transform.position;
        
        
        // //Debug.Log(cos_theta_goal);
        int now_cp=cp_trigger[0].now_cp;//進んだ後のnow_cp
        // //Debug.LogWarning(now_cp);
        // float distanceToTarget=Vector3.Distance(top_point_origin.transform.position,target.position);//targetとの距離が初期値
        dis2target=Vector3.Distance(top_point_origin.transform.position,target.position);//targetとの距離が初期値
        
        // if(now_cp!=cp_trigger[0].count_check_point){
        //     goal_dir= cp_trigger[0].cp_list[now_cp_past].transform.position - top_point_origin.transform.position;
        //     distanceToTarget=Vector3.Distance(top_point_origin.transform.position,cp_trigger[0].cp_list[now_cp_past].transform.position);//現在のcpとの距離。
        // }
        Vector3 body_dir= top_point_dir.transform.position - top_point_origin.transform.position;
        float body_dir_mag=Mathf.Sqrt(body_dir.x*body_dir.x+body_dir.z*body_dir.z);
        float goal_dir_mag=Mathf.Sqrt(goal_dir.x*goal_dir.x+goal_dir.z*goal_dir.z);
        float naiseki = goal_dir.x*body_dir.x+goal_dir.z*body_dir.z;//内積
        cos_theta_goal=naiseki/goal_dir_mag/body_dir_mag;//ゴールの方向角度
        // //Debug.LogWarning(cos_theta_goal);
        if(cos_theta_goal>0.0f){
            AddReward(cos_theta_goal*0.001f);//cpに対する角度による報酬
            // //Debug.Log(cos_theta_goal*0.01f);
            sum_reward+=cos_theta_goal*0.001f;
        }

        // //Debug.Log(cos_theta_goal);
        float body_now_z = body.transform.position.z;
        // //Debug.Log(distanceToTarget);
        // if(distanceToTarget<distanceToTarget_past){
        //     float dis_reward=distanceToTarget_past - distanceToTarget;
        //     AddReward(dis_reward*10.0f);
        //     // //Debug.LogWarning(dis_reward*10.0f);
        //     sum_reward+=dis_reward*10.0f;
        //     // //Debug.Log(dis_reward*10.0f);
        //     // AddReward(0.1f);
        //     // sum_reward+=distanceToTarget_past - distanceToTarget;
        //     // //Debug.Log(distanceToTarget_past);
        //     distanceToTarget_past=distanceToTarget;
        // }
        if(dis2target<dis2target_past){
            float dis_reward=dis2target_past - dis2target;
            AddReward(dis_reward*10.0f);
            // //Debug.LogWarning(dis_reward*10.0f);
            sum_reward+=dis_reward*10.0f;
            // Debug.Log(dis_reward*10.0f);
            // AddReward(0.1f);
            // sum_reward+=distanceToTarget_past - distanceToTarget;
            // //Debug.Log(distanceToTarget_past);
            dis2target_past=dis2target;
        }
        //近づいたときに報酬
        // float dis_reward=distanceToTarget_past - distanceToTarget;
        // if(dis_reward>0){
        //     AddReward(dis_reward*10.0f);
        // }
        
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

        // if(cp_trigger[0].cp_through){//距離が近づいたら、distanceToTarget_startを次のcpとの距離にし、falseにする、indexを増やす。
        //     cp_trigger[0].cp_through=false;
        //     if(now_cp!=cp_trigger[0].count_check_point){//indexを超えてしまったら、次はゴールとの設定にする
        //         distanceToTarget_past=Vector3.Distance(top_point_origin.transform.position,cp_trigger[0].cp_list[now_cp].transform.position);
        //         distanceTocp=distanceToTarget_past;
        //         // AddReward(cp_trigger[0].turn_reward);
        //         // sum_reward+=cp_trigger[0].turn_reward;
        //         // //Debug.Log(cp_trigger[0].turn_reward);
        //         // sum_reward+=cp_trigger[0].cp_reward;
        //         // //Debug.LogError("cpこえた2");
                
        //     }            
        // }
        
        count_allstep++;
        count_step++;
        // //Debug.LogWarning(body_initial_pos);
        // //Debug.LogWarning(body.transform.position);
        // //Debug.LogWarning(body_initial_pos.y-body.transform.position.y);
        
        if(dis2target<2.0f){//ゴール

            AddReward(1.0f+5.0f*(max_step-count_step)/max_step);
            sum_reward+=1.0f+3.0f*(max_step-count_step)/max_step;
            count_goal_continuous++;
            // if(count_goal_continuous>=20){
            //     //Debug.LogError(step_height_persent+"の高さで、"+count_goal_continuous);
            // }
            goal_percent[goal_percent_index]=1;
            goal_percent_index++;
            keep_value_list(1.0f,0.0f,0.0f);//成功、転倒、カウント超過
            //Debug.Log("1");
            EndEpisode();
        }

        if(body.transform.position.y<-2.0f){//落下
            AddReward(-5.0f);
            sum_reward+=-1.0f;
            count_goal_continuous=0;
            goal_percent[goal_percent_index]=0;
            goal_percent_index++;
            // fault_brain=true;
            // Destroy(prefab);
            //Debug.Log("2");
            EndEpisode();
        }

        if(Mathf.Abs(body_pitch_x)>80.0f||Mathf.Abs(roll_z)>80.0f){
            AddReward(-5.0f);
            sum_reward+=-5.0f;
            count_goal_continuous=0;
            goal_percent[goal_percent_index]=0;
            goal_percent_index++;
            keep_value_list(0.0f,1.0f,0.0f);//成功、転倒、カウント超過
            fault_roll=true;
            // Destroy(prefab);
            //Debug.Log("3");
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
            // Destroy(prefab);
            //Debug.Log("4");
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
}



