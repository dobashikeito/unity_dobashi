using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.UrdfImporter;

public class imax_flipper_control_fr_ML : MonoBehaviour
{
    Unity.Robotics.UrdfImporter.Control.Controller_imax_ML controller;
    
    // public Unity.Robotics.UrdfImporter.Control.RotationDirection direction;
    public Unity.Robotics.UrdfImporter.Control.ControlType controltype;
    public float speed =30.0f;
    public float torque=100.0f ;
    public float acceleration=5.0f;
    public ArticulationBody joint;
    private int direction=0;
    public ArticulationDrive currentDrive;
    private float flipper_rot_spd = 75f;
    public float action_FrontR = 0.0f;

//private float flipper_rot_spd = 10.45f;


    public void Start()
    {
        // direction = 0;
        // controller = (Unity.Robotics.UrdfImporter.Control.Controller_m)this.GetComponentInParent(typeof(Unity.Robotics.UrdfImporter.Control.Controller_m));
        joint = this.GetComponent<ArticulationBody>();
        currentDrive = joint.xDrive;
        currentDrive.stiffness = 100000;
        currentDrive.damping = 1000;
        currentDrive.forceLimit = 1000;
        currentDrive.upperLimit = 0;
        currentDrive.lowerLimit = -180;
        // currentDrive.target = -45;
        // currentDrive.lowerLimit = -135;
        joint.xDrive = currentDrive;
    }

    void FixedUpdate(){

        if (joint.jointType != ArticulationJointType.FixedJoint)
        {
            if (controltype == Unity.Robotics.UrdfImporter.Control.ControlType.PositionControl)
            {
                // ArticulationDrive currentDrive = joint.xDrive;
                float newTargetDelta = action_FrontR*flipper_rot_spd*0.02f;
                // float newTargetDelta = action_FrontR*0.96f;

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
