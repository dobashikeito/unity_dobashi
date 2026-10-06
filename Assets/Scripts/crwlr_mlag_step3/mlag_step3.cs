using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System;

// RollerAgent
public class mlag_step3 : Agent
{
    public Transform target; // TargetのTransform
    private move_body_ML[] move_body_ml=new move_body_ML[1];
    private move_flipper_ML[] move_flipper_ml =new move_flipper_ML[4];
    private flipper_F_ML[] flipper_f_ml = new flipper_F_ML[2];
    private flipper_R_ML[] flipper_r_ml = new flipper_R_ML[2];
    private Rigidbody[] rtrace = new Rigidbody[8];

    [SerializeField]
    public GameObject body;
    public GameObject flipper_LR;
    public GameObject flipper_LF;
    public GameObject flipper_RF;
    public GameObject flipper_RR;
    public GameObject link;
    public GameObject table;
    public GameObject hand;
    public GameObject top_point_origin;
    public GameObject top_point_dir;
    private double distanceToTarget_past=1000.0;
    private float angleToTarget_past=180.0f;
    private int count_disatance_near=0;
    private int count_disatance_far=0;
    private int count_dir_near=0;
    private int count_dir_far=0;


    //AIにより決定した値
    public float action_Go;//move_body、move_flipperに渡す値
    public float action_Front;//flipper_F_MLに渡す値
    public float action_Rear;//flipper_R_MLに渡す値
    public float action_rotate;//move_bodyに渡す値



    

     // RollerAgentのRigidtrace

    // 初期化時に呼ばれる
    void Start(){
        // trace=GameObject.Find("body");
        // rtrace = trace.GetComponent<Rigidbody>();
    }
    public override void Initialize()
    {
        move_body_ml[0]=body.GetComponent<move_body_ML>();

        move_flipper_ml[0]=flipper_LF.GetComponent<move_flipper_ML>();
        move_flipper_ml[1]=flipper_LR.GetComponent<move_flipper_ML>();
        move_flipper_ml[2]=flipper_RF.GetComponent<move_flipper_ML>();
        move_flipper_ml[3]=flipper_RR.GetComponent<move_flipper_ML>();

        flipper_f_ml[0]=flipper_LF.GetComponent<flipper_F_ML>();
        flipper_f_ml[1]=flipper_RF.GetComponent<flipper_F_ML>();

        flipper_r_ml[0]=flipper_LR.GetComponent<flipper_R_ML>();
        flipper_r_ml[1]=flipper_RR.GetComponent<flipper_R_ML>();

        rtrace[0]=body.GetComponent<Rigidbody>();
        rtrace[1]=flipper_LR.GetComponent<Rigidbody>();
        rtrace[2]=flipper_LF.GetComponent<Rigidbody>();
        rtrace[3]=flipper_RF.GetComponent<Rigidbody>();
        rtrace[4]=flipper_RR.GetComponent<Rigidbody>();
        rtrace[5]=link.GetComponent<Rigidbody>();
        rtrace[6]=table.GetComponent<Rigidbody>();
        rtrace[7]=hand.GetComponent<Rigidbody>();

        // RollerAgentのRigidtraceの参照の取得
        // move_body_ml=trace.GetComponent<move_body_ML>();
        
    }

