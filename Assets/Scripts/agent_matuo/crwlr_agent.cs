using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public class crwlr_agent : Agent
{
    // public Transform target;

    public GameObject crwlr_nowc_kansei;
    public GameObject target;
    public GameObject body;
    public GameObject flipper_LR;
    public GameObject flipper_LF;
    public GameObject flipper_RF;
    public GameObject flipper_RR;

    private move_body_ML[] move_body_ml = new move_body_ML[1];
    private move_flipper_ML[] move_flipper_ml = new move_flipper_ML[4];
    private flipper_F_ML[] flipper_f_ml = new flipper_F_ML[2];
    private flipper_R_ML[] flipper_r_ml = new flipper_R_ML[2];
    private Rigidbody[] rb = new Rigidbody[5];

    Rigidbody rb_body;
    Rigidbody rb_fLR;
    Rigidbody rb_fLF;
    Rigidbody rb_fRF;
    Rigidbody rb_fRR;

    //AIにより決定した値
    public float action_Go;//move_body、move_flipperに渡す値
    public float action_Front;//flipper_F_MLに渡す値
    public float action_Rear;//flipper_R_MLに渡す値
    public float action_rotate;//move_bodyに渡す値

    private Vector3 myposition = Vector3.zero;//自分の位置を入れるいれもの
    private Vector3 target_position = Vector3.zero;//ゴールの位置をいれる
    private float me2target;
    private float me2target_past=100;

    public override void Initialize(){
        
        move_body_ml[0]=body.GetComponent<move_body_ML>();

        move_flipper_ml[0]=flipper_LF.GetComponent<move_flipper_ML>();
        move_flipper_ml[1]=flipper_LR.GetComponent<move_flipper_ML>();
        move_flipper_ml[2]=flipper_RF.GetComponent<move_flipper_ML>();
        move_flipper_ml[3]=flipper_RR.GetComponent<move_flipper_ML>();

        flipper_f_ml[0]=flipper_LF.GetComponent<flipper_F_ML>();
        flipper_f_ml[1]=flipper_RF.GetComponent<flipper_F_ML>();

        flipper_r_ml[0]=flipper_LR.GetComponent<flipper_R_ML>();
        flipper_r_ml[1]=flipper_RR.GetComponent<flipper_R_ML>();
        
        rb[0]=flipper_LF.GetComponent<Rigidbody>();
        rb[1]=flipper_LR.GetComponent<Rigidbody>();
        rb[2]=flipper_RF.GetComponent<Rigidbody>();
        rb[3]=flipper_RR.GetComponent<Rigidbody>();
        rb[5]=body.GetComponent<Rigidbody>();
        
        me2target_past=(crwlr_nowc_kansei.transform.localPosition-target.transform.localPosition).magnitude;
    }

    public override void OnEpisodeBegin(){
        crwlr_nowc_kansei.transform.localPosition=new Vector3(Random.Range(-40,40),-0.8f,Random.Range(-30,30));
        // for(int i=0;i<rb.Length;i++){
        //     rb[i].velocity=Vector3.zero;
        // } 
    }

    public override void CollectObservations(VectorSensor sensor){
//Rayでの物体までの距離、前後フリッパの角度（同期的なら片足だけ）、クローラ本体の傾き（どっちに向いてるか）
        sensor.AddObservation(target.transform.localPosition.x);
        sensor.AddObservation(target.transform.localPosition.z);
        sensor.AddObservation(body.transform.localPosition.x);
        sensor.AddObservation(body.transform.localPosition.z);
        sensor.AddObservation(rb_body.linearVelocity.x);
        sensor.AddObservation(rb_body.linearVelocity.z);
    }

    public override void OnActionReceived(ActionBuffers actions){
        //AI選択した値を変数に代入
        action_Go =     actions.DiscreteActions[0];
        action_rotate = actions.DiscreteActions[1];
        action_Front =  actions.DiscreteActions[2];
        action_Rear =   actions.DiscreteActions[3];

        //AIによって決定した値をそれぞれのスクリプトに与える
        move_body_ml[0].action_Go=action_Go;
        move_body_ml[0].action_rotate=action_rotate;
        for(int i=0; i<move_flipper_ml.Length; i++){
            move_flipper_ml[i].action_Go = action_Go;
        }
        for(int i=0; i<flipper_f_ml.Length; i++){
            flipper_f_ml[i].action_Front=action_Front;
        }
        for(int i=0; i<flipper_r_ml.Length; i++){
            flipper_r_ml[i].action_Rear=action_Rear;
        }

        myposition=crwlr_nowc_kansei.transform.localPosition;//自分の位置を取得、全体のGameobject
        target_position=target.transform.localPosition;
        AddReward(-0.001f);//小さなマイナス報酬を与えることで、早くなる
        me2target = (target_position-myposition).magnitude;//ゴールと自分の距離を計算
        if(me2target<me2target_past){
            AddReward(me2target_past-me2target);
        }


        //落下
        if(myposition.y<-3.0f){
            AddReward(-1.0f);
            EndEpisode();
        }
        //転倒
        if(crwlr_nowc_kansei.transform.localEulerAngles.x<-120.0f||crwlr_nowc_kansei.transform.localEulerAngles.x>-30.0f){
            AddReward(-1.0f);
            EndEpisode();
        }
        //成功
        if(me2target<1.0f){
            AddReward(1.0f);
            EndEpisode();
        }
                me2target_past=me2target;

    }

    public override void Heuristic(in ActionBuffers actionsOut){
        var action = actionsOut.ContinuousActions;
        if (Input.GetAxisRaw("Vertical") >= 0.85f) action[0] = 5; //8.5 ～1 //actionGoは-5~5なのに対して，action[0]は 0~10 なので，5を足しとく
        else if (Input.GetAxisRaw("Vertical") >= 0.75f) action[0] = 4; //7.5 ～8.5
        else if (Input.GetAxisRaw("Vertical") >= 0.55f) action[0] = 3; //5.5 ～7.5
        else if (Input.GetAxisRaw("Vertical") >= 0.35f) action[0] = 2; //3.5 ～5.5
        else if (Input.GetAxisRaw("Vertical") >= 0.15f) action[0] = 1; //1.5 ～3.5
        else if (Input.GetAxisRaw("Vertical") >= -0.15f) action[0] = 0; //-1.5 ～1.5
        else if (Input.GetAxisRaw("Vertical") >= -0.35f) action[0] = -1; //-3.5 ～ -1.5
        else if (Input.GetAxisRaw("Vertical") >= -0.55f) action[0] = -2; //-5.5 ～ -3.5
        else if (Input.GetAxisRaw("Vertical") >= -0.75f) action[0] = -3; //-7.5 ～-5.5
        else if (Input.GetAxisRaw("Vertical") >= -0.85f) action[0] = -4; //-8.5 ～-7.5
        else if (Input.GetAxisRaw("Vertical") >= -1f) action[0] = -5; //-1 ～-8.5

        if(Input.GetKey(KeyCode.RightArrow)){action[1]=1;}//右に旋回
        else if(Input.GetKey(KeyCode.LeftArrow)){action[1]=-1;}//左に旋回
        else action[1]=0;

        if(Input.GetKey(KeyCode.Q)){action[2]=1;}//前フリッパ//上げる
        else if(Input.GetKey(KeyCode.W)){action[2]=-1;}//前フリッパ//下げる
        else action[2]=0;

        if(Input.GetKey(KeyCode.A)){action[3]=1;}//後ろフリッパ//下げる
        else if(Input.GetKey(KeyCode.S)){action[3]=-1;}//後ろフリッパ//上げる
        else action[3]=0;

    }
}
