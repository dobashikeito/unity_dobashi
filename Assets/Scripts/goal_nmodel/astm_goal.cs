using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System;
using System.Collections;

// RollerAgent
public class astm_goal : Agent
{
    private float sum_reward=0.0f;
    public Transform target; // TargetのTransform
    public GameObject crwlr_model;
    private move_body_ML_new[] move_body_ml=new move_body_ML_new[1];
    private move_flipper_ML_new[] move_flipper_ml =new move_flipper_ML_new[4];
    private flipperLF_ML_new[] flipper_lf_ml = new flipperLF_ML_new[1];
    private flipperLR_ML_new[] flipper_lr_ml = new flipperLR_ML_new[1];
    private flipperRF_ML_new[] flipper_rf_ml = new flipperRF_ML_new[1];
    private flipperRR_ML_new[] flipper_rr_ml = new flipperRR_ML_new[1];
    private Rigidbody[] rtrace = new Rigidbody[5];
    private shock_detect_new shock;
    public float max_step=1250;
    HingeJoint[] joint = new HingeJoint[4];
    JointSpring[] hingeSpring = new JointSpring[4];
    JointLimits[] limits = new JointLimits[4];
    private realsensed455[] realsense = new realsensed455[1];
    private cp_trigger[] cp_trigger=new cp_trigger[1];
    private touch_wall[] touch_wall=new touch_wall[5];

    //学習を始める最初の位置
    private Vector3 body_initial_pos;
    private Vector3 flipper_LF_initial_pos;
    private Vector3 flipper_LR_initial_pos;
    private Vector3 flipper_RF_initial_pos;
    private Vector3 flipper_RR_initial_pos;
    private Vector3 top_point_dir_initial_pos;
    private Vector3 top_point_origin_initial_pos;

    private Vector3 target_initial_pos;
    //学習を始める最初の角度
    private Vector3 body_initial_rotate;
    private Vector3 flipper_LF_initial_rotate;
    private Vector3 flipper_LR_initial_rotate;
    private Vector3 flipper_RF_initial_rotate;
    private Vector3 flipper_RR_initial_rotate;
    private Vector3 top_point_dir_initial_rotate;
    private Vector3 top_point_origin_initial_rotate;
    


    [SerializeField]
    public GameObject body;
    public GameObject flipper_LR;
    public GameObject flipper_LF;
    public GameObject flipper_RF;
    public GameObject flipper_RR;
    public GameObject top_point_origin;
    public GameObject top_point_dir;
    public GameObject step;
    public GameObject ray;
    public GameObject start;
    private float distanceToTarget_past=1000.0f;
    private float distanceTocp=0.0f;
    private float angleToTarget_past=180.0f;
    private float max_distance = 0.01245f;//actionGoが１で進める最大距離

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


    private float step_heihgt_flipper100;//フリッパの高さと釣り合う、stepのスケール
    private float step_height;
    public float step_height_persent=0.1f;
    private float config_shp=0.0f;
    private float step_height_persent_past;


    //AIにより決定した値
    public float action_Go;//move_body、move_flipperに渡す値
    public float action_rotate;//move_bodyに渡す値
    public float action_FrontL;//flipper_LF_MLに渡す値
    public float action_FrontR;//flipper_RF_MLに渡す値
    public float action_RearL;//flipper_LR_MLに渡す値
    public float action_RearR;//flipper_RR_MLに渡す値
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
    public bool fault_brain=false;//brain中の失敗を検知するため

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


    // 初期化時に呼ばれる
    void Start(){
        //初期のpositionとrotation
        body_initial_pos=body.transform.position;
        flipper_LF_initial_pos=flipper_LF.transform.position;
        flipper_LR_initial_pos=flipper_LR.transform.position;
        flipper_RF_initial_pos=flipper_RF.transform.position;
        flipper_RR_initial_pos=flipper_RR.transform.position;
        top_point_dir_initial_pos=top_point_dir.transform.position;
        top_point_origin_initial_pos=top_point_origin.transform.position;

        body_initial_rotate=body.transform.eulerAngles;
        flipper_LF_initial_rotate=flipper_LF.transform.eulerAngles;
        flipper_LR_initial_rotate=flipper_LR.transform.eulerAngles;
        flipper_RF_initial_rotate=flipper_RF.transform.eulerAngles;
        flipper_RR_initial_rotate=flipper_RR.transform.eulerAngles;
        top_point_dir_initial_rotate=top_point_dir.transform.eulerAngles;
        top_point_origin_initial_rotate=top_point_origin.transform.eulerAngles;

        target_initial_pos=target.position;



    }

