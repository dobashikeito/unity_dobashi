using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System;
using System.Collections;

// RollerAgent
public class step3_heuristic : Agent
{
    public Transform target; // TargetのTransform
    public Transform crwlr_model;
    private move_body_ML[] move_body_ml=new move_body_ML[1];
    private move_flipper_ML[] move_flipper_ml =new move_flipper_ML[4];
    private flipper_LF_capsule[] flipper_lf_ml = new flipper_LF_capsule[1];
    private flipper_LR_capsule[] flipper_lr_ml = new flipper_LR_capsule[1];
    private flipper_RF_capsule[] flipper_rf_ml = new flipper_RF_capsule[1];
    private flipper_RR_capsule[] flipper_rr_ml = new flipper_RR_capsule[1];
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
    public GameObject ray;
    private double distanceToTarget_past=1000.0;
    private float angleToTarget_past=180.0f;
    private int count_disatance_near=0;
    private int count_disatance_far=0;
    private int count_dir_near=0;
    private int count_dir_far=0;
    private int count_step=0;
    private int count_goal=0;


    //AIにより決定した値
    public float action_Go;//move_body、move_flipperに渡す値
    public float action_rotate;//move_bodyに渡す値
    public float action_FrontL;//flipper_LF_MLに渡す値
    public float action_FrontR;//flipper_RF_MLに渡す値
    public float action_RearL;//flipper_LR_MLに渡す値
    public float action_RearR;//flipper_RR_MLに渡す値
    private float approach_reward;

