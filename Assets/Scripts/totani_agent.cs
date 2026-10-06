using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System;
using System.Collections;

// RollerAgent
public class totani_agent : Agent
{
    public Transform target; // TargetのTransform
    public Transform crwlr_model;
    private move_body_ML_new[] move_body_ml=new move_body_ML_new[1];
    private move_flipper_ML_new[] move_flipper_ml =new move_flipper_ML_new[4];
    private flipperLF_ML_new[] flipper_lf_ml = new flipperLF_ML_new[1];
    private flipperLR_ML_new[] flipper_lr_ml = new flipperLR_ML_new[1];
    private flipperRF_ML_new[] flipper_rf_ml = new flipperRF_ML_new[1];
    private flipperRR_ML_new[] flipper_rr_ml = new flipperRR_ML_new[1];
    private Rigidbody[] rtrace = new Rigidbody[7];
    HingeJoint[] joint = new HingeJoint[4];
    JointSpring[] hingeSpring = new JointSpring[4];
    JointLimits[] limits = new JointLimits[4];
    private realsensed455[] realsense = new realsensed455[1];

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
    private float distanceToTarget_past=1000.0f;
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
    private int count_check_point=1;//距離毎にチェックポイントを作成し、応じた報酬を与えていく


    private float step_heihgt_flipper100=1.0f;//フリッパの高さと釣り合う、stepのスケール
    private float step_height;
    public float step_height_persent=0.1f;
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

    public float start_step_height=0.1f;//brainで最初の高さを設定

    private bool find_object=false;
    private int count_ray_distance_true=0;
    private float body_pitch_x=0.0f;
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

        distanceToTarget_start=Vector3.Distance(top_point_origin_initial_pos,target_initial_pos);

