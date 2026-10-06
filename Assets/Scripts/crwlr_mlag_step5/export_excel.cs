using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class export_excel : MonoBehaviour{
    public string file_name;
    StreamWriter file;

    void Start(){
        file = new StreamWriter("Assets/"+file_name+".csv",false);

    }
    void Update(){
        if(Input.GetKeyDown(KeyCode.Q)){
            Debug.Log("Qを押しました");
            file.WriteLine("Q"+","+"U");
            file.Flush();
        }
        if(Input.GetKeyDown(KeyCode.W)){
            Debug.Log("Wを押しました");
            file.WriteLine("W");
            file.Flush();
        }
        if(Input.GetKeyDown(KeyCode.E)){
            Debug.Log("Eを押しました");
            file.WriteLine("E");
            file.Flush();
        }
        if(Input.GetKeyDown(KeyCode.R)){
            Debug.Log("Rを押しました");
            file.WriteLine("R");
            file.Flush();
        }
    }
}