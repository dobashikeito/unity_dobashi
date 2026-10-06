//位置、速度、加速度、角速度、角加速度を計算
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Valve.VR.InteractionSystem;//https://gelare.amebaownd.com/posts/5816488/
//[VRを扱う]

public class curriculate_acc : MonoBehaviour
{
    //[System.NonSerialized]
    //public float shock = 0;

    // private VelocityEstimator VE;//[VRを扱う]
    [HideInInspector] public Vector3 Acc = Vector3.zero;
    //[HideInInspector] public Vector3 Acc_fixed = Vector3.zero;
    [HideInInspector] public Vector3 jerk = Vector3.zero;
    //private float Acc_mag = 0;
    private Rigidbody rb;
    [HideInInspector] public Vector3 past_vel = Vector3.zero;
    [HideInInspector] public Vector3 now_vel = Vector3.zero;
    [HideInInspector] public Vector3 past_acc = Vector3.zero;
    [HideInInspector] public Vector3 now_acc = Vector3.zero;
    [HideInInspector] public Vector3 past_angVel = Vector3.zero;
    [HideInInspector] public Vector3 now_angVel = Vector3.zero;
    [HideInInspector] public Vector3 past_angAcc = Vector3.zero;
    [HideInInspector] public Vector3 now_angAcc = Vector3.zero;

    private float a_past_time = 0, a_now_time = 0;

    private float maxAcc_y = 0;
    private float minAcc_y = 0;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    //void Update()     //2020_0518_ここをUpdate()にするとDebug.Logなどの重い処理が毎step入ると、Accとかjerkが通常とは異なる値が計算結果になるので
    void FixedUpdate()  //FixedUpdateにする(学習中に重い処理が来たときにUpdate()だと同じ事が起こるかもしれん)
    {
        
        now_vel = rb.linearVelocity;//[剛体の速度ベクトル,位置の変化率を表示]
        now_angVel = rb.angularVelocity;//[角速度ベクトルをrad/sで測定]
        
        now_acc = (now_vel - past_vel) / Time.deltaTime;//[現在と過去の速度をフレーム時間で割ることで、加速度を得ている]
        now_angAcc = (now_angVel - past_angVel) / Time.deltaTime;//[現在と過去の角速度をフレーム時間で割ることで、角加速度を得ている]

        past_acc = now_acc;//[過去の加速度を更新]
        past_vel = now_vel;//[過去の速度を更新]
        past_angVel = now_angVel;//[過去の角速度を更新]
        past_angAcc = now_angAcc;//[過去の角加速度を更新]

    }

}
