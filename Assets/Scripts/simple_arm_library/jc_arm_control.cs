using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.UrdfImporter;

public class jc_arm_control : MonoBehaviour
{
    // Unity.Robotics.UrdfImporter.Control.controller_arm controller;
    
    // public Unity.Robotics.UrdfImporter.Control.RotationDirection direction;
    public Unity.Robotics.UrdfImporter.Control.ControlType controltype;
    public float speed =30.0f;
    public float torque=100.0f ;
    public float acceleration=5.0f;
    public ArticulationBody joint;
    private int direction=0;
    public ArticulationDrive currentDrive;
    private float flipper_rot_spd = 75f;
    public GameObject controller_arm_go;
    private controller_arm controller_arm;
    public int joint_num;
//private float flipper_rot_spd = 10.45f;


    void Start()
    {
        // direction = 0;
        // controller = (Unity.Robotics.UrdfImporter.Control.Controller_m)this.GetComponentInParent(typeof(Unity.Robotics.UrdfImporter.Control.Controller_m));
        controller_arm=controller_arm_go.GetComponent<controller_arm>();
        joint = this.GetComponent<ArticulationBody>();
        currentDrive = joint.xDrive;
        currentDrive.stiffness = 100000;
        currentDrive.damping = 1000;
        currentDrive.forceLimit = 180;
        currentDrive.upperLimit = 180;
        currentDrive.lowerLimit = -180;
        currentDrive.targetVelocity=100;
        joint.xDrive = currentDrive;
    }

    void FixedUpdate(){
        ArticulationDrive currentDrive = joint.xDrive;
        currentDrive.target=controller_arm.publish_angle[joint_num];
        joint.xDrive = currentDrive;
        // if (joint.jointType != ArticulationJointType.FixedJoint)
        // {
        //     if (controltype == Unity.Robotics.UrdfImporter.Control.ControlType.PositionControl)
        //     {
        //         // ArticulationDrive currentDrive = joint.xDrive;
        //         float newTargetDelta = direction*flipper_rot_spd*0.02f;

        //         if (joint.jointType == ArticulationJointType.RevoluteJoint)
        //         {
        //             if (joint.twistLock == ArticulationDofLock.LimitedMotion)
        //             {
        //                 if (newTargetDelta + currentDrive.target > currentDrive.upperLimit)
        //                 {
        //                     currentDrive.target = currentDrive.upperLimit;
        //                 }
        //                 else if (newTargetDelta + currentDrive.target < currentDrive.lowerLimit)
        //                 {
        //                     currentDrive.target = currentDrive.lowerLimit;
        //                 }
        //                 else
        //                 {
        //                     currentDrive.target += newTargetDelta;
        //                 }
        //             }
        //             else
        //             {
        //                 currentDrive.target += newTargetDelta;
   
        //             }
        //         }

        //         else if (joint.jointType == ArticulationJointType.PrismaticJoint)
        //         {
        //             if (joint.linearLockX == ArticulationDofLock.LimitedMotion)
        //             {
        //                 if (newTargetDelta + currentDrive.target > currentDrive.upperLimit)
        //                 {
        //                     currentDrive.target = currentDrive.upperLimit;
        //                 }
        //                 else if (newTargetDelta + currentDrive.target < currentDrive.lowerLimit)
        //                 {
        //                     currentDrive.target = currentDrive.lowerLimit;
        //                 }
        //                 else
        //                 {
        //                     currentDrive.target += newTargetDelta;
        //                 }
        //             }
        //             else
        //             {
        //                 currentDrive.target += newTargetDelta;
   
        //             }
        //         }

        //         joint.xDrive = currentDrive;

        //     }
        // }
    }
}
