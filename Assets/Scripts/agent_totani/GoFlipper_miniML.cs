//前進、後進を制御するプログラム、AIの部分も入っているかも
//is_wheelでwheelcolliderを使用、実際は使ってない
//(1)Start()でRigidbody、LineRendererを取得
//(2)OnCollisionStay()でactionGoを用いて、速度を計算し、決定
//(3)LineRendererの位置を設定
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoFlipper_miniML : MonoBehaviour
{
    public List<AxleInfo> axleInfos; // 個々の車軸の情報
    public float maxMotorTorque; //ホイールに適用可能な最大トルク
    public float maxSteeringAngle; // 適用可能な最大ハンドル角度


    [Tooltip("wheel_colliderを使って動かすのかどうか")]
    public bool is_wheel = false;//wheel_colliderを使って動かすのかどうか


    //public Rigidbody rb;
    //public Rigidbody rbGo;
    //private const float speedGo = 29.5f;
    //[SerializeField] private float MAX_SPEED = 0.335f;//OneDriveの森田・佐藤研究室/研究team[佐藤研]/NITRoCrawlerの測定結果/測定結果.txtより
    //[SerializeField] private float MAX_SPEED = 0.288f;// hitroを7割の速度にした時の平均速度
    [SerializeField] private float MAX_SPEED = 0.5f;//
    [SerializeField] private float Force_Coef = 100.0f;//係数
    //[SerializeField] private float Kp = 100.0f;//係数
    //[SerializeField] private float Ki = 100.0f;//係数
    //[SerializeField] private float Kd = 100.0f;//係数
    //private Vector3[] diff = new Vector3[2];
    //private Vector3 integral = Vector3.zero;
    //public GameObject RedArrow;
    // Use this for initialization
    public float actionGo;//学習器からやってくる，並進速度の指令値
    private Rigidbody rb;
    private new LineRenderer renderer;
    [HideInInspector] public Vector3 robo_axis = Vector3.zero;//robotの中心軸(右フリッパー中心から左フリッパー中心へ向かうベクトル)
    private float time = 0;
    private bool is_ForceMode_Force = true;
    //private float cnt = 0;
    [HideInInspector]public Vector3 moveVec = Vector3.zero;
    [HideInInspector] public Vector3 tan_vec = Vector3.zero;// kaitenという変数名から tan_vecという変数名に変更 2020_05_20
    [HideInInspector] public Vector3 vel_now = Vector3.zero;
    void Start()
    {
        rb = GetComponent<Rigidbody>();//OnCollisionStay()の中のやつをこっちに移動 2020_04_22 
        rb.sleepThreshold = 0.0f;//Rigidbodyは動きが小さいと無効になるらしくて，無効状態だと衝突判定が行われないので，Thresholdを0にする(マイナスの方が確実かも)http://inaenomaki.hatenablog.com/entry/2017/01/20/215629
        //renderer = gameObject.GetComponent<LineRenderer>();
        renderer = gameObject.AddComponent<LineRenderer>();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        
        // Rigidbody rb = GetComponent<Rigidbody>();
        //Debug.Log(rb.velocity.magnitude);
        //Debug.Log(rbGo.velocity.magnitude);
        //float motor = maxMotorTorque * Input.GetAxis("Vertical");
        //float steering = maxSteeringAngle * Input.GetAxis("Horizontal");
        if (is_wheel == false) return;// AddForce使う(つまりwheel colliderを使わない)なら以下動作しない

        //以下wheel colliderを使った時のプログラム
        foreach (AxleInfo axleInfo in axleInfos)
        {
            //if (axleInfo.steering)
            //{
            //    axleInfo.Wheel.steerAngle = steering;
            //}
            if (axleInfo.motor)
            {
                float _Go = 0;
                if (actionGo > 0) _Go = 0.1f * actionGo + 0.5f;//{5, 4, 3, ..., -5} ⇒{1, 0.9, 0.8, ..., -1}
                else if (actionGo < 0) _Go = 0.1f * actionGo - 0.5f;//{5, 4, 3, ..., -5} ⇒{1, 0.9, 0.8, ..., -1}
                else _Go = 0;
                float motor = maxMotorTorque * _Go;
                axleInfo.FrontWheel.motorTorque = motor;
                axleInfo.MiddleWheel.motorTorque = motor;
                axleInfo.RearWheel.motorTorque = motor;
            }
        }//[ここまでwheel colliderを使った時のプログラム]
    }

    [System.Serializable]
    public class AxleInfo
    {
        public WheelCollider FrontWheel;
        public WheelCollider MiddleWheel;
        public WheelCollider RearWheel;
        public bool motor; //このホイールがエンジンにアタッチされているかどうか
        public bool steering; // このホイールがハンドルの角度を反映しているかどうか
    }

    void OnCollisionStay(Collision other)//[衝突して、接触中にイベント発生]
    {

        //if(rb.name== "GoFlipperRight") Debug.Log(" tan_vec: " + tan_vec + "   " + rb.name + "     moveVec: " + moveVec.ToString("F3") + " vel_now: " + vel_now.ToString("F3"));
        moveVec = Vector3.zero;//たぶん 地面とか以外の  Crawler_hontaiとかに接触してるから  地面に接触してなくても初期化される FrontFlipとRearFlipはね  GoFlipはならなかった
        vel_now = rb.linearVelocity;//[剛体の速度ベクトル、位置の変化率を示す]

        time += Time.deltaTime; if (time > 10000000) time = 0;
        //if ((int)time % 2 == 0) Debug.LogWarning("<color=white>1</color> :<color=red>" + this.gameObject.name + "</color> <color=green>" + other.gameObject.name + "</color> <color=blue>" + other.gameObject.tag + "</color> other.GetContact(0).normal: <color=red>" + other.GetContact(0).normal + "</color> other.GetContacts(other.contacts): <color>" + other.GetContacts(other.contacts) + "</color> ");

        if (is_wheel == true) return;//wheel colliderを使うなら以下動作しない
        //if (rb.name == "GoFlipperRight") Debug.Log("<color=red>!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!</color>");

        //以下AddForce (つまりwheel colliderを使わない)場合のプログラム
        
        if (other.gameObject.tag == "robot") return;//2020_04_22
        //if ((int)time % 2 == 0) Debug.Log("<color=white>1</color> :<color=red>" + this.gameObject.name + "</color> <color=green>" + other.gameObject.name + "</color> <color=blue>" + other.gameObject.tag);// + "</color> other.GetContact(0).normal: <color=red>" + other.GetContact(0).normal + "</color> other.GetContacts(other.contacts): <color>" + other.GetContacts(other.contacts) + "</color> ");


        //if(rb.name== "GoFlipperLeft") Debug.Log(" other.GetContact(0).point: " + other.GetContact(0).point);
        //2020_04_21 代わりにnormal_vec と　robo_axisの外積をとる
        //Vector3 tan_vec = other.contacts[0].normal.normalized; //衝突した平面の法線ベクトルを代入
        Vector3 normal_vec = other.GetContact(0).normal.normalized; //衝突した平面の法線ベクトルを代入//2020_04_21一行上の代わりにこっちを使う.なんか上は推奨されてないらしい
        //[↑正規化？]
        //Debug.Log(rb.name + "other.contacts[0].normal: <color>" + other.contacts[0].normal + "</color> other.GetContact(0).normal: <color>" + other.GetContact(0).normal + "</color> other.GetContacts(other.contacts): <color>" + other.GetContacts(other.contacts) + "</color> ");
        tan_vec[0] = normal_vec[0];
        tan_vec[1] = -normal_vec[2];//x軸を-90度回転。//[するため逆]
        tan_vec[2] = normal_vec[1];//x軸を-90度回転。
        //Debug.Log(" tan_vec: " + tan_vec + "   " + rb.name + " other.GetContact(0).normal: <color=red>" + other.GetContact(0).normal + "</color> other.GetContacts(other.contacts): <color>" + other.GetContacts(other.contacts) + "</color> ");

        
        
        
       
        //if (kaiten == tan_vec) Debug.Log(" kaiten == tan_vec: <color=red>" + (kaiten == tan_vec) + "</color>");
        //if (kaiten != tan_vec) Debug.LogError(" kaiten == tan_vec: <color=red>" + (kaiten == tan_vec) + "</color>");

        /*//2020_04_23やっぱり，↑の外積ではなく回転を使う 旋回が入ってないから，左右で力がずれるとまがってってしまう
        Vector3 normal_vec = other.GetContact(0).normal.normalized;//2020_04_21
        Vector3 tan_vec = Vector3.Cross(robo_axis.normalized, normal_vec);
        //Debug.Log(rb.name + " normal_vec: <color=red>" + normal_vec + "</color> robo_axis.normalized: <color>" + robo_axis.normalized + "</color> tan_vec: <color>" + tan_vec + "</color>" );
        */

        /*//Debug.Log(actionGo);
        if (actionGo == 5)      rb.velocity = 1.0f * speedGo * tan_vec * Time.deltaTime;  //前進
        else if (actionGo == 4) rb.velocity = 0.9f * speedGo * tan_vec * Time.deltaTime;
        else if (actionGo == 3) rb.velocity = 0.8f * speedGo * tan_vec * Time.deltaTime;
        else if (actionGo == 2) rb.velocity = 0.7f * speedGo * tan_vec * Time.deltaTime;
        else if (actionGo == 1) rb.velocity = 0.6f * speedGo * tan_vec * Time.deltaTime;

        else if (actionGo == -5) rb.velocity = -1.0f * speedGo * tan_vec * Time.deltaTime; //後退
        else if (actionGo == -4) rb.velocity = -0.9f * speedGo * tan_vec * Time.deltaTime;
        else if (actionGo == -3) rb.velocity = -0.8f * speedGo * tan_vec * Time.deltaTime;
        else if (actionGo == -2) rb.velocity = -0.7f * speedGo * tan_vec * Time.deltaTime;
        else if (actionGo == -1) rb.velocity = -0.6f * speedGo * tan_vec * Time.deltaTime;
        else;//rb.velocity = Vector3.zero;//Stop!!
        */
        /*
        if (actionGo == 5)      rb.AddForce(1.0f * MAX_SPEED * tan_vec , ForceMode.VelocityChange);  //前進
        else if (actionGo == 4) rb.AddForce(0.9f * MAX_SPEED * tan_vec , ForceMode.VelocityChange);
        else if (actionGo == 3) rb.AddForce(0.8f * MAX_SPEED * tan_vec , ForceMode.VelocityChange);
        else if (actionGo == 2) rb.AddForce(0.7f * MAX_SPEED * tan_vec , ForceMode.VelocityChange);
        else if (actionGo == 1) rb.AddForce(0.6f * MAX_SPEED * tan_vec , ForceMode.VelocityChange);

        else if (actionGo == -5) rb.AddForce(-1.0f * MAX_SPEED * tan_vec , ForceMode.VelocityChange); //後退
        else if (actionGo == -4) rb.AddForce(-0.9f * MAX_SPEED * tan_vec , ForceMode.VelocityChange);
        else if (actionGo == -3) rb.AddForce(-0.8f * MAX_SPEED * tan_vec , ForceMode.VelocityChange);
        else if (actionGo == -2) rb.AddForce(-0.7f * MAX_SPEED * tan_vec , ForceMode.VelocityChange);
        else if (actionGo == -1) rb.AddForce(-0.6f * MAX_SPEED * tan_vec , ForceMode.VelocityChange);
        else                     ;//rb.AddForce(0.0f * MAX_SPEED * tan_vec , ForceMode.VelocityChange);//Stop!!
        */



        float _Go = 0;
        if (actionGo > 0) _Go = 0.1f * actionGo + 0.5f;//{5, 4, 3, ..., -5} ⇒{1, 0.9, 0.8, ..., -1}
        else if (actionGo < 0) _Go = 0.1f * actionGo - 0.5f;//{5, 4, 3, ..., -5} ⇒{1, 0.9, 0.8, ..., -1}
        else _Go = 0;
        float vel_target = _Go * MAX_SPEED;
        //if (actionGo != 0 && tan_vec.x != 0) moveVec.x = vel_target * tan_vec.x - vel_now.x;//rb.velocityはtan_vecみたいにきれいな値じゃないので，
        //if (actionGo != 0 && tan_vec.y != 0) moveVec.y = vel_target * tan_vec.y - vel_now.y;//ベクトル同士そのまま引くとmoveVeも汚い値になる．そのため，
        //if (actionGo != 0 && tan_vec.z != 0) moveVec.z = vel_target * tan_vec.z - vel_now.z;//ifで分岐する    (actionGo!=0も条件に入れないと 0なのに出力が出る2020_0518)
        if (actionGo != 0) moveVec = vel_target * tan_vec - vel_now;

        //rb.AddForce(Force_Coef * moveVec, ForceMode.Force);//rb.AddForce(Force_Coef * moveVec);
        rb.AddForceAtPosition(Force_Coef * moveVec, other.GetContact(0).point, ForceMode.Force); //[トルクと力が与えられる]
        //Debug.Log(" other.GetContact(0).point: " + other.GetContact(0).point);
        //rb.AddForceAtPosition(Force_Coef * moveVec, Force_Coef * moveVec, ForceMode.VelocityChange);

        //if (Input.GetKeyUp("f")) { is_ForceMode_Force = true; Debug.Log("is_ForceMode_Force: " + is_ForceMode_Force); }
        //else if (Input.GetKeyUp("i")) { is_ForceMode_Force = false; Debug.Log("is_ForceMode_Force: " + is_ForceMode_Force); }
        //if (is_ForceMode_Force)rb.AddForce(Force_Coef * moveVec, ForceMode.Force);//rb.AddForce(Force_Coef * moveVec);
        //else rb.AddForce(Force_Coef* moveVec, ForceMode.Impulse);
        ////Debug.LogWarning(" actionGo: <color=green>" + actionGo  + "</color> _Go: <color=green>" + _Go + "</color> moveVec: <color=green>" + moveVec + "</color> tan_vec: <color=green>" + tan_vec + " </color> rb.velocity: <color=green>" + rb.velocity + "</color> AddForce : <color=green>" + Force_Coef * (moveVec - rb.velocity) + "</color> ");
        //Debug.LogWarning(" actionGo: <color=green>" + actionGo  + "</color> _Go: <color=green>" + _Go + "</color> tan_vec: <color=green>" + tan_vec.ToString("F3") + "</color> moveVec: <color=green>" + moveVec.ToString("F3") +  " </color> rb.velocity: <color=green>" + rb.velocity.ToString("F3") + "</color> AddForce : <color=green>" + (Force_Coef * moveVec).ToString("F3") + "</color> ");
        //if(cnt % 1000 == 0) Debug.Log(" moveVec: " + moveVec.ToString("F3") + " vel_now: " + vel_now.ToString("F3"));
        //cnt++; if (cnt >= 100100100) cnt = 0;
        ////////////////////////////////////////////////////////////////////////////////////////////////https://monoist.atmarkit.co.jp/mn/articles/1007/26/news083.html
        //diff[0] = diff[1];
        //diff[1] = vel_target * tan_vec - vel_now;
        //integral += (diff[1] + diff[0]) / 2.0f * Time.fixedDeltaTime;

        //Vector3 p = Kp * diff[1];
        //Vector3 i = Ki * integral;
        //Vector3 d = Kd * (diff[1] - diff[0]) / Time.fixedDeltaTime;
        //rb.AddForce(p + i + d, ForceMode.Force);
        ////////////////////////////////////////////////////////////////////////////////////////////////
        ///

        const int shorten = 1;//長さをshorten分の1にする
        Vector3[] vec = { other.GetContact(0).point, other.GetContact(0).point + moveVec / shorten };//[GetContactはindexで指定された接触点を取得]
        renderer.SetPositions(vec);//[vecの場所にセットする]
        // renderer.SetWidth(0.1f, 0.1f);//[最初と最後の線幅を設定（最初,最後）]
        renderer.startWidth = 0.1f;//[最初を設定]
        renderer.endWidth = 0.1f;//[最後を設定]
    }
}