    void Curriculum_parameter(){
        EnvironmentParameters curri_para=Academy.Instance.EnvironmentParameters;
        // approach_reward=curri_para.GetWithDefault("approach_reward",0.1f);
        // config_shp=curri_para.GetWithDefault("step_height",start_step_height);
        // if(count_goal_continuous>10){
        //     // Debug.Log("step_height_persent::"+step_height_persent+"::allstep::"+count_allstep);
        //     // step_height_persent+=0.1f;
        // }
        // Debug.Log(step_heihgt_flipper100);
        step.transform.localScale=new Vector3(step.transform.localScale.x,step_heihgt_flipper100*step_height_persent,step.transform.localScale.z);
        // step.transform.localScale=new Vector3(step.transform.localScale.x,step_heihgt_flipper100*config_shp,step.transform.localScale.z);
    }


    public override void Initialize()
    {
        //ゴールの確率をリストにまとめる
        for(int i=0;i<goal_percent.Length;i++){
            goal_percent[i]=0;
        }

        move_body_ml[0]=body.GetComponent<move_body_ML_new>();

        move_flipper_ml[0]=flipper_LF.GetComponent<move_flipper_ML_new>();
        move_flipper_ml[1]=flipper_LR.GetComponent<move_flipper_ML_new>();
        move_flipper_ml[2]=flipper_RF.GetComponent<move_flipper_ML_new>();
        move_flipper_ml[3]=flipper_RR.GetComponent<move_flipper_ML_new>();

        flipper_lf_ml[0]=flipper_LF.GetComponent<flipperLF_ML_new>();
        flipper_rf_ml[0]=flipper_RF.GetComponent<flipperRF_ML_new>();

        flipper_lr_ml[0]=flipper_LR.GetComponent<flipperLR_ML_new>();
        flipper_rr_ml[0]=flipper_RR.GetComponent<flipperRR_ML_new>();

        rtrace[0]=body.GetComponent<Rigidbody>();
        rtrace[1]=flipper_LF.GetComponent<Rigidbody>();
        rtrace[2]=flipper_LR.GetComponent<Rigidbody>();
        rtrace[3]=flipper_RF.GetComponent<Rigidbody>();
        rtrace[4]=flipper_RR.GetComponent<Rigidbody>();


        joint[0]=flipper_LF.GetComponent<HingeJoint>();
        joint[1]=flipper_LR.GetComponent<HingeJoint>();
        joint[2]=flipper_RF.GetComponent<HingeJoint>();
        joint[3]=flipper_RR.GetComponent<HingeJoint>();

        realsense[0]=ray.GetComponent<realsensed455>();
        
        touch_wall[0]=body.GetComponent<touch_wall>();
        touch_wall[1]=flipper_LF.GetComponent<touch_wall>();
        touch_wall[2]=flipper_LR.GetComponent<touch_wall>();
        touch_wall[3]=flipper_RF.GetComponent<touch_wall>();
        touch_wall[4]=flipper_RR.GetComponent<touch_wall>();

        shock=body.GetComponent<shock_detect_new>();

        for(int i=0;i<joint.Length;i++){
            hingeSpring[i]=joint[i].spring;
            hingeSpring[i].spring = 400;  //力の大きさ
            hingeSpring[i].damper = 50;  
            hingeSpring[i].targetPosition = 0;  // 目的の角度
            joint[i].spring = hingeSpring[i];  // 上記三つをjointに渡す
            joint[i].useSpring = true;
        }

        cp_trigger[0]=body.GetComponent<cp_trigger>();
        //stepの10割の高さを保存
        step_heihgt_flipper100=step.transform.localScale.y;
        step_height_persent_past=step_height_persent;
        Curriculum_parameter();
        //cpの一番最初のやつとの距離を初期値として保存
        
        
        // RollerAgentのRigidtraceの参照の取得
        // move_body_ml=trace.GetComponent<move_body_ML>();
        
    }

