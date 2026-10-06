using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class dobrain_change_step : MonoBehaviour
{
    // Start is called before the first frame update
    private int file_num=1;
    public string file_name;
    StreamWriter file;
    private float time=0;
    private float time_memo=0.0f;
    private float time_past=0;
    private Vector3 initial_pos;
    private bool sg_switch=false;
    public GameObject start;
    public GameObject goal;
    public GameObject agent;
    public GameObject flipperLF;
    public GameObject flipperLR;
    public GameObject flipperRF;
    public GameObject flipperRR;

    
    public touch_wall[] touch_wall=new touch_wall[5];
    //step変更に使う
    public int step_height=0;
    private int count_step=0;
    public GameObject astm;
    private nmodel4_cp_8nozi nc8;
    public Vector3 astm_scale;

    
    private shock_detect_new shock;
    private nmodel4_cp_8nozi astm_Goal;
    private int count_over_shock=0;
    private int count_brain=101;//その重心で100回測定
    void Start()
    {
        initial_pos=this.transform.position;
        shock=this.GetComponent<shock_detect_new>();
        astm_Goal=agent.GetComponent<nmodel4_cp_8nozi>();
        touch_wall[0]=this.GetComponent<touch_wall>();
        touch_wall[1]=flipperLF.GetComponent<touch_wall>();
        touch_wall[2]=flipperLR.GetComponent<touch_wall>();
        touch_wall[3]=flipperRF.GetComponent<touch_wall>();
        touch_wall[4]=flipperRR.GetComponent<touch_wall>();
        nc8=agent.GetComponent<nmodel4_cp_8nozi>();
        astm_scale=astm.transform.localScale;
        

        make_file();
    }

    // Update is called once per frame
    void Update()
    {
        if(sg_switch){
            export_excel();
        }
        if(astm_Goal.fault_over){//転倒とカウントオーバーと分ける。
            astm_Goal.fault_over=false;
            int count_touch=0;  
            goal.SetActive(false);
            start.SetActive(true);
            for(int i=0;i<touch_wall.Length;i++){
                count_touch+=touch_wall[i].count_touch;
                touch_wall[i].count_touch=0;
            }
            file.WriteLine(""+","+","+"count_touch"+","+count_touch+","+"over"+","+"count_shock"+","+count_over_shock);
            file.Flush();
            sg_switch=false;

            time=0;
            time_past=0;
            count_over_shock=0;
            file_num++;
            if(file_num==count_brain){//100回になったら、ファイル生成
                make_file();
            }
            
        }
        if(astm_Goal.fault_roll){//転倒とカウントオーバーと分ける。
            astm_Goal.fault_roll=false;
            int count_touch=0;  
            goal.SetActive(false);
            start.SetActive(true);
            for(int i=0;i<touch_wall.Length;i++){
                count_touch+=touch_wall[i].count_touch;
                touch_wall[i].count_touch=0;
            }
            file.WriteLine(""+","+","+"count_touch"+","+count_touch+","+"roll"+","+"count_shock"+","+count_over_shock);
            file.Flush();
            sg_switch=false;

            time=0;
            time_past=0;
            count_over_shock=0;
            file_num++;
            if(file_num==count_brain){//100回になったら、ファイル生成
                make_file();
            }
            
        }
    }
    void make_file(){//重心の位置変更、ファイル生成
        file_num=1;
        if(step_height>250){
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        // file=new StreamWriter("Assets/csv_file/change_step/"+file_name+"sh"+step_height+".csv",false);//desktopデータ共有に保存
        file=new StreamWriter("C:/Users/clz13166/OneDrive - NITech/学習結果/desktopデータ共有/desktop_csv/"+file_name+"sh"+step_height+".csv",false);//desktopデータ共有に保存
        // astm.transform.localScale=new Vector3(astm_scale.x,astm_scale.y*step_height,astm_scale.z);
        nc8.step_height_persent+=0.1f;
        step_height+=10;
        
    }
    void export_excel(){//値の出力
        
        
        time=Time.realtimeSinceStartup-time_memo;
        if(time-time_past>0.2f||time==0){
            float pitch=this.transform.localEulerAngles.x;
            float roll=this.transform.localEulerAngles.z;
            if(pitch>180.0f){
                pitch=pitch-360.0f;
            }
            if(roll>300){
                roll=roll-360.0f;
            }
            float x=this.transform.position.x-initial_pos.x;
            float y=this.transform.position.y-initial_pos.y;
            float z=this.transform.position.z-initial_pos.z;
            float acc_y=shock.now_acc.y;
            file.WriteLine(time+","+pitch+","+roll+","+y+","+x+","+z+","+acc_y);
            if(acc_y>10.0f){
                count_over_shock++;
            }
            file.Flush();
            time_past=time;
        }

    }
    void OnTriggerEnter(Collider other) {
        
        if(other.name=="start"){
            Debug.LogError(file_num+"回目がスタート");
            sg_switch=true;//export_excelのスイッチ
            start.SetActive(false);
            goal.SetActive(false);
            file.WriteLine("time(s)"+","+"pitch(deg)"+","+"roll(deg)"+","+"y(m)"+","+"x(m)"+","+"z(m)"+","+"y_acc"+","+file_num);
            file.Flush();
            time_memo=Time.realtimeSinceStartup;
        }
        if(other.name=="goal"){
            int count_touch=0;
            for(int i=0;i<touch_wall.Length;i++){
                    count_touch+=touch_wall[i].count_touch;
                    touch_wall[i].count_touch=0;
                }
            file.WriteLine(""+","+","+"count_touch"+","+count_touch+","+","+","+"count_shock"+","+count_over_shock);
            file.Flush();
            sg_switch=false;
            goal.SetActive(false);
            start.SetActive(true);
            time=0;
            time_past=0;
            count_over_shock=0;
            file_num++;
            if(file_num==count_brain){//100回になったら、ファイル生成
                make_file();
            }
        }
        
        if(other.name=="check10"){
            goal.SetActive(true);
        }
    }
}
