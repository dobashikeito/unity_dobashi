//x→いじらない、何もなし→場合によって変える
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System;
using System.Collections;
using System.IO;

public class ufm_agent_sra : Agent
{
    private float sum_reward=0.0f;
    public Transform target; //x 使ってないがプログラム上修正めんどくさいので残す、TargetのTransform
    public GameObject[] field_list = new GameObject[3];//x inspector上で、地形を設定する。3つの地形を想定していたので、3となっているだけ。
    private imax_flipper_control_fl_ML[] flipper_FL_ml = new imax_flipper_control_fl_ML[1];//x フリッパ前左のスクリプトを参照する
    private imax_flipper_control_rl_ML[] flipper_RL_ml = new imax_flipper_control_rl_ML[1];//x フリッパ前右のスクリプトを参照する
    private imax_flipper_control_fr_ML[] flipper_FR_ml = new imax_flipper_control_fr_ML[1];//x フリッパ後右のスクリプトを参照する
    private imax_flipper_control_rr_ML[] flipper_RR_ml = new imax_flipper_control_rr_ML[1];//x フリッパ後左のスクリプトを参照する
    private shock_detect_new shock;//x 今は使っていない、衝撃を求めるように使っていたが、エージェントファイル上でできるように変更した。
    public float max_step=10000;//inspector上で設定。inspectorの一番上のmaxstepと同じにする。ゴール時に与える報酬を増やすのに使用
    private rs_origin_ufm[] realsense = new rs_origin_ufm[1];//realsense値取得スクリプトの参照、realsenseの数を増やしたいときは、1を変更する。
    public sra_cp_trigger[] cp_trigger=new sra_cp_trigger[1];//x ウェイポイント判定のスクリプトを参照 
    public touch_wall[] touch_wall=new touch_wall[5];//x 壁触れた判定スクリプトに使用。接触判定増やしたいときは値を変える
    public rs_caribrate rs_caribrate;//xrealsenseの距離値を修正するのに使用。realsenseは、照射場所からの距離を図る。そのため、1m先に壁があったら、画角内の値はすべて1mになる。しかし、unityでの実装は、照射場所と当たった箇所の距離となるため、端っこの値はどうしても長くなる。それを本来の値に修正するためのスクリプトの参照。
    private ros_pubsub_ai ros_pubsub_ai;//x 現実の値を使用するのに使用、学習には使わない。
    public Unity.Robotics.UrdfImporter.Control.Controller_imax_ML urdf_control;//x urdfから生成されているロボットを動かすためのスクリプトを参照

    //学習を始める最初の位置
    private Vector3 target_initial_pos;//x 使っていない
    private Vector3 body_initial_pos;//x 使っていない
    //学習を始める最初の角度

    [SerializeField]
    public GameObject top_object;//x prefabの頂点にあるオブジェクト。これでリセット。
    public GameObject body_state;//x 状態量を得るのに使用、RPY角
    public GameObject body;//x cpにふれたかの判定に使用、状態量を得るのにも使用。logi_cp/detect_touch
    public GameObject body_fl;//x flipper_flの接触判定。以下3つ同様。
    public GameObject body_fr;//x
    public GameObject body_rl;//x
    public GameObject body_rr;//x
    public GameObject flipper_RL;//x フリッパのスクリプトを参照するために使用
    public GameObject flipper_FL;//x
    public GameObject flipper_FR;//x
    public GameObject flipper_RR;//x
    public GameObject top_point_origin;//x ロボット先端のgameobject
    public GameObject top_point_dir;//x ロボット先端から少し前に出したのgameobject。上との組合せで、ロボットが向いている方向を求める。
    public GameObject step;//x 高さを変えられる張本人。scaleをいじられる
    public GameObject ray;//x realsenseのオブジェクト取得。realsenseを増やすときは、この名前に似たgameobjectを増やすと良い
    public GameObject start;//x 使っていない
    public GameObject create_prefab;//inspectorで指定。ウェイポイントとロボットのセットのprefabを指定。エピソードごとに生成するprefab
    public GameObject prefab;//x 上で作ったprefabを補完するgameobject
    public GameObject ros_pubsub_ai_go;//x 実機からの値を得る
    public GameObject rs_caribrate_obj;//x 上で述べたように、realsenseのcaribrateを使うため。
    public GameObject click_point_go;//x click cp modeのみ。学習には使わないclickしたときのgoを保存。
    private float distanceToTarget_past=1000.0f;//x 初期値、無視
    private float distanceTocp=0.0f;//x cpとの距離
    private float angleTocp=0.0f;//x cpとの間の角度
    private float angleToTarget_past=180.0f;//x 初期値、無視
    private float max_distance = 0.01245f;//x 今は使っていない。actionGoが１で進める最大距離
    private float dis2target_past = 0.0f;//x 今は使っていない。消去めんどくさくて取ってある。下も同じ。targetとの2次元距離過去
    private float dis2target = 0.0f;//x targetとの2次元距離

