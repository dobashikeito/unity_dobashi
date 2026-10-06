using System.Collections;
using System.IO;//csvファイルに書き出すのに必要
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Unity.Barracuda;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Sensors.Reflection;
using Unity.MLAgents.Demonstrations;
using Unity.MLAgents.Policies;
using UnityEngine.Serialization;

public class Crawler_Curriculum_Agent : Agent
{
    public GameObject Crawler_hontai;
    public GameObject FrontRight;
    public GameObject FrontLeft;
    public GameObject RearRight;
    public GameObject RearLeft;
    public GameObject GoFlipperRight;
    public GameObject GoFlipperLeft;
    public GameObject Game; //リセットするときにGameオブジェクト全体の位置座標も使う
    public GameObject Step0; //リセットするときに段差(Step)の高さを毎回変える(y軸のpositionを-0.5から0.5の間に変える)
    public GameObject Step1;
    public GameObject Step2;
    public GameObject plane0;
    public GameObject plane1;
    public GameObject plane2;
    public GameObject ThinStep0;
    public GameObject ThinStep1;
    public GameObject ThinStep2;
    //private int carriculum_stage = 0;
    public GameObject PostureCamera;
    //private double[] step_height = new double[3];
    private float[] step_height = { 0, 0, 0 };
    public float actionGo;//他のスクリプトからこのスクリプトのactionGoの値を取得できるようにpublicにする(RosCommunicatorというオブジェクトにアタッチしたRos_exampleeeee.csというスクリプトに送ってみる)
    public float actionFront;
    public float actionRear;

    int goal_flag = -1;//成功かどうかを示すフラグ ゴールしなかったら-1点を与える
    bool isHalfPoint = false;//そのエピソード中に1回でも半分の地点にきたかどうかのラベル　 ゴールまでの中間地点に来たら1回ポイントをあげる
    //int height = 0; //カリキュラム学習的なやつを書いてみる
    int episode = -1; //試行回数をカウント episode数==試行回数だよね?//最初の1回は練習できるように-1   -1→0は最初のリセットで+1  0→1は練習 2019_8_28
    int success_count = 0; //成功回数をカウント
    const int consecutive_num = 15; // この回数連続成功したら、次へ行くというパラメータ
    int[] success_for_rate = new int[consecutive_num];//過去２０回の成功率を計算する箱 2019_8_28
    float success_rate = 0;
    int loop_consecutive = 0; //過去20回ということをしるために、0~20をループする

