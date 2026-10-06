using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class real_robot_parameter : MonoBehaviour {

    public GameObject agent_go;
    // private urdfmodel_agent_dp agent;
    // private ufm_agent_linear_route agent;
    private ufm_agent_krail_route agent;
    public GameObject body;
    public GameObject fl;
    public GameObject fr;
    public GameObject rl;
    public GameObject rr;
    public GameObject ros_pubsub_ai_go;
    public GameObject ray_go;
    public GameObject start_position;
    private ros_pubsub_ai ros_pubsub_ai;
    private HingeJoint[] joint=new HingeJoint[4];
    private JointSpring[] hingeSpring=new JointSpring[4];
    private JointLimits[] limits=new JointLimits[4];
    

    private float maxdeg = 180;
    private float mindeg = -180;

    //仕様値
    public float fov_horizon_degree=87.0f;//fovの水平角度
    public float fov_vertical_degree=58.0f;//fovの垂直角度
    public float min_measure = 0.0f;//取れる最小の距離m
    public float max_measure = 1.0f;//取れる最大の距離m

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
    public Color color=new Color(1.0f, 0.6f, 0.4f, 0.3f);

    //変更する値、センサの本数を変えるとき
    public int ray_num_height = 9;
    public int ray_num_width = 3;
    public float[] ray_distance;//距離を保存
    
    // private float[] ray_caribrate = {0.7913407f,0.830627f,0.8450827f,0.830627f,0.7913407f,0.9141496f,0.97629f,1.0f,0.97629f,0.9141496f,0.7913407f,0.830627f,0.8450827f,0.830627f,0.7913407f};
    private Ray[] ray;
    private RaycastHit[] hit;
    private int center_slide_height;
    private int center_slide_width;

    private int ray_num;
    // ボタンが押された場合、今回呼び出される関数
    private void Start() {
    //    agent=agent_go.GetComponent<urdfmodel_agent_dp>();
    //    agent=agent_go.GetComponent<ufm_agent_linear_route>();
       agent=agent_go.GetComponent<ufm_agent_krail_route>();
       joint[0]=fl.GetComponent<HingeJoint>();
       joint[1]=fr.GetComponent<HingeJoint>();
       joint[2]=rl.GetComponent<HingeJoint>();
       joint[3]=rr.GetComponent<HingeJoint>();
       ros_pubsub_ai=ros_pubsub_ai_go.GetComponent<ros_pubsub_ai>();
       for(int i=0;i<4;i++){
            hingeSpring[i] = joint[i].spring;
            hingeSpring[i].spring = 400;  //力の大きさ
            hingeSpring[i].damper = 50;  // 大きいほど速度が落ちる（摩擦？）
            hingeSpring[i].targetPosition = 0;  // 目的の角度
            joint[i].spring = hingeSpring[i];  // 上記三つをjointに渡す
            joint[i].useSpring = true;

            limits[i]=joint[i].limits;
            limits[i].min=mindeg;
            limits[i].max=maxdeg;
            limits[i].bounceMinVelocity = 0;
            joint[i].limits=limits[i];
            joint[i].useLimits=true;
        }

        ray=new Ray[ray_num_height*ray_num_width];
        hit=new RaycastHit[ray_num_height*ray_num_width];
        fov_width =2*max_measure*Mathf.Tan(fov_horizon_degree/2.0f/180*Mathf.PI);
        fov_height=2*max_measure*Mathf.Tan(fov_vertical_degree/2.0f/180*Mathf.PI);

        ray_w_interval=fov_width/ray_num_width;
        ray_h_interval=fov_height/ray_num_height;
        //キャリブレーションするために、壁を生成
        obj_x=(dir_x.transform.position-ray_go.transform.position).normalized;
        obj_y=(dir_y.transform.position-ray_go.transform.position).normalized;
        obj_z=(dir_z.transform.position-ray_go.transform.position).normalized;
        // Debug.Log(obj_direction.normalized);
        //9→4 7→3　3→1
        center_slide_width=ray_num_width/2;
        center_slide_height=ray_num_height/2;
        ray_num=ray_num_height*ray_num_width;
        for(int i=0;i<ray_num_width;i++){
            for(int v=0;v<ray_num_height;v++){
                ray[ray_num_height*i+v]=new Ray(ray_go.transform.position,obj_z*max_measure+obj_x*ray_w_interval*(i-center_slide_width)+obj_y*ray_h_interval*(v-center_slide_height)+obj_z*min_measure);//(origin,direction)どっちもVector3
            }
        }
        for(int i=0;i<ray_num;i++){
            if(Physics.Raycast(ray[i],out hit[i],max_measure)){
                ray_distance[i]=hit[i].distance;
            }
        }
        //初期値はmaxにする。
        for(int i=0;i<ray_num_height*ray_num_width;i++){
            ray_distance[i]=max_measure;
        }

    }

    private void Update(){
        hingeSpring[0].targetPosition = ros_pubsub_ai.flipper_data_list_new[0]*-1.0f;
        joint[0].spring=hingeSpring[0];
        joint[0].useSpring=true;

        hingeSpring[1].targetPosition = ros_pubsub_ai.flipper_data_list_new[1];
        joint[1].spring=hingeSpring[1];
        joint[1].useSpring=true;

        hingeSpring[2].targetPosition = ros_pubsub_ai.flipper_data_list_new[2]*-1.0f;
        joint[2].spring=hingeSpring[2];
        joint[2].useSpring=true;

        hingeSpring[3].targetPosition = ros_pubsub_ai.flipper_data_list_new[3]*1.0f;
        joint[3].spring=hingeSpring[3];
        joint[3].useSpring=true;

        Transform body_transform = body.transform;
        // Transform body_transform;
        body_transform.position=new Vector3(ros_pubsub_ai.gazebo_pos[0]+start_position.transform.localPosition.x, ros_pubsub_ai.gazebo_pos[1]+start_position.transform.localPosition.y, ros_pubsub_ai.gazebo_pos[2]+start_position.transform.localPosition.z);
        Vector3 newEulerAngles = new Vector3(ros_pubsub_ai.imu_data_list[0],ros_pubsub_ai.imu_data_list[1],ros_pubsub_ai.imu_data_list[2]);
        body_transform.eulerAngles = newEulerAngles;
        // body_transform.position = agent.body_tf;//gazeboからtfを受け取っていないとき
        
        realsense_depth();
        
    }

    private void realsense_depth(){
        obj_x=(dir_x.transform.position-ray_go.transform.position).normalized;
        obj_y=(dir_y.transform.position-ray_go.transform.position).normalized;
        obj_z=(dir_z.transform.position-ray_go.transform.position).normalized;

        for(int i=0;i<ray_num_width;i++){
            for(int v=0;v<ray_num_height;v++){
                ray[ray_num_height*i+v]=new Ray(ray_go.transform.position,obj_z*max_measure+obj_x*ray_w_interval*(i-center_slide_width)+obj_y*ray_h_interval*(v-center_slide_height)+obj_z*min_measure);//(origin,direction)どっちもVector3
            }
        }
        for(int i=0;i<ray_num;i++){
            if(Physics.Raycast(ray[i],out hit[i],max_measure)){
                ray_distance[i]=ros_pubsub_ai.rs_data_list[i];
                // ray_distance[i]=hit[i].distance;
            }
            else{
                // ray_distance[i]=max_measure;
                ray_distance[i]=ros_pubsub_ai.rs_data_list[i];
            }
        }
        for(int i=0;i<ray_num;i++){
            Debug.DrawRay(ray[i].origin,ray[i].direction*ray_distance[i],color,0.1f);
        }
    }
}