    private int count_disatance_near=0;//x 今は使っていない。
    private int count_disatance_far=0;//x 今は使っていない。
    private float count_dir_near=0.1f;//x 今は使っていない。
    private int count_dir_far=0;//x 今は使っていない。
    public int count_allstep=0;//x 前エピソードのステップ数
    private int count_step=0;//x １エピソードのステップ数
    private int count_goal=0;//x ゴールした回数
    private int count_goalstep=0;//x ゴールした時のステップ数
    public int count_goal_continuous=0;//x 連続ゴール数を記録
    private int count_check_point;//x チェックポイントの数
    private int now_cp_index=0;//x 現在のウェイポイントのindex番号
    private int count_episode_reset=0;//x エピソードの回数、リセットのために使用


    private float step_heihgt_flipper100;//x 今は使っていない。フリッパの高さと釣り合う、stepのスケール。めんどくさくて残してる？
    private float step_position;//x 今は使っていない。stepの初期ポジション
    private float step_height;//x 今は使っていない。
    public float step_height_persent=0.1f;//x inspectorで、play時の段差の高さの％を指定できる。
    private float config_shp=0.0f;//x 今は使っていない。config/.yamlファイルでカリキュラムラーニング使っていた時に使用していたもの。
    private float step_height_persent_past;//x 今は使っていない。段差の高さの前回値を保存しておく
    private Vector3 cp_dis_vector=Vector3.zero;//x cpとの二次元距離算出に使用


    //AIにより決定した値、この値でロボットを動かしている。
    public float action_Go_Left=0.0f;//imax_body_control_lefが取得する値
    public float action_Go_Right=0.0f;//imax_body_control_rightが取得する値
    public float action_FrontL=0.0f;//imax_flipper_control_LF_MLに渡す値
    public float action_FrontR=0.0f;//imax_flipper_control_RF_MLに渡す値
    public float action_RearL=0.0f;//imax_flipper_control_LR_MLに渡す値
    public float action_RearR=0.0f;//imax_flipper_control_RR_MLに渡す値
    private float approach_reward;//最初の100000ステップだけ与える報酬

    private float distanceToTarget_start;//x 今は使っていない。初期ポジとtargetの距離、正規化するのに使用

    // public float start_step_height=0.0f;//brainで最初の高さを設定

    // private bool find_object=false;//x 今は使っていない。
    // private int count_ray_distance_true=0;
    private float body_pitch_x=0.0f;//x pitchの角度を-180～180に変更したくて
    public int[] goal_percent=new int[20];//goalしたら1、失敗は0になる。20回試行での成功率なので、20となっている。
    private int goal_percent_index=0;//x goal_percentに記録するときの、保存する場所のindexを変えるために使用。
    // public int goal_percent_sum=0;
    private float seconds=0.0f;//x 速度求めるのに使用、0.5秒間隔にしたかったので。
    public bool fault_over=false;//x brain中の失敗　カウントオーバー
    public bool fault_roll=false;//x brin中の失敗　転倒
    public bool onoff_8wall_fault=false;//x 8wall間違えたら失敗   

    // brainに使用
    StreamWriter file;
    [Header("brainするときにtrue")]
    public string file_name;//x 自律走行評価に使用。csv fileで掃き出してくれる
    public bool brain_on;//x 自律走行評価に使用。trueでcsv fileで掃き出してくれる。
    private float time_brain=0;//x 自律走行評価に使用。csv fileで掃き出す際に、ある時間間隔で出力したかったため、その計算用
    private float time_memo_brain=0;//x 自律走行評価に使用
    private float time_past_brain=0;//x 自律走行評価に使用
    private float time_reward=0;// 報酬に使用、汚い
    private float time_past_reward=0;// 報酬に使用、汚い

    private Vector3 initial_pos_dt;//x 最初のpositionを保存
    private float now_acc_y_reward;//y方向の加速度。報酬用。汚いので修正したい
    private float[] past_xyzrpy4vel = new float[6];//x 過去のvel速度を求めるために、positionとlocaleuylerangleを保存してる。
    private float[] now_xyzrpy4vel = new float[6];//x 現在のvel速度を求めるために、positionとlocaleuylerangleを保存してる。
    private float[] vel_xyzrpy = new float[6];//x 上2つで求められた速度。
    
    private int file_num=1;//x 自律走行評価時に使用、この値が、決めた閾値を超えたときに強制敵に評価のcsv出力が終わる。

    //報酬、ステップ量、接触回数、失敗（転倒、カウント超過を分ける）のデータをリストに保存
    private int count_episode=0;
    [HideInInspector]
    public List<float> reward_list=new List<float>();//1エピソードの累計報酬を保存
    [HideInInspector]
    public List<float> step_list=new List<float>();//1エピソードのステップ数を保存
    [HideInInspector]
    public List<float> touch_list_body=new List<float>();//1エピソードの本体の接触回数を保存
    [HideInInspector]
    public List<float> touch_list_fl=new List<float>();//1エピソードのflipper_flの接触回数を保存。以下3つ同様。
    [HideInInspector]
    public List<float> touch_list_fr=new List<float>();
    [HideInInspector]
    public List<float> touch_list_rl=new List<float>();
    [HideInInspector]
    public List<float> touch_list_rr=new List<float>();
    [HideInInspector]
    public float[] touch_count_now=new float[5];//高柳追加。接触を即時報酬にするため。touch_wall[]の値と同じにする
    [HideInInspector]
    public float[] touch_count_past=new float[5];//高柳追加。行動実行前の接触回数を保存しておく。touch_wall[]の値と同じにする
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
    public bool create_cp_target=false;
    private int click_tf_index=0;
    private create_toggle4checkpoint create_toggle4checkpoint;
    //↑
    // 初期化時に呼ばれる
    private float past_y = 0.0f;
    
