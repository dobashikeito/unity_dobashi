//x→いじらない、何もなし→場合によって変える
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.UrdfImporter;

public class imax_flipper_control_rr_ML : MonoBehaviour
{
    Unity.Robotics.UrdfImporter.Control.Controller_imax_ML controller;// x
    // public Unity.Robotics.UrdfImporter.Control.RotationDirection direction;
    public Unity.Robotics.UrdfImporter.Control.ControlType controltype;// x
    public float speed =30.0f;// x
    public float torque=100.0f;// x
    public float acceleration=5.0f;// x
    public ArticulationBody joint;// x
    private int direction=0;// x
    public ArticulationDrive currentDrive;// x
    private float flipper_rot_spd = 75f;// フリッパ速度を変更可能 deg/sec
    public float action_RearR = 0.0f;// x
//private float flipper_rot_spd = 10.45f;


    public void Start()
    {
        // トルク情報など、モータ情報のイメージ。基本いじらない。
        joint = this.GetComponent<ArticulationBody>();
        currentDrive = joint.xDrive;
        currentDrive.stiffness = 100000;
        currentDrive.damping = 1000;
        currentDrive.forceLimit = 1000;
        currentDrive.upperLimit = 180;
        // currentDrive.upperLimit = 135;
        currentDrive.lowerLimit = 0;
        // currentDrive.target = 90;//defaultだと0度で真上に向く。初期値を変えたければ、コメントイン
        joint.xDrive = currentDrive;

    }

    void FixedUpdate(){
        if (joint.jointType != ArticulationJointType.FixedJoint)
        {
            if (controltype == Unity.Robotics.UrdfImporter.Control.ControlType.PositionControl)
            {
                // ArticulationDrive currentDrive = joint.xDrive;
                float newTargetDelta = action_RearR*flipper_rot_spd*-0.02f;
                // float newTargetDelta = action_RearR*-0.96f;//実機の値

                if (joint.jointType == ArticulationJointType.RevoluteJoint)
                {
                    if (joint.twistLock == ArticulationDofLock.LimitedMotion)
                    {
                        if (newTargetDelta + currentDrive.target > currentDrive.upperLimit)
                        {
                            currentDrive.target = currentDrive.upperLimit;
                        }
                        else if (newTargetDelta + currentDrive.target < currentDrive.lowerLimit)
                        {
                            currentDrive.target = currentDrive.lowerLimit;
                        }
                        else
                        {
                            currentDrive.target += newTargetDelta;
                        }
                    }
                    else
                    {
                        currentDrive.target += newTargetDelta;
   
                    }
                }

                else if (joint.jointType == ArticulationJointType.PrismaticJoint)
                {
                    if (joint.linearLockX == ArticulationDofLock.LimitedMotion)
                    {
                        if (newTargetDelta + currentDrive.target > currentDrive.upperLimit)
                        {
                            currentDrive.target = currentDrive.upperLimit;
                        }
                        else if (newTargetDelta + currentDrive.target < currentDrive.lowerLimit)
                        {
                            currentDrive.target = currentDrive.lowerLimit;
                        }
                        else
                        {
                            currentDrive.target += newTargetDelta;
                        }
                    }
                    else
                    {
                        currentDrive.target += newTargetDelta;
   
                    }
                }

                joint.xDrive = currentDrive;

            }
        }
    }
}
