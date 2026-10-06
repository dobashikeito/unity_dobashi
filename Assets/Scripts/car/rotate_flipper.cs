using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
// 入力した方向キーの向きに回転するオブジェクトのスクリプト
public class rotate_flipper : MonoBehaviour
{
 
    Rigidbody rb;
    Vector3 torque;
 
 
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // 重力を無効にする
    }
 
 
    void Update()
    {
        // 入力がなければトルクを0にする
      torque = Vector3.zero;
 
      if (Input.GetKey("right")) 
        {
          torque = Vector3.back;
        }
 
        if (Input.GetKey(KeyCode.LeftArrow)) 
        {
          torque = new Vector3(1f,0,0);
        }
 
        if (Input.GetKey("up")) 
        {
          torque = Vector3.right;
        }
 
        if (Input.GetKey("down")) 
        {
          torque = Vector3.left;
        }
 
        // 現在の角速度
        // print(rb.angularVelocity); 
 
        // トルクを加える
        // rb.AddTorque(torque, ForceMode.Acceleration);
 
        // 加える位置を帰る場合はこちら
        Vector3 vector = new Vector3 (0,0,-0.5f);
        rb.AddForceAtPosition(torque,transform.position,ForceMode.Force);
    }
}