    private float y_reward =0.0f;
    private float past_y_reward = 0.0f;//報酬用。汚い。
    private float past_past_y_reward = 0.0f;//報酬用。汚い。
    private float past_veloy = 0.0f;//報酬用。汚い。
    private float count_fall = 0;
    //なぜ失敗かをチェックするため
    public string[] check_cause=new string[20];//失敗した理由
    private int check_cause_index=0;
    public float[] each_route_check = new float[4]; //失敗した数
    public float[] each_route_try = new float[4];//挑戦した数

    //noiseを加える
    public int input_noise = 0;//noiseの%
    public float input_noise_max;
    public float input_noise_min;
    public bool yes_update_late;//遅延ありの状態で学習するかどうか
    private float[,] updating_time_list = new float[10,24];//floatの数10が遅延数、0.02*10=0.2秒 24は状態数
    private int now_data_value = 9;//現在値のindex
    private int past_data_value = 0;//過去値のindex
    public int field_number = 0;//fieldの指定　0→astm　1→steps  2→steps
    public int episode_step =0;
    void Start(){
        //現実世界モードか、click cp modeかをチェック。学習するなら使用しない。
        if(real_world){
            ros_pubsub_ai = ros_pubsub_ai_go.GetComponent<ros_pubsub_ai>();
        }
        if(create_cp_target){
            create_toggle4checkpoint = click_point_go.GetComponent<create_toggle4checkpoint>();
        }
        
        
    }

    void Curriculum_parameter(){
        //カリキュラムラーニングによる高さを変更するところ。onepisodebeginで発動する関数。
        EnvironmentParameters curri_para=Academy.Instance.EnvironmentParameters;
        //noiseを乗せるところ。
        input_noise_max=1.0f+input_noise/100.0f;
        input_noise_min=1.0f-input_noise/100.0f;
        //実際にスケール、高さを変更するところ。
        step.transform.localScale=new Vector3(step.transform.localScale.x,step_heihgt_flipper100*step_height_persent,step.transform.localScale.z);
        
    }


    public override void Initialize()//play時に最初に発動する関数
    {
        //inspector上で指定したフィールドをtrueにされ、出現する。
        field_list[field_number].SetActive(true);
        input_noise_max=1.0f+input_noise/100.0f;
        input_noise_min=1.0f-input_noise/100.0f;
        rs_caribrate = rs_caribrate_obj.GetComponent<rs_caribrate>();
        //ゴールの確率をリストにまとめる
        for(int i=0;i<goal_percent.Length;i++){
            goal_percent[i]=0;
        }
        //0で割るを回避するため、それぞれの経路の成功率をチェック
        for(int i=0;i<each_route_check.Length;i++){
            each_route_check[i]=1.0f;
            each_route_try[i]=1.0f;
        }
        //stepの10割の高さを保存
        step_heihgt_flipper100=step.transform.localScale.y;
        step_position=step.transform.localPosition.y;
        step_height_persent_past=step_height_persent;
        Curriculum_parameter();
        //cpの一番最初のやつとの距離を初期値として保存
        if(brain_on){
            file=new StreamWriter("Assets/csv_file/brain/ASTM/"+file_name+"_brain_"+this.gameObject.name+".csv",false);
        }
        
        
    }

