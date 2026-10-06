//クリックしたところにcpを作成するスクリプト。カメラの操作も書いてある
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//追加
using System.Linq;
using UnityEditor;
public class create_toggle4checkpoint : MonoBehaviour {

    public GameObject start_position;//realrobotの原点とする。rosからのcp作成に使用
    public Material cp_material;
    [HideInInspector]
    public List<Toggle> cp_toggle_list=new List<Toggle>();//toggle自体を保存
    private int cp_toggle_list_count=0;
    [HideInInspector]
    public List<GameObject> cp_go_list=new List<GameObject>();//toggleのgameobjectの保存
    private List<int> cp_check_list=new List<int>();
    public GameObject Toggle_sample;
    private Toggle toggle;
    // public GameObject checkpoints;
    public GameObject camera_go;
    public GameObject ros_pubsub_ai_go;
    public GameObject saved_cp_list_go;//保存してあるチェック・ポイントをあタッチする
    private List<GameObject> saved_cp_list=new List<GameObject>();//チェック・ポイントのリストを格納
    private ros_pubsub_ai ros_pubsub_ai;
    private Camera cam;
    private Vector3 plane_normal;
    private Vector3 plane_next;
    private Vector3 plane_past;
    private Vector3 click_point_past;
    private Vector3 click_point_next;
    private Vector3 zero_axis;
    private float angle;
    // [HideInInspector]
    public List<GameObject> go_list = new List<GameObject>();//cpのgameobjectを保存
    [HideInInspector]
    public List<Vector3> tf_list = new List<Vector3>();//cpのtransformを保存
    public GameObject agent_go;
    private ufm_agent_nolearning ufm_agent_nolearning;
    //カメラ動かす用https://esprog.hatenablog.com/entry/2016/03/20/033322
    [SerializeField, Range(0.1f, 10f)]
    private float wheelSpeed = 10.0f;
    [SerializeField, Range(0.1f, 10f)]
    private float moveSpeed = 10.0f;
    [SerializeField, Range(0.1f, 10f)]
    private float rotateSpeed = 1.0f;
    private Vector3 preMousePos;
    public bool only_cp_write_true=true;
    private bool show_cp_button = true;

    //create_robo_trajectory.csと同じ
    // public Camera cam;
    private double time_past;
    [SerializeField] Material lineMaterial;
    [SerializeField] Color lineColor;
    [Range(0.1f, 0.5f)]
    [SerializeField] float lineWidth;
    
    //追加　LineRdenerer型のリスト宣言
    public List<LineRenderer> lineRenderers;
    public float updating_time = 0.1f;
    public Button rt_onoff_button;

    private void Start() {
        plane_normal=Vector3.up;
        zero_axis=new Vector3(0.0f,0.0f,1.0f);
        click_point_past =this.transform.position;
        plane_past=Vector3.ProjectOnPlane(zero_axis,plane_normal);
        cam = camera_go.GetComponent<Camera>();
        ufm_agent_nolearning = agent_go.GetComponent<ufm_agent_nolearning>();
        ros_pubsub_ai = ros_pubsub_ai_go.GetComponent<ros_pubsub_ai>();
        //線書く用
        time_past = 0;
        //追加　Listの初期化
        lineRenderers = new List<LineRenderer>();
        //保存してあるチェック・ポイントを変数に格納
        for(int i=0;i<saved_cp_list_go.transform.childCount;i++){
            saved_cp_list.Add(saved_cp_list_go.transform.GetChild(i).gameObject);
        }
    }
    public void create_toggle_onclick(){//Canvas状にtoggleを追加するプログラム
        GameObject checkpoint = Instantiate(Toggle_sample);
        Toggle cp_toggle = checkpoint.GetComponent<Toggle>();
        Text cp_text = checkpoint.GetComponent<Text>();
        cp_go_list.Add(checkpoint);
        cp_toggle_list.Add(cp_toggle);
        checkpoint.transform.parent = this.transform;
        checkpoint.transform.name = cp_toggle_list_count+"";
        checkpoint.transform.localPosition = this.transform.localPosition + new Vector3(600.0f,-20.0f*cp_toggle_list_count,0.0f);
        cp_toggle.isOn = false;
        cp_text.text = "checkpoint"+cp_toggle_list_count;
        cp_toggle_list_count++;
    }
    public void show_toggle_onclick(){//cpを表示するモードに切り替えかつcpを全表示
        if(show_cp_button){
            show_cp_button=false;
            for(int i=0;i<go_list.Count;i++){
                go_list[i].GetComponent<MeshRenderer>().enabled=false;
            }
        }
        else{
            show_cp_button=true;
            for(int i=0;i<go_list.Count;i++){
                go_list[i].GetComponent<MeshRenderer>().enabled=true;
            }
        }
    }
    public void delete_toggle_onclick(){//Deleteボタンクリック時の処理。
        for(int i=0;i<cp_toggle_list.Count;i++){
            // if(cp_check_list[i]==1){//toggleがcheckされているかの確認
            //     cp_go_list[i].SetActive(false);
            //     go_list[i].SetActive(false);
            // }
            Destroy(cp_go_list[i]);
            Destroy(go_list[i]);
        }
        for(int i=0;i<lineRenderers.Count;i++){
            Destroy(lineRenderers[i]);
        }
        go_list = new List<GameObject>();
        cp_go_list = new List<GameObject>();
        cp_toggle_list = new List<Toggle>();
        tf_list = new List<Vector3>();
        cp_toggle_list_count=0;
        click_point_past =this.transform.position;
        plane_past=Vector3.ProjectOnPlane(zero_axis,plane_normal);
        ufm_agent_nolearning.index_reset_button();
        lineRenderers = new List<LineRenderer>();
        ros_pubsub_ai.reset_cp_send();//rosにempty msgを送る
    }


    private void MouseUpdate(){//カメラ動かす
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel")*3.0f;
        if(scrollWheel != 0.0f){
            MouseWheel(scrollWheel);
        }

        if(Input.GetMouseButtonDown(0)||Input.GetMouseButtonDown(1)||Input.GetMouseButtonDown(2)){
            preMousePos = Input.mousePosition;
        }
            
        MouseDrag(Input.mousePosition);
    }

    private void MouseWheel(float delta){//カメラ動かす
        camera_go.transform.position += camera_go.transform.forward * delta * wheelSpeed;
        return;
    }

    private void MouseDrag(Vector3 mousePos){//カメラ動かす
        Vector3 diff = mousePos - preMousePos;

        if(diff.magnitude < Vector3.kEpsilon)
            return;

        if(Input.GetMouseButton(2)){
            camera_go.transform.Translate(-diff * Time.deltaTime * moveSpeed);
        }
        
        else if(Input.GetMouseButton(1)){
            CameraRotate(new Vector2(-diff.y, diff.x) * rotateSpeed);
        }
        preMousePos = mousePos;
    }

    public void CameraRotate(Vector2 angle){
        camera_go.transform.RotateAround(camera_go.transform.position, camera_go.transform.right, angle.x);
        camera_go.transform.RotateAround(camera_go.transform.position, Vector3.up, angle.y);
    }
    public void only_cp_write(){
        if (Input.GetMouseButtonDown(0)){
             RaycastHit hit;
             Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            //  Debug.LogError(Input.mousePosition[0]+","+Input.mousePosition[1]);
             if(Input.mousePosition[0]>1440||Input.mousePosition[1]<152){//カメラのどこをクリックしてもcpができてしまうので、それをロボットの画面のみにしたいので。
                return;
             }
             if (Physics.Raycast(ray, out hit)){
                // GameObject new_checkpoint = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                // if(go_list.Count!=0){auto_create_cp(click_point_past,hit.point);}
                GameObject new_checkpoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
                new_checkpoint.transform.parent=this.transform;
                go_list.Add(new_checkpoint);
                new_checkpoint.transform.name = "click_point"+go_list.Count;
                click_point_next = new Vector3 (hit.point.x,hit.point.y, hit.point.z);
                // Debug.LogError(click_point_next);
                tf_list.Add(click_point_next);
                Vector3 vec = click_point_next - click_point_past;
                
                plane_next=Vector3.ProjectOnPlane(vec,plane_normal);
                angle = Vector3.SignedAngle(plane_next,zero_axis,plane_normal);
                
                new_checkpoint.transform.localScale = new Vector3(1.5f,5.0f,0.01f);
                // new_checkpoint.transform.localScale = new Vector3(1.5f,2.0f,1.5f);
                new_checkpoint.transform.position = click_point_next;
                new_checkpoint.transform.localEulerAngles = new Vector3(0.0f,-1.0f*angle,0.0f);
                new_checkpoint.layer = 2;
                new_checkpoint.GetComponent<Renderer>().material = cp_material;
                BoxCollider collider=new_checkpoint.GetComponent<BoxCollider>();
                // CapsuleCollider collider=new_checkpoint.GetComponent<CapsuleCollider>();
                collider.isTrigger = true;

                plane_past = plane_next;
                click_point_past = click_point_next;
                create_toggle_onclick();
            }
        }
    }
    public void cp_write_and_line(){//create_robo_trajectory.csと同じ
        if (Input.GetMouseButtonDown(0)){
            //lineObjを生成し、初期化する
            _addLineObject();
        }

        //追加　クリック中（ストローク中）
        if (Input.GetMouseButton(0)){
            if(Input.mousePosition[0]>1330||Input.mousePosition[1]<88){//カメラのどこをクリックしてもcpができてしまうので、それをロボットの画面のみにしたいので。
                return;
            }
            if(Time.time - time_past > updating_time){
                time_past = Time.time;
                _addPositionDataToLineRendererList();
                }
        }
        //追加　クリックしたら発動
    void _addLineObject()//create_robo_trajectory.csと同じ
    {
        //空のゲームオブジェクト作成
        GameObject lineObj = new GameObject();
        //オブジェクトの名前をStrokeに変更
        lineObj.name = "Stroke";
        //lineObjにLineRendereコンポーネント追加
        lineObj.AddComponent<LineRenderer>();
        //lineRendererリストにlineObjを追加
        lineRenderers.Add(lineObj.GetComponent<LineRenderer>());
        //lineObjを自身の子要素に設定
        lineObj.transform.SetParent(transform);
        lineObj.GetComponent<LineRenderer>().numCapVertices = 10;
        //lineObj初期化処理
        _initRenderers();
    }

    //lineObj初期化処理
    void _initRenderers(){//create_robo_trajectory.csと同じ
        //線をつなぐ点を0に初期化
        lineRenderers.Last().positionCount = 0;
        //マテリアルを初期化
        lineRenderers.Last().material = lineMaterial;
        //色の初期化
        lineRenderers.Last().material.color = lineColor;
        //太さの初期化
        lineRenderers.Last().startWidth = lineWidth;
        lineRenderers.Last().endWidth = lineWidth;
        
    }

    //押しながらで起動
    void _addPositionDataToLineRendererList(){//create_robo_trajectory.csと同じ
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if(Input.mousePosition[0]>1440||Input.mousePosition[1]<152){//カメラのどこをクリックしてもcpができてしまうので、それをロボットの画面のみにしたいので。
                return;
             }
        if (Physics.Raycast(ray, out hit)){
            
            //スクリーン座標をワールド座標に変換
            Vector3 worldPosition = new Vector3 (hit.point.x,hit.point.y+0.02f, hit.point.z);
            
            //ワールド座標をローカル座標に変換
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition.x, worldPosition.y, -1.0f);
            
            //lineRenderersの最後のlineObjのローカルポジションを上記のローカルポジションに設定
            lineRenderers.Last().transform.localPosition = localPosition;
            
            //lineObjの線と線をつなぐ点の数を更新
            lineRenderers.Last().positionCount += 1;
            
            //LineRendererコンポーネントリストを更新
            lineRenderers.Last().SetPosition(lineRenderers.Last().positionCount - 1, worldPosition);
            
            //あとから描いた線が上に来るように調整
            lineRenderers.Last().sortingOrder = lineRenderers.Count;
            float distance = Vector3.Distance(worldPosition, click_point_past);
            if(distance>0.1f){//cpの間隔を20cm以上に設定、近すぎるのを防止
                // if(go_list.Count!=0){auto_create_cp(click_point_past,hit.point);}
                GameObject new_checkpoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
                new_checkpoint.transform.parent=this.transform;
                go_list.Add(new_checkpoint);
                new_checkpoint.transform.name = "click_point"+go_list.Count;
                click_point_next = new Vector3 (hit.point.x,hit.point.y, hit.point.z);
                // Debug.LogError(click_point_next);
                tf_list.Add(click_point_next);
                Vector3 vec = click_point_next - click_point_past;
                
                plane_next=Vector3.ProjectOnPlane(vec,plane_normal);
                angle = Vector3.SignedAngle(plane_next,zero_axis,plane_normal);
                
                new_checkpoint.transform.localScale = new Vector3(1.5f,5.0f,0.01f);
                // new_checkpoint.transform.localScale = new Vector3(1.5f,2.0f,1.5f);
                new_checkpoint.transform.position = click_point_next;
                new_checkpoint.transform.localEulerAngles = new Vector3(0.0f,-1.0f*angle,0.0f);
                new_checkpoint.layer = 2;
                //cpのmaterialを決める。しかしこの方法はうまくいっていない。表示しない。ボタンで表示する方式に変更
                MeshRenderer renderer_=new_checkpoint.GetComponent<MeshRenderer>();
                renderer_.material = cp_material;
                if(!show_cp_button){renderer_.enabled=false;}  

                BoxCollider collider=new_checkpoint.GetComponent<BoxCollider>();
                // CapsuleCollider collider=new_checkpoint.GetComponent<CapsuleCollider>();
                collider.isTrigger = true;

                plane_past = plane_next;
                click_point_past = click_point_next;
                create_toggle_onclick();
            }
            
            
        }
    }
    }
    
    void auto_create_cp(Vector3 past,Vector3 next){
        float cp2cp = Vector3.Distance(past, next);//cp間距離
        int cp_num_between = (int)(cp2cp/0.2f);//自動生成するべきcpの数//位置のみ、角度はpastと同じで
        if(cp_num_between==0){
            return;
        }
        float interval_x = (next.x-past.x)/cp_num_between;
        float interval_y = (next.y-past.y)/cp_num_between;
        float interval_z = (next.z-past.z)/cp_num_between;
        for(int i=0;i<cp_num_between;i++){
            GameObject new_checkpoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
            new_checkpoint.transform.parent=this.transform;
            go_list.Add(new_checkpoint);
            new_checkpoint.transform.name = "click_point"+go_list.Count;
            click_point_next = new Vector3(past.x+interval_x*i,past.y+interval_y*i,past.z+interval_z*i);
            // Debug.LogError(click_point_next);
            tf_list.Add(click_point_next);
            Vector3 vec = click_point_next - click_point_past;
            
            plane_next=Vector3.ProjectOnPlane(vec,plane_normal);
            angle = Vector3.SignedAngle(plane_next,zero_axis,plane_normal);
            
            new_checkpoint.transform.localScale = new Vector3(1.5f,5.0f,0.01f);
            // new_checkpoint.transform.localScale = new Vector3(1.5f,2.0f,1.5f);
            new_checkpoint.transform.position = click_point_next;
            new_checkpoint.transform.localEulerAngles = new Vector3(0.0f,-1.0f*angle,0.0f);
            new_checkpoint.layer = 2;
            //cpのmaterialを決める。しかしこの方法はうまくいっていない。表示しない。ボタンで表示する方式に変更
            MeshRenderer renderer_=new_checkpoint.GetComponent<MeshRenderer>();
            renderer_.material = cp_material;
            if(!show_cp_button){renderer_.enabled=false;}  

            BoxCollider collider=new_checkpoint.GetComponent<BoxCollider>();
            // CapsuleCollider collider=new_checkpoint.GetComponent<CapsuleCollider>();
            collider.isTrigger = true;

            plane_past = plane_next;
            click_point_past = click_point_next;
            create_toggle_onclick();
        }
    }


    public void create_cp_byros(float[] xyzrpy){
        GameObject new_checkpoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        new_checkpoint.transform.parent=this.transform;
        go_list.Add(new_checkpoint);
        new_checkpoint.transform.name = "click_point"+go_list.Count;
        click_point_next = new Vector3 (-1.0f*xyzrpy[1],xyzrpy[2], xyzrpy[0]);
        // Debug.LogError(click_point_next);
        tf_list.Add(click_point_next);
        for(int i=3;i<6;i++){//unity仕様に変えるため。-180-180→0-360
            xyzrpy[i]=xyzrpy[i]*-1.0f;
            if(xyzrpy[i]<0){
                xyzrpy[i]+=360;
            }
        }
        
        new_checkpoint.transform.localScale = new Vector3(1.5f,5.0f,0.01f);
        // new_checkpoint.transform.localScale = new Vector3(1.5f,2.0f,1.5f);
        new_checkpoint.transform.position = click_point_next+start_position.transform.position;
        new_checkpoint.transform.localEulerAngles = new Vector3(xyzrpy[3],xyzrpy[4],xyzrpy[5]);
        new_checkpoint.layer = 2;
        //cpのmaterialを決める。しかしこの方法はうまくいっていない。表示しない。ボタンで表示する方式に変更
        MeshRenderer renderer_=new_checkpoint.GetComponent<MeshRenderer>();
        renderer_.material = cp_material;
        if(!show_cp_button){renderer_.enabled=false;}  

        BoxCollider collider=new_checkpoint.GetComponent<BoxCollider>();
        // CapsuleCollider collider=new_checkpoint.GetComponent<CapsuleCollider>();
        collider.isTrigger = true;

        plane_past = plane_next;
        click_point_past = click_point_next;
        create_toggle_onclick();
    }
    public void robot_trajctory_save_start(){//ロボットの位置を保存し、cpを表示する
        if(!ufm_agent_nolearning.robot_trajctory_save){
            rt_onoff_button.image.color=Color.red;
            ufm_agent_nolearning.robot_trajctory_save=true;
            ufm_agent_nolearning.rt_rot_list = new List<Quaternion>();
            ufm_agent_nolearning.rt_tf_list = new List<Vector3>();
        }
        else{
            rt_onoff_button.image.color=Color.white;
            ufm_agent_nolearning.robot_trajctory_save=false;
        }
    }
    public void robot_trajctory_trace_play(){
        delete_toggle_onclick();
        List<Quaternion> rt_rot_list=new List<Quaternion>();
        for(int i=2;i<ufm_agent_nolearning.rt_tf_list.Count;i++){
            tf_list.Add(ufm_agent_nolearning.rt_tf_list[i]);
            rt_rot_list.Add(ufm_agent_nolearning.rt_rot_list[i]);
        }
        
        for(int i=0;i<tf_list.Count;i++){//robotとcpとの接触を考え、i=0,1の最初の方は使わない
            GameObject new_checkpoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
            new_checkpoint.transform.parent=this.transform;
            go_list.Add(new_checkpoint);
            new_checkpoint.transform.name = "click_point"+go_list.Count;
            click_point_next =tf_list[i];
            new_checkpoint.transform.localScale = new Vector3(1.5f,5.0f,0.01f);
            // new_checkpoint.transform.localScale = new Vector3(1.5f,2.0f,1.5f);
            new_checkpoint.transform.position = click_point_next;
            new_checkpoint.transform.rotation = rt_rot_list[i];
            new_checkpoint.layer = 2;
            //cpのmaterialを決める。しかしこの方法はうまくいっていない。表示しない。ボタンで表示する方式に変更
            MeshRenderer renderer_=new_checkpoint.GetComponent<MeshRenderer>();
            renderer_.material = cp_material;
            if(!show_cp_button){renderer_.enabled=false;}  

            BoxCollider collider=new_checkpoint.GetComponent<BoxCollider>();
            // CapsuleCollider collider=new_checkpoint.GetComponent<CapsuleCollider>();
            collider.isTrigger = true;

            plane_past = plane_next;
            click_point_past = click_point_next;
            create_toggle_onclick();
        }
    }

    public void robot_trajctory_trace_inverse_play(){
        delete_toggle_onclick();
        List<Quaternion> rt_rot_list=new List<Quaternion>();
        for(int i=ufm_agent_nolearning.rt_tf_list.Count-3;i>-1;i--){//robotとcpとの接触を考え、i=0,1の最初の方は使わない
            Debug.LogWarning(i);
            tf_list.Add(ufm_agent_nolearning.rt_tf_list[i]);
            rt_rot_list.Add(ufm_agent_nolearning.rt_rot_list[i]);
        }
        for(int i=0;i<tf_list.Count;i++){
            GameObject new_checkpoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
            new_checkpoint.transform.parent=this.transform;
            go_list.Add(new_checkpoint);
            new_checkpoint.transform.name = "click_point"+go_list.Count;
            click_point_next = tf_list[i];
            new_checkpoint.transform.localScale = new Vector3(1.5f,5.0f,0.01f);
            // new_checkpoint.transform.localScale = new Vector3(1.5f,2.0f,1.5f);
            new_checkpoint.transform.position = click_point_next;
            new_checkpoint.transform.rotation = rt_rot_list[i];
            new_checkpoint.layer = 2;
            //cpのmaterialを決める。しかしこの方法はうまくいっていない。表示しない。ボタンで表示する方式に変更
            MeshRenderer renderer_=new_checkpoint.GetComponent<MeshRenderer>();
            renderer_.material = cp_material;
            if(!show_cp_button){renderer_.enabled=false;}  
            
            BoxCollider collider=new_checkpoint.GetComponent<BoxCollider>();
            // CapsuleCollider collider=new_checkpoint.GetComponent<CapsuleCollider>();
            collider.isTrigger = true;

            plane_past = plane_next;
            click_point_past = click_point_next;
            create_toggle_onclick();
        }
    }


    public void save_cp_forval(){
        for(int i=0;i<go_list.Count;i++){
            Vector3 da =go_list[i].gameObject.transform.position;
            go_list[i].transform.position = da+new Vector3(1119.75f,551.0f,0);
            #if UnityEditor
            PrefabUtility.SaveAsPrefabAsset(go_list[i], "Assets/Scripts/urdfmodel_agent_8nozi_dp/ufm_agent_validation/checkpoint/check"+i.ToString()+".prefab");
            #endif
        }
        Debug.LogWarning("Save Checkpoints");
        
    }

    public void saved_call_cp_list(){
        delete_toggle_onclick();
        List<Quaternion> rt_rot_list=new List<Quaternion>();
        Vector3 robo_pos = new Vector3(ufm_agent_nolearning.now_xyzrpy4vel4real[0]+ufm_agent_nolearning.initial_pos_dt4real.x,ufm_agent_nolearning.now_xyzrpy4vel4real[1]+ufm_agent_nolearning.initial_pos_dt4real.y,ufm_agent_nolearning.now_xyzrpy4vel4real[2]+ufm_agent_nolearning.initial_pos_dt4real.z);
        Quaternion robo_rot = Quaternion.Euler(ufm_agent_nolearning.now_xyzrpy4vel4real[3],ufm_agent_nolearning.now_xyzrpy4vel4real[4],ufm_agent_nolearning.now_xyzrpy4vel4real[5]);
        if(!ufm_agent_nolearning.real_world){
            robo_pos = new Vector3(ufm_agent_nolearning.now_xyzrpy4vel[0]+ufm_agent_nolearning.initial_pos_dt.x,ufm_agent_nolearning.now_xyzrpy4vel[1]+ufm_agent_nolearning.initial_pos_dt.y,ufm_agent_nolearning.now_xyzrpy4vel[2]+ufm_agent_nolearning.initial_pos_dt.z);
            robo_rot = Quaternion.Euler(ufm_agent_nolearning.now_xyzrpy4vel[3],ufm_agent_nolearning.now_xyzrpy4vel[4],ufm_agent_nolearning.now_xyzrpy4vel[5]);
        }
        saved_cp_list_go.transform.position = robo_pos;
        // saved_cp_list_go.transform.rotation = robo_rot;
        
        for(int i=0;i<saved_cp_list.Count;i++){
            Vector3 cp_pos = saved_cp_list[i].transform.position;
            Quaternion cp_rot = saved_cp_list[i].transform.rotation;
            tf_list.Add(cp_pos);
            rt_rot_list.Add(cp_rot);
        }
        
        for(int i=0;i<tf_list.Count;i++){//robotとcpとの接触を考え、i=0,1の最初の方は使わない
            GameObject new_checkpoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
            new_checkpoint.transform.parent=this.transform;
            go_list.Add(new_checkpoint);
            new_checkpoint.transform.name = "click_point"+go_list.Count;
            click_point_next =tf_list[i];
            new_checkpoint.transform.localScale = new Vector3(1.5f,5.0f,0.01f);
            // new_checkpoint.transform.localScale = new Vector3(1.5f,2.0f,1.5f);
            new_checkpoint.transform.position = click_point_next;
            new_checkpoint.transform.rotation = rt_rot_list[i];
            new_checkpoint.layer = 2;
            //cpのmaterialを決める。しかしこの方法はうまくいっていない。表示しない。ボタンで表示する方式に変更
            MeshRenderer renderer_=new_checkpoint.GetComponent<MeshRenderer>();
            renderer_.material = cp_material;
            if(!show_cp_button){renderer_.enabled=false;}  

            BoxCollider collider=new_checkpoint.GetComponent<BoxCollider>();
            // CapsuleCollider collider=new_checkpoint.GetComponent<CapsuleCollider>();
            collider.isTrigger = true;

            plane_past = plane_next;
            click_point_past = click_point_next;
            create_toggle_onclick();
        }
    }

    void Update()//map上にcheckpointの作成
    {
        // move_camera();
        MouseUpdate();
        cp_check_list=new List<int>();
        for(int i=0;i<cp_toggle_list.Count;i++){//常にcheckされているかを監視
            if(cp_toggle_list[i].isOn){
                cp_check_list.Add(1);
                go_list[i].GetComponent<Renderer>().material.color = Color.red;
            }
            else{
                cp_check_list.Add(0);
                go_list[i].GetComponent<Renderer>().material.color = Color.white;
            }
        }
        if(only_cp_write_true){
            only_cp_write();//クリックでcpだけを表示。
        }
        else{
            cp_write_and_line();//線書きつつでcpを表現
        }
    }
}