    // エピソード開始時に呼ばれる
    public override void OnEpisodeBegin()
    {
        // RollerAgentが床から落下している時
        if (body.transform.position.y < 0)
        {
            // RollerAgentの位置と速度をリセット
            for(int i=0;i<rtrace.Length;i++){
                rtrace[i].angularVelocity = Vector3.zero;
                rtrace[i].linearVelocity = Vector3.zero;
            }
            
            body.transform.position = new Vector3(0.0f, 1.5f, 0.0f);
            flipper_LR.transform.position = new Vector3(0.0f, 1.5f, 0.0f);
            flipper_LF.transform.position = new Vector3(0.0f, 1.5f, 0.0f);
            flipper_RF.transform.position = new Vector3(0.0f, 1.5f, 0.0f);
            flipper_RR.transform.position = new Vector3(0.0f, 1.5f, 0.0f);
            link.transform.position =  new Vector3(0.0f, 1.5f, 0.0f);
            table.transform.position =  new Vector3(0.0f, 1.5f, 0.0f);
            hand.transform.position = new Vector3(0.0f, 1.5f, 0.0f);
            
        }

        // Targetの位置のリセット
        // target.position = new Vector3(UnityEngine.Random.Range(-40,40), 0.5f, UnityEngine.Random.Range(-40,40));
        distanceToTarget_past=1000.0;
        angleToTarget_past=180.0f;
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
        
        action_rotate = actionBuffers.ContinuousActions[0];
        action_Go = actionBuffers.ContinuousActions[1];
        action_Front = actionBuffers.ContinuousActions[2];
        action_Rear = actionBuffers.ContinuousActions[3];
        // Debug.Log(action_rotate);
        // rtrace[0].AddForce(controlSignal * 150, ForceMode.Acceleration);

        for(int i=0;i<move_body_ml.Length;i++){
            move_body_ml[i].action_Go=action_Go;
        }
        for(int i=0;i<move_flipper_ml.Length;i++){
            move_flipper_ml[i].action_Go=action_Go;
        }
        for(int i=0;i<move_body_ml.Length;i++){
            move_body_ml[i].action_rotate=action_rotate;
        }
        for(int i=0;i<flipper_f_ml.Length;i++){
            flipper_f_ml[i].action_Front=action_Front;
        }
        for(int i=0;i<flipper_r_ml.Length;i++){
            flipper_r_ml[i].action_Rear=action_Rear;
        }
        // for(int i=0;i<move_body_ml.Length;i++){
        //     move_flipper_ml[i].action_rotate=action_rotate;
        // }
        Vector3 target_to_origin=target.transform.position-top_point_origin.transform.position;
        Vector3 top_point_to_origin=top_point_dir.transform.position-top_point_origin.transform.position;
        // RollerAgentがTargetの位置にたどりついた時
        double distanceToTarget = Math.Pow(Math.Pow(body.transform.position[0]-target.position[0],2)+Math.Pow(body.transform.position[2]-target.position[2],2),0.5);
        float angleToTarget=Vector3.Angle(target_to_origin,top_point_to_origin);//targetとクローラが向いている方向の間の角度
        Debug.LogWarning("傾き::"+angleToTarget);
        // Debug.LogWarning("x::"+body.transform.position[0]+"y::"+body.transform.position[2]);
        // Debug.Log(distanceToTarget);
        Debug.Log("現在の距離"+distanceToTarget);
        Debug.Log("過去の距離"+distanceToTarget_past);
        if(angleToTarget<30.0f){
            AddReward(0.5f);
        }
        if(angleToTarget_past>angleToTarget){
            Debug.Log("targetのほうを向いたよ");
            angleToTarget_past=angleToTarget;//向いたので、過去の値を更新
            if(count_dir_near>10){//連続で10回近づいた場合、プラスの報酬を与える
                AddReward(0.1f);
            }
        }
        if(angleToTarget_past<=angleToTarget){
            Debug.Log("targetのほうではないよ");
            if(count_dir_far>10){//連続で10回離れた場合、プラスの報酬を与える
                AddReward(-0.1f);
            }
        }

        if(distanceToTarget<distanceToTarget_past){
            Debug.Log("近づいたよ");
            count_disatance_far=0;
            count_disatance_near++;
            distanceToTarget_past=distanceToTarget;
            if(count_disatance_near>10){//連続で10回近づいた場合、プラスの報酬を与える
                count_disatance_near=0;
                AddReward(0.5f);
            }
            
        }
        if(distanceToTarget>distanceToTarget_past){
            Debug.Log("遠のいたよ");
            count_disatance_far++;
            count_disatance_near=0;
            if(count_disatance_far>10){//連続で10回離れてしまった場合、マイナスの報酬を与える
                Debug.Log("距離リセット");
                count_disatance_far=0;
                AddReward(-0.5f);
                distanceToTarget_past =distanceToTarget+1;
            }
        }
        if (distanceToTarget < 10.0f)
        {
            AddReward(1.0f);
            EndEpisode();
        }

        // RollerAgentが床から落下した時
        if (body.transform.position.y < 0)
        {
            AddReward(-1.0f);
            EndEpisode();
        }
        
    }

    // ヒューリスティックモードの行動決定時に呼ばれる
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
        if(Input.GetKey(KeyCode.Q)){//前フリッパ上げる
           continuousActionsOut[2] = 1;
        }
        if(Input.GetKey(KeyCode.A)){//前フリッパ下げる
           continuousActionsOut[2] = -1;
        }
        if(Input.GetKey(KeyCode.W)){//後ろフリッパ上げる
           continuousActionsOut[2] = 1;
        }
        if(Input.GetKey(KeyCode.S)){//後ろフリッパ下げる
           continuousActionsOut[3] = -1;
        }
        
    }
}