    // エピソード開始時に呼ばれる
    public override void OnEpisodeBegin()
    {   //prefabを消す。上記でも多分書いたが、urdfで生成されたロボットはtransformで動かせないので、一度ロボットとウェイポイントのprefabを削除して、再出現させている。
        if(count_episode>0){
            Destroy(prefab);
        }
        //上のif文の判定用。
        count_episode+=1;
        //prefab生成
        prefab = Instantiate(create_prefab, prefab_pos+this.transform.position, Quaternion.identity);
        //各々のgameobjectを生成したprefabから発見して取得。
        prefab.transform.parent = this.transform;
        flipper_FL = transform.Find("base_link_and_sra_cp(Clone)/base_link_cp/fl").gameObject;
        flipper_FR = transform.Find("base_link_and_sra_cp(Clone)/base_link_cp/fr").gameObject;
        flipper_RL = transform.Find("base_link_and_sra_cp(Clone)/base_link_cp/rl").gameObject;
        flipper_RR = transform.Find("base_link_and_sra_cp(Clone)/base_link_cp/rr").gameObject;
        // step = transform.Find("astm3").gameObject;
        top_point_dir = transform.Find("base_link_and_sra_cp(Clone)/base_link_cp/top_point_dir").gameObject;
        top_point_origin = transform.Find("base_link_and_sra_cp(Clone)/base_link_cp/top_point_origin").gameObject;
        ray = transform.Find("base_link_and_sra_cp(Clone)/base_link_cp/ray_origin").gameObject;
        body =transform.Find("base_link_and_sra_cp(Clone)/base_link_cp/detect_touch").gameObject;
        body_fl = transform.Find("base_link_and_sra_cp(Clone)/base_link_cp/fl/detect_touch").gameObject;
        body_fr = transform.Find("base_link_and_sra_cp(Clone)/base_link_cp/fr/detect_touch").gameObject;
        body_rl = transform.Find("base_link_and_sra_cp(Clone)/base_link_cp/rl/detect_touch").gameObject;
        body_rr = transform.Find("base_link_and_sra_cp(Clone)/base_link_cp/rr/detect_touch").gameObject;
        body_state = transform.Find("base_link_and_sra_cp(Clone)/base_link_cp").gameObject;
        //一度prefabを削除しているので、もう一度スクリプトのstart関数を実行している。
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

        realsense[0]=ray.GetComponent<rs_origin_ufm>();
        realsense[0].Start();

        touch_wall[0]=body.GetComponent<touch_wall>();
        touch_wall[0].Start();
        touch_wall[1]=body_fl.GetComponent<touch_wall>();
        touch_wall[1].Start();
        touch_wall[2]=body_fr.GetComponent<touch_wall>();
        touch_wall[2].Start();
        touch_wall[3]=body_rl.GetComponent<touch_wall>();
        touch_wall[3].Start();
        touch_wall[4]=body_rr.GetComponent<touch_wall>();
        touch_wall[4].Start();

        touch_wall[0].touch_reward_once = -1.0f;
        touch_wall[1].touch_reward_once = -1.0f;
        touch_wall[2].touch_reward_once = -1.0f;
        touch_wall[3].touch_reward_once = -1.0f;
        touch_wall[4].touch_reward_once = -1.0f;

        shock=body.GetComponent<shock_detect_new>();
        shock.Start();

        cp_trigger[0]=body.GetComponent<sra_cp_trigger>();
        cp_trigger[0].make_cp(field_number);

        //値の初期化　ここから
        if(goal_percent_index>=20){
            goal_percent_index=0;
            check_cause_index=0;
        }
        
        cp_dis_vector = top_point_origin.transform.position - cp_trigger[0].cp_list[0].transform.position;
        distanceToTarget_start=Mathf.Sqrt(Mathf.Pow(cp_dis_vector[0],2)+Mathf.Pow(cp_dis_vector[2],2));
        
        cp_trigger[0].now_cp=0;//indexを0にする
        distanceToTarget_past=distanceToTarget_start;
        distanceTocp=distanceToTarget_start;
        Curriculum_parameter();

        if(start!=null){
            start.SetActive(true);
        }

        prefab.name="base_link_and_sra_cp(Clone)"+count_episode+this.gameObject.name;
        target.transform.localPosition=new Vector3(UnityEngine.Random.Range(-4.5f,4.5f),0.43f,UnityEngine.Random.Range(-4.5f,4.5f)-12.0f);
        dis2target_past=Vector3.Distance(top_point_origin.transform.position,target.position);
        if(brain_on){
            export_brain_start();
        }
        
        for(int i=0;i<10;i++){
            for(int k=0;k<24;k++){
                updating_time_list[i,k]=0.0f;
            }
        }
        initial_pos_dt=body_state.transform.position;//y加速度求める用
        now_xyzrpy4vel[0]=initial_pos_dt.x;
        now_xyzrpy4vel[1]=initial_pos_dt.y;
        now_xyzrpy4vel[2]=initial_pos_dt.z;
        now_xyzrpy4vel[3]=body_state.transform.localEulerAngles.x;
        now_xyzrpy4vel[4]=body_state.transform.localEulerAngles.y;
        now_xyzrpy4vel[5]=body_state.transform.localEulerAngles.z;

        for (int i = 0; i < 6; i++)
        {
            vel_xyzrpy[i] = 0.0f;
            past_xyzrpy4vel[i] = now_xyzrpy4vel[i];
        }
        //ここまで
    }

    IEnumerator Time_stop()
    {//1秒止める、確実に着地してから力を与えるため。今は使っていない。
        yield return new WaitForSeconds(10);
    }
    