    private GoFlipper_miniML[] GetComp_GoFlipper = new GoFlipper_miniML[6];
    private FrontFlipper_miniML[] GetComp_FrontFlipper = new FrontFlipper_miniML[2];
    private RearFlipper_miniML[] GetComp_RearFlipper = new RearFlipper_miniML[2];
    private RayTest GetComp_RayTest;
    private Change_Center_of_Mass GetComp_CenterOfMass;
    private shock_detect GetComp_shockDetect;
    private shock_detect[] shock_detect = new shock_detect[7]; 
    private Rigidbody[] Crawler_rb = new Rigidbody[7];//速度を取得するために入れておく
    private Vector3 myposition = Vector3.zero;
    private Vector3 myposition_past = Vector3.zero; //自分の過去のTransform(位置や回転)を入れる箱
    //[↓いらないはず]
    // BoxMuller BoxMull = new BoxMuller();//Random.Rangeは一様分布だが，これを使えば正規分布でランダム2020_0521
    public float seconds = 0;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < consecutive_num; i++)
        {
            success_for_rate[i] = 0; //一応初期化しとく
        }
        myposition = Crawler_hontai.transform.position;
        myposition_past = Crawler_hontai.transform.position;  //73行目(InitializeAgent()の中の = myposition) の代わりに入れます 2020_04_21

        GetComp_GoFlipper[0] = GoFlipperRight.GetComponent<GoFlipper_miniML>();//GoFlipperRightにアタッチしてある、GoFlipper_mini_MLというスクリプトのactionGoの値をこのスクリプトのactionGoの値にする。
        GetComp_GoFlipper[1] = GoFlipperLeft.GetComponent<GoFlipper_miniML>();
        GetComp_GoFlipper[2] = FrontRight.GetComponent<GoFlipper_miniML>();
        GetComp_GoFlipper[3] = FrontLeft.GetComponent<GoFlipper_miniML>();
        GetComp_GoFlipper[4] = RearRight.GetComponent<GoFlipper_miniML>();
        GetComp_GoFlipper[5] = RearLeft.GetComponent<GoFlipper_miniML>();
        GetComp_FrontFlipper[0] = FrontRight.GetComponent<FrontFlipper_miniML>();
        GetComp_FrontFlipper[1] = FrontLeft.GetComponent<FrontFlipper_miniML>();
        GetComp_RearFlipper[0] = RearRight.GetComponent<RearFlipper_miniML>();
        GetComp_RearFlipper[1] = RearLeft.GetComponent<RearFlipper_miniML>();
        GetComp_RayTest = Crawler_hontai.GetComponent<RayTest>();
        GetComp_CenterOfMass = Crawler_hontai.GetComponent<Change_Center_of_Mass>();
        GetComp_shockDetect = Crawler_hontai.GetComponent<shock_detect>();
        shock_detect[0] = Crawler_hontai.GetComponent<shock_detect>();
        shock_detect[1] = GoFlipperRight.GetComponent<shock_detect>();
        shock_detect[2] = GoFlipperLeft.GetComponent<shock_detect>();
        shock_detect[3] = FrontRight.GetComponent<shock_detect>();
        shock_detect[4] = FrontLeft.GetComponent<shock_detect>();
        shock_detect[5] = RearRight.GetComponent<shock_detect>();
        shock_detect[6] = RearLeft.GetComponent<shock_detect>();
        Crawler_rb[0] = Crawler_hontai.GetComponent<Rigidbody>();
    }
    void Update()
    {
        //Monitor.Log("reward", GetCumulativeReward(), Crawler_hontai.transform, Posture_cameraScript);//2020_05_18
        //Monitor.Log("reward", GetCumulativeReward() / 2, null);//2020_05_18

        seconds += Time.deltaTime;
        //GetComp_TimeScript.seconds = seconds;
        //if (Input.GetKeyUp("q")) Done();//段差が登れなさそうな時につかうリセットボタン
        /*
        if(seconds >= 30f)//30秒たったらゲームオーバ-
        {
            //seconds = 0f;//Done()の関数に入れたからコメントアウトしました。
            //SetReward(-1f);//画像だけ入力にしているから時間による報酬は理解できないんじゃないかな??
            Done();
        }
        */
        //Debug.Log(Step.transform.rotation);//この値を参考に以下のstepのrotationの初期値を決めた。結局うまくいかなかった
    }
    public override void Initialize()//[InitializeAgent()をInitialize()に変更]
    {
        //base.InitializeAgent();    //何やっているのだろう?
        //CrawlerRB = Crawler.GetComponent<Rigidbody>();//ゴール判定にTriggerを使うなら必要??

        //Vector3 myposition = Crawler_hontai.transform.position; //自分のTransform(位置や回転)を取得
        //myposition_past = myposition; //ここでmyTransform_pastの定義をしておかないと、80行目では参照しているが、その前にある(62行目の)とこで、何も参照してないよって言われる。
    }


    public void StateTransition()    //報酬の定義やエピソードの終了条件の定義
    {
        //(1)mypositionに現在のクローラの位置座標をいれる
        //(2)右に行くだけより大きな報酬を受ける
        //(3)衝撃報酬を与える上での値を定義
        //(4)それぞれの場合において報酬を与えるif文
        myposition = Crawler_hontai.transform.position; //自分のTransform(位置や回転)を取得

        AddReward(-0.001f);//毎回小さなマイナスを与えることで、早くゴールしようとする。       この行だけでmax_stepが1800なら 最小で-1.8点になるね　 
        //AddReward(1f * (myposition.z - myposition_past.z)); //前回より右にいれば報酬がもらえる(右に進むほどもらえることになってるよね？(左に行けばマイナスの報酬) 
        if (myposition_past.z - Game.transform.position.z >= 0) AddReward(1f * (myposition.z - myposition_past.z)); //各agentは 段差環境の0より右にいるときだけdistポイントもらえる(2020_0921)
        float goal_point = 4f;//
        //const float step_start = 2f;

        //if (isHalfPoint == false && step_start + Game.transform.position.z < myposition.z)//段差のはじまり地点に来たら一回だけポイント
        //{
        //    AddReward(1.0f);//goalpointみたいな感じで1点       // AddReward(1f * (myposition.z - myposition_past.z));(92行目くらい)の代わりに入れる
        //    AddReward(1.5f);//さらにgoalできたとしたら1.5[m]さきのとこで1.5点もらえるはずなので      // AddReward(1f * (myposition.z - myposition_past.z));(92行目くらい)の代わりに入れる
        //    isHalfPoint = true;
        //}

        const float MIN_SEC = 0.5f;
        ////const float LIM_VEL = 0.4f;
        //const float LIM_VEL = 0.443f;// 1[cm]の高さからの自由落下で0.443[m/s]くらいになる
        ////if (shock_detect[0].now_vel.y < -LIM_VEL && seconds > MIN_SEC) AddReward(shock_detect[0].now_vel.y/3f);//20201109_ボディ部分のみ(この行だけで衝撃報酬はいいかな?)
        ////if (shock_detect[0].now_vel.y < -LIM_VEL && seconds > MIN_SEC) AddReward(-1f/3f);//20201109_ボディ部分のみ(この行だけで衝撃報酬はいいかな?)
        //if (shock_detect[1].now_vel.y < -LIM_VEL && seconds > MIN_SEC) AddReward(shock_detect[1].now_vel.y / 3f);
        //if (shock_detect[3].now_vel.y < -LIM_VEL && seconds > MIN_SEC) AddReward(shock_detect[3].now_vel.y / 3f);
        //if (shock_detect[5].now_vel.y < -LIM_VEL && seconds > MIN_SEC) AddReward(shock_detect[5].now_vel.y / 3f);
        ////const float LIM_AngVel = 3.50f; // 回転中心から先端まで155[mm]の棒が10[mm]下に自由落下(実際には回転)したときの角速度が3.498[rad/s]
        //const float LIM_AngVel = 1.51f; // 回転中心から先端まで358.95[mm]の棒が10[mm]下に自由落下(実際には回転)したときの角速度が1.5105[rad/s]
        ////const float LIM_AngVel = 2.14f; //回転中心から先端まで358.95[mm]の棒が20[mm]下に自由落下(実際には回転)したときの角速度が2.14[rad/s]
        ////const float LIM_AngVel = 2.62f; //回転中心から先端まで358.95[mm]の棒が30[mm]下に自由落下(実際には回転)したときの角速度が2.616[rad/s]
        const float LIM_AngAcc = 5.0f;
        //if ((Crawler_rb[0].angularVelocity.x < -LIM_AngVel || LIM_AngVel < Crawler_rb[0].angularVelocity.x) && seconds > MIN_SEC) AddReward(-Mathf.Abs(Crawler_rb[0].angularVelocity.x) / 2f);//20201110_衝撃は速度じゃなくて角速度の方が良いかも
        ////if ((Crawler_rb[0].angularVelocity.x < -LIM_AngVel || LIM_AngVel < Crawler_rb[0].angularVelocity.x) && seconds > MIN_SEC) AddReward(-1f / 3f);//20201110_衝撃は速度じゃなくて角速度の方が良いかも
        //if (shock_detect[0].now_acc.y >= 5.0 && seconds > MIN_SEC) AddReward(-  (shock_detect[0].now_acc.y - 5.0f ) / 2.5f);
        //if (shock_detect[0].now_acc.y >= 25.0f && seconds > MIN_SEC) AddReward(-3.0f);
        //else if (shock_detect[0].now_acc.y >= 15.0f && seconds > MIN_SEC) AddReward(-1.5f);
        //else if (shock_detect[0].now_acc.y >= 5.0f && seconds > MIN_SEC) AddReward(-0.5f);
        if (shock_detect[0].now_acc.y >= 5.0f && seconds > MIN_SEC) AddReward(-1f / 600.0f * ((shock_detect[0].now_acc.y + 10) * (shock_detect[0].now_acc.y + 10) - 25));//(5, -0.5), (15, -1.5), (25, -3)を通る2次関数
        //if (shock_detect[0].now_acc.y >= 5.0f && seconds > MIN_SEC) AddReward(-1f / 600.0f * ((shock_detect[0].now_acc.y + 10) * (shock_detect[0].now_acc.y + 10) - 25));//(5, -0.5/3), (15, -0.5), (25, -1)を通る2次関数
        //if (shock_detect[1].now_acc.y >= LIM_AngAcc && seconds > MIN_SEC) AddReward(-1f / 1800.0f * ((shock_detect[1].now_acc.y + 10) * (shock_detect[1].now_acc.y + 10) - 25));//(5, -0.5/3), (15, -0.5), (25, -1)を通る2次関数
        //if (shock_detect[3].now_acc.y >= LIM_AngAcc && seconds > MIN_SEC) AddReward(-1f / 1800.0f * ((shock_detect[3].now_acc.y + 10) * (shock_detect[3].now_acc.y + 10) - 25));//(5, -0.5/3), (15, -0.5), (25, -1)を通る2次関数
        //if (shock_detect[5].now_acc.y >= LIM_AngAcc && seconds > MIN_SEC) AddReward(-1f / 1800.0f * ((shock_detect[5].now_acc.y + 10) * (shock_detect[5].now_acc.y + 10) - 25));//(5, -0.5/3), (15, -0.5), (25, -1)を通る2次関数
        //if (shock_detect[0].now_acc.y >= 5.0f && seconds > MIN_SEC) AddReward(-1f / 1200.0f * ((shock_detect[0].now_acc.y + 10) * (shock_detect[0].now_acc.y + 10) - 25));//(5, -0.5/3), (15, -0.5), (25, -1)を通る2次関数
        //if(shock_detect[1].now_acc.y >= 5.0 && seconds > MIN_SEC) AddReward(-(shock_detect[1].now_acc.y) / 30f);
        //if (shock_detect[3].now_acc.y >= 5.0 && seconds > MIN_SEC) AddReward(-(shock_detect[3].now_acc.y) / 30f);
        //if (shock_detect[5].now_acc.y >= 5.0 && seconds > MIN_SEC) AddReward(-(shock_detect[5].now_acc.y) / 30f);
        const float R = 0.35895f;// クローラの中心から先端までの長さ　段差落ちるときや上るときにこれが回転半径となると仮定
        //if ((shock_detect[0].now_angAcc.x * R <= -10f || 10f <= shock_detect[0].now_angAcc.x * R) && seconds > MIN_SEC) AddReward(-Mathf.Abs(shock_detect[0].now_angAcc.x * R / 500f));// 角加速度×半径　＝　加速度
        //if (shock_detect[0].now_acc.y >= 5.0f && seconds > MIN_SEC) AddReward(-1.0f / 2.0f / Mathf.Sqrt(6) * Mathf.Pow(6.0f, 1.0f / 10 * shock_detect[0].now_acc.y));//指数関数acc=5.0のとき-0.5点、被験者実験の結果15[m/ss]のとき-3点になるようにしてみる

        //if (shock_detect[0].now_acc.y >= 5.0f && seconds > MIN_SEC) AddReward(-1.0f / 100 * shock_detect[0].now_acc.y * shock_detect[0].now_acc.y + 1.0f / 4.0f);// acc=5.0のとき0点、被験者実験の結果18[m/ss]のとき-3点になるようにしてみる
        //if (shock_detect[0].now_acc.y >= 5.0f && seconds > MIN_SEC) AddReward(-1.0f  * (shock_detect[0].now_acc.y * shock_detect[0].now_acc.y) / 25.0f + 1.0f);//acc=5.0のとき0点、被験者実験の結果10[m/ss]のとき-3点になるようにしてみる
        //if (shock_detect[0].now_acc.y >= 5.0f && seconds > MIN_SEC) AddReward(-3.0f  * (shock_detect[0].now_acc.y * shock_detect[0].now_acc.y) / 200.0f + 3.0f/8.0f);//acc=5.0のとき0点、被験者実験の結果15[m/ss]のとき-3点になるようにしてみる
        //if (shock_detect[0].now_acc.y >= 5.0f && seconds > MIN_SEC) AddReward(-3.0f  * (shock_detect[0].now_acc.y) / 10.0f + 3.0f/2.0f);//acc=5.0のとき0点、被験者実験の結果15[m/ss]のとき-3点になるようにしてみる
        //if (shock_detect[0].now_acc.y >= 0.0f && seconds > MIN_SEC) AddReward(-3.0f  * (shock_detect[0].now_acc.y * shock_detect[0].now_acc.y) / 10.0f);// acc=0.0のとき0点、被験者実験の結果10[m/ss]のとき-3点になるようにしてみる
        //if (shock_detect[0].now_acc.y >= 0.0f && seconds > MIN_SEC) AddReward(-3.0f * (shock_detect[0].now_acc.y * shock_detect[0].now_acc.y)/ 10.0f);// acc=0.0のとき0点、被験者実験の結果10[m/ss]のとき-3点になるようにしてみる
        //if (shock_detect[0].now_acc.y >= 10.0f && seconds > MIN_SEC) AddReward(-3.0f * (shock_detect[0].now_acc.y * shock_detect[0].now_acc.y) / 125.0f + 12.0f / 5.0f);//acc=10.0のとき0点、被験者実験の結果15[m/ss]のとき-3点になるようにしてみる


        const float SUCESS = 1f;
        if (goal_point + Game.transform.position.z < myposition.z)  //段差を乗り越えて少し進むとゴール 
        {
            //Debug.Log("Goooaaaalll   myposition_past.z: " + myposition_past.z + "  Game.transform.position.z: " + Game.transform.position.z + " myposition_past.z - Game.transform.position.z: " + (myposition_past.z - Game.transform.position.z));
            //SetReward(10000f);   //AddReward　と　SetRewardの違いは分からない。
            AddReward(SUCESS);   //AddReward　と　SetRewardの違いは分からない。
            goal_flag = 1;
            EndEpisode();
            //wait(1000);
            //success_count += 1;//成功回数をカウント(リセットしたときの関数でカウントすることにしたのでこっちはコメントアウト
        }


        //const float falls_thresh = 15, falls_thresh_quat = 0.75f;
        const float falls_thresh_quat = 0.75f;
        if (Crawler_hontai.transform.rotation.x <= -falls_thresh_quat || falls_thresh_quat <= Crawler_hontai.transform.rotation.x)  //転倒したらダメ　
        {
            AddReward(-SUCESS);
            goal_flag = 2;
            EndEpisode();
        }

        if (myposition.y < -0.2 + Game.transform.position.y)  //落ちたらダメ(ステージから0.2下に行ったらダメ)
        {
            AddReward(-SUCESS);
            goal_flag = 3;
            EndEpisode();
        }

        if (myposition.z < -3 + Game.transform.position.z)  //左にいきすぎるとダメ 
        {
            AddReward(-SUCESS);   //AddReward　と　SetRewardの違いは分からない。
            goal_flag = 3;
            EndEpisode();//[Done()をEndEpisode()に変更]
        }

        myposition_past = myposition;  //Crawler_hontaiの現在の位置を代入.
    }

    public override void CollectObservations(VectorSensor sensor)//強化学習の状態を定義
    //[CollectObservations()をCollectObservations(VectorSensor sensor)に変更] 
    //[]
    {
        float[] distance_to_object = GetComp_RayTest.distance_to_object;//RayTest.csにあるdis(物体までの距離の値)をこのスクリプトのdisに代入
        sensor.AddObservation(distance_to_object);//入力画像にfrontCameraを入れない場合はこれを入れる.
        //[AddVectorObsをsensor.AddObservationに変更]

        float FrontAngle = GetComp_FrontFlipper[0].FrontAngle;//FrontRightのフリッパーの角度 (現在の角度+ノイズになっているはず)
        sensor.AddObservation(FrontAngle);//FrontAngleは おそらく-90から90

        float RearAngle = GetComp_RearFlipper[0].RearAngle;//RearRightのフリッパーの角度 (現在の角度+ノイズになっているはず)
        sensor.AddObservation(RearAngle);//RearAngleは おそらく-90から90

        sensor.AddObservation(Crawler_hontai.transform.localEulerAngles.x + UnityEngine.Random.Range(-0.0035f / 2 * 360 / (2 * Mathf.PI), 0.0035f / 2 * 360 / (2 * Mathf.PI)));//ノイズの大きさが0.002906[rad]だったので  0.0035の大きさを足す   
        //Debug.Log("rotation.x : " + Crawler_hontai.transform.localEulerAngles.x + "  FrontAngle: " + FrontAngle + "   RearAngle: " + RearAngle);
    }


    public void MoveAgent(ActionBuffers act)//強化学習の行動の定義//[float[]をActionBuffersに]
    //(1)AIにより、actionGo、actionFront、actionRearの値を決定
    //(2)左右前足の位置座標の差を取得(robo_axisが用いられてないので必要ない)
    //(3)GoFlipper_miniML、FrontFlipper_miniML、RearFlipper_miniMLにAIに決定された値を渡す。
    //(4)カメラん+の位置設定

    {   //[act[0]をact.ContinuousActions[0]に変更]
        // actionGo = act.ContinuousActions[0] - 5;//act[0]は0~10の11段階だが，いままで，-5~5の11段階でプログラムを書いていたので，0~10 → -5~5に変換しておく //以前はfloat actionGo = act[0]-5;としてここで定義していたが，そうするとros_exampleeeee.csでactionGoの値を取得するときに0しか取得できなかったので，定義は上のpublicで定義するだけにする(定義は1回だけ)
        // actionFront = act.ContinuousActions[1] - 1;//act[1]は0~2の3段階だが，いままで，-1~1の3段階でプログラムを書いていたので，0~2 → -1~1に変換しておく
        // actionRear = act.ContinuousActions[2] - 1;//act[2]は0~2の3段階だが，いままで，-1~1の3段階でプログラムを書いていたので，0~2 → -1~1に変換しておく
        
        actionGo = act.DiscreteActions[0] - 5;//act[0]は0~10の11段階だが，いままで，-5~5の11段階でプログラムを書いていたので，0~10 → -5~5に変換しておく //以前はfloat actionGo = act[0]-5;としてここで定義していたが，そうするとros_exampleeeee.csでactionGoの値を取得するときに0しか取得できなかったので，定義は上のpublicで定義するだけにする(定義は1回だけ)
        actionFront = act.DiscreteActions[1] - 1;//act[1]は0~2の3段階だが，いままで，-1~1の3段階でプログラムを書いていたので，0~2 → -1~1に変換しておく
        actionRear = act.DiscreteActions[2] - 1;//act[2]は0~2の3段階だが，いままで，-1~1の3段階でプログラムを書いていたので，0~2 → -1~1に変換しておく
        

        Vector3 robo_center_R = GoFlipperRight.transform.position;//2020_04_21
        Vector3 robo_center_L = GoFlipperLeft.transform.position;
        Vector3 robo_axis = robo_center_R - robo_center_L;
        for (int i = 0; i < GetComp_GoFlipper.Length; i++) { GetComp_GoFlipper[i].robo_axis = robo_axis; }//GoFlipperRightにアタッチしてある、GoFlipper_mini_MLというスクリプトのrobo_axisの値をこのスクリプトのactionGoの値にする。

        for (int i = 0; i < GetComp_GoFlipper.Length; i++) { GetComp_GoFlipper[i].actionGo = actionGo; }
        for (int i = 0; i < GetComp_FrontFlipper.Length; i++) { GetComp_FrontFlipper[i].actionFront = actionFront; }
        for (int i = 0; i < GetComp_RearFlipper.Length; i++) { GetComp_RearFlipper[i].actionRear = actionRear; }

        PostureCamera.transform.position = new Vector3(Crawler_hontai.transform.position.x + 0.5f, Crawler_hontai.transform.position.y, Crawler_hontai.transform.position.z) + Game.transform.position;//カメラをクローラの少し後ろにつくように動かす
    
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)//[AgentAction()をOnActionReceived()に変更]
    {
        //[AgentAction(float[] vectorAction)をOnActionReceived(ActionBuffers actionBuffers)に変更]
        MoveAgent(actionBuffers);//行動 /f/MoveAgentはすぐ上にある関数です。vectorActionとは??//[vectorActionをactionBuffersに変更]
        StateTransition();//状態の遷移 //上で定義している関数(報酬を与える関数)
    }

    public override void OnEpisodeBegin() //[AgentReset()をOnEpisodeBegin()に変更]// エピソードの切り替えの処理　ロボットを初期位置に戻したり，センサの位置をずらしたり，段差の高さを変更したり．
    {
        isHalfPoint = false;//ゴール地点の半分の地点まで来たら点数を与える　ラベルを falseに戻す．これにより1エピソードごとに1回だけ半分の地点で点数がもらえる//[今使っていない]
        //if (goal_flag == 0) AddReward(-1.0f);//ゴールしていなかったら-1点
        seconds = 0f;//時間をリセット

        ///////////////////////////  Time_Counter//////////////////////////////////////////////////////////////////////////////////////////////////////////////↓↓
        if (episode == 1) logSave(1, "#############################");//episodeの始めの区切りのセルを入れる。

        //if(seconds >= 30) logSave(seconds  , "episode" + "," + episode + "," +  "time" + "," + seconds  );//csvファイルに書き出し　logSaveという関数は下に定義してある これだと、左に落ちた時もokと表示されてしまう。
        //else              logSave(seconds, "episode" + "," + episode + "," + "time" + "," + seconds + "," + "success_count" +  "," + success_count);//csvファイルに書き出し　logSaveという関数は下に定義してある



        //logSave(1, "episode" + "," + episode + "," + "height" + "," + Mathf.Round(100 + (100 * (Step.transform.position.y - 0.15495f) / 0.3099f)) + "," + "success[]" + "," + success[(int)Mathf.Round(100 + (100 * (Step.transform.position.y - 0.15495f) / 0.3099f))]);//csvファイルに書き出し　logSaveという関数は下に定義してある


        //logSave(seconds, "episode" + "," + episode + "," + "time" + "," + seconds + "," + "success_count" + "," + success_count);//csvファイルに書き出し　logSaveという関数は下に定義してある
        //seconds = 0f;//時間をリセット
        //Text.GetComponent<TimeScript>().seconds = seconds;
        ///////////////////////////  Time_Counter//////////////////////////////////////////////////////////////////////////////////////////////////////////////↑↑

        episode += 1; //試行回数をカウント ゲーム開始時にリセット関数が入るから、この位置にして、最初のリセットはカウントしない。
        
        loop_consecutive++;//[合計値]
        loop_consecutive = loop_consecutive % consecutive_num;//loop_20は0~20
        if (goal_flag == 1)//[段差走行に成功した場合]
        {
            success_count += 1;
            success_for_rate[loop_consecutive] = 1;
        }
        else
        {
            success_count = 0;
            success_for_rate[loop_consecutive] = 0;
        }

        for (int i = 0; i < consecutive_num; i++)
        {
            success_rate += success_for_rate[i];//success_rate は　consecutive_numでわることで　確率になるよ↓↓
        }
        //[GetStepCount()をStepCountに]
        if (goal_flag == 0) logSave(1, "episode" + "," + episode + "," + "height0" + "," + step_height[0] + "," + "height1" + "," + step_height[1] + "," + "height2" + "," + step_height[2] + "," + "success_rate" + "," + success_rate / consecutive_num + "," + "StepCount()" + "," + StepCount + "," + "GetCumulativeReward()" + "," + GetCumulativeReward() + "," + "timeOver");
        else if (goal_flag == 1) logSave(1, "episode" + "," + episode + "," + "height0" + "," + step_height[0] + "," + "height1" + "," + step_height[1] + "," + "height2" + "," + step_height[2] + "," + "success_rate" + "," + success_rate / consecutive_num + "," + "StepCount()" + "," + StepCount + "," + "GetCumulativeReward()" + "," + GetCumulativeReward() + "," + "success");
        else if (goal_flag == 2) logSave(1, "episode" + "," + episode + "," + "height0" + "," + step_height[0] + "," + "height1" + "," + step_height[1] + "," + "height2" + "," + step_height[2] + "," + "success_rate" + "," + success_rate / consecutive_num + "," + "StepCount()" + "," + StepCount + "," + "GetCumulativeReward()" + "," + GetCumulativeReward() + "," + "fall");
        else if (goal_flag == 3) logSave(1, "episode" + "," + episode + "," + "height0" + "," + step_height[0] + "," + "height1" + "," + step_height[1] + "," + "height2" + "," + step_height[2] + "," + "success_rate" + "," + success_rate / consecutive_num + "," + "StepCount()" + "," + StepCount + "," + "GetCumulativeReward()" + "," + GetCumulativeReward() + "," + "left_-0.95");
        success_rate = 0;//上のforループでカウントしたのをリセットする　これをしないと確率が1を超えてしまう

        //Debug.Log("success_count" + ":" + success_count); //printする。
        if (success_count == consecutive_num)
        {
            //height += 1;//連続 consecutive_num回クリアしたらstepの高さを上げる
            //Debug.Log("success_count" + ":" + success_count); //printする。
            //success_count = 0;
        }

        float min0, min1, min2, max0, max1, max2;
        float[] min = new float[3];
        float[] max = new float[3];
        min[0] = Academy.Instance.EnvironmentParameters.GetWithDefault("step0_min",  75);//[FloatProperties.GetPropertyWithDefault()をEnvironmentParameters.GetWithDefault]
        min[1] = Academy.Instance.EnvironmentParameters.GetWithDefault("step1_min",  75);
        min[2] = Academy.Instance.EnvironmentParameters.GetWithDefault("step2_min",  0);
        max[0] = Academy.Instance.EnvironmentParameters.GetWithDefault("step0_max",  75);
        max[1] = Academy.Instance.EnvironmentParameters.GetWithDefault("step1_max",  75);
        max[2] = Academy.Instance.EnvironmentParameters.GetWithDefault("step2_max",  0);
        //step_height[0] = Random.Range(min[0], max[0]);
        //step_height[1] = Random.Range(min[1], max[1]);
        //step_height[2] = Random.Range(min[2], max[2]);
        //////////////
        float[] sigma = new float[3];
        float[] mu = new float[3];
        for (int i = 0; i < sigma.Length; i++)
        {
            //mu[i] = (max[i] + min[i]) / 2;
            //sigma[i] = (max[i] - min[i]) / 6;// min～max の範囲を  mu-3σ～mu+3σ(99.7%の確率で起こる)にする
            //step_height[i] = BoxMull.Rand(mu[i], sigma[i], true);//2020_0618_なんか結局2020_06_03_6のやつがRayRandom 0.1でやれるし一番強そうなのでBoxMull使う
            step_height[i] = UnityEngine.Random.Range(min[i], max[i]);//20201220やっぱ一様分布じゃないと汎用性が保たれないのでは?        正規分布ではなく,一様分布を使う こうしないと最後の 15~90とかで15とか90がでにくくなる2020_06_10   
        }
        step_height[0] = step_height[1] - step_height[0];// Up→Downしたいときはこれを入れて! 2020_0623

        ///////////////////
        //Step.transform.position = new Vector3(0, 0.15495f - 0.3099f * 0.9f, 1.5f) + Game.transform.position; //Stepの高さをフリッパーの長さ(309.9mm)の10%の高さにする
        Step0.transform.position = new Vector3(0, 0.15495f - 0.3099f * (100 - step_height[0]) / 100, -1.5f) + Game.transform.position;
        Step1.transform.position = new Vector3(0, 0.15495f - 0.3099f * (100 - step_height[1]) / 100,  2.0f) + Game.transform.position;
        Step1.transform.GetChild(0).transform.localScale = new Vector3(Step1.transform.GetChild(0).transform.localScale.x, Step1.transform.GetChild(0).transform.localScale.y, UnityEngine.Random.Range(1.0f, 2.0f));//20210109_エピソードごとに段差の距離を変更する
        //Step11.transform.position = new Vector3(0, FLIP_LENGTH / 2 - FLIP_LENGTH * (100 - step_height[1]) / 100, 1.5f) + Game.transform.position;


        //float ANGLE = 0f, Step1z = Step11.transform.position.z, Step1y = FLIP_LENGTH- Step11.transform.position.y, step1cos = Mathf.Cos(ANGLE * Mathf.PI / 180f), step1sin = Mathf.Sin(ANGLE * Mathf.PI / 180f);
        //float Step1z = -Step11.transform.localScale.z / 2, Step1y = FLIP_LENGTH / 2;//長方形(段差)の左上の頂点(長方形の中心点から見た相対的な座標)
        //float ANGLE = -10f, step1cos = Mathf.Cos(ANGLE * Mathf.PI / 180f), step1sin = Mathf.Sin(ANGLE * Mathf.PI / 180f);
        //float _Step1z = Step1z * step1cos - Step1y * step1sin, _Step1y = Step1z * step1sin + Step1y * step1cos;//回転した後の座標(長方形の中心を原点とした場合を考えています．)
        //float d_Step1z = _Step1z - Step1z, d_Step1y = _Step1y - Step1y;//回転させて左上の頂点が動いた距離
        //Debug.Log(" Step1z : " + Step1z + "  Step1y : " + Step1y + "           d_Step1z : " + d_Step1z + "  d_Step1y : " + d_Step1y);
        //Step11.transform.position += new Vector3(0, -d_Step1y, -d_Step1z);
        //Step11.transform.rotation = Quaternion.Euler(-ANGLE, 0, 0);
        //Step2.transform.position = new Vector3(0, FLIP_LENGTH / 2 - FLIP_LENGTH * (100 - step_height[2]) / 100, 2.5f) + Game.transform.position;
        //Step3.transform.position = new Vector3(0, FLIP_LENGTH / 2 - FLIP_LENGTH * (100 - step_height[3]) / 100, 4.0f) + Game.transform.position;

        Step2.transform.position = new Vector3(0, 0.15495f - 0.3099f * (100 - step_height[2]) / 100,  4.0f) + Game.transform.position;
        plane0.transform.position = new Vector3(0, 0.3099f * step_height[0] / 100, -1.5f) + Game.transform.position;
        plane1.transform.position = new Vector3(0, 0.3099f * step_height[1] / 100,  2.0f) + Game.transform.position;
        plane2.transform.position = new Vector3(0, 0.3099f * step_height[2] / 100,  4.0f) + Game.transform.position;
        ThinStep0.transform.position = new Vector3(0, -ThinStep0.transform.localScale.y / 2.0f + 0.3099f * step_height[0] / 100, -1.5f) + Game.transform.position;
        ThinStep1.transform.position = new Vector3(0, -ThinStep1.transform.localScale.y / 2.0f + 0.3099f * step_height[1] / 100, 2.0f) + Game.transform.position;
        ThinStep2.transform.position = new Vector3(0, -ThinStep2.transform.localScale.y / 2.0f + 0.3099f * step_height[2] / 100, 4.0f) + Game.transform.position;

        if ((episode - 1) % 100 == 0) Debug.Log("episode" + ":" + episode + " height0: <color=blue>" + step_height[0] + "</color> height1: <color=blue>" + step_height[1] + "</color> height2: <color=blue>" + step_height[2] 
                                        + "</color> min0: " + min[0] + " max0: " + max[0] + " min1: " + min[1] + " max1: " + max[1] + " min2: " + min[2] + " max2: " + max[2]);//1000試行ごとにprintする(最初にリセット関数が入るので、episode－１が試行した回数になるはず
        //if (episode == 150) UnityEditor.EditorApplication.isPlaying = false;//10episodeで終了する

        goal_flag = 0;//成功したというフラグを消す
        Vector3 randompos = new Vector3(0, 0, UnityEngine.Random.Range(-1.5f, 0.5f));//ランダムにz方向の初期位置を変える  引数をfloatにする場合 min<=random_pos<=max
        //Vector3 randompos = Vector3.zero;//初期位置は固定、ランダムにしたいなら上を使おう
        
        float shoki = 0.08f + 0.3099f*step_height[0]/100;//

        Crawler_hontai.transform.position = new Vector3(0, shoki, 0) + randompos + Game.transform.position;
        FrontRight.transform.position = new Vector3(0.16f, shoki, 0.204f) + randompos + Game.transform.position;
        FrontLeft.transform.position = new Vector3(-0.16f, shoki, 0.204f) + randompos + Game.transform.position;
        RearRight.transform.position = new Vector3(0.16f, shoki, -0.204f) + randompos + Game.transform.position;
        RearLeft.transform.position = new Vector3(-0.16f, shoki, -0.204f) + randompos + Game.transform.position;
        GoFlipperLeft.transform.position = new Vector3(-0.12f, shoki, 0) + randompos + Game.transform.position;
        GoFlipperRight.transform.position = new Vector3(0.12f, shoki, 0) + randompos + Game.transform.position;


        Crawler_hontai.transform.rotation = new Quaternion(0, 0, 0, 0);//rotationを(0,90,0)(初期位置)にする
        //Debug.Log(GoFlipperLeft.transform.position);//この値を参考に以下のrotationの初期値を決めた。
        FrontRight.transform.rotation = new Quaternion(-0.7f, 0.0f, 0.0f, 0.7f);
        FrontLeft.transform.rotation = new Quaternion(-0.7f, 0.0f, 0.0f, 0.7f);
        RearRight.transform.rotation = new Quaternion(-0.7f, 0.0f, 0.0f, 0.7f);
        RearLeft.transform.rotation = new Quaternion(-0.7f, 0.0f, 0.0f, 0.7f);
        GoFlipperLeft.transform.rotation = new Quaternion(-0.7f, 0.0f, 0.0f, 0.7f);
        GoFlipperRight.transform.rotation = new Quaternion(-0.7f, 0.0f, 0.0f, 0.7f);

        //フリッパーも初期位置にしたほうがよいかな??
        for (int i = 0; i < GetComp_FrontFlipper.Length; i++) { GetComp_FrontFlipper[i].Targetdeg = 0; }//FrontRight等にアタッチしてある、FrontFripperというスクリプトのTargetdegの値を0にする.
        for (int i = 0; i < GetComp_RearFlipper.Length; i++) { GetComp_RearFlipper[i].Targetdeg = 0; }


        float rayposRand = 0.025f;//2.5[cm]
        GetComp_RayTest.randompos = new Vector3(0, UnityEngine.Random.Range(-rayposRand, rayposRand), UnityEngine.Random.Range(-rayposRand, rayposRand));//AgentReset()が起こるたびに位置が変更
        float rayangRand = 2.5f;// [deg]
        GetComp_RayTest.random_shift = UnityEngine.Random.Range(-rayangRand, rayangRand);
        //GetComp_CenterOfMass.center_of_mass = new Vector3(0, Random.Range(0.05f, 0.2f), Random.Range(-0.1f, 0.1f));
        //GetComp_CenterOfMass.center_of_mass = new Vector3(0, 0.2f, Random.Range(-0.1f, 0f));
        //GetComp_CenterOfMass.center_of_mass = new Vector3(0, Random.Range(0.1f, 0.3f), Random.Range(-0.1f, 0.1f));
        //GetComp_CenterOfMass.center_of_mass = new Vector3(0, Random.Range(0.1f, 0.3f), 0);
        myposition = Crawler_hontai.transform.position; //自分のTransform(位置や回転)を取得
        myposition_past = Crawler_hontai.transform.position;//最初にリセット関数が入るからここでもこの行がいるんだっけ？忘れた

    }


    public override void Heuristic(in ActionBuffers actionsOut)// 学習器を使わず，手動で動かすための関数らしい(ML-Agentsで用意されていたのをオーバーライド)
    //[public override float[] Heuristic()をpublic override void Heuristic(in ActionBuffers actionsOut)にに変更]
    {
        // var action = new float[3];//[これを下に変更]
        var action = actionsOut.ContinuousActions;
        float actionGo = 0, actionFront = 0, actionRear = 0;

        if (Input.GetAxisRaw("Vertical") >= 0.85f) action[0] = 5f + 5; //8.5 ～1 //actionGoは-5~5なのに対して，action[0]は 0~10 なので，5を足しとく
        else if (Input.GetAxisRaw("Vertical") >= 0.75f) action[0] = 4f + 5; //7.5 ～8.5
        else if (Input.GetAxisRaw("Vertical") >= 0.55f) action[0] = 3f + 5; //5.5 ～7.5
        else if (Input.GetAxisRaw("Vertical") >= 0.35f) action[0] = 2f + 5; //3.5 ～5.5
        else if (Input.GetAxisRaw("Vertical") >= 0.15f) action[0] = 1f + 5; //1.5 ～3.5
        else if (Input.GetAxisRaw("Vertical") >= -0.15f) action[0] = 0f + 5; //-1.5 ～1.5
        else if (Input.GetAxisRaw("Vertical") >= -0.35f) action[0] = -1f + 5; //-3.5 ～ -1.5
        else if (Input.GetAxisRaw("Vertical") >= -0.55f) action[0] = -2f + 5; //-5.5 ～ -3.5
        else if (Input.GetAxisRaw("Vertical") >= -0.75f) action[0] = -3f + 5; //-7.5 ～-5.5
        else if (Input.GetAxisRaw("Vertical") >= -0.85f) action[0] = -4f + 5; //-8.5 ～-7.5
        else if (Input.GetAxisRaw("Vertical") >= -1f) action[0] = -5 + 5; //-1 ～-8.5

        //位置制御みたいなやつ
        if (Input.GetButton("FrontFlipperDown")) action[1] = 1 + 1;
        else if (Input.GetButton("FrontFlipperUp")) action[1] = -1 + 1;
        else action[1] = 0 + 1;
        if (Input.GetButton("RearFlipperUp")) action[2] = 1 + 1;
        else if (Input.GetButton("RearFlipperDown")) action[2] = -1 + 1;
        else action[2] = 0 + 1;


        actionGo += 5;//actionGoは-5~5なのに対して，action[0]は 0~10 なので，5を足しとく
        actionFront += 1;
        actionRear += 1;
        action[0] = actionGo;
        action[1] = actionFront;
        action[2] = actionRear;
        // return action;//[必要ないみたい]
    }

    public void logSave(float num, string txt)//csvファイルにデータを書き込む関数
    {
        //StreamWriter sw;
        //FileInfo fi;
        //fi = new FileInfo(Application.dataPath + "/learning_stage_2020_06_10_1_ChangeCarri_memory96_RAYPOS0_0.28_0.08_75e5.csv");//Application.dataPathはAssetフォルダの直下らしい
        //sw = fi.AppendText();
        //sw.WriteLine(txt);
        ////if(num < 30)sw.Write("," + "oK"); //30秒以内にクリアしたなら〇を隣のセルに追加
        ////sw.WriteLine(num + "," + txt);// + "," + を入れると、横にセル移動してくれるみたい
        ////sw.Write("1");//Writeは改行しない?
        ////sw.WriteLine("test output");//WriteLineは改行する//textを書いてから改行する？
        //sw.Flush();
        //sw.Close();
    }

}