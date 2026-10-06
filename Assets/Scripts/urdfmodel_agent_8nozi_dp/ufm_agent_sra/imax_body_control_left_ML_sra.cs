//x→いじらない、何もなし→場合によって変える
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.UrdfImporter;

public class imax_body_control_left_ML_sra : MonoBehaviour
{
    Unity.Robotics.UrdfImporter.Control.Controller_imax_ML controller;
    
    // public Unity.Robotics.UrdfImporter.Control.RotationDirection direction;
    public Unity.Robotics.UrdfImporter.Control.ControlType controltype;
    public float speed =30.0f;// ここで、スピード調整可能だが、エージェントファイルから与えられる値で調整も可能。
    public float torque=100.0f ;// x
    public float acceleration=5.0f;// x
    public ArticulationBody joint;// x
    private int direction=0;// x
    public float action_Go = 0.0f;// x
    private GameObject agent;// x
    private urdfmodel_agent urdfmodel_agent;// x
    private urdfmodel_agent_dp urdfmodel_agent_dp;// x
    private ufm_agent_sra ufm_agent_sra;// x
    private bool fake_start = false;// x　？


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
        ufm_agent_sra = GetComponentInParent<ufm_agent_sra>();//親オブジェクトである、エージェントファイルがアタッチされたgameobjectのスクリプトを取ってくる処理。これがあるので、エージェントファイルが変わるたびに、体を動かすファイルだけ生成しなければならない。仕様変更したいところではあるが、これが一番手っ取り早い。

        fake_start = true;
        
    }

    void FixedUpdate(){
        if(!fake_start){//？
            fake_start=true;
            joint = this.GetComponent<ArticulationBody>();
            // agent = GameObject.Find("urdfmodel_agent");
            // agent = GetComponentInParent<Transform>();
            // urdfmodel_agent = agent.GetComponent<urdfmodel_agent>();
            // urdfmodel_agent = GetComponentInParent<urdfmodel_agent>();
            // urdfmodel_agent_dp = GetComponentInParent<urdfmodel_agent_dp>();
            ufm_agent_sra = GetComponentInParent<ufm_agent_sra>();
        }
        // action_Go=urdfmodel_agent.action_Go_Left;
        // action_Go=urdfmodel_agent_dp.action_Go_Left;
        action_Go=ufm_agent_sra.action_Go_Left;
        if (joint.jointType != ArticulationJointType.FixedJoint)
        {
            if (controltype == Unity.Robotics.UrdfImporter.Control.ControlType.PositionControl)
            {
                ArticulationDrive currentDrive = joint.xDrive;
                float newTargetDelta = action_Go * Time.fixedDeltaTime * speed;//ここで設定された値分、角度を変えてロボットを動かす。

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
                        currentDrive.target += newTargetDelta;//ここで実際に角度を変えてロボットを動かす。
   
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