        // Debug.LogWarning("body::"+body.transform.position);
        // Debug.LogWarning("flipper_LR::"+flipper_LR.transform.position);
        // Debug.LogWarning("flipper_LF::"+flipper_LF.transform.position);
        // Debug.LogWarning("flipper_RF::"+flipper_RF.transform.position);
        // Debug.LogWarning("flipper_RR::"+flipper_RR.transform.position);
        // Debug.LogWarning("top_point_dir::"+top_point_dir.transform.position);
        // Debug.LogWarning("top_point_origin::"+top_point_origin.transform.position);

    }

    void Curriculum_parameter(){
        EnvironmentParameters curri_para=Academy.Instance.EnvironmentParameters;
        approach_reward=curri_para.GetWithDefault("approach_reward",0.1f);
        // step_height_persent=curri_para.GetWithDefault("step_height",start_step_height);
        // if(count_goal_continuous>10){
        //     // Debug.Log("step_height_persent::"+step_height_persent+"::allstep::"+count_allstep);
        //     // step_height_persent+=0.1f;
        // }
        step.transform.localScale=new Vector3(2.46f,step_heihgt_flipper100*step_height_persent,8.11f);
   
    }


    public override void Initialize()
    {
        

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
        rtrace[5]=top_point_dir.GetComponent<Rigidbody>();
        rtrace[6]=top_point_origin.GetComponent<Rigidbody>();

        joint[0]=flipper_LF.GetComponent<HingeJoint>();
        joint[1]=flipper_LR.GetComponent<HingeJoint>();
        joint[2]=flipper_RF.GetComponent<HingeJoint>();
        joint[3]=flipper_RR.GetComponent<HingeJoint>();

        realsense[0]=ray.GetComponent<realsensed455>();
        for(int i=0;i<joint.Length;i++){
            hingeSpring[i]=joint[i].spring;
            hingeSpring[i].spring = 400;  //力の大きさ
            hingeSpring[i].damper = 50;  
            hingeSpring[i].targetPosition = 0;  // 目的の角度
            joint[i].spring = hingeSpring[i];  // 上記三つをjointに渡す
            joint[i].useSpring = true;
        }
        step_height_persent_past=step_height_persent;
        Curriculum_parameter();
        if(step_height_persent_past!=step_height_persent){//カリキュラム学習で、段差の高さが変化した場合、ゴールにかかるステップ数を一度リセットする
            count_goalstep=0;
            count_goal_continuous=0;
        }
        
        // RollerAgentのRigidtraceの参照の取得
        // move_body_ml=trace.GetComponent<move_body_ML>();
        
    }

    // エピソード開始時に呼ばれる
    public override void OnEpisodeBegin()
    {   //crwlr_model.position=new Vector3(0.0f,1.5f,0.0f);
        // RollerAgentが床から落下している時
        count_step=0;
        count_check_point=1;
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
        distanceToTarget_past=distanceToTarget_start;
        Curriculum_parameter();
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
        // sensor.AddObservation(body.transform.localEulerAngles.y);
        // sensor.AddObservation(body.transform.localEulerAngles.z); //Agentの向き//yaw角のみなので、y値のみ、本来はz値だが、クローラモデルの関係から90°ずれている
        // sensor.AddObservation(rtrace[0].velocity.x); // AgentのX速度
        // sensor.AddObservation(rtrace[0].velocity.z); // AgentのZ速度
        sensor.AddObservation(flipper_lf_ml[0].angle);
        sensor.AddObservation(flipper_rf_ml[0].angle);
        sensor.AddObservation(flipper_lr_ml[0].angle);
        sensor.AddObservation(flipper_rr_ml[0].angle);

    }

    // 行動実行時に呼ばれる
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {//段差認識に使う値を初期化
        float body_past_z=body.transform.position.z;//過去の位置
        // RollerAgentに力を加える
        //旋回と進むを同時にしないため、値は-1～1;
        //(1-(-1))/9=
        if(actionBuffers.ContinuousActions[0]>0.33f){
            action_Go=1.0f;
            action_rotate=0;
        }
        else if(actionBuffers.ContinuousActions[0]>-0.33f){
            action_Go=0.0f;
            action_rotate=0;
        }
        else{
            action_Go=-1.0f;
            action_rotate=0;
        }


        if(actionBuffers.ContinuousActions[1]>0.33f){
            action_FrontL=1;
            action_FrontR=1;
            }
        else if(actionBuffers.ContinuousActions[1]>-0.33f){
            action_FrontL=0;
            action_FrontR=0;
            }
        else{
            action_FrontL=-1;
            action_FrontR=-1;
        }

        if(actionBuffers.ContinuousActions[2]>0.33f){
            action_RearL=1;
            action_RearR=1;
            }
        else if(actionBuffers.ContinuousActions[2]>-0.33f){
            action_RearL=0;
            action_RearR=0;
            }
        else{
            action_RearL=-1;
            action_RearR=-1;
        }

        
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

        flipper_lf_ml[0].action_FrontL=action_FrontL;
        flipper_lr_ml[0].action_RearL=action_RearL;
        flipper_rf_ml[0].action_FrontR=action_FrontR;
        flipper_rr_ml[0].action_RearR=action_RearR;

        //体の角度が横に傾いている量に応じて、ガウス分布的な報酬を与える。-0.95~0.05(90deg~0deg)
        float pitch_x=0.0f;
        if(body.transform.localEulerAngles.x>300){
            pitch_x=360.0f-body.transform.localEulerAngles.x;
        }
        else{
            pitch_x=body.transform.localEulerAngles.x;
        }

        

        // AddReward(Mathf.Exp(-5*Mathf.Pow((roll_z/180.0f*Mathf.PI),2))-0.95f);

        AddReward(-0.001f);

        float body_now_z = body.transform.position.z;
 
        if(body_now_z-body_past_z<0){
            AddReward(body_past_z-body_now_z);
        }
        count_allstep++;
        if(body_now_z<target.position.z){//ゴール
            AddReward(1.0f);
            count_goal_continuous++;
            EndEpisode();
        }

        if(body_initial_pos.y-body.transform.position.y>2.0f){//落下
            AddReward(-1.0f);
            count_goal_continuous=0;
            EndEpisode();
        }

        if(pitch_x>75.0f){
            AddReward(-1.0f);
            count_goal_continuous=0;
            EndEpisode();
        }

        
    }

    // ヒューリスティックモードの行動決定時に呼ばれる
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("move");
        continuousActionsOut[1] = Input.GetAxis("flipperLF");
        continuousActionsOut[2] = Input.GetAxis("flipperLR");
        // continuousActionsOut[3] = Input.GetAxis("flipperRF");
        // continuousActionsOut[4] = Input.GetAxis("flipperRR");
    }
}



