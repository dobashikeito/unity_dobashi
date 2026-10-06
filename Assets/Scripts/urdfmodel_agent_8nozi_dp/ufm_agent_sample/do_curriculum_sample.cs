//x→いじらない、何もなし→場合によって変える
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class do_curriculum_sample : MonoBehaviour
{
    private ufm_agent_sample[] step4=new ufm_agent_sample[3];//agentファイル内の変数値を変更するための、ファイルを参照する。
    public GameObject[] Learn=new GameObject[21];//x agentファイルがアタッチされたgameobjectをinspector内でいれる。同時学習する数だけ、
    public float[] lesson_goal=new float[21];//x 同時学習する数に合わせる。成功率が閾値を超えていたら、対応するindexの値が、1になる。超えていないと、0
    public float[] each_route_percent=new float[4];//x 使う経路の数に合わせる。度の経路の成功率が低いかを知るために作っている

    private int num_agent = 0;//x 設定した閾値の成功率を越している数を記録するためのもの、いじらない
    private bool random_step_switch=false;//x 地形の高さをランダムに変える仕様を使うかどうかを決める変数。下で場合によってtrueになる。
     [Header("年＿日付＿何回目")]
    public string file_name;//x 基本は　年_日付_何回目かを記述
    //学習内容の特徴を記述し、どういう目的とかをメモしておく。
     [Header("目的")]
    public string objective;//x inspector内でメモしたものを記憶しておくため
     [Header("変更点")]
    public string change;//x inspector内でメモしたものを記憶しておくため
     [Header("メモ")]
    public string memo;//x inspector内でメモしたものを記憶しておくため

    StreamWriter file_curriculum;//x curriculumの進行を保存するcsvファイルの変数
    StreamWriter file_learning;//x 報酬などを記録
    StreamWriter file_text;//x inspector上でメモしたものを保存するtxtファイルの変数
    [System.NonSerialized]
    public float time=0;//x ある秒ごとにcsvファイルを保存したいので、その計算用
    [System.NonSerialized]
    public float time_past=0;//x ある秒ごとにcsvファイルを保存したいので、その計算用
    private int end_switch=0;//x 100％の高さの状態をクリアしたら加算される。加算値の保存用
    private float add_step_height = 0.2f;//高さを変える量。20％ずつなら0.2f
    
    public List<int> length_list=new List<int>();//x csvファイルに報酬などを書き込むためなどに使用

    private int succes_int = 17;// カリキュラムを変更する閾値を決定する変数。17/20で85%。60%の成功率で変更したければ、12に値を変える。
    private int end_num = 21;// 学習を終えるために、繰り返される回数　20回→21に設定。
    void Start()
    {
    //csvファイル、txtファイルを作成するためのあれこれ、agentファイルをコピーする。
        //folder作成C:\Users\sato\OneDrive - NITech\学習結果\desktopデータ共有\data_keep
        Directory.CreateDirectory("C:/Users/sato/OneDrive - NITech/学習結果/学習データ/"+file_name+"_gtune");
        // Directory.CreateDirectory("C:/Users/satolab/OneDrive - NITech/学習結果/学習データ"+file_name);
        #if UNITY_EDITOR
        AssetDatabase.CopyAsset("Assets/Scripts/urdfmodel_agent_8nozi_dp/ufm_agent_sample/ufm_agent_sample.cs","C:/Users/sato/OneDrive - NITech/学習結果/学習データ/"+file_name+"_gtune/ufm_agent_sample.cs");
        #endif
        file_curriculum=new StreamWriter("C:/Users/sato/OneDrive - NITech/学習結果/学習データ/"+file_name+"_gtune/"+file_name+"_curriculum.csv",false);
        file_curriculum.WriteLine("time(sec)"+","+"step"+","+"step_height(%)");
        file_text=new StreamWriter("C:/Users/sato/OneDrive - NITech/学習結果/学習データ/"+file_name+"_gtune/"+file_name+".txt",false);
        file_text.WriteLine("目的：");
        file_text.WriteLine(objective);
        file_text.Flush();
        file_text.WriteLine("変更点：");
        file_text.WriteLine(change);
        file_text.Flush();
        file_text.WriteLine("メモ");
        file_text.WriteLine(memo);
        file_text.Flush();
        file_text.Close();
        file_learning=new StreamWriter("C:/Users/sato/OneDrive - NITech/学習結果/学習データ/"+file_name+"_gtune/"+file_name+"_learning.csv",false);
        file_learning.WriteLine("episode"+","+"mean_reward"+","+"step/episode"+","+"touch(times)"+","+"roll"+","+"count_over"+","+"success"+","+"count"+"step_height(%)");
    //ここまで
    //agentファイルを編集可能にするため、このスクリプトで参照してくる。
        for(int i=0;i<step4.Length;i++){
            step4[i]=Learn[i].GetComponent<ufm_agent_sample>();
        }
    // ここまで

    }

    // Update is called once per frame
    void Update()
    {
        num_agent=0;//成功率が閾値を超えたか判断するための変数をリセット。
        //each routeの成功率を計算するのに使用
        float[] er_bunnsi = new float[4];
        float[] er_bunbo = new float[4];
        for(int i=0;i<step4.Length;i++){
            int gp_sum = 0;
            // goal_percentのリストで、ゴールか失敗かが保存されているので、それを足し合わせることで、成功数を計算
            for(int k=0;k<step4[i].goal_percent.Length;k++){
                gp_sum += step4[i].goal_percent[k];
                lesson_goal[i]=0;
            }
            //経路ごとの成功率の計算
            for(int j=0;j<step4[i].each_route_check.Length;j++){
                er_bunnsi[j] += step4[i].each_route_check[j];
                er_bunbo[j] += step4[i].each_route_try[j];
                lesson_goal[i]=0;
            }
            if(gp_sum>=succes_int){//20回に対する成功率、17/20で85%。60%の成功率で変更したければ、12に値を変える。
                num_agent++;
                lesson_goal[i]=1;
            }
        }
        //経路ごとの成功率の計算値を全部で平均化する。
        for(int i=0;i<each_route_percent.Length;i++){
            each_route_percent[i]=er_bunnsi[i]/er_bunbo[i];
        }
        // ランダムで高さを変更しないとき
        if(!random_step_switch){
            if(num_agent>=step4.Length){//同時学習の数。
                // agentファイル内の成功率を図るgoal_percentを初期化、高さを変更しているstep_height_persentを変更して、高さを変更する。
                for(int i=0;i<step4.Length;i++){
                    if(step4[i].step_height_persent>0.4f){
                        add_step_height = 0.1f;
                    }
                    step4[i].step_height_persent+=add_step_height;
                    step4[i].count_goal_continuous=0;
                    for(int k=0;k<step4[i].goal_percent.Length;k++){
                        step4[i].goal_percent[k]=0;
                    }
                    //カリキュラム次で、each route の値をリセット
                    for(int j=0;j<step4[i].each_route_check.Length;j++){
                        step4[i].each_route_check[j]=1;
                        step4[i].each_route_try[j]=1;
                    }
                }
                Debug.Log("現在の高さは "+step4[0].step_height_persent+" です。現在のステップ数は、"+step4[0].count_allstep);
            }
            if(step4[0].step_height_persent>1.05f){//最終的な段差の割合。
                for(int i=0;i<step4.Length;i++){
                    step4[i].step_height_persent=1.0f;
                    step4[i].count_goal_continuous=0;
                }
                end_switch+=1;//最終的な段差の成功率が閾値超えたときを何回繰り返すかを判断するために使用
                // random_step_switch=true;//ランダムな高さ変更をしたいなら、コメントイン
            }
        }
        // ランダムで高さを変更しないとき
        if(random_step_switch){
            if(num_agent>=step4.Length){
                // int random_step_height=Random.Range(0,13);
                for(int i=0;i<step4.Length;i++){
                    // step4[i].step_height_persent=random_step_height/10.0f;
                    step4[i].step_height_persent=Random.Range(0,11)/10.0f;//ランダム値を作成。
                    step4[i].count_goal_continuous=0;
                    for(int k=0;k<step4[i].goal_percent.Length;k++){
                        step4[i].goal_percent[k]=0;
                    }
                }
                Debug.Log("現在の高さは "+step4[0].step_height_persent+" です。現在のステップ数は、"+step4[0].count_allstep);
            }
        }

        if(end_switch==end_num){//最終的な段差の割合を繰り返す回数。unityの再生を強制終了させる。
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        export_excel();
    }
    void export_excel(){//カリキュラムの進行度をcsvファイルに書き込む。
        time=Time.realtimeSinceStartup;
        if(time-time_past>1.0f){//1秒ごとに記入、0.2秒ごとなら、1.0f→0.2fに変更。
            file_curriculum.WriteLine(time+","+step4[0].count_allstep+","+step4[0].step_height_persent);//時間、ステップ数、ステップの高さ
            file_curriculum.Flush();
            time_past=time;
        }
    }
    void OnApplicationQuit(){//unityの再生を強制終了させるときに、発動する関数。報酬の値などをcsvファイルに出力させる。
        //リストの長さを保存、index out of lengthを防ぐため
        int max_length=0;
        // Debug.Log(step4[0].reward_list.Count);
        for(int i=0;i<step4.Length;i++){
            length_list.Add(step4[i].reward_list.Count);
            if(length_list[i]>=max_length){
                max_length=length_list[i];
            }
        }
        
        //値を計算しそれぞれリストに保存
        for(int k=0;k<max_length;k++){
            float count=0.0f;
            float reward=0.0f;
            float step=0.0f;
            float touch=0.0f;
            float roll=0.0f;
            float count_over=0.0f;
            float succes=0.0f;
            for(int i=0;i<step4.Length;i++){
                if(k<length_list[i]){
                    reward+=step4[i].reward_list[k];
                    // Debug.Log(reward);
                    step+=step4[i].step_list[k];
                    touch+=step4[i].touch_list[k];
                    roll+=step4[i].roll_list[k];
                    count_over+=step4[i].count_over_list[k];
                    succes+=step4[i].succes_list[k];
                    count+=1.0f;
                }
            }
            //平均化
            int episode=k+1;
            reward=reward/count;
            step=step/count;
            touch=touch/count;

            file_learning.WriteLine(episode+","+reward+","+step+","+touch+","+roll+","+count_over+","+succes+","+count+","+step4[0].curriculum_list[k]);
            file_learning.Flush();
        }
        
    }
}
