// 実世界、シミュレータのデータ取得用

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class get_data : MonoBehaviour
{
    public string file = "1";
    public string file_rs = "1";
    StreamWriter csv;
    StreamWriter csv_rs;
    //CSVに出力する感覚
    private float time_memo=0;
    private float time_start;
    //値のとり方0→real,1→brain,2→両方
    public int get_value_form = 0;

    // Start is called before the first frame update
    void Start()
    {
        time_start = Time.realtimeSinceStartup;
        csv=new StreamWriter("Assets/csv_file/input_check/"+file+".csv",false);
        csv_rs=new StreamWriter("Assets/csv_file/input_check/"+file_rs+"_rs.csv",false);
        switch(get_value_form){
            case 0://real
                csv.WriteLine("time(s)"+","+"rpitch(deg)"+","+"ryaw(deg)"+","+"rroll(deg)"+","+"rLF"+","+"rLR"+","+"rRF"+","+"rRR"+","+"rx"+","+"ry"+","+"rz");
                csv_rs.WriteLine("r0"+"r1"+","+"r2"+","+"r3"+","+"r4"+","+"r5"+","+"r6"+","+"r7"+","+"r8"+","+"r9"+","+"r10"+","+"r11"+","+"r12"+","+"r13"+","+"r14");
                break;
            case 1://brain
                csv.WriteLine("time(s)"+","+"spitch(deg)"+","+"syaw(deg)"+","+"sroll(deg)"+","+"sLF"+","+"sLR"+","+"sRF"+","+"sRR"+","+"sx"+","+"sy"+","+"sz");
                csv_rs.WriteLine("s0"+"s1"+","+"s2"+","+"s3"+","+"s4"+","+"s5"+","+"s6"+","+"s7"+","+"s8"+","+"s9"+","+"s10"+","+"s11"+","+"s12"+","+"s13"+","+"s14");
                break;
            case 2://both
                csv.WriteLine("time(s)"+","+"rpitch(deg)"+","+"spitch(deg)"+","+"ryaw(deg)"+","+"syaw(deg)"+","+"rroll(deg)"+","+"sroll(deg)"+","+"rLF"+","+"sLF"+","+"rLR"+","+"sLR"+","+"rRF"+","+"sRF"+","+"rRR"+","+"sRR"+","+"rx"+","+"sx"+","+"ry"+","+"sy"+","+"rz"+","+"sz");
                csv_rs.WriteLine("r0"+","+"s0"+","+"r1"+","+"s1"+","+"r2"+","+"s2"+","+"r3"+","+"s3"+","+"r4"+","+"s4"+","+"r5"+","+"s5"+","+"r6"+","+"s6"+","+"r7"+","+"s7"+","+"r8"+","+"s8"+","+"r9"+","+"s9"+","+"r10"+","+"s10"+","+"r11"+","+"s11"+","+"r12"+","+"s12"+","+"r13"+","+"s13"+","+"r14"+","+"s14");
                break;
        }
        csv.WriteLine("time(s)"+","+"rpitch(deg)"+","+"spitch(deg)"+","+"ryaw(deg)"+","+"syaw(deg)"+","+"rroll(deg)"+","+"sroll(deg)"+","+"rLF"+","+"sLF"+","+"rLR"+","+"sLR"+","+"rRF"+","+"sRF"+","+"rRR"+","+"sRR");
        csv_rs.WriteLine("r0"+","+"s0"+","+"r1"+","+"s1"+","+"r2"+","+"s2"+","+"r3"+","+"s3"+","+"r4"+","+"s4"+","+"r5"+","+"s5"+","+"r6"+","+"s6"+","+"r7"+","+"s7"+","+"r8"+","+"s8"+","+"r9"+","+"s9"+","+"r10"+","+"s10"+","+"r11"+","+"s11"+","+"r12"+","+"s12"+","+"r13"+","+"s13"+","+"r14"+","+"s14");
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.realtimeSinceStartup-time_memo>0.2f){
            float time = Time.realtimeSinceStartup - time_start;
            
        }
    }

    // void export(time){
    //     switch(get_value_form){
    //         case 0:
                
    //             break;
    //         case 1:
    //             break;
    //         case 2:
    //             break;
    //     }
    // }
}
