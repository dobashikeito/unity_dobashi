using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.UrdfImporter;

public class imax_flipper_control_rr : MonoBehaviour
{
    Unity.Robotics.UrdfImporter.Control.Controller_imax controller;
    
    // public Unity.Robotics.UrdfImporter.Control.RotationDirection direction;
    public Unity.Robotics.UrdfImporter.Control.ControlType controltype;
    public float speed =30.0f;
    public float torque=100.0f ;
    public float acceleration=5.0f;
    public ArticulationBody joint;
    private int direction=0;
    public ArticulationDrive currentDrive;
    private float flipper_rot_spd = 75f;
    private GameObject ps5;
    public imax_ps5 imax_ps5;
    public string robot_name;
//private float flipper_rot_spd = 10.45f;


    void Start()
    {
        ps5 = GameObject.Find(robot_name);
        imax_ps5 = ps5.GetComponent<imax_ps5>();
        joint = this.GetComponent<ArticulationBody>();
        currentDrive = joint.xDrive;
        currentDrive.stiffness = 100000;
        currentDrive.damping = 1000;
        currentDrive.forceLimit = 100;
        currentDrive.upperLimit = 180;
        currentDrive.lowerLimit = 0;
        joint.xDrive = currentDrive;
    }

    void FixedUpdate(){
        // direction=0;
        if(Input.GetKey(KeyCode.Q)){
            direction=1;
        }
        else if(Input.GetKey(KeyCode.W)){
            direction=-1;
        }
        else if(imax_ps5.tri_cross!=0){
            direction=imax_ps5.tri_cross;
        }
        else{
            direction=0;
        }
        if (joint.jointType != ArticulationJointType.FixedJoint)
        {
            if (controltype == Unity.Robotics.UrdfImporter.Control.ControlType.PositionControl)
            {
                // ArticulationDrive currentDrive = joint.xDrive;
                float newTargetDelta = direction*flipper_rot_spd*0.02f;

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