    // 状態取得時に呼ばれる。
    public override void CollectObservations(VectorSensor sensor)
    {
        // sensor.AddObservation(target.position.x); //TargetのX座標
        // sensor.AddObservation(target.position.z); //TargetのZ座標
        // sensor.AddObservation(body.transform.position.x); //AgentのX座標
        // sensor.AddObservation(body.transform.position.z); //AgentのZ座標
        // sensor.AddObservation(body.transform.position.y)
        if(!real_world){
            //値が取れなくてエラーがでたときがあったので、エラー回避用。
            if(float.IsNaN(body_state.transform.localEulerAngles.x)||float.IsNaN(flipper_FL_ml[0].currentDrive.target)||float.IsNaN(flipper_FR_ml[0].currentDrive.target)||float.IsNaN(flipper_RL_ml[0].currentDrive.target)||float.IsNaN(flipper_RR_ml[0].currentDrive.target)||float.IsNaN(realsense[0].ray_distance[0])){
                for(int i=0;i<23;i++){
                    sensor.AddObservation(0.0f);
                }
            }
            else{
                if(yes_update_late){//取得できる値に遅延があるという条件で学習するとき。10の長さを持つリストに順番に値をいれていき、その中から値を取っていくことで、遅延を再現する。
                    updating_time_list[now_data_value,0]=distanceTocp*UnityEngine.Random.Range(input_noise_min,input_noise_max);
                    updating_time_list[now_data_value,1]=angleTocp*UnityEngine.Random.Range(input_noise_min,input_noise_max);
                    updating_time_list[now_data_value,2]=body_state.transform.localEulerAngles.x*UnityEngine.Random.Range(input_noise_min,input_noise_max);
                    updating_time_list[now_data_value,3]=body_state.transform.localEulerAngles.y*UnityEngine.Random.Range(input_noise_min,input_noise_max);
                    updating_time_list[now_data_value,4]=body_state.transform.localEulerAngles.z*UnityEngine.Random.Range(input_noise_min,input_noise_max);
                    updating_time_list[now_data_value,5]=flipper_RR_ml[0].currentDrive.target*UnityEngine.Random.Range(input_noise_min,input_noise_max);
                    updating_time_list[now_data_value,6]=flipper_FL_ml[0].currentDrive.target*UnityEngine.Random.Range(input_noise_min,input_noise_max);
                    updating_time_list[now_data_value,7]=flipper_FR_ml[0].currentDrive.target*UnityEngine.Random.Range(input_noise_min,input_noise_max);
                    updating_time_list[now_data_value,8]=flipper_RL_ml[0].currentDrive.target*UnityEngine.Random.Range(input_noise_min,input_noise_max);
                    for(int i=0;i<15;i++){
                        updating_time_list[now_data_value,i+9]=realsense[0].ray_distance[i]*rs_caribrate.ray_caribrate[i]*UnityEngine.Random.Range(input_noise_min,input_noise_max);
                    }
                    for(int i=0;i<24;i++){
                        sensor.AddObservation(updating_time_list[past_data_value,i]);
                    }
                    now_data_value+=1;
                    past_data_value+=1;
                    if(now_data_value>9){//遅延の数
                        now_data_value=0;
                    }
                    if(past_data_value>9){//遅延の数
                        past_data_value=0;
                    }
                }
                else{
                    sensor.AddObservation(distanceTocp*UnityEngine.Random.Range(input_noise_min,input_noise_max));//距離mm 平面距離
                    // Debug.LogError(distanceTocp);
                    sensor.AddObservation(angleTocp*UnityEngine.Random.Range(input_noise_min,input_noise_max));//角度deg
                    // Debug.LogWarning(angleTocp);
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
                    float[] realsense_val = realsense[0].ray_distance;//realsenseの数、行列変更したら毎回変更する必要あり。
                    for(int i=0;i<realsense_val.Length;i++){
                        if(float.IsNaN(realsense_val[i])){
                            realsense_val[i]=0.0f;
                        }
                        else{
                            // Debug.LogWarning("1");
                            // Debug.LogWarning(realsense[0].ray_distance[i]);
                            // Debug.LogWarning("2");
                            // Debug.LogWarning(rs_caribrate.ray_caribrate[i]);
                            realsense_val[i]=realsense[0].ray_distance[i]*rs_caribrate.ray_caribrate[i]*UnityEngine.Random.Range(input_noise_min,input_noise_max);
                            // Debug.LogWarning("2");
                            // realsense_val[i]=realsense[0].ray_distance[i];
                            // Debug.Log(realsense_val[i]);
                        }
                        
                    }
                    sensor.AddObservation(realsense_val);
                    sensor.AddObservation(vel_xyzrpy);
                }
                
            }
        }
        else{
            sensor.AddObservation(ros_pubsub_ai.rs_data_list);
            sensor.AddObservation(ros_pubsub_ai.imu_data_list);
            sensor.AddObservation(ros_pubsub_ai.flipper_data_list);
        }
        
        

    }

    // 行動実行時に呼ばれる、update関数みたいな感じ。
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if(brain_on){
            export_excel();
        }

        int now_cp_past = cp_trigger[0].now_cp;//進む前のnow_cp
        float body_past_z=body.transform.position.z;//
        //旋回と進むを同時にしないため、値は-1～1;
        if(!real_world){
            robot_Action(actionBuffers);//実環境上のロボットに行動値を渡すとき
        }
        else{
            realrobot_action(actionBuffers);//シミュレータ上のロボット
        }
        
        
        //体の角度が横に傾いている量に応じて、ガウス分布的な報酬を与える。-0.01~0.005(90deg~0deg) 0-20は正の報酬が得られる
        //roll_z角を保存、0がうまく反映されないので、if文を用いて調整。-180～180に
        float roll_z=0.0f;
        if(body_state.transform.localEulerAngles.z>180.0f){
            roll_z=body_state.transform.localEulerAngles.z-360.0f;
        }
        else{
            roll_z=body_state.transform.localEulerAngles.z;
        }
        //ロール角に応じた報酬
        AddReward(0.02f*(Mathf.Exp(-5*Mathf.Pow((roll_z/180.0f*Mathf.PI),2))-0.55f));
        //のちにcsv fileに掃き出すために。sum_rewardに足す、設定した報酬に対しては。
        sum_reward+=0.02f*(Mathf.Exp(-5*Mathf.Pow((roll_z/180.0f*Mathf.PI),2))-0.55f);


