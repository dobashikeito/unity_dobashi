using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.UrdfImporter;

public class imax_body_control_left_ML_leftwall : MonoBehaviour
{
    Unity.Robotics.UrdfImporter.Control.Controller_imax_ML controller;
    
    // public Unity.Robotics.UrdfImporter.Control.RotationDirection direction;
    public Unity.Robotics.UrdfImporter.Control.ControlType controltype;
    public float speed =30.0f;
    public float torque=100.0f ;
    public float acceleration=5.0f;
    public ArticulationBody joint;
    private int direction=0;
    public float action_Go = 0.0f;
    private GameObject agent;
    private urdfmodel_agent urdfmodel_agent;
    private urdfmodel_agent_dp urdfmodel_agent_dp;
    private ufm_agent_several_route ufm_agent_several_route;
    private left_wall_driving_agent left_wall_driving_agent;
    private bool fake_start = false;


    void Start()
    {
        // direction = 0;
        // controller = (Unity.Robotics.UrdfImporter.Control.Controller_m)this.GetComponentInParent(typeof(Unity.Robotics.UrdfImporter.Control.Controller_m));
        joint = this.GetComponent<ArticulationBody>();
        // agent = GameObject.Find("urdfmodel_agent");
        // agent = GetComponentInParent<Transform>();
        // urdfmodel_agent = agent.GetComponent<urdfmodel_agent>();
        // urdfmodel_agent = GetComponentInParent<urdfmodel_agent>();
        // urdfmodel_agent_dp = GetComponentInParent<urdfmodel_agent_dp>();
        // ufm_agent_several_route = GetComponentInParent<ufm_agent_several_route>();
        left_wall_driving_agent = GetComponentInParent<left_wall_driving_agent>();
        fake_start = true;
        
    }

    void FixedUpdate(){
        if(!fake_start){
            fake_start=true;
            joint = this.GetComponent<ArticulationBody>();
            // agent = GameObject.Find("urdfmodel_agent");
            // agent = GetComponentInParent<Transform>();
            // urdfmodel_agent = agent.GetComponent<urdfmodel_agent>();
            // urdfmodel_agent = GetComponentInParent<urdfmodel_agent>();
            // urdfmodel_agent_dp = GetComponentInParent<urdfmodel_agent_dp>();
            // ufm_agent_several_route = GetComponentInParent<ufm_agent_several_route>();
            left_wall_driving_agent = GetComponentInParent<left_wall_driving_agent>();
        }
        // action_Go=urdfmodel_agent.action_Go_Left;
        // action_Go=urdfmodel_agent_dp.action_Go_Left;
        // action_Go=ufm_agent_several_route.action_Go_Left;
        action_Go=left_wall_driving_agent.action_Go_Left;
        if (joint.jointType != ArticulationJointType.FixedJoint)
        {
            if (controltype == Unity.Robotics.UrdfImporter.Control.ControlType.PositionControl)
            {
                ArticulationDrive currentDrive = joint.xDrive;
                float newTargetDelta = action_Go * Time.fixedDeltaTime * speed;

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