    // エピソード開始時に呼ばれる
    public override void OnEpisodeBegin()
    {   
        // RollerAgentの位置と速度をリセット
        for(int i=0;i<rtrace.Length;i++){
            rtrace[i].angularVelocity = Vector3.zero;
            rtrace[i].linearVelocity = Vector3.zero;
        }
        
        body.transform.eulerAngles = new Vector3(0.0f,0.0f,0.0f);
        flipper_LR.transform.eulerAngles = new Vector3(0.0f,0.0f,0.0f);
        flipper_LF.transform.eulerAngles = new Vector3(0.0f,0.0f,0.0f);
        flipper_RF.transform.eulerAngles = new Vector3(0.0f,0.0f,0.0f);
        flipper_RR.transform.eulerAngles = new Vector3(0.0f,0.0f,0.0f);
        top_point_dir.transform.eulerAngles = new Vector3(0.0f,0.0f,0.0f);
        top_point_origin.transform.eulerAngles = new Vector3(0.0f,0.0f,0.0f);
            
        body.transform.position = body_initial_pos;
        flipper_LR.transform.position = flipper_LR_initial_pos;
        flipper_LF.transform.position = flipper_LF_initial_pos;
        flipper_RF.transform.position = flipper_RF_initial_pos;
        flipper_RR.transform.position = flipper_RR_initial_pos;
        top_point_dir.transform.position = top_point_dir_initial_pos;
        top_point_origin.transform.position = top_point_origin_initial_pos;

        flipper_lf_ml[0].angle=0;
        flipper_rf_ml[0].angle=0;
        flipper_lr_ml[0].angle=0;
        flipper_rr_ml[0].angle=0;
        for(int i=0;i<joint.Length;i++){
            joint[i].useLimits=true;
            hingeSpring[i].targetPosition = 0;  // 目的の角度
            joint[i].spring = hingeSpring[i];  // 上記三つをjointに渡す
            joint[i].useSpring = true;
        }
            
            // Debug.LogWarning(body.transform.position);
            // Debug.LogWarning(body.transform.localPosition);
            // StartCoroutine("Time_stop");
        

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

        // if(start!=null){
        //     start.SetActive(true);
        // }
        // angleToTarget_past=180.0f;

    }

    IEnumerator Time_stop(){//1秒止める、確実に着地してから力を与えるため
        yield return new WaitForSeconds(2);
    }

    // 状態取得時に呼ばれる
    public override void CollectObservations(VectorSensor sensor)
    {
        // sensor.AddObservation(target.position.x); //TargetのX座標
        // sensor.AddObservation(target.position.z); //TargetのZ座標
        // sensor.AddObservation(body.transform.position.x); //AgentのX座標
        // sensor.AddObservation(body.transform.position.z); //AgentのZ座標
        // sensor.AddObservation(body.transform.position.y)

        sensor.AddObservation(realsense[0].ray_distance);
        // sensor.AddObservation(count_step);
        sensor.AddObservation(body.transform.localEulerAngles.x);
        sensor.AddObservation(body.transform.localEulerAngles.y);
        sensor.AddObservation(body.transform.localEulerAngles.z); //Agentの向き//yaw角のみなので、y値のみ、本来はz値だが、クローラモデルの関係から90°ずれている
        // sensor.AddObservation(rtrace[0].velocity.x); // AgentのX速度
        // sensor.AddObservation(rtrace[0].velocity.z); // AgentのZ速度
        sensor.AddObservation(flipper_lf_ml[0].angle);
        sensor.AddObservation(flipper_rf_ml[0].angle);
        sensor.AddObservation(flipper_lr_ml[0].angle);
        sensor.AddObservation(flipper_rr_ml[0].angle);

    }

    // 行動実行時に呼ばれる
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int now_cp_past = cp_trigger[0].now_cp;//進む前のnow_cp
        float body_past_z=body.transform.position.z;// RollerAgentに力を加える
        //旋回と進むを同時にしないため、値は-1～1;
        //(1-(-1))/9=
        if(actionBuffers.DiscreteActions[0]==0){
            action_Go=-1;
            action_rotate=0;
        }
        else if(actionBuffers.DiscreteActions[0]==1){
            action_Go=1;
            action_rotate=0;
        }
        else if(actionBuffers.DiscreteActions[0]==2){
            action_Go=0;
            action_rotate=0;
        }
        else if(actionBuffers.DiscreteActions[0]==3){
            action_Go=0;
            action_rotate=-1;
        }
        else if(actionBuffers.DiscreteActions[0]==4){
            action_Go=0;
            action_rotate=-0.5f;
        }
        else if(actionBuffers.DiscreteActions[0]==5){
            action_Go=0;
            action_rotate=0.5f;
        }
        else if(actionBuffers.DiscreteActions[0]==6){
            action_Go=0;
            action_rotate=1;
        }
        // else if(actionBuffers.DiscreteActions[0]==4){
        //     action_Go=0;
        //     action_rotate=0.5f;
        // }


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
        
        // action_FrontL = actionBuffers.ContinuousActions[1];
        // action_RearL = actionBuffers.ContinuousActions[2];
        // action_FrontR = actionBuffers.ContinuousActions[3];
        // action_RearR = actionBuffers.ContinuousActions[4];

        for(int i=0;i<move_body_ml.Length;i++){
            move_body_ml[i].action_Go=action_Go;
        }
        for(int i=0;i<move_flipper_ml.Length;i++){
            move_flipper_ml[i].action_Go=action_Go;
        }
        for(int i=0;i<move_body_ml.Length;i++){
            move_body_ml[i].action_rotate=action_rotate;
        }
        for(int i=0;i<move_flipper_ml.Length;i++){
            move_flipper_ml[i].action_rotate=action_rotate;
        }

        flipper_lf_ml[0].action_FrontL=action_FrontL;
        flipper_lr_ml[0].action_RearL=action_RearL;
        flipper_rf_ml[0].action_FrontR=action_FrontR;
        flipper_rr_ml[0].action_RearR=action_RearR;

        //体の角度が横に傾いている量に応じて、ガウス分布的な報酬を与える。-0.95~0.05(90deg~0deg)
        //roll_z角を保存、0がうまく反映されないので、if文を用いて調整
        float roll_z=0.0f;
        if(body.transform.localEulerAngles.z>180.0f){
            roll_z=body.transform.localEulerAngles.z-360.0f;
        }
        else{
            roll_z=body.transform.localEulerAngles.z;
        }

        

        // AddReward(Mathf.Exp(-5*Mathf.Pow((roll_z/180.0f*Mathf.PI),2))-0.95f);

        AddReward(-0.001f);
        sum_reward+=-0.001f;
        // Debug.Log(action_rotate);
        // rtrace[0].AddForce(controlSignal * 150, ForceMode.Acceleration);