        //ピッチ角も-180～180に
        body_pitch_x=body_state.transform.localEulerAngles.x;
        if(body_state.transform.localEulerAngles.x>180){
            body_pitch_x=body_state.transform.localEulerAngles.x-360.0f;
        }


        //速度を求める用の値を算出　ここから
        if(body_state.transform.localEulerAngles.x>180.0f){
            now_xyzrpy4vel[3]=body_state.transform.localEulerAngles.x-360.0f;
        }
        else{
            now_xyzrpy4vel[3]=body_state.transform.localEulerAngles.x;
        }
        if(body_state.transform.localEulerAngles.x>180.0f){
            now_xyzrpy4vel[4]=body_state.transform.localEulerAngles.y-360.0f;
        }
        else{
            now_xyzrpy4vel[4]=body_state.transform.localEulerAngles.y;
        }
        if(body_state.transform.localEulerAngles.x>180.0f){
            now_xyzrpy4vel[5]=body_state.transform.localEulerAngles.z-360.0f;
        }
        else{
            now_xyzrpy4vel[5]=body_state.transform.localEulerAngles.z;
        }
        now_xyzrpy4vel[0]=body_state.transform.position.x-initial_pos_dt.x;
        now_xyzrpy4vel[1]=body_state.transform.position.y-initial_pos_dt.y;
        now_xyzrpy4vel[2]=body_state.transform.position.z-initial_pos_dt.z;
        //ここまで

        //常時与える
        AddReward(-0.005f);
        sum_reward+=-0.005f;

        
        for (int i = 0; i < touch_count_now.Length; i++)//接触回数の増加分だけ一回当たりの報酬を加算
        {
            touch_count_now[i] = touch_wall[i].count_touch;
            AddReward(touch_wall[i].touch_reward_once*(touch_count_now[i] - touch_count_past[i]));
            sum_reward += touch_wall[i].touch_reward_once*(touch_count_now[i] - touch_count_past[i]);
            // Debug.Log($"{touch_wall[i].touch_reward_once*(touch_count_now[i] - touch_count_past[i])}");
            touch_count_past[i] = touch_count_now[i];
        }

        y_reward=body_state.transform.position.y-initial_pos_dt.y;
        float veloy = y_reward - past_y_reward;
        now_acc_y_reward = veloy - past_veloy;
        past_y_reward = y_reward;
        past_veloy = veloy;
        // Debug.Log($"{now_acc_y_reward}");

