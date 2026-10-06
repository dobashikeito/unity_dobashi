using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System;

// RollerAgent
public class target_trace : Agent
{
    public Transform target; // TargetのTransform
    private move_body_ML move_body_ml;
    [SerializeField]
    public GameObject trace;
    public GameObject flipper_LR;
    public GameObject flipper_LF;
    public GameObject flipper_RF;
    public GameObject flipper_RR;
    public GameObject link;
    public GameObject table;
    public GameObject hand;
    private double distanceToTarget_past=1000.0;
    private int count=0;
    private int count_1=0;
    

    private Rigidbody[] rtrace = new Rigidbody[8]; // RollerAgentのRigidtrace

    // 初期化時に呼ばれる
    void Start(){
        // trace=GameObject.Find("body");
        // rtrace = trace.GetComponent<Rigidbody>();
    }
    public override void Initialize()
    {
        rtrace[0]=trace.GetComponent<Rigidbody>();
        rtrace[1]=flipper_LR.GetComponent<Rigidbody>();
        rtrace[2]=flipper_LF.GetComponent<Rigidbody>();
        rtrace[3]=flipper_RF.GetComponent<Rigidbody>();
        rtrace[4]=flipper_RR.GetComponent<Rigidbody>();
        rtrace[5]=link.GetComponent<Rigidbody>();
        rtrace[6]=table.GetComponent<Rigidbody>();
        rtrace[7]=hand.GetComponent<Rigidbody>();

        // RollerAgentのRigidtraceの参照の取得
        move_body_ml=trace.GetComponent<move_body_ML>();
        
    }

    // エピソード開始時に呼ばれる
    public override void OnEpisodeBegin()
    {
        // RollerAgentが床から落下している時
        if (trace.transform.position.y < 0)
        {
            // RollerAgentの位置と速度をリセット
            for(int i=0;i<rtrace.Length;i++){
                rtrace[i].angularVelocity = Vector3.zero;
                rtrace[i].linearVelocity = Vector3.zero;
            }
            
            trace.transform.position = new Vector3(0.0f, 1.5f, 0.0f);
            flipper_LR.transform.position = new Vector3(0.0f, 1.5f, 0.0f);
            flipper_LF.transform.position = new Vector3(0.0f, 1.5f, 0.0f);
            flipper_RF.transform.position = new Vector3(0.0f, 1.5f, 0.0f);
            flipper_RR.transform.position = new Vector3(0.0f, 1.5f, 0.0f);
            link.transform.position =  new Vector3(0.0f, 1.5f, 0.0f);
            table.transform.position =  new Vector3(0.0f, 1.5f, 0.0f);
            hand.transform.position = new Vector3(0.0f, 1.5f, 0.0f);
            
        }

        // Targetの位置のリセット
        target.position = new Vector3(UnityEngine.Random.Range(-40,40), 0.5f, UnityEngine.Random.Range(-40,40));
        distanceToTarget_past=1000.0;
    }

    // 状態取得時に呼ばれる
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target.position.x); //TargetのX座標
        sensor.AddObservation(target.position.z); //TargetのZ座標
        sensor.AddObservation(trace.transform.position.x); //RollerAgentのX座標
        sensor.AddObservation(trace.transform.position.z); //RollerAgentのZ座標
        sensor.AddObservation(rtrace[0].linearVelocity.x); // RollerAgentのX速度
        sensor.AddObservation(rtrace[0].linearVelocity.z); // RollerAgentのZ速度
    }

    // 行動実行時に呼ばれる
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // RollerAgentに力を加える
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        rtrace[0].AddForce(controlSignal * 150, ForceMode.Acceleration);

        // RollerAgentがTargetの位置にたどりついた時
        double distanceToTarget = Math.Pow(Math.Pow(trace.transform.position[0]-target.position[0],2)+Math.Pow(trace.transform.position[2]-target.position[2],2),0.5);
        // Debug.Log(distanceToTarget);
        Debug.Log("現在の距離"+distanceToTarget);
        Debug.Log("過去の距離"+distanceToTarget_past);
        if(distanceToTarget<distanceToTarget_past){
            Debug.Log("近づいたよ");
            count=0;
            count_1++;
            distanceToTarget_past=distanceToTarget;
            if(count_1>10){
                count_1=0;
                AddReward(0.5f);
            }
            
        }
        if(distanceToTarget>distanceToTarget_past){
            Debug.Log("遠のいたよ");
            count_1=0;
            count++;
            if(count>10){
                Debug.Log("距離リセット");
                count=0;
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
        if (trace.transform.position.y < 0)
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
    }
}
