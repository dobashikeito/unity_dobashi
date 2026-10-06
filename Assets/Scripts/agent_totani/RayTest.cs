//センサが衝突したら、センサのレーザーを表示の定義
//realsenseの実測値を使っている
// using RosSharp.RosBridgeClient;//[RosSharpは使わない]
//for文を用いてDepth値を保存していく
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTest : MonoBehaviour
{

    ////public GameObject Object_to_send_dis;//disの値を送りたいオブジェクト(例えばdisの値を学習に使うためminiCrawler3Dというオブジェクトに送りたい)
    public readonly float RANGE_DEG = 21;//以前は30でやってたけど， 24,27,30[deg]の位置が取れてないことが判明したので上下21度でやる
    ////public readonly float SHIFT_RANGE = 21;//20210108 RealSenseを下向きに傾ける
    ////public readonly float SHIFT_RANGE = 7;//20210114 RealSenseを下向きに傾ける // 上14, 下28 (1:2にする)
    ////public readonly float SHIFT_RANGE = 9;//20210114 RealSenseを下向きに傾ける // 上12, 下30
    public readonly float SHIFT_RANGE = 15;//20210115 RealSenseを下向きに傾ける // 上6, 下36
    ////public readonly float SHIFT_RANGE = 0;
    public float random_shift = 0.0f;
    public readonly float ADD_DEG = 3f;
    private const float MAX_DIS = 1.5f;//RealSense D435の最大レンジ https://www.switch-science.com/catalog/3633/
    ////private const float MAX_DIS = 2.0f;//20201228範囲を狭めてみる
    private const float MIN_DIS = 0.2f;
    [HideInInspector] public Vector3 randompos = new Vector3(0, 0, 0);//AgentReset()が起こるたびに位置が変更できるようにpulicにした.(ランダムに初期位置を変える  引数をfloatにする場合 min<=random_pos<=max
    ////↑inference_stageでは距離センサの位置を一定にするかもしれないから,ここでも一応初期化しておく．

    ////public float[] distance_to_object = new float[(int)(2 * RANGE_DEG / ADD_DEG) + 1];//Rayの原点から衝突地点までの距離を得る
    ////public float[] distance_to_object2 = new float[(int)(2 * RANGE_DEG / ADD_DEG) + 1];//Ray2の原点から衝突地点までの距離を得る
    ////public float[] depth = new float[(int)(2 * RANGE_DEG / ADD_DEG) + 1];
    ////public float[] depth2 = new float[(int)(2 * RANGE_DEG / ADD_DEG) + 1];
    public float[] distance_to_object;//Rayの原点から衝突地点までの距離を得る//[保存していく]
    public float[] distance_to_object2;//Ray2の原点から衝突地点までの距離を得る//[保存していく]
    public float[] depth;//[保存していく]
    public float[] depth2;//[保存していく]

    ////public float hole_dis = 100, hole_dis2 = 100;
    ////private float RAY_HEIGHT = 0.25f;//0.25 
    ////private Vector3 INITIAL_ARM_POS = new Vector3(0, 0.25f, 0.1f);//2020_06_03_Crawler_hontaiから実機のロボットアームについているRealSenseのおおよその初期位置だよ 上のRAY_HEIGHTの代わりに使う
    ////private Vector3 INITIAL_ARM_POS = new Vector3(0, 0.285f, 0.13f);//2020_12_25_再度測り直した
    ////private Vector3 INITIAL_ARM_POS = new Vector3(0, 0.285f, 0.1f);///1225_7で学習
    ////private Vector3 INITIAL_ARM_POS = new Vector3(0, 0.25f, 0.13f);///1226_1で学習
    ////private Vector3 INITIAL_ARM_POS = new Vector3(0, 0.26f, 0.14f);///2021_0108 RealSenseをdynamixelで傾けて挑戦
    private Vector3 INITIAL_ARM_POS = new Vector3(0, 0.25f, 0.16f);///2021_0113  21deg傾けたRealSenseの位置を再測定
    [HideInInspector] public readonly float dis_rays = 0.15f;//2つのrayの距離 //constとかstatic readonlyだと他のクラスからアクセスできなかったのでreadonlyを使う
    void Start()
    {
        int LENGTH = (int)(2 * RANGE_DEG / ADD_DEG) + 1;//[初期位置？]
        distance_to_object = new float[LENGTH];
        distance_to_object2 = new float[LENGTH];
        depth = new float[LENGTH];
        depth2 = new float[LENGTH];
        ////distance_to_object = new float[( 2 * RANGE_DEG/ADD_DEG ) + 1];//初期化しないとOutOfRangeがでた
        ////randompos = new Vector3(0, Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f));//AgentReset()が起こるたびに位置が変更できるようにpublicにした.(ランダムに初期位置を変える  引数をfloatにする場合 min<=random_pos<=max
        ////↑inference_stageでは距離センサの位置を一定にするかもしれないから,ここでも一応初期化しておく．
    }
    void Update()
    {
        Ray();//[20220721]下のif文より、rossharpを使わないときは、この関数をやり続けているので、下をコメントアウトし、追加
        //Debug.Log("x :"+ GetComponent<Float64MultiArray_Subscriber_ML_inference_stage> ().distance_to_object.Length);
        // if (GetComponent<Float64MultiArray_Subscriber>().is_Subscribe == false) Ray();

        // //rosからrealsenseデータが送られてくる場合はこっちを使う.
        // //Debug.Log("1 : distance_to_object %d : " + distance_to_object.Length);
        // else
        // {
        //     distance_to_object = GetComponent<Float64MultiArray_Subscriber>().distance_to_object;//rosのrealsenseからのデータを使う場合はこっちを使う
        //     distance_to_object2 = GetComponent<Float64MultiArray_Subscriber>().distance_to_object2;//rosのrealsenseからのデータを使う場合はこっちを使う

        //     int for_count = 0;
        //     for (float angle_deg = -RANGE_DEG; angle_deg <= RANGE_DEG; angle_deg = angle_deg + ADD_DEG)// -RANGE_DEG から RANGE_DEGまでループ
        //     {
        //         var tan = Mathf.Tan(angle_deg * (Mathf.PI / 180));//deg → rad してからtanにする
        //         Ray ray = new Ray(transform.position + this.gameObject.transform.rotation * new Vector3(dis_rays / 2 + randompos.x, INITIAL_ARM_POS.y + randompos.y, INITIAL_ARM_POS.z + randompos.z), this.gameObject.transform.rotation * new Vector3(0, tan, 1.0f));//transform.forward = this.gameObject.transform.rotation * new Vector3(0, 0, 1.0f);が成り立つっぽいhttps://qiita.com/Nekomasu/items/a7459a32adb4770d81c8
        //         Ray ray2 = new Ray(transform.position + this.gameObject.transform.rotation * new Vector3(-dis_rays / 2 + randompos.x, INITIAL_ARM_POS.y + randompos.y, INITIAL_ARM_POS.z + randompos.z), this.gameObject.transform.rotation * new Vector3(0, tan, 1.0f));//transform.forward = this.gameObject.transform.rotation * new Vector3(0, 0, 1.0f);が成り立つっぽいhttps://qiita.com/Nekomasu/items/a7459a32adb4770d81c8

        //         Debug.DrawRay(ray.origin, distance_to_object[for_count] * ray.direction, Color.red, Time.deltaTime);
        //         Debug.DrawRay(ray2.origin, distance_to_object2[for_count] * ray.direction, Color.red, Time.deltaTime);
        //         for_count++;
        //     }
        // }

    }

    //http://megumisoft.hatenablog.com/entry/2015/08/13/172136 参考URL
    public LayerMask mask;

    void Ray()
    {
        int for_count = 0;
        ////Vector3 randompos = new Vector3(0, Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f));//ランダムに初期位置を変える  引数をfloatにする場合 min<=random_pos<=max ここでこれをやるとエピソード毎ではなく，action毎に位置が変わってしまう．
        ////Vector3 Ray_pos = new Vector3(0, 0.1f, 0) + randompos;//crawler_hontaiにのっている距離センサの位置(crawler_hontaiの中心から10cm上

        for (float angle_deg = -RANGE_DEG- SHIFT_RANGE + random_shift; angle_deg <= RANGE_DEG-SHIFT_RANGE + random_shift; angle_deg = angle_deg + ADD_DEG)// -RANGE_DEG から RANGE_DEGまでループ
        {//[()内はrealsenseの傾きを模擬している]
            float random_noise = Random.Range(-0.0025f, 0.0025f);// ノイズの大きさが0.004022だったので， 0.005のノイズを入れる//[-0.0025から0.0025内の乱数を返す]
            float random_noise2 = Random.Range(-0.0025f, 0.0025f);// ノイズの大きさが0.004022だったので， 0.005のノイズを入れる
            ////random_noise = 0; random_noise2 = 0; ; randompos = Vector3.zero;

            //// 1.
            //// Rayの作成
            var tan = Mathf.Tan(angle_deg * (Mathf.PI / 180));//deg → rad してからtanにする
            Ray ray = new Ray(transform.position + this.gameObject.transform.rotation * new Vector3(dis_rays / 2 + randompos.x, INITIAL_ARM_POS.y + randompos.y, INITIAL_ARM_POS.z + randompos.z), this.gameObject.transform.rotation * new Vector3(0, tan, 1.0f));//[Ray(原点,方向)]
            //transform.forward = this.gameObject.transform.rotation * new Vector3(0, 0, 1.0f);が成り立つっぽいhttps://qiita.com/Nekomasu/items/a7459a32adb4770d81c8
            ////↑ 引数はワールド座標を受け取ってみるみたいで，this.gameObject.transform.rotationを掛けないとロボが回転しても，Rayの原点も回転してくれない
            ////Ray ray = new Ray(transform.position + this.gameObject.transform.TransformPoint(new Vector3(dis_rays / 2 / this.gameObject.transform.localScale.x, 0.25f/ this.gameObject.transform.localScale.y, 0)), this.gameObject.transform.rotation * new Vector3(0, tan, 1.0f));//transform.forward = this.gameObject.transform.rotation * new Vector3(0, 0, 1.0f);が成り立つっぽいhttps://qiita.com/Nekomasu/items/a7459a32adb4770d81c8
            ////Debug.Log(" this.gameObject.transform.localScale: " +this.gameObject.transform.localScale.ToString("F3") + "      " + this.gameObject.transform.TransformPoint(new Vector3(dis_rays / 2 / this.gameObject.transform.localScale.x, 0.25f / this.gameObject.transform.localScale.y, 0)).ToString("F3") );
            ////Debug.Log(" randompos: " + randompos.ToString("F3") + "hontai.transform.pos: " + gameObject.transform.position.ToString("F3")+ " RayPosRelative_withRand: " + (this.gameObject.transform.rotation * new Vector3(dis_rays / 2 + randompos.x, RAY_HEIGHT + randompos.y, 0 + randompos.z)).ToString("F3") + " RayPosWorld: "+ (transform.position + this.gameObject.transform.rotation * new Vector3(dis_rays / 2 + randompos.x, RAY_HEIGHT + randompos.y, 0 + randompos.z)).ToString("F3"));// + transform.position + this.gameObject.transform.rotation * new Vector3(dis_rays / 2, RAY_HEIGHT, 0));


            // 2.		
            // Rayが衝突したコライダーの情報を得る
            RaycastHit hit;//[RaycastHitはRaycastから情報を得れる]
            // Rayが衝突したかどうか
            if (Physics.Raycast(ray, out hit, MAX_DIS, mask))//[bool値を返す、ぶつかったらtrue]
            {
                // Rayの原点から衝突地点までの距離を得る
                ////float dis = hit.distance;
                ////Debug.Log("angle_deg: " + angle_deg + "  RANGE_DEG: " + RANGE_DEG + "  angle_deg+RANGE_DEG : "+(angle_deg + RANGE_DEG + "   dis.Length: " + dis.Length));
                if (MIN_DIS < hit.distance + random_noise) //[センサからの距離がrealsenseの最小レンジより大きいとき]
                distance_to_object[for_count] = hit.distance + random_noise;//上でpublicとして定義した
                //[hit.distanceはrayの原点から衝突点までの距離]
                else distance_to_object[for_count] = MIN_DIS;//以下なら最小の値にする


                ////Debug.Log("hit.distance"+ " : " + hit.distance);
            }
            else distance_to_object[for_count] = MAX_DIS;//Rayがhitしなかったら大きい値を入れる InfDisは上で定義してある.
            depth[for_count] = distance_to_object[for_count] * Mathf.Sin(angle_deg * (Mathf.PI / 180));

            Ray ray2 = new Ray(transform.position + this.gameObject.transform.rotation * new Vector3(-dis_rays / 2 + randompos.x, INITIAL_ARM_POS.y + randompos.y, INITIAL_ARM_POS.z + randompos.z), this.gameObject.transform.rotation * new Vector3(0, tan, 1.0f));//transform.forward = this.gameObject.transform.rotation * new Vector3(0, 0, 1.0f);が成り立つっぽいhttps://qiita.com/Nekomasu/items/a7459a32adb4770d81c8
            RaycastHit hit2;
            // Rayが衝突したかどうか
            if (Physics.Raycast(ray2, out hit2, MAX_DIS, mask))
            {
                // Rayの原点から衝突地点までの距離を得る
                if (MIN_DIS < hit2.distance + random_noise2) distance_to_object2[for_count] = hit2.distance + random_noise2;//上でpublicとして定義した
                else distance_to_object2[for_count] = MIN_DIS;
            }
            else distance_to_object2[for_count] = MAX_DIS;//Rayがhitしなかったら大きい値を入れる InfDisは上で定義してある.
            depth2[for_count] = distance_to_object2[for_count] * Mathf.Sin(angle_deg * (Mathf.PI / 180));
            ///distance_to_object[for_count] = 0;//試しに，常に一定値にしてみる!
            // 5.		
            // Rayの可視化
            Debug.DrawRay(ray.origin, distance_to_object[for_count] * ray.direction, Color.red, Time.deltaTime);
            ////Debug.DrawRay(ray2.origin, distance_to_object2[for_count] * ray.direction, Color.red, Time.deltaTime);

            for_count++;//[値を保存するためのindex設定]
            ////Debug.Log("  for_count: " + for_count+ " angle_deg:  "+ angle_deg);

        }



        // 1.
        // Rayの作成
        //Ray ray = new Ray(transform.position, this.gameObject.transform.rotation * new Vector3(0, 0, 1.0f));//transform.forward = this.gameObject.transform.rotation * new Vector3(0, 0, 1.0f);が成り立つっぽいhttps://qiita.com/Nekomasu/items/a7459a32adb4770d81c8

        //Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        // 2.		
        // Rayが衝突したコライダーの情報を得る
        /*RaycastHit hit;
        // Rayが衝突したかどうか
        if (Physics.Raycast(ray, out hit, 10.0f, mask))
        {
            // Examples
            // 衝突したオブジェクトの色を赤に変える
            //hit.collider.GetComponent<MeshRenderer>().material.color = Color.red;

            // 衝突したオブジェクトを消す
            //Destroy(hit.collider.gameObject);

            // Rayの衝突地点に、このスクリプトがアタッチされているオブジェクトを移動させる
            //this.transform.position = hit.point;

            // Rayの原点から衝突地点までの距離を得る
            //float dis = hit.distance;
            dis = hit.distance;//上でpublicとして定義した
            //Debug.Log("hit.distance"+ " : " + hit.distance);

            // 衝突したオブジェクトのコライダーを非アクティブにする
            //hit.collider.enabled = false;
        }
        */


        // 3.		
        // Rayが衝突した全てのコライダーの情報を得る。＊順序は保証されない
        /*
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
        foreach (var obj in hits)
        {
            
            // 衝突したオブジェクトのタグ毎に色を変え、当てはまらない場合はオブジェクトを非アクティブにする
            switch (obj.collider.tag)
            {
                case "Red":
                    obj.collider.GetComponent<MeshRenderer>().material.color = Color.red;
                    break;
                case "Blue":
                    obj.collider.GetComponent<MeshRenderer>().material.color = Color.blue;
                    break;
                case "Green":
                    obj.collider.GetComponent<MeshRenderer>().material.color = Color.green;
                    break;
                default:
                    obj.collider.gameObject.SetActive(false);
                    break;
            }
            
        }
        */

        // 4.		
        // 球形のRayをとばす
        //if (Physics.SphereCast(ray, 5.0f, out hit, 10.0f)) { }

        /*
        // 5.		
        // Rayの可視化
        Debug.DrawRay(ray.origin, ray.direction, Color.red, 3.0f);
        */

        // 6.		
        /*
        // マウスがEventSystem上にあるか
        UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        // マウスがEventSystem上にないときに、左クリックした場合
        if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
        {
            // 処理内容
        }
        */
    }
}
