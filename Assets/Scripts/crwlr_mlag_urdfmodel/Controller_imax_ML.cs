using System;
using Unity.Robotics;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Unity.Robotics.UrdfImporter.Control
{
    // public enum RotationDirection { None = 0, Positive = 1, Negative = -1 };
    // public enum ControlType { PositionControl };

    public class Controller_imax_ML : MonoBehaviour
    {
        private ArticulationBody[] articulationChain;
        // Stores original colors of the part being highlighted
        private Color[] prevColor;
        private int previousIndex;

        [InspectorReadOnly(hideInEditMode: true)]
        public string selectedJoint;
        [HideInInspector]
        public int selectedIndex;
    //  初期値、指定
        // public ControlType control = ControlType.PositionControl;
        public float stiffness=10000;
        public float damping=1000;
        public float forceLimit=131;
        public float speed = 5f; // Units: degree/s
        public float torque = 100f; // Units: Nm or N
        public float acceleration = 5f;// Units: m/s^2 / degree/s^2 

        [Tooltip("Color to highlight the currently selected join")]
        public Color highLightColor = new Color(1.0f, 0, 0, 1.0f);

        public void Start()
        {//初期のjoint指定、jointはリストのインデックスで指定される。
            previousIndex = selectedIndex = 1;
            // this.gameObject.AddComponent<FKRobot>();//FKRobotは謎
            articulationChain = this.GetComponentsInChildren<ArticulationBody>();//子オブジェクトのArticulationBodyを取得し、リスト化
            int defDyanmicVal = 10;
            int i=0;
            foreach (ArticulationBody joint in articulationChain)//取得した子オブジェクトのリストそれぞれにJoyControlのスクリプトを追加
            {
                // joint.gameObject.AddComponent<JoyControl_self>();
                // joint.gameObject.GetComponent<JoyControl_self>();
                joint.gameObject.GetComponent<imax_body_control_left_ML>();
                if (joint == null){
                    joint.gameObject.GetComponent<imax_body_control_right_ML>();
                    if (joint == null){
                        joint.gameObject.GetComponent<imax_flipper_control_fl_ML>();
                        if (joint == null){
                            joint.gameObject.GetComponent<imax_flipper_control_fr_ML>();
                            if (joint == null){
                                joint.gameObject.GetComponent<imax_flipper_control_rl_ML>();
                                if (joint == null){
                                    joint.gameObject.GetComponent<imax_flipper_control_rr_ML>();
                                }
                            }
                        }
                        //フリッパのxDrive調整
                        joint.useGravity = true;
                        joint.jointFriction = defDyanmicVal;
                        joint.angularDamping = defDyanmicVal;
                        i++;
                        break;
                    }
                }
                //ギアのxDrive調整
                joint.useGravity = true;
                joint.jointFriction = defDyanmicVal;
                joint.angularDamping = defDyanmicVal;
                ArticulationDrive currentDrive = joint.xDrive;
                currentDrive.stiffness = 100000;
                currentDrive.damping = 1000;
                currentDrive.forceLimit = forceLimit;
                joint.xDrive = currentDrive;
                i++;
                
            }
            // Debug.LogWarning(i);
            // DisplaySelectedJoint(selectedIndex);//Game画面にセレクトしているオブジェクト名を表示？
            // StoreJointColors(selectedIndex);//セレクトされているものの色を変更
        }

        void Update()
        {
            // bool SelectionInput1 = Input.GetKeyDown("right");
            // bool SelectionInput2 = Input.GetKeyDown("left");
            // //キーボードの横で、selectedIndexを変更するプログラム↓
            // SetSelectedJointIndex(selectedIndex); // to make sure it is in the valid range//indexが適切な値を超えた時の処理
            // UpdateDirection(selectedIndex);//selectしたもののモーターを動かす

            // if (SelectionInput2)
            // {
            //     SetSelectedJointIndex(selectedIndex - 1);
            //     Highlight(selectedIndex);
            // }
            // else if (SelectionInput1)
            // {
            //     SetSelectedJointIndex(selectedIndex + 1);
            //     Highlight(selectedIndex);
            // }
            // //キーボードの横で、selectedIndexを変更するプログラム↑
            // UpdateDirection(selectedIndex);//selectしたもののモーターを動かす
        }

        /// <summary>
        /// Highlights the color of the robot by changing the color of the part to a color set by the user in the inspector window
        /// </summary>
        /// <param name="selectedIndex">Index of the link selected in the Articulation Chain</param>

    }
}
