using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class map_button : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Fields;
    List<GameObject> Field_list=new List<GameObject>();
    public GameObject Toggle_sample;
    List<Toggle> toggle_list=new List<Toggle>();
    void Start()
    {
        //Gameobjectのリストを作成、フィールドにおける
        for(int i=0;i<Fields.transform.childCount;i++){
            Field_list.Add(Fields.transform.GetChild(i).gameObject);
        }
        //ボタンを作成
        for(int i=0;i<Fields.transform.childCount;i++){
            GameObject go_Toggle = Instantiate(Toggle_sample);
            Toggle Toggle = go_Toggle.GetComponent<Toggle>();
            toggle_list.Add(Toggle);
            GameObject go_text_Toggle = go_Toggle.transform.GetChild(0).gameObject;
            Text text_Toggle = go_text_Toggle.GetComponent<Text>();
            text_Toggle.text = Field_list[i].transform.name;
            go_Toggle.transform.parent = this.transform;
            go_Toggle.transform.name = i + "Toggle";
            go_Toggle.transform.localPosition = Toggle_sample.transform.localPosition + new Vector3(0.0f,-20.0f*i,0.0f);
        }
         
    }

    // Update is called once per frame
    void Update()
    {
        List<int> toggle_check_list=new List<int>();
        for(int i=0;i<toggle_list.Count;i++){//常にcheckされているかを監視
            if(toggle_list[i].isOn){
                Field_list[i].SetActive(true);
            }
            else{
                toggle_check_list.Add(0);
                Field_list[i].SetActive(false);
            }
        }
        
    }
    public void Onclick_field_Toggle(){
        //常に監視
        
    }
}
