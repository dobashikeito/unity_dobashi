using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class do_curriculum_validation : MonoBehaviour
{
    public ufm_agent_validation[] step4=new ufm_agent_validation[21];
    public GameObject[] brains=new GameObject[21];
    private int endnum = 0;

    void Start()
    {
        endnum = step4.Length;
        for(int i=0;i<endnum;i++){
            step4[i] =brains[i].GetComponent<ufm_agent_validation>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        int check_num = 0;
        for(int i=0;i<endnum;i++){
            if(step4[i].end_field){
                check_num++;
            }
        }
        if(check_num == endnum){
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }

}