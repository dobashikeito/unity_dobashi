using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System;
using System.Collections;

// RollerAgent
public class model_ver2 : Agent
{
    public Transform target; // TargetのTransform
    public Transform crwlr_model;
    private move_body_ML[] move_body_ml=new move_body_ML[1];
    private move_flipper_ML[] move_flipper_ml =new move_flipper_ML[4];
    private flipper_F_ML[] flipper_f_ml = new flipper_F_ML[2];
    private flipper_R_ML[] flipper_r_ml = new flipper_R_ML[2];
    private Rigidbody[] rtrace = new Rigidbody[7];

    //学習を始める最初の位置
    private Vector3 body_initial_pos;
    private Vector3 flipper_LF_initial_pos;
    private Vector3 flipper_LR_initial_pos;
    private Vector3 flipper_RF_initial_pos;
    private Vector3 flipper_RR_initial_pos;
    private Vector3 top_point_dir_initial_pos;
    private Vector3 top_point_origin_initial_pos;
    private Vector3 target_initial_pos;

    // private Vector3 body_initial_rotate;
    // private Vector3 flipper_LF_initial_rotate;
    // private Vector3 flipper_LR_initial_rotate;
    // private Vector3 flipper_RF_initial_rotate;
    // private Vector3 flipper_RR_initial_rotate;
    // private Vector3 top_point_dir_initial_rotate;
    // private Vector3 top_point_origin_initial_rotate;


    [SerializeField]
    public GameObject body;
    public GameObject flipper_LR;
    public GameObject flipper_LF;
    public GameObject flipper_RF;
    public GameObject flipper_RR;
    public GameObject top_point_origin;
    public GameObject top_point_dir;
    private double distanceToTarget_past=1000.0;
    private float angleToTarget_past=180.0f;
    private int count_disatance_near=0;
    private int count_disatance_far=0;
    private int count_dir_near=0;
    private int count_dir_far=0;
    private int count_step=0;


    //AIにより決定した値
    public float action_Go;//move_body、move_flipperに渡す値
    public float action_Front;//flipper_F_MLに渡す値
    public float action_Rear;//flipper_R_MLに渡す値
    public float action_rotate;//move_bodyに渡す値



    

     // RollerAgentのRigidtrace

    // 初期化時に呼ばれる
    void Start(){
        body_initial_pos=body.transform.position;
        flipper_LF_initial_pos=flipper_LF.transform.position;
        flipper_LR_initial_pos=flipper_LR.transform.position;
        flipper_RF_initial_pos=flipper_RF.transform.position;
        flipper_RR_initial_pos=flipper_RR.transform.position;
        top_point_dir_initial_pos=top_point_dir.transform.position;
        top_point_origin_initial_pos=top_point_origin.transform.position;
        target_initial_pos=target.position;

        // body_initial_rotate=body.transform.rotation;
        // flipper_LF_initial_rotate=flipper_LR.transform.rotation;
        // flipper_LR_initial_rotate=flipper_LR.transform.rotation;
        // flipper_RF_initial_rotate=flipper_LR.transform.rotation;
        // flipper_RR_initial_rotate=flipper_LR.transform.rotation;
        // top_point_dir_initial_rotate=top_point_dir.transform.rotation;
        // top_point_origin_initial_rotate=top_point_origin.transform.rotation;

        // Debug.LogWarning("body::"+body.transform.position);
        // Debug.LogWarning("flipper_LR::"+flipper_LR.transform.position);
        // Debug.LogWarning("flipper_LF::"+flipper_LF.transform.position);
        // Debug.LogWarning("flipper_RF::"+flipper_RF.transform.position);
        // Debug.LogWarning("flipper_RR::"+flipper_RR.transform.position);
        // Debug.LogWarning("top_point_dir::"+top_point_dir.transform.position);
        // Debug.LogWarning("top_point_origin::"+top_point_origin.transform.position);

    }
    public override void Initialize()
    {
        

        move_body_ml[0]=body.GetComponent<move_body_ML>();

        move_flipper_ml[0]=flipper_LF.GetComponent<move_flipper_ML>();
        move_flipper_ml[1]=flipper_LR.GetComponent<move_flipper_ML>();
        move_flipper_ml[2]=flipper_RF.GetComponent<move_flipper_ML>();
        move_flipper_ml[3]=flipper_RR.GetComponent<move_flipper_ML>();

        rtrace[0]=body.GetComponent<Rigidbody>();
        rtrace[1]=flipper_LR.GetComponent<Rigidbody>();
        rtrace[2]=flipper_LF.GetComponent<Rigidbody>();
        rtrace[3]=flipper_RF.GetComponent<Rigidbody>();
        rtrace[4]=flipper_RR.GetComponent<Rigidbody>();
        rtrace[5]=top_point_dir.GetComponent<Rigidbody>();
        rtrace[6]=top_point_origin.GetComponent<Rigidbody>();

        // RollerAgentのRigidtraceの参照の取得
        // move_body_ml=trace.GetComponent<move_body_ML>();
        
    }

