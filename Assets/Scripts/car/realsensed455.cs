using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class realsensed455 : MonoBehaviour
{
    //仕様値
    public float fov_horizon_degree=87.0f;//fovの水平角度
    public float fov_vertical_degree=58.0f;//fovの垂直角度
    public float min_measure = 0.0f;//取れる最小の距離m
    public float max_measure = 1.0f;//取れる最大の距離m
    public GameObject caribrate_wall;

    public GameObject dir_x;//右
    public GameObject dir_y;//上
    public GameObject dir_z;//前
    private float fov_width=0;//fovの水平距離
    private float fov_height=0;//fovの垂直距離
    private float ray_w_interval=0.0f;//幅のrayの間隔
    private float ray_h_interval=0.0f;//高さのrayの間隔
    private Vector3 obj_x=Vector3.zero;
    private Vector3 obj_y=Vector3.zero;
    private Vector3 obj_z=Vector3.zero;
    private bool caribration=true;//rayを原点から発射するため、distanceを調整する必要がある。それを実行
    public Color color=new Color(0.3f, 0.4f, 0.6f, 0.3f);
    Rigidbody rb;

    //変更する値、センサの本数を変えるとき
    public int ray_num_height = 9;
    public int ray_num_width = 3;
    public float[] ray_distance;//距離を保存
    public float[] ray_caribrate;
    
    // private float[] ray_caribrate = {0.7913407f,0.830627f,0.8450827f,0.830627f,0.7913407f,0.9141496f,0.97629f,1.0f,0.97629f,0.9141496f,0.7913407f,0.830627f,0.8450827f,0.830627f,0.7913407f};
    private Ray[] ray;
    private RaycastHit[] hit;
    private int center_slide_height;
    private int center_slide_width;

    private int ray_num;
    // private RaycastHit hit;
        // Start is called before the first frame update
    public void Start()
    {
        ray_distance=new float[ray_num_height*ray_num_width];
        ray_caribrate=new float[ray_num_height*ray_num_width];
        ray_caribrate[0]=0.7913407f;
        ray_caribrate[1]=0.830627f;
        ray_caribrate[2]=0.8450827f;
        ray_caribrate[3]=0.830627f;
        ray_caribrate[4]=0.7913407f;
        ray_caribrate[5]=0.9141496f;
        ray_caribrate[6]=0.97629f;
        ray_caribrate[7]=1.0f;
        ray_caribrate[8]=0.97629f;
        ray_caribrate[9]=0.9141496f;
        ray_caribrate[10]=0.7913407f;
        ray_caribrate[11]=0.830627f;
        ray_caribrate[12]=0.8450827f;
        ray_caribrate[13]=0.830627f;
        ray_caribrate[14]=0.7913407f;

        ray=new Ray[ray_num_height*ray_num_width];
        hit=new RaycastHit[ray_num_height*ray_num_width];
        fov_width =2*max_measure*Mathf.Tan(fov_horizon_degree/2.0f/180*Mathf.PI);
        fov_height=2*max_measure*Mathf.Tan(fov_vertical_degree/2.0f/180*Mathf.PI);

        rb=gameObject.GetComponent<Rigidbody>();
        ray_w_interval=fov_width/ray_num_width;
        ray_h_interval=fov_height/ray_num_height;
        
        //キャリブレーションするために、壁を生成
        obj_x=(dir_x.transform.position-gameObject.transform.position).normalized;
        obj_y=(dir_y.transform.position-gameObject.transform.position).normalized;
        obj_z=(dir_z.transform.position-gameObject.transform.position).normalized;
        // Debug.Log(obj_direction.normalized);
        //9→4 7→3　3→1
        center_slide_width=ray_num_width/2;
        center_slide_height=ray_num_height/2;
        ray_num=ray_num_height*ray_num_width;
        for(int i=0;i<ray_num_width;i++){
            for(int v=0;v<ray_num_height;v++){
                ray[ray_num_height*i+v]=new Ray(gameObject.transform.position,obj_z*max_measure+obj_x*ray_w_interval*(i-center_slide_width)+obj_y*ray_h_interval*(v-center_slide_height)+obj_z*min_measure);//(origin,direction)どっちもVector3
            }
        }
        for(int i=0;i<ray_num;i++){
            if(Physics.Raycast(ray[i],out hit[i],max_measure)){
                ray_distance[i]=hit[i].distance;
            }
        }
        // for(int i=0;i<ray_num;i++){
        //     ray_caribrate[i]=ray_distance[(ray_num-1)/2]/ray_distance[i];
        // }

        //初期値はmaxにする。
        for(int i=0;i<ray_num_height*ray_num_width;i++){
            ray_distance[i]=max_measure*ray_caribrate[i];
        }
        caribrate_wall.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        obj_x=(dir_x.transform.position-gameObject.transform.position).normalized;
        obj_y=(dir_y.transform.position-gameObject.transform.position).normalized;
        obj_z=(dir_z.transform.position-gameObject.transform.position).normalized;
        // Debug.Log(obj_direction.normalized);
        //9→4 7→3　3→1
        // Debug.Log(obj_x*max_measure);
        // Debug.Log(obj_y);
        // Debug.Log(obj_z);
        for(int i=0;i<ray_num_width;i++){
            for(int v=0;v<ray_num_height;v++){
                ray[ray_num_height*i+v]=new Ray(gameObject.transform.position,obj_z*max_measure+obj_x*ray_w_interval*(i-center_slide_width)+obj_y*ray_h_interval*(v-center_slide_height)+obj_z*min_measure);//(origin,direction)どっちもVector3
            }
        }
        for(int i=0;i<ray_num;i++){
            if(Physics.Raycast(ray[i],out hit[i],max_measure)){
                ray_distance[i]=hit[i].distance*ray_caribrate[i];
                // ray_distance[i]=hit[i].distance;
            }
            else{
                // ray_distance[i]=max_measure;
                ray_distance[i]=max_measure*ray_caribrate[i];
            }
        }
        for(int i=0;i<ray_num;i++){
            Debug.DrawRay(ray[i].origin,ray[i].direction*ray_distance[i],color,0.1f);
        }
        
        

        // if(Input.GetKey(KeyCode.Q)){
        //     rb.angularVelocity=new Vector3(0.0f,10.0f,0.0f);
        // }
        // if(Input.GetKey(KeyCode.W)){
        //     rb.angularVelocity=new Vector3(0.0f,-10.0f,0.0f);
        // }
        

    }
}
