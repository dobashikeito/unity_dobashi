//[重心を変更、重心をシーン上に表示できるスクリプト（赤い球）]
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Change_Center_of_Mass : MonoBehaviour
{
    public Vector3 center = new Vector3(0, 0.15f, 0f);//[重心をここに設定]
    private Rigidbody rb;//inspector上に表示させなくて良いのでprivate


    float abs(float x) { return (x >= 0) ? x : -x; }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
    }   

    // Update is called once per frame
    void Update()
    {
        //Debug.DrawLine(transform.position, transform.position + transform.rotation * center_of_mass);
        //if (transform.position.z < 1.5f)    center_of_mass.z = -1 * abs(center_of_mass.z);
        //else if(transform.position.z >= 1.5f)center_of_mass.z = 1 * abs(center_of_mass.z);
        rb.centerOfMass = center;//重心を設定
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;//[Gizmosを使うと、シーンビューにアイコンやテクスチャ、メッシュなどを表示できる]
        Gizmos.DrawSphere(transform.position + transform.rotation * center, 0.05f);//重心の位置に赤い球を表示できるようにしている
    }
}