        //rayで物体認識
        for(int i=0;i<realsense[0].ray_distance.Length;i++){
            if(realsense[0].ray_distance[i]<=0.1f){
                count_ray_distance_true+=1;
            }
        }
        // Debug.LogWarning(count_ray_distance_true);
        if(count_ray_distance_true>0){
            find_object=true;//物体あるよ
        }
        else{
            find_object=false;
        }
        body_pitch_x=body.transform.localEulerAngles.x;
        if(body.transform.localEulerAngles.x>180){
                body_pitch_x=body.transform.localEulerAngles.x-360.0f;
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
        
        
        // Debug.Log(cos_theta_goal);
        int now_cp=cp_trigger[0].now_cp;//進んだ後のnow_cp
        // Debug.LogWarning(now_cp);
        float distanceToTarget=Vector3.Distance(top_point_origin.transform.position,target.position);//targetとの距離が初期値
        if(now_cp!=cp_trigger[0].count_check_point){
            goal_dir= cp_trigger[0].cp_list[now_cp_past].transform.position - top_point_origin.transform.position;
            distanceToTarget=Vector3.Distance(top_point_origin.transform.position,cp_trigger[0].cp_list[now_cp_past].transform.position);//現在のcpとの距離。
        }
        Vector3 body_dir= top_point_dir.transform.position - top_point_origin.transform.position;
        float body_dir_mag=Mathf.Sqrt(body_dir.x*body_dir.x+body_dir.z*body_dir.z);
        float goal_dir_mag=Mathf.Sqrt(goal_dir.x*goal_dir.x+goal_dir.z*goal_dir.z);
        float naiseki = goal_dir.x*body_dir.x+goal_dir.z*body_dir.z;//内積
        float cos_theta_goal=naiseki/goal_dir_mag/body_dir_mag;//ゴールの方向角度
        if(cos_theta_goal>0){
            // AddReward(cos_theta_goal*0.001f);
            // sum_reward+=cos_theta_goal*0.001f;
        }
        

        // Debug.Log(cos_theta_goal);
        float body_now_z = body.transform.position.z;
        // Debug.Log(distanceToTarget);
        if(distanceToTarget<distanceToTarget_past){
            float dis_reward=distanceToTarget_past - distanceToTarget;
            AddReward(dis_reward*10.0f);
            sum_reward+=dis_reward*10.0f;
            // Debug.Log(dis_reward*10.0f);
            // AddReward(0.1f);
            // sum_reward+=distanceToTarget_past - distanceToTarget;
            // Debug.Log(distanceToTarget_past);
            distanceToTarget_past=distanceToTarget;
        }
        //近づいたときに報酬
        // float dis_reward=distanceToTarget_past - distanceToTarget;
        // if(dis_reward>0){
        //     AddReward(dis_reward*10.0f);
        // }
        
            // Debug.Log(dis_reward*10.0f);
            // AddReward(0.1f);
        // sum_reward+=distanceToTarget_past - distanceToTarget;
            // Debug.Log(distanceToTarget_past);
        // distanceToTarget_past=distanceToTarget;
        // Debug.Log(distanceTocp);
        // if(distanceToTarget>distanceTocp){
        //     AddReward(distanceTocp-distanceToTarget);
        //     Debug.Log(distanceTocp-distanceToTarget);
        // }
        seconds+=Time.deltaTime;
        // Debug.LogWarning(shock.now_acc.y);
        // if (shock.now_acc.y >= 5.0f && seconds > 0.5f){
        // if (shock.now_acc.y >= 15.0f){
        //     AddReward(-1f / 600.0f * ((shock.now_acc.y + 10) * (shock.now_acc.y + 10) - 25));
        //     seconds=0.0f;
        //     // Debug.LogError(-1f / 600.0f * ((shock.now_acc.y + 10) * (shock.now_acc.y + 10) - 25));
        // } 

        if(cp_trigger[0].cp_through){//距離が近づいたら、distanceToTarget_startを次のcpとの距離にし、falseにする、indexを増やす。
            cp_trigger[0].cp_through=false;
            if(now_cp!=cp_trigger[0].count_check_point){//indexを超えてしまったら、次はゴールとの設定にする
                distanceToTarget_past=Vector3.Distance(top_point_origin.transform.position,cp_trigger[0].cp_list[now_cp].transform.position);
                distanceTocp=distanceToTarget_past;
                // AddReward(cp_trigger[0].turn_reward);
                // sum_reward+=cp_trigger[0].turn_reward;
                // Debug.Log(cp_trigger[0].turn_reward);
                // sum_reward+=cp_trigger[0].cp_reward;
                // Debug.LogError("cpこえた2");
                
            }            
        }
        
        count_allstep++;
        count_step++;

        if(now_cp==10){//ゴール

            AddReward(1.0f+3.0f*(max_step-count_step)/max_step);
            sum_reward+=1.0f+3.0f*(max_step-count_step)/max_step;
            count_goal_continuous++;
            // if(count_goal_continuous>=20){
            //     Debug.LogError(step_height_persent+"の高さで、"+count_goal_continuous);
            // }
            goal_percent[goal_percent_index]=1;
            goal_percent_index++;
            keep_value_list(1.0f,0.0f,0.0f);//成功、転倒、カウント超過
            EndEpisode();
        }

        if(body_initial_pos.y-body.transform.position.y>2.0f){//落下
            AddReward(-1.0f);
            sum_reward+=-1.0f;
            count_goal_continuous=0;
            goal_percent[goal_percent_index]=0;
            goal_percent_index++;
            fault_brain=true;
            EndEpisode();
        }

        if(Mathf.Abs(body_pitch_x)>90.0f||Mathf.Abs(roll_z)>90.0f){
            AddReward(-1.0f);
            sum_reward+=-1.0f;
            count_goal_continuous=0;
            goal_percent[goal_percent_index]=0;
            goal_percent_index++;
            keep_value_list(0.0f,1.0f,0.0f);//成功、転倒、カウント超過
            fault_brain=true;
            EndEpisode();
        }
        if(count_step>=max_step-1){
            keep_value_list(0.0f,0.0f,1.0f);//成功、転倒、カウント超過
            fault_brain=true;
            count_goal_continuous=0;
            goal_percent[goal_percent_index]=0;
            goal_percent_index++;
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
            discreteActionsOut[0]=6;
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