        if ( 0.006f < now_acc_y_reward && count_step > 100 && past_veloy < 0)
        {
            AddReward(-0.0f);
            sum_reward+=-0.0f;
            count_fall++;
            // Debug.Log($"{now_acc_y_reward}");
        }


        
        //学習時はこっち
        if(!create_cp_target){
            //ウェイポイントとの相対角度を求める、ここから
            int now_cp=cp_trigger[0].now_cp;//進んだ後のnow_cp
            //ゴールの方向に向いてたら、追加報酬
            if(now_cp!=cp_trigger[0].count_check_point){//ゴール時にゴール判定の前にこの関数が実行されてしまうので、indexのエラーが起きる。→そのための回避。
                //ウェイポイント方向算出
                cp_dis_vector=cp_trigger[0].cp_list[now_cp].transform.position - top_point_origin.transform.position;
            }
            //ロボット方向算出
            Vector3 body_dir= top_point_dir.transform.position - top_point_origin.transform.position;
            Vector3 plane_normal = Vector3.up;
            var plane_cp_dis_vector=Vector3.ProjectOnPlane(cp_dis_vector,plane_normal);
            var plane_body_dir = Vector3.ProjectOnPlane(body_dir,plane_normal);
            //ロボットとウェイポイントの間の角度算出
            angleTocp = Vector3.SignedAngle(plane_body_dir,plane_cp_dis_vector,plane_normal);
            float cos_theta_goal = Mathf.Cos(angleTocp/180.0f*3.14f);
            //ここまで
            // angleTocp = Mathf.Acos(cos_theta_goal); 
            // Debug.LogWarning(cos_theta_goal);
            // Debug.LogError(angleTocp);
            if(cos_theta_goal>0.0f){
                AddReward(cos_theta_goal*0.005f);//cpに対する角度による報酬
                // Debug.Log(cos_theta_goal);
                sum_reward+=cos_theta_goal*0.005f;
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
                //距離報酬算出
                AddReward(dis_reward*10.0f);
                sum_reward+=dis_reward*10.0f;
                distanceToTarget_past=distanceTocp;
            }
            

            seconds+=Time.deltaTime;
            if (seconds > 0.5f){//直線速度、角速度を求める。0.5秒ごとで算出
                for(int i=0;i<6;i++){
                    vel_xyzrpy[i]=(now_xyzrpy4vel[i]-past_xyzrpy4vel[i])/seconds;
                    past_xyzrpy4vel[i] = now_xyzrpy4vel[i];
                }
                // if(Mathf.Abs(vel_xyzrpy[3])>40.0f){//速度がある閾値を超えたらマイナスを与える。//yからpitch速度へ変更
                //     AddReward(-5.0f);
                //     sum_reward-=5.0f;
                // }
                // AddReward(Mathf.Pow(vel_xyzrpy[3],2)*-0.001f);
                // sum_reward+=Mathf.Pow(vel_xyzrpy[3],2)*-0.001f;
                seconds=0.0f;
            }

            if(cp_trigger[0].cp_through){//距離が近づいたら、distanceToTarget_startを次のcpとの距離にし、falseにする、indexを増やす。
                cp_trigger[0].cp_through=false;
                if(now_cp!=cp_trigger[0].count_check_point){//indexを超えてしまったら、次はゴールとの設定にする
                    // distanceToTarget_past=Vector3.Distance(top_point_origin.transform.position,cp_trigger[0].cp_list[now_cp].transform.position);
                    distanceToTarget_past=Mathf.Sqrt(Mathf.Pow(cp_dis_vector[0],2)+Mathf.Pow(cp_dis_vector[2],2));
                    distanceTocp=distanceToTarget_past;
                    // AddReward(cp_trigger[0].turn_reward);
                    // sum_reward+=cp_trigger[0].turn_reward;
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
                // Debug.LogWarning(goal_percent_index);
                goal_percent[goal_percent_index]=1;
                goal_percent_index++;
                check_cause[check_cause_index]=cp_trigger[0].cp_index.ToString()+"_clear";
                check_cause_index++;
                each_route_try[cp_trigger[0].cp_index]+=1;

                // Debug.LogWarning(sum_reward);
                keep_value_list(1.0f,0.0f,0.0f);//成功、転倒、カウント超過。csvに書き込む用に補完しておく。
                if(brain_on){
                    export_brain_goal("clear");
                }
                EndEpisode();
            }
        }
        else {//clickした箇所に行くときに使用、学習には必要ないので無視する。
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

        if(Mathf.Abs(body_pitch_x)>80.0f||Mathf.Abs(roll_z)>80.0f){//転倒時処理
            AddReward(-30.0f);
            sum_reward+=-30.0f;
            count_goal_continuous=0;
            goal_percent[goal_percent_index]=0;
            goal_percent_index++;
            check_cause[check_cause_index]=cp_trigger[0].cp_index.ToString()+"_tumble";
            check_cause_index++;
            keep_value_list(0.0f,1.0f,0.0f);//成功、転倒、カウント超過
            fault_roll=true;
            each_route_check[cp_trigger[0].cp_index]+=1;
            each_route_try[cp_trigger[0].cp_index]+=1;
            if(brain_on){
                export_brain_goal("tumble");
            }
            EndEpisode();
        }
        if(count_step>=max_step-1){//時間超過時処理
            AddReward(-5.0f);
            sum_reward+=-5.0f;
            keep_value_list(0.0f,0.0f,1.0f);//成功、転倒、カウント超過
            fault_over=true;
            count_goal_continuous=0;
            goal_percent[goal_percent_index]=0;
            goal_percent_index++;
            check_cause[check_cause_index]=cp_trigger[0].cp_index.ToString()+"_time_over";
            check_cause_index++;
            each_route_check[cp_trigger[0].cp_index]+=1;
            each_route_try[cp_trigger[0].cp_index]+=1;
            if(brain_on){
                export_brain_goal("timeover");
            }
            EndEpisode();
        }
        
        
    }

    // ヒューリスティックモードの行動決定時に呼ばれる
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //ps4、ps5コントローラで動かせる。
        float a=Input.GetAxis("Axis 2");
        float b=Input.GetAxis("Axis 1");
        float c=Input.GetAxis("Axis 6");
        float d=Input.GetAxis("Axis 3");

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
        //Lstick↑↓
        if(Mathf.Abs(a)>Mathf.Abs(b)){
            if (a > 0f){
                discreteActionsOut[0]=0;
            }
            else if (a < 0f){
                discreteActionsOut[0]=1;
            }
        }
        else if(Mathf.Abs(a)<Mathf.Abs(b)){
            //Lstick→←
            if (b > 0f){
                 discreteActionsOut[0]=4;
            }
            else if (b < 0f){
                discreteActionsOut[0]=3;
            }
            else{
                discreteActionsOut[0]=2;
            }
        }
        else{
            discreteActionsOut[0]=2;
        }
        if(Input.GetKey(KeyCode.Q)||Input.GetKey(KeyCode.JoystickButton4)){//左前脚上げる
            discreteActionsOut[1]=0;
        }
        else if(Input.GetKey(KeyCode.E)||Input.GetKey(KeyCode.JoystickButton6)){//左前脚下げる
            discreteActionsOut[1]=2;
        }
        else{
            discreteActionsOut[1]=1;
        }
        if(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.JoystickButton5)){//右前脚上げる
            discreteActionsOut[2]=0;
        }
        else if(Input.GetKey(KeyCode.R)||Input.GetKey(KeyCode.JoystickButton7)){//右前脚下げる
            discreteActionsOut[2]=2;
        }
        else{
            discreteActionsOut[2]=1;
        }
        if(Input.GetKey(KeyCode.Z)||Input.GetAxis("Axis 8") > 0f){//左後ろ脚上げる
            discreteActionsOut[3]=0;
        }
        else if(Input.GetKey(KeyCode.C)||Input.GetAxis("Axis 8") < 0f){//左後ろ脚下げる
            discreteActionsOut[3]=2;
        }
        else{
            discreteActionsOut[3]=1;
        }
        if(Input.GetKey(KeyCode.X)||Input.GetKey(KeyCode.JoystickButton3)){//右後ろ脚上げる
            discreteActionsOut[4]=0;
        }
        else if(Input.GetKey(KeyCode.V)||Input.GetKey(KeyCode.JoystickButton1)){//右後ろ下げる
            discreteActionsOut[4]=2;
        }
        else{
            discreteActionsOut[4]=1;
        }

    }


    void keep_value_list(float success,float roll,float count_over){//学習時の報酬などをcsvに出力するために使用される。

        touch_list_body.Add(touch_count_now[0]); //本体の接触回数をリストに保存
        touch_list_fl.Add(touch_count_now[1]); //flipper_flの接触回数をリストに保存
        touch_list_fr.Add(touch_count_now[2]);
        touch_list_rl.Add(touch_count_now[3]);
        touch_list_rr.Add(touch_count_now[4]); 

        reward_list.Add(sum_reward);
        step_list.Add(count_step);
        curriculum_list.Add(step_height_persent);

        roll_list.Add(roll);
        count_over_list.Add(count_over);
        succes_list.Add(success);
        //1エピソードのステップをリセット
        count_step=0;
        //1エピソードの報酬をリセット
        sum_reward=0.0f;
        //接触回数をリセット
        for (int i = 0; i < touch_count_now.Length; i++)
        {
            touch_count_now[i] = 0;
            touch_count_past[i] = 0;
        }
    }

    void export_excel(){//brainへ書き込み、自律走行評価時に使用、csvfileに出力用。
        episode_step++;
        time_brain=Time.realtimeSinceStartup-time_memo_brain;
        if(time_brain-time_past_brain>0.2f||time_brain==0){
            float pitch=body_state.transform.localEulerAngles.x;
            float roll=body_state.transform.localEulerAngles.z;
            if(pitch>180.0f){
                pitch=pitch-360.0f;
            }
            if(roll>300){
                roll=roll-360.0f;
            }
            float x=body_state.transform.position.x-initial_pos_dt.x;
            float y=body_state.transform.position.y-initial_pos_dt.y;
            float z=body_state.transform.position.z-initial_pos_dt.z;
            float now_acc_y = now_acc_y_reward*2500;//stepdistance=0.02sより
            file.WriteLine(time_brain+","+pitch+","+roll+","+y+","+x+","+z+","+now_acc_y+","+flipper_FL_ml[0].currentDrive.target+","+flipper_FR_ml[0].currentDrive.target+","+flipper_RL_ml[0].currentDrive.target+","+flipper_RR_ml[0].currentDrive.target);
            file.Flush();
            time_past_brain=time_brain;
            past_y = y;
        }

    }
    void export_brain_start(){//brainへ書き込み、自律走行評価時に使用、csvfileに出力用。
        initial_pos_dt=body_state.transform.position;
        file.WriteLine("time(s)"+","+"pitch(deg)"+","+"roll(deg)"+","+"y(m)"+","+"x(m)"+","+"z(m)"+","+"y_acc"+","+"FL"+","+"FR"+","+"RL"+","+"RR"+","+"touch_body(times)"+","+"touch_FL(times)"+","+"touch_FR(times)"+","+"touch_RL(times)"+","+"touch_RR(times)"+","+file_num);
        time_memo_brain=Time.realtimeSinceStartup;
    }
    void export_brain_goal(string fault){//brainへ書き込み、自律走行評価時に使用、csvfileに出力用。
        file.WriteLine(fault+","+episode_step+","+","+","+","+","+","+","+","+","+","+touch_list_body[count_episode]+","+touch_list_fl[count_episode]+","+touch_list_fr[count_episode]+","+touch_list_rl[count_episode]+","+touch_list_rr[count_episode]+","+count_fall);           
        time_brain=0;
        time_past_brain=0;
        Debug.LogWarning(file_num);
        file_num++;
        episode_step = 0;
        count_fall=0;
        if(file_num==31){
            // Debug.LogWarning(file+"は30回行きました");
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }

    void robot_Action(ActionBuffers actionBuffers){//行動値による動作を設定、inspector上で、フリッパの同期かどうかを設定できる。
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

    void realrobot_action(ActionBuffers actionBuffers){//実機のロボットでするときに使用、値の大きさが違うことがわかる。実機用、シミュレータ用で変えられている。
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