    private bool find_object=false;
    private int count_ray_distance_true=0;

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
    }

    public override void Initialize()
    {
        

        move_body_ml[0]=body.GetComponent<move_body_ML>();

        move_flipper_ml[0]=flipper_LF.GetComponent<move_flipper_ML>();
        move_flipper_ml[1]=flipper_LR.GetComponent<move_flipper_ML>();
        move_flipper_ml[2]=flipper_RF.GetComponent<move_flipper_ML>();
        move_flipper_ml[3]=flipper_RR.GetComponent<move_flipper_ML>();

        flipper_lf_ml[0]=flipper_LF.GetComponent<flipper_LF_capsule>();
        flipper_rf_ml[0]=flipper_RF.GetComponent<flipper_RF_capsule>();

        flipper_lr_ml[0]=flipper_LR.GetComponent<flipper_LR_capsule>();
        flipper_rr_ml[0]=flipper_RR.GetComponent<flipper_RR_capsule>();

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
            hingeSpring[i].spring = 10000;  //力の大きさ
            hingeSpring[i].damper = 10000;  
            hingeSpring[i].targetPosition = 0;  // 目的の角度
            joint[i].spring = hingeSpring[i];  // 上記三つをjointに渡す
            joint[i].useSpring = true;
        }
        Curriculum_parameter();
        
        // RollerAgentのRigidtraceの参照の取得
        // move_body_ml=trace.GetComponent<move_body_ML>();
        
    }

    // エピソード開始時に呼ばれる
    public override void OnEpisodeBegin()
    {   //crwlr_model.position=new Vector3(0.0f,1.5f,0.0f);
        // RollerAgentが床から落下している時
        count_step=0;
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
        distanceToTarget_past=1000.0;
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
        sensor.AddObservation(body.transform.localEulerAngles.x);
        sensor.AddObservation(body.transform.localEulerAngles.y);
        sensor.AddObservation(body.transform.localEulerAngles.z); //Agentの向き//yaw角のみなので、y値のみ、本来はz値だが、クローラモデルの関係から90°ずれている
        // sensor.AddObservation(rtrace[0].velocity.x); // AgentのX速度
        // sensor.AddObservation(rtrace[0].velocity.z); // AgentのZ速度
        sensor.AddObservation(flipper_lf_ml[0].angle);
        sensor.AddObservation(flipper_rf_ml[0].angle);
        sensor.AddObservation(flipper_lr_ml[0].angle*-1.0f);
        sensor.AddObservation(flipper_rr_ml[0].angle*-1.0f);

    }

    // 行動実行時に呼ばれる
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        count_ray_distance_true=0;
        find_object=false;
        // RollerAgentに力を加える
        //旋回と進むを同時にしないため、値は-1～1
        action_Go=actionBuffers.ContinuousActions[0]*0.06f;
        action_rotate=actionBuffers.ContinuousActions[5];
        // action_Go=actionBuffers.ContinuousActions[0];
        // action_rotate=0;
        // action_Go=0.0f;
        // action_rotate=0.0f;
        action_FrontL = actionBuffers.ContinuousActions[1];
        action_RearL = actionBuffers.ContinuousActions[2];
        action_FrontR = actionBuffers.ContinuousActions[3];
        action_RearR = actionBuffers.ContinuousActions[4];

        float roll_z=0.0f;
        if(body.transform.localEulerAngles.z>300){
            roll_z=360.0f-body.transform.localEulerAngles.z;
        }
        else{
            roll_z=body.transform.localEulerAngles.z;
        }
        // Debug.Log(Mathf.Exp(-5*Mathf.Pow((roll_z/180.0f*Mathf.PI),2))-0.95f);
        AddReward(Mathf.Exp(-5*Mathf.Pow((roll_z/180.0f*Mathf.PI),2))-0.95f);
        AddReward(-0.001f);
        // Debug.Log(action_rotate);
        // rtrace[0].AddForce(controlSignal * 150, ForceMode.Acceleration);

        for(int i=0;i<realsense[0].ray_distance.Length;i++){
            if(realsense[0].ray_distance[i]<0.1f){
                count_ray_distance_true+=1;
            }
        }
        if(count_ray_distance_true>0){
            find_object=true;
        }
        else{
            find_object=false;
        }
        // Debug.Log(find_object+":::"+count_ray_distance_true);

        for(int i=0;i<move_body_ml.Length;i++){
            move_body_ml[i].action_Go=action_Go*3.0f;
        }
        for(int i=0;i<move_flipper_ml.Length;i++){
            move_flipper_ml[i].action_Go=action_Go*3.0f;
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
        
        Vector3 target_to_origin=target.transform.position-top_point_origin.transform.position;

        //targetとの距離測定
        double distanceToTarget = Math.Pow(Math.Pow(top_point_origin.transform.position[0]-target.position[0],2)+Math.Pow(top_point_origin.transform.position[2]-target.position[2],2),0.5);
	// if(count_step!=0){//近づく量毎回報酬を与える
	//     AddReward((float)distanceToTarget_past-(float)distanceToTarget);
	// }
        // Debug.LogWarning(approach_reward);
        if(body.transform.localEulerAngles.x>300){
            Debug.Log(body.transform.localEulerAngles.x-360.0f);
        }
        else{Debug.Log(body.transform.localEulerAngles.x);}


        if(distanceToTarget_past>distanceToTarget){//最初の10000ステップだけ、近づいたら報酬を与える
            AddReward(approach_reward);
        }
        if(distanceToTarget_past<distanceToTarget){//最初の10000ステップだけ、近づいたら報酬を与える
            AddReward(-1.0f*approach_reward);
        }
        // Debug.Log(distanceToTarget_past-distanceToTarget);
        distanceToTarget_past=distanceToTarget;
        //成功
        if (distanceToTarget < 10.0f)
        {
            AddReward(1.0f);
            count_goal+=1;
            Debug.LogError(count_goal);
            EndEpisode();
        }

        // RollerAgentが床から落下した時
        // if (body.transform.position.y <body_initial_pos.y -2)
        // {
        //     AddReward(-1.0f);
        //     EndEpisode();
        // }
        
        if(count_step>10000){
            AddReward(-1.0f);
            EndEpisode();
        }
        count_step++;
        
    }

    // ヒューリスティックモードの行動決定時に呼ばれる
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        // if(Input.GetAxis("move")>0.8f){continuousActionsOut[0] = 1.0f;}
        // else if(Input.GetAxis("move")>0.6f){continuousActionsOut[0] = 0.8f;}
        // else if(Input.GetAxis("move")>0.4f){continuousActionsOut[0] = 0.6f;}
        // else if(Input.GetAxis("move")>0.2f){continuousActionsOut[0] = 0.4f;}
        // else if(Input.GetAxis("move")>0.0f){continuousActionsOut[0] = 0.2f;}
        // else if(Input.GetAxis("move")==0.0f){continuousActionsOut[0] = 0.0f;}
        // else if(Input.GetAxis("move")>-0.2f){continuousActionsOut[0] =-0.2f;}
        // else if(Input.GetAxis("move")>-0.4f){continuousActionsOut[0] =-0.4f;}
        // else if(Input.GetAxis("move")>-0.6f){continuousActionsOut[0] =-0.6f;}
        // else if(Input.GetAxis("move")>-0.8f){continuousActionsOut[0] =-0.8f;}
        // else if(Input.GetAxis("move")>-1.1f){continuousActionsOut[0] =-1.0f;}
        continuousActionsOut[0] = Input.GetAxis("move");
        continuousActionsOut[1] = Input.GetAxis("flipperLF");
        continuousActionsOut[2] = Input.GetAxis("flipperLR");
        continuousActionsOut[3] = Input.GetAxis("flipperRF");
        continuousActionsOut[4] = Input.GetAxis("flipperRR");
        continuousActionsOut[5] = Input.GetAxis("rotation");
    }
}