    // エピソード開始時に呼ばれる
    public override void OnEpisodeBegin()
    {   //crwlr_model.position=new Vector3(0.0f,1.5f,0.0f);
        // RollerAgentが床から落下している時
        count_step=0;
        if (body.transform.position.y < body_initial_pos.y-2)
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
            
            // Debug.LogWarning(body.transform.position);
            // Debug.LogWarning(body.transform.localPosition);
            // StartCoroutine("Time_stop");
        }

        // Targetの位置のリセット
        target.position = new Vector3(UnityEngine.Random.Range(-40,40), 0.0f, UnityEngine.Random.Range(-40,40))+target_initial_pos;
        distanceToTarget_past=1000.0;
        // angleToTarget_past=180.0f;

    }

    IEnumerator Time_stop(){//1秒止める、確実に着地してから力を与えるため
        yield return new WaitForSeconds(2);
    }

    // 状態取得時に呼ばれる
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target.position.x); //TargetのX座標
        sensor.AddObservation(target.position.z); //TargetのZ座標
        sensor.AddObservation(body.transform.position.x); //AgentのX座標
        sensor.AddObservation(body.transform.position.z); //AgentのZ座標
        sensor.AddObservation(body.transform.localEulerAngles.y); //Agentの向き//yaw角のみなので、y値のみ、本来はz値だが、クローラモデルの関係から90°ずれている
        sensor.AddObservation(rtrace[0].linearVelocity.x); // AgentのX速度
        sensor.AddObservation(rtrace[0].linearVelocity.z); // AgentのZ速度

    }

    // 行動実行時に呼ばれる
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // RollerAgentに力を加える
        if(actionBuffers.ContinuousActions[0]<0){//旋回と進むを同時にしないため、値は-0.5～0.5
            action_Go=0;
            action_rotate=actionBuffers.ContinuousActions[0]+0.5f;
        }
        else{
            action_Go=actionBuffers.ContinuousActions[0]-0.5f;
            action_rotate=0;
        }
        AddReward(-0.001f);
        // action_rotate = actionBuffers.ContinuousActions[0];
        // action_Go = action_rotate;
        // action_Go = actionBuffers.ContinuousActions[1];
        // Debug.Log(action_rotate);
        // rtrace[0].AddForce(controlSignal * 150, ForceMode.Acceleration);

        for(int i=0;i<move_body_ml.Length;i++){
            move_body_ml[i].action_Go=action_Go*15f;
        }
        for(int i=0;i<move_flipper_ml.Length;i++){
            move_flipper_ml[i].action_Go=action_Go*15f;
        }
        for(int i=0;i<move_body_ml.Length;i++){
            move_body_ml[i].action_rotate=action_rotate*30f;
        }

        Vector3 target_to_origin=target.transform.position-top_point_origin.transform.position;
        // RollerAgentがTargetの位置にたどりついた時
        double distanceToTarget = Math.Pow(Math.Pow(top_point_origin.transform.position[0]-target.position[0],2)+Math.Pow(top_point_origin.transform.position[2]-target.position[2],2),0.5);

        // Debug.Log("現在の距離"+distanceToTarget);
        // Debug.Log("過去の距離"+distanceToTarget_past);

        AddReward((float)distanceToTarget_past/10.0f-(float)distanceToTarget/10.0f);
        // if(distanceToTarget<distanceToTarget_past){
        //     // Debug.Log("近づいたよ");
        //     // count_disatance_far=0;
        //     // count_disatance_near++;
            
        //     distanceToTarget_past=distanceToTarget;
        //     // if(count_disatance_near>10){//連続で10回近づいた場合、プラスの報酬を与える
        //     //     count_disatance_near=0;
            
        //     }
            
        // }
        // if(distanceToTarget>distanceToTarget_past){
        //     // Debug.Log("遠のいたよ");
        //     count_disatance_far++;
        //     count_disatance_near=0;
        //     if(count_disatance_far>20){//連続で10回離れてしまった場合、マイナスの報酬を与える
        //         // Debug.Log("距離リセット");
        //         count_disatance_far=0;
        //         AddReward(-0.1f);
        //         distanceToTarget_past =distanceToTarget+1;
        //     }
        // }
        //成功
        distanceToTarget_past=distanceToTarget;
        if (distanceToTarget < 15.0f)
        {
            AddReward(1.0f);
            EndEpisode();
        }

        // RollerAgentが床から落下した時
        if (body.transform.position.y < body_initial_pos.y-2)
        {
            AddReward(-1.0f);
            EndEpisode();
        }
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
        continuousActionsOut[0] = Input.GetAxis("rotation");
        // continuousActionsOut[1] = Input.GetAxis("move");
    }
}

