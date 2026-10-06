using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class dobrain_ufm_leg : MonoBehaviour
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
    public GameObject lf;
    public GameObject lr;
    public GameObject rf;
    public GameObject rr;
    private imax_flipper_control_fl_ML imax_flipper_control_fl_ML;
    private imax_flipper_control_rl_ML imax_flipper_control_rl_ML;
    private imax_flipper_control_fr_ML imax_flipper_control_fr_ML;
    private imax_flipper_control_rr_ML imax_flipper_control_rr_ML;
    
    private shock_detect_new shock;
    void Start()
    {
        initial_pos=this.transform.position;
        shock=this.GetComponent<shock_detect_new>();
        imax_flipper_control_fl_ML=lf.GetComponent<imax_flipper_control_fl_ML>();
        imax_flipper_control_rl_ML=lr.GetComponent<imax_flipper_control_rl_ML>();
        imax_flipper_control_fr_ML=rf.GetComponent<imax_flipper_control_fr_ML>();
        imax_flipper_control_rr_ML=rr.GetComponent<imax_flipper_control_rr_ML>();
        make_file();
    }

    // Update is called once per frame
    void Update()
    {
        if(sg_switch){
            export_excel();
        }
    }
    void make_file(){
        file=new StreamWriter("Assets/csv_file/brain/"+file_name+"_brain.csv",false);
        
    }
    void export_excel(){
        
        
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
            file.WriteLine(time+","+pitch+","+roll+","+y+","+x+","+z+","+shock.now_acc.y+","+","+imax_flipper_control_fl_ML.currentDrive.target+","+imax_flipper_control_rl_ML.currentDrive.target+","+imax_flipper_control_fr_ML.currentDrive.target+","+imax_flipper_control_rr_ML.currentDrive.target);
            file.Flush();
            time_past=time;
        }

    }
    void OnTriggerEnter(Collider other) {
        if(other.name=="start"){
            sg_switch=true;
            start.SetActive(false);
            goal.SetActive(false);
            file.WriteLine("time(s)"+","+"pitch(deg)"+","+"roll(deg)"+","+"y(m)"+","+"x(m)"+","+"z(m)"+","+"y_acc"+","+file_num+","+"LF"+","+"LR"+","+"RF"+","+"RR");
            time_memo=Time.realtimeSinceStartup;
        }
        if(other.name=="goal"){
            file.WriteLine("");
            sg_switch=false;
            goal.SetActive(false);
            start.SetActive(true);
            time=0;
            time_past=0;
            file_num++;
            if(file_num==101){
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
            }
        }
        if(other.name=="check10"){
            goal.SetActive(true);
        }
    }
}
