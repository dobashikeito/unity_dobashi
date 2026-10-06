// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class change_field_ui : MonoBehaviour
// {
//     // Start is called before the first frame update
//     public GameObject scale_point;
//     private Vector3 scale_point_initial;
//     public GameObject field_down_right;
//     public GameObject field_down_left;
//     public GameObject field_left_up;
//     public GameObject field_left_down;
//     public GameObject field_up_right;
//     public GameObject field_up_left;
//     public GameObject field_right_up;
//     public GameObject field_right_down;

//     public GameObject body;
//     private touch_goal touch_Goal;
//     public int stage_number=0;
//     List<float> astm3_cube_astm3=new List<float>();
//     List<GameObject> astm3_cube_list=new List<GameObject>();
//     void Start()
//     {
//         float[] astm3_cube_astm3={0.2f,0.2f,0.2f,0.2f,0.2f,0.2f,0.2f,0.2f,0.2f,0.2f,0.2f,0.2f, 0.2f,0.2f,0.2f,0.2f,0.2f,0.2f,0.2f,0.2f,0.2f,0.2f,0.2f,0.2f, 0.4f,0.4f,0.4f,0.4f,0.2f,0.2f,0.2f,0.2f,0.4f,0.4f,0.4f,0.4f, 0.6f,0.6f,0.6f,0.4f,0.2f,0.2f,0.2f,0.2f,0.4f,0.6f,0.6f,0.6f, 0.8f,0.8f,0.6f,0.4f,0.2f,0.2f,0.2f,0.2f,0.4f,0.6f,0.8f,0.8f, 1.0f,0.8f,0.6f,0.4f,0.2f,0.2f,0.2f,0.2f,0.4f,0.6f,0.8f,1.0f, 1.0f,0.8f,0.6f,0.4f,0.2f,0.2f,0.2f,0.2f,0.4f,0.6f,0.8f,1.0f, 0.8f,1.0f,0.8f,0.6f,0.4f,0.2f,0.2f,0.4f,0.6f,0.8f,1.0f,0.8f, 0.6f,0.8f,1.0f,0.8f,0.6f,0.4f,0.4f,0.6f,0.8f,1.0f,0.8f,0.6f, 0.4f,0.6f,0.8f,1.0f,0.8f,0.6f,0.6f,0.8f,1.0f,0.8f,0.6f,0.4f, 0.2f,0.4f,0.6f,0.8f,1.0f,0.8f,0.8f,1.0f,0.8f,0.6f,0.4f,0.2f, 0.2f,0.2f,0.4f,0.6f,0.8f,1.0f,1.0f,0.8f,0.6f,0.4f,0.2f,0.2f, 0.2f,0.2f,0.4f,0.6f,0.8f,1.0f,1.0f,0.8f,0.6f,0.4f,0.2f,0.2f, 0.2f,0.2f,0.4f,0.6f,0.8f,0.8f,0.8f,0.8f,0.6f,0.4f,0.2f,0.2f, 0.4f,0.4f,0.4f,0.6f,0.6f,0.6f,0.6f,0.6f,0.6f,0.4f,0.4f,0.4f, 0.6f,0.6f,0.6f,0.4f,0.4f,0.4f,0.4f,0.4f,0.4f,0.6f,0.6f,0.6f, 0.8f,0.8f,0.6f,0.4f,0.2f,0.2f,0.2f,0.2f,0.4f,0.6f,0.8f,0.8f, 1.0f,0.8f,0.6f,0.4f,0.2f,0.2f,0.2f,0.2f,0.4f,0.6f,0.8f,1.0f, 1.0f,0.8f,0.6f,0.4f,0.2f,0.2f,0.2f,0.2f,0.4f,0.6f,0.8f,1.0f, 0.8f,1.0f,0.8f,0.6f,0.4f,0.2f,0.2f,0.4f,0.6f,0.8f,1.0f,0.8f, 0.6f,0.8f,1.0f,0.8f,0.6f,0.4f,0.4f,0.6f,0.8f,1.0f,0.8f,0.6f, 0.4f,0.6f,0.8f,1.0f,0.8f,0.6f,0.6f,0.8f,1.0f,0.8f,0.6f,0.4f, 0.2f,0.4f,0.6f,0.8f,1.0f,0.8f,0.8f,1.0f,0.8f,0.6f,0.4f,0.2f, 0.2f,0.2f,0.4f,0.6f,0.8f,1.0f,1.0f,0.8f,0.6f,0.4f,0.2f,0.2f, 0.2f,0.2f,0.4f,0.6f,0.8f,1.0f,1.0f,0.8f,0.6f,0.4f,0.2f,0.2f, 0.2f,0.2f,0.4f,0.6f,0.8f,0.8f,0.8f,0.8f,0.6f,0.4f,0.2f,0.2f, 0.4f,0.4f,0.4f,0.6f,0.6f,0.6f,0.6f,0.6f,0.6f,0.4f,0.4f,0.4f, 0.6f,0.6f,0.6f,0.4f,0.4f,0.4f,0.4f,0.4f,0.4f,0.6f,0.6f,0.6f, 0.8f,0.8f,0.6f,0.4f,0.2f,0.2f,0.2f,0.2f,0.4f,0.6f,0.8f,0.8f, 1.0f,0.8f,0.6f,0.4f,0.2f,0.2f,0.2f,0.2f,0.4f,0.6f,0.8f,1.0f, 1.0f,0.8f,0.6f,0.4f,0.2f,0.2f,0.2f,0.2f,0.4f,0.6f,0.8f,1.0f, 0.8f,1.0f,0.8f,0.6f,0.4f,0.2f,0.2f,0.4f,0.6f,0.8f,1.0f,0.8f, 0.6f,0.8f,1.0f,0.8f,0.6f,0.4f,0.4f,0.6f,0.8f,1.0f,0.8f,0.6f, 0.4f,0.6f,0.8f,1.0f,0.8f,0.6f,0.6f,0.8f,1.0f,0.8f,0.6f,0.4f, 0.2f,0.4f,0.6f,0.8f,1.0f,0.8f,0.8f,1.0f,0.8f,0.6f,0.4f,0.2f, 0.2f,0.2f,0.4f,0.6f,0.8f,1.0f,1.0f,0.8f,0.6f,0.4f,0.2f,0.2f};
//         scale_point_initial=scale_point.transform.localScale;
//         touch_Goal=body.GetComponent<touch_goal>();
//         for(int i=0;i<field_up_right.transform.childCount;i++){
//             astm3_cube_list.Add(field_up_right.transform.GetChild(i).gameObject);
//         }
//         for(int i=0;i<astm3_cube_list.Count;i++){
//             Vector3 now=astm3_cube_list[i].transform.localScale;
//             astm3_cube_list[i].transform.localScale=new Vector3(now.x,Random.Range(0,8)/10.0f,now.z);
//                 // astm3_cube_list[i].transform.localScale=new Vector3(now.x,astm3_cube_astm3[i],now.z);
//             }
//         field_right_down.SetActive(true);
//         for(int i=0;i<75;i++){
//             GameObject cube=GameObject.CreatePrimitive(PrimitiveType.Cube);
//             Rigidbody rb = cube.AddComponent<Rigidbody>();
//             rb.mass=10.0f;
//             cube.transform.parent=field_right_down.transform;
//             cube.transform.localPosition=new Vector3(Random.Range(0.0f,2.78f),Random.Range(1.5f,3.0f),Random.Range(0.5f,-5.67f));
//             cube.transform.localScale=new Vector3(Random.Range(0.01f,0.75f),Random.Range(0.01f,0.2f),Random.Range(0.01f,0.75f));
//             cube.transform.localEulerAngles=new Vector3(0.0f,Random.Range(0.0f,90.0f),0.0f);
//         }
//         field_right_down.SetActive(false);
//     }

//     // Update is called once per frame
//     void Update()
//     {

//         float side=Input.GetAxis("Axis 3");
//         float up_down=Input.GetAxis("Axis 6");

//         float analog_angle=Mathf.Atan2(up_down,side)*180.0f/Mathf.PI*-1.0f;
//         float analog_mag=Mathf.Sqrt(Mathf.Pow(up_down,2)+Mathf.Pow(side,2));
//         if(analog_mag>0.0f){
//             analog_mag=1.0f;
//         }


//         // Debug.LogError(analog_angle+"::::::"+analog_mag);
//         Vector3 now_angle=scale_point.transform.localEulerAngles;
//         Vector3 now_scale=scale_point.transform.localScale;
//         scale_point.transform.localEulerAngles=new Vector3(now_angle.x,now_angle.y,analog_angle);
//         scale_point.transform.localScale=new Vector3(scale_point_initial.x*analog_mag,now_scale.y,now_scale.z);
//         analog_angle=-1.0f*analog_angle;
//         if(analog_angle>135.0f){//left_down//slalom_step
//             stage_number=0;
//             touch_Goal.reset_position();
//             field_up_left.SetActive(false);
//             field_up_right.SetActive(false);
//             field_down_left.SetActive(false);
//             field_down_right.SetActive(false);
//             field_right_up.SetActive(false);
//             field_right_down.SetActive(false);
//             field_left_up.SetActive(false);
//             field_left_down.SetActive(true);
//         }
//         else if(analog_angle>90.0f){//down_left//stairs
//             stage_number=1;
//             touch_Goal.reset_position();
//             field_up_left.SetActive(false);
//             field_up_right.SetActive(false);
//             field_down_left.SetActive(true);
//             field_down_right.SetActive(false);
//             field_right_up.SetActive(false);
//             field_right_down.SetActive(false);
//             field_left_up.SetActive(false);
//             field_left_down.SetActive(false);
//         }
//         else if(analog_angle>45.0f){//down_right//plane
//             stage_number=2;
//             touch_Goal.reset_position();
//             field_up_left.SetActive(false);
//             field_up_right.SetActive(false);
//             field_down_left.SetActive(false);
//             field_down_right.SetActive(false);
//             field_right_up.SetActive(false);
//             field_right_down.SetActive(false);
//             field_left_up.SetActive(false);
//             field_left_down.SetActive(false);
//         }
//         else if(analog_angle>0.0f){//right_down//rubble_field
//             stage_number=3;
//             touch_Goal.reset_position();
//             field_up_left.SetActive(false);
//             field_up_right.SetActive(false);
//             field_down_left.SetActive(false);
//             field_down_right.SetActive(false);
//             field_right_up.SetActive(false);
//             field_right_down.SetActive(true);
//             field_left_up.SetActive(false);
//             field_left_down.SetActive(false);
//         }
//         else if(analog_angle>-45.0f&&analog_angle<-1.0f){//right_up//astm3
//             stage_number=4;
//             touch_Goal.reset_position();
//             field_up_left.SetActive(false);
//             field_up_right.SetActive(false);
//             field_down_left.SetActive(false);
//             field_down_right.SetActive(false);
//             field_right_up.SetActive(true);
//             field_right_down.SetActive(false);
//             field_left_up.SetActive(false);
//             field_left_down.SetActive(false);
//         }
//         else if(analog_angle>-90.0f&&analog_angle<-1.0f){//up_right//random
//             stage_number=5;
//             touch_Goal.reset_position();
//             field_up_left.SetActive(false);
//             field_down_left.SetActive(false);
//             field_down_right.SetActive(false);
//             field_right_up.SetActive(false);
//             field_right_down.SetActive(false);
//             field_left_up.SetActive(false);
//             field_left_down.SetActive(false);
//             field_up_right.SetActive(true);
//             for(int i=0;i<astm3_cube_list.Count;i++){
//                 Vector3 now=astm3_cube_list[i].transform.localScale;
//                 astm3_cube_list[i].transform.localScale=new Vector3(now.x,Random.Range(0,8)/10.0f,now.z);
//                 // astm3_cube_list[i].transform.localScale=new Vector3(now.x,astm3_cube_astm3[i],now.z);
//             }
            
//         }
//         else if(analog_angle>-135.0f&&analog_angle<-1.0f){//up_left//one_step
//             stage_number=6;
//             touch_Goal.reset_position();
//             field_up_left.SetActive(true);
//             field_up_right.SetActive(false);
//             field_down_left.SetActive(false);
//             field_down_right.SetActive(false);
//             field_right_up.SetActive(false);
//             field_right_down.SetActive(false);
//             field_left_up.SetActive(false);
//             field_left_down.SetActive(false);
//         }
//         else if(analog_angle>=-180.0f&&analog_angle<-1.0f){//left_up//astm2
//             stage_number=7;
//             touch_Goal.reset_position();
//             field_up_left.SetActive(false);
//             field_up_right.SetActive(false);
//             field_down_left.SetActive(false);
//             field_down_right.SetActive(false);
//             field_right_up.SetActive(false);
//             field_right_down.SetActive(false);
//             field_left_up.SetActive(true);
//             field_left_down.SetActive(false);
//         }

//         // if(Mathf.Abs(up_down)>Mathf.Abs(side)){
//         //     if (up_down > 0f){//↓
//         //         touch_Goal.reset_position();
//         //         field_up.SetActive(false);
//         //         field_down.SetActive(true);
//         //         field_left.SetActive(false);
//         //         field_right.SetActive(false);
//         //         field_up_right.SetActive(false);
//         //     }
//         //     else if (up_down < 0f){//↑
//         //         touch_Goal.reset_position();
//         //         field_up.SetActive(true);
//         //         field_down.SetActive(false);
//         //         field_left.SetActive(false);
//         //         field_right.SetActive(false);
//         //         field_up_right.SetActive(false);
//         // }
//         // }
//         // else if(Mathf.Abs(up_down)<Mathf.Abs(side)){
//         //     //Lstick→←
//         //     if (side > 0f){//→
//         //         touch_Goal.reset_position();
//         //         field_up.SetActive(false);
//         //         field_down.SetActive(false);
//         //         field_left.SetActive(false);
//         //         field_right.SetActive(true);
//         //         field_up_right.SetActive(false);
//         //     }
//         //     else if (side < 0f){//←
//         //         touch_Goal.reset_position();
//         //         field_up.SetActive(false);
//         //         field_down.SetActive(false);
//         //         field_left.SetActive(true);
//         //         field_right.SetActive(false);
//         //         field_up_right.SetActive(false);
//         //     }
//         // }

//         // if(Input.GetKey(KeyCode.Joystick1Button11)){
//         //     touch_Goal.reset_position();
//         //     field_up.SetActive(false);
//         //     field_down.SetActive(false);
//         //     field_left.SetActive(false);
//         //     field_right.SetActive(false);
//         //     for(int i=0;i<astm3_cube_list.Count;i++){
//         //         Vector3 now=astm3_cube_list[i].transform.localScale;
//         //         astm3_cube_list[i].transform.localScale=new Vector3(now.x,Random.Range(0,8)/10.0f,now.z);
//         //     }
//         //     field_up_right.SetActive(true);
//         // }
        
//     }
// }
