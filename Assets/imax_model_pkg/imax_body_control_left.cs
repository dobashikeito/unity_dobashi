using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.UrdfImporter;

public class imax_body_control_left : MonoBehaviour
{
    Unity.Robotics.UrdfImporter.Control.Controller_imax controller;
    
    // public Unity.Robotics.UrdfImporter.Control.RotationDirection direction;
    public Unity.Robotics.UrdfImporter.Control.ControlType controltype;
    public float speed =30.0f;
    public float torque=100.0f ;
    public float acceleration=5.0f;
    public ArticulationBody joint;
    private int direction=0;
    private GameObject ps5;
    public imax_ps5 imax_ps5;
    public string robot_name;
    private int add_angle=10;


    void Start()
    {
        // direction = 0;
        // controller = (Unity.Robotics.UrdfImporter.Control.Controller_m)this.GetComponentInParent(typeof(Unity.Robotics.UrdfImporter.Control.Controller_m));
        ps5 = GameObject.Find(robot_name);
        imax_ps5 = ps5.GetComponent<imax_ps5>();
        joint = this.GetComponent<ArticulationBody>();

    }

    void FixedUpdate(){
        // direction=0;
        if(Input.GetAxis("Vertical")>0){
            direction=add_angle;
        }
        else if(Input.GetAxis("Vertical")<0){
            direction=-1*add_angle;
        }
        else if(Input.GetAxis("Horizontal")>0){
            direction=add_angle;
        }
        else if(Input.GetAxis("Horizontal")<0){
            direction=-1*add_angle;
        }
        else if(imax_ps5.Lstick>0||imax_ps5.Rstick>0){
            direction=add_angle;
        }
        else if(imax_ps5.Lstick<0||imax_ps5.Rstick<0){
            direction=-1*add_angle;
        }
        else {
            direction = 0;
        }

        if (joint.jointType != ArticulationJointType.FixedJoint)
        {
            if (controltype == Unity.Robotics.UrdfImporter.Control.ControlType.PositionControl)
            {
                ArticulationDrive currentDrive = joint.xDrive;
                float newTargetDelta = direction * Time.fixedDeltaTime * speed;

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
