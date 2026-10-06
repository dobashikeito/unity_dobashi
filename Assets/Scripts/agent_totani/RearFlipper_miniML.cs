//「////」は、もとから使ってないやつ
//[後ろフリッパーの角度をhingejointを用いて、変更するスクリプト]
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using RosSharp.RosBridgeClient;

public class RearFlipper_miniML : MonoBehaviour
{

    ////public Rigidbody rb;
    ////public Rigidbody rbGo;
    ////public GameObject Go;
    public float addDeg = 22.8f;
    public float speedDeg = 200;
    public float FIXED_ANG = -1f;//この値が-1のとき，自由にフリッパをコントロールできる，-1以外の時はその角度に固定される
    [HideInInspector] public float moveThresh = 6;
    [HideInInspector] public float SA = 1;
    ////public float speedGo = 5;//FlontFlipperとあわせて20 前後左右たして40
    ////public Transform PlayerTransform;
    ////public GameObject RedArrow;
    private float Mindeg = -75;
    private float Maxdeg = 75;
    public float Targetdeg = 0;//目標角度
    public float actionRear;
    public float RearAngle;
    const float Potentiometer2Deg = -360/(2*Mathf.PI);//Crawler本体についてる，ポテンショメータを角度[deg]に変えるパラメータ
    private HingeJoint hinge;
    // Float64Subscriber GetComp_Float64_Sub;//[RosSharpはまだ使わない]
    // Use this for initialization
    void Start()
    {

        ////Rigidbody rbGo = GetComponent<Rigidbody>();
        ////GameObject Go = GetComponent<GameObject>();//これやるとエラー起きる。Awake関数を使ってもダメだった。FixedUpdateのなかにnullのときの条件文を入れないかんかな？
        ////そもそも↑この行を入れんでも機能してそう。

        hinge = GetComponent<HingeJoint>();//[HingeJointのコンポーネントを取得]
        JointLimits limits = hinge.limits;//[HingeJointの角度の制限を設定するための変数]
        limits.min = Mindeg;//[今回は－75°]
        limits.max = Maxdeg;//[今回は75°]
        hinge.useLimits = true;//[(limits)制限を有効にする]
        // GetComp_Float64_Sub = GetComponent<Float64Subscriber>();//[RosSharpはまだ使わない]
    }

    // Update is called once per frame
    void Update()
    {

        
        //JointSpring hingeSpring = hinge.spring;
        JointMotor motor = hinge.motor;//[HingeJointをモーター化している]
        JointSpring spring = hinge.spring;//[HingeJointに弾性力を追加する]

        /*
        ////位置制御みたいなやつ
        if (Input.GetKey("R"))
        {
            Targetdeg += addDeg;
        }

        if (Input.GetKey("V"))
        {
            Targetdeg -= addDeg;
        }
        */


        //位置制御みたいなやつ
        if (actionRear == 1)
        {
            //Targetdeg += addDeg * Time.deltaTime;
            //Targetdeg = hinge.angle + addDeg * Time.deltaTime;//2020_0527_これでフリッパーが動かない時に目標角だけ変化し続けるのを防げるかな?
            if (Mathf.Abs(Targetdeg - hinge.angle) < SA * addDeg) Targetdeg += addDeg * Time.deltaTime;//[hinge.angleは現在のjointの角度を示す]
            //[Time.deltaTimeは最後と現在のフレームまでの間隔秒]
            //else Debug.Log(" <color=red>NO MOVING</color> ");
        }

        if (actionRear == -1)
        {
            ////Targetdeg -= addDeg * Time.deltaTime;
            ////Targetdeg = hinge.angle - addDeg * Time.deltaTime;//2020_0527_まったくフリッパ動かなくなった．ので↓ 
            if ( Mathf.Abs(Targetdeg - hinge.angle) < SA * addDeg) Targetdeg -= addDeg * Time.deltaTime;
            //if文で目標角と現在角に　SA[s]分の差があるときは目標角を変化させないようにした
            ////else Debug.Log(" <color=red>NO MOVING</color> ");
        }
        
        if (Targetdeg > Maxdeg) Targetdeg = Maxdeg;//[最大を超えてしまったら]
        if (Targetdeg < Mindeg) Targetdeg = Mindeg;//[最小を超えてしまったら]
        if (FIXED_ANG != -1 && Targetdeg > FIXED_ANG) Targetdeg = FIXED_ANG;//角度を固定させる
        if (FIXED_ANG != -1 && Targetdeg < -FIXED_ANG) Targetdeg = -FIXED_ANG;//角度を固定させる
        ////Debug.Log(Targetdeg);
        ////Debug.Log(transform.eulerAngles.z);
        ////Debug.Log(hinge.jointAngle);
        ////Debug.Log(hinge.angle);
        //[↓　RosSharpはまだ使わない]
        // if (GetComp_Float64_Sub.is_Subscribe == false) RearAngle = hinge.angle + Random.Range(-0.02f / 2 * 360 / (2 * Mathf.PI), 0.02f / 2 * 360 / (2 * Mathf.PI));//rosから送られてこないならunityの角度データを取得//ノイズ0.015355[rad]足す
        // else RearAngle = GetComp_Float64_Sub.data * Potentiometer2Deg;

        if (moveThresh <= RearAngle - Targetdeg)
        {
            ////motor.maxMotorTorque = 50;
            motor.targetVelocity = -speedDeg;//[目標速度に到達するまで、力が与えられる、speedDegが目標速度？]
        }
        else if (RearAngle - Targetdeg <= -moveThresh)
        {
            motor.targetVelocity = speedDeg;//[目標速度に到達するまで、力が与えられる、speedDegが目標速度？]
        }
        else motor.targetVelocity = 0;

        spring.targetPosition = Targetdeg;//[目標位置まで力が与えられる]


        if(hinge.useMotor) hinge.motor = motor;  //おそらくこの行でパラメータの代入をする。
        if(hinge.useSpring) hinge.spring = spring;
    }

    /*//
    void OnCollisionStay(Collision other)
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        Vector3 kaiten = 5 * other.contacts[0].normal; //衝突した平面の法線ベクトルを代入
        if (Input.GetKey("j"))
        {

            float kaiten0 = kaiten[0];
            kaiten[0] = kaiten[1];          //z軸を-90度回転。
            kaiten[1] = -kaiten0;             //z軸を-90度回転。
            rb.velocity = speedGo * kaiten;
        }

        if (Input.GetKey("f"))
        {

            float kaiten0 = kaiten[0];
            kaiten[0] = -kaiten[1];          //z軸を90度回転。
            kaiten[1] = kaiten0;             //z軸を90度回転。
            rb.velocity = speedGo * kaiten;
        }

    }
    */
}


