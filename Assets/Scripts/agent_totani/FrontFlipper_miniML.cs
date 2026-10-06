//「////」は、もとから使ってないやつ
//[前フリッパーの角度をhingejointを用いて、変更するスクリプト]
//(1)actionFrontを受け取り、その値によってaddDeg*Timeを設定
//(2)Targetdegに値をいれる、
//(3)spring.targetPositionにTargetDegの値をいれ、hingeを動かす
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using RosSharp.RosBridgeClient;//[Rossharpはまだ使わない]

public class FrontFlipper_miniML : MonoBehaviour
{


    ////public Rigidbody rb;
    ////public Rigidbody rbGo;
    ////public GameObject Go;
    public float addDeg = 22.8f;
    public float speedDeg = 200;
    public float FIXED_ANG = -1f;//この値が-1のとき，自由にフリッパをコントロールできる，-1以外の時はその角度に固定される
    [HideInInspector] public float moveThresh = 6;
    [HideInInspector] public float SA = 1;
    ////public float speedGo = 5;//RearFlipperとあわせて20 前後左右たして40
    ////public Transform PlayerTransform;
    ////public GameObject RedArrow;
    private float Mindeg = -75; //制限角度下限
    private float Maxdeg = 75;  //制限角度上限
    public float Targetdeg = 0;//目標角度
    public float actionFront;
    public float FrontAngle;
    const float Potentiometer2Deg = 360 / (2 * Mathf.PI);//Crawler本体についてる，ポテンショメータを角度[deg]に変えるパラメータ
    HingeJoint hinge;//[HingeJointの変数]
    // Float64Subscriber GetComp_Float64_Sub;//[RosSharpはまだ使わない]
    //// Use this for initialization
    void Start()//[HingeJointの角度制限の設定]
    {
        ////Rigidbody rb = GetComponent<Rigidbody>();
        ////Rigidbody rbGo = GetComponent<Rigidbody>();
        ////GameObject Go = GetComponent<GameObject>();//これやるとエラー起きる。Awake関数を使ってもダメだった。FixedUpdateのなかにnullのときの条件文を入れないかんかな？
        ////そもそも↑この行を入れんでも機能してそう。

        hinge = GetComponent<HingeJoint>();//[HingeJointのコンポーネントを取得]
        JointLimits limits = hinge.limits;//[HingeJointの角度の制限を設定するための変数]
        limits.min = Mindeg;//[今回は－75°]
        limits.max = Maxdeg;//[今回は75°]
        hinge.useLimits = true;//[(limits)制限を有効にする]
        ////JointMotor motor = hinge.motor;
        ////motor.force = 1000000;
        //// GetComp_Float64_Sub = GetComponent<Float64Subscriber>();//[RosSharpはまだ使わない]
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        
        
        ////JointSpring hingeSpring = hinge.spring;
        JointMotor motor = hinge.motor;//[HingeJointをモーター化している]
        JointSpring spring = hinge.spring;//[HingeJointに弾性力を追加する]
        /*
        ////速度制御
        if (Input.GetKey("m"))
        {
            motor.targetVelocity = -speedDeg;
        }

        else if (Input.GetKey("u"))
        {
            motor.targetVelocity = speedDeg;
        }

        else motor.targetVelocity = 0;
        */
        ////Debug.Log(actionFront);

        //位置制御みたいなやつ
        if (actionFront == 1)
        {
            ////Targetdeg += addDeg * Time.deltaTime;
            ////Targetdeg = hinge.angle + addDeg * Time.deltaTime;//2020_0527_これでフリッパーが動かない時に目標角だけ変化し続けるのを防げるかな?
            if (Mathf.Abs(Targetdeg - hinge.angle) < SA * addDeg) Targetdeg += addDeg * Time.deltaTime;//[hinge.angleは現在のjointの角度を示す]
            //[Time.deltaTimeは最後と現在のフレームまでの間隔秒]
            //else Debug.Log(" <color=red>NO MOVING</color> ");
        }

        if (actionFront == -1)
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
        // if (GetComp_Float64_Sub.is_Subscribe == false) FrontAngle = hinge.angle + Random.Range(-0.02f/2 * 360 / (2 * Mathf.PI), 0.02f/2 * 360 / (2 * Mathf.PI));//rosから送られてこないならunityの角度データを取得 //ノイズ0.015355[rad]足す
        // else FrontAngle = GetComp_Float64_Sub.data * Potentiometer2Deg;

        if (moveThresh <= FrontAngle - Targetdeg)
        {
            //motor.maxMotorTorque = 50;
            motor.targetVelocity = -speedDeg;//[目標速度に到達するまで、力が与えられる、speedDegが目標速度？]
        }
        else if (FrontAngle - Targetdeg <= -moveThresh)
        {
            motor.targetVelocity = speedDeg;
        }
        else motor.targetVelocity = 0;//[速度を０にする]
        spring.targetPosition = Targetdeg;//[目標位置まで力が与えられる]

        if (hinge.useMotor) { hinge.motor = motor; }  ////Debug.Log(" useMotor"); }  //おそらくこの行でパラメータの代入をする。
        if(hinge.useSpring) hinge.spring = spring;
    }

    /*//
    void OnTriggerEnter(Collider other)//OnColliderはColliderのIsTriggerにチェックが入ってちゃダメっぽいからOnTriggerを使う
    {
        if (other.gameObject.tag == "ball3")//ball2というタグを持つものに当たったら報酬もらえる
        {
            //Debug.Log("point3");
            //SetReward(1.5f);
            //Destroy(other.gameObject);//触れたら消えるようにするなら、リセットしたときにふかっつさせいかん。
            //point = 1;//最初から１にしとけばよさそう

            miniCrawler3D.GetComponent<miniCrawler3DAgent>().point = 3;//point地点に来たことを知らせる。
        }


        else if (other.gameObject.tag == "ball2")//ball2というタグを持つものに当たったら報酬もらえる
        {
            //Debug.Log("point2");
            //SetReward(1.5f);
            //Destroy(other.gameObject);//触れたら消えるようにするなら、リセットしたときにふかっつさせいかん。
            //point = 1;//最初から１にしとけばよさそう

            miniCrawler3D.GetComponent<miniCrawler3DAgent>().point = 2;//point地点に来たことを知らせる。
        }

        else if (other.gameObject.tag == "ball")//ballというタグを持つものに当たったら報酬もらえる
        {
            //Debug.Log("point");
            //SetReward(1.5f);
            //Destroy(other.gameObject);//触れたら消えるようにするなら、リセットしたときにふかっつさせいかん。
            //point = 1;//最初から１にしとけばよさそう

            miniCrawler3D.GetComponent<miniCrawler3DAgent>().point = 1;//point地点に来たことを知らせる。
        }


        else if (other.gameObject.tag == "wall")//ball2というタグを持つものに当たったら報酬もらえる
        {
            //Debug.Log("wall");
            //SetReward(1.5f);
            //Destroy(other.gameObject);//触れたら消えるようにするなら、リセットしたときにふかっつさせいかん。
            //point = 1;//最初から１にしとけばよさそう

            miniCrawler3D.GetComponent<miniCrawler3DAgent>().point = 0;//point地点に来たことを知らせる。
        }


    }
    */

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
