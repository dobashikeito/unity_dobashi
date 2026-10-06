using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class do_curriculum_8nozi : MonoBehaviour
{
    private nmodel4_cp_8nozi[] step4=new nmodel4_cp_8nozi[20];
    public GameObject[] Learn=new GameObject[20];
    public float[] lesson_goal=new float[20];
    // private GameObject[] Learn_child=new GameObject[16];

    private int num_agent = 0;
    private bool random_step_switch=false;
     [Header("年＿日付＿何回目")]
    public string file_name;//基本は　年_日付_何回目かを記述
    //学習内容の特徴を記述し、どういう目的とかをメモしておく。
     [Header("目的")]
    public string objective;
     [Header("変更点")]
    public string change;
     [Header("メモ")]
    public string memo;

    StreamWriter file_curriculum;
    StreamWriter file_learning;
    StreamWriter file_text;
    [System.NonSerialized]
    public float time=0;
    [System.NonSerialized]
    public float time_past=0;
    private int end_switch=0;

    //値を保存に必要
    public List<int> length_list=new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        // for(int i=0;i<Learn.Length;i++){
        //     Learn[i]=transform.GetChild(i).gameObject;
        // }
        // for(int i=0;i<Learn.Length;i++){
        //     Learn_child[i]=Learn[i].GetComponent<Transform>().transform.GetChild(0).gameObject;
        // }
        //folder作成
        Directory.CreateDirectory("C:/Users/学校用/OneDrive - NITech/学習結果/desktopデータ共有/data_keep/"+file_name);
        // Directory.CreateDirectory("C:/Users/学校用/OneDrive - NITech/学習結果/desktopデータ共有/data_keep"+file_name);
        #if UNITY_EDITOR
        AssetDatabase.CopyAsset("Assets/Scripts/crwlr_mlag_step4/nmodel4_cp_8nozi.cs","C:/Users/学校用/OneDrive - NITech/学習結果/desktopデータ共有/data_keep/"+file_name+"/nmodel4_cp_8nozi.cs");
        #endif
        file_curriculum=new StreamWriter("C:/Users/学校用/OneDrive - NITech/学習結果/desktopデータ共有/data_keep/"+file_name+"/"+file_name+"_curriculum.csv",false);
        file_curriculum.WriteLine("time(sec)"+","+"step"+","+"step_height(%)");
        file_text=new StreamWriter("C:/Users/学校用/OneDrive - NITech/学習結果/desktopデータ共有/data_keep/"+file_name+"/"+file_name+".txt",false);
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
        file_learning=new StreamWriter("C:/Users/学校用/OneDrive - NITech/学習結果/desktopデータ共有/data_keep/"+file_name+"/"+file_name+"_learning.csv",false);
        file_learning.WriteLine("episode"+","+"mean_reward"+","+"step/episode"+","+"touch(times)"+","+"roll"+","+"count_over"+","+"success"+","+"count"+"step_height(%)");
        for(int i=0;i<step4.Length;i++){
            step4[i]=Learn[i].GetComponent<nmodel4_cp_8nozi>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        num_agent=0;
        for(int i=0;i<step4.Length;i++){
            int gp_sum = 0;
            for(int k=0;k<step4[i].goal_percent.Length;k++){
                gp_sum += step4[i].goal_percent[k];
                lesson_goal[i]=0;
            }
            if(gp_sum>=19){
                num_agent++;
                lesson_goal[i]=1;
            }
        }
        if(!random_step_switch){
            if(num_agent>=20){
                for(int i=0;i<step4.Length;i++){
                    step4[i].step_height_persent+=0.1f;
                    step4[i].count_goal_continuous=0;
                    for(int k=0;k<step4[i].goal_percent.Length;k++){
                        step4[i].goal_percent[k]=0;
                    }
                }
                Debug.Log("現在の高さは "+step4[0].step_height_persent+" です。現在のステップ数は、"+step4[0].count_allstep);
            }
            if(step4[0].step_height_persent>0.55f){
                for(int i=0;i<step4.Length;i++){
                    step4[i].step_height_persent=1.0f;
                    step4[i].count_goal_continuous=0;
                }
                end_switch+=1;
            }
        }
        // if(random_step_switch){
        //     if(num_agent>=20){
        //         int random_step_height=Random.Range(0,11);
        //         for(int i=0;i<step4.Length;i++){
        //             step4[i].step_height_persent=random_step_height/10.0f;
        //             step4[i].count_goal_continuous=0;
        //             for(int k=0;k<step4[i].goal_percent.Length;k++){
        //                 step4[i].goal_percent[k]=0;
        //             }
        //         }
        //         Debug.Log("現在の高さは "+step4[0].step_height_persent+" です。現在のステップ数は、"+step4[0].count_allstep);
        //     }
        // }
        if(end_switch==41){
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        export_excel();
    }
    void export_excel(){
        time=Time.realtimeSinceStartup;
        if(time-time_past>0.2f){
            file_curriculum.WriteLine(time+","+step4[0].count_allstep+","+step4[0].step_height_persent);
            file_curriculum.Flush();
            time_past=time;
        }
    }
    void OnApplicationQuit(){
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
