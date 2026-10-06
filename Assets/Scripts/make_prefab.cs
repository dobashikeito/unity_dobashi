using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class make_prefab : MonoBehaviour
{
    // プレハブ格納用
    public GameObject CubePrefab;
    private GameObject rr;
 
    // Start is called before the first frame update
    void Start()
    {
         Vector3 pos = new Vector3(0.0f, 10.0f, 0.0f);
            // プレハブを指定位置に生成
            rr = Instantiate(CubePrefab, pos, Quaternion.identity);
            rr.transform.parent = this.transform;
        
    }
 
    // Update is called once per frame
    void Update()
    {
        // 一定時間ごとにプレハブを生成
        // if(Time.frameCount % 60 == 0)
        // {
        //     // 生成位置
        //     Vector3 pos = new Vector3(0.0f, 10.0f, 0.0f);
        //     // プレハブを指定位置に生成
        //     rr = Instantiate(CubePrefab, pos, Quaternion.identity);
        //     rr.transform.parent = this.transform;
        // }
        // Destroy(rr,1.0f);
    }
}
