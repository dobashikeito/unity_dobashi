using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class try_excel : MonoBehaviour
{
    StreamWriter file;
    List<float> ff=new List<float>();
    void Start() {
        file=new StreamWriter("Assets/ff.csv",false);
        for(int i=0;i<10;i++){
            ff.Add(i*1.0f);
        }
    }
    void OnApplicationQuit() {
        float r=0.0f;
        float count=0.0f;
        for(int i=0;i<10;i++){
            r+=ff[i];
            count+=1.0f;
        }
        r=r/count;
        Debug.Log(r);
        file.WriteLine(r);
        file.Flush();
    }
}