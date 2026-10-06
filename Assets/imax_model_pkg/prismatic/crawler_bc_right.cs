using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.UrdfImporter;

public class crawler_bc_right : MonoBehaviour
{
    Unity.Robotics.UrdfImporter.Control.controller_prismatic controller;
    
    // public Unity.Robotics.UrdfImporter.Control.RotationDirection direction;
    public Unity.Robotics.UrdfImporter.Control.ControlType controltype;
    public float speed =30.0f;
    public float torque=100.0f ;
    public float acceleration=5.0f;
    public ArticulationBody joint;
    private float direction=0;
    private float add_angle=0.01f;
    private float back=-1.0f;//collision消え元の位置に戻るとき
    public List<GameObject> collision_list=new List<GameObject>();
    public bool true_dissappear=false;

    void Start()
    {
        // direction = 0;
        // controller = (Unity.Robotics.UrdfImporter.Control.Controller_m)this.GetComponentInParent(typeof(Unity.Robotics.UrdfImporter.Control.Controller_m));
        joint = this.GetComponent<ArticulationBody>();
        collision_list = get_allchildren.GetAll (gameObject);
        if(true_dissappear){
            back=back*-1.0f;
        }
		// foreach (GameObject obj in list) {
		// 	obj.name = "get";
		// }

    }

    void FixedUpdate(){
        for(int i=0;i<collision_list.Count;i++){
            collision_list[i].SetActive(true_dissappear);
        }
        // direction=0;
        // if(Input.GetAxis("Vertical")>0){
        if(Input.GetAxis("move")>0){
            direction=-1.0f*back*add_angle;
        }
        // else if(Input.GetAxis("Vertical")<0){
        else if(Input.GetAxis("move")<0){
            direction=back*add_angle;
        }
        // else if(Input.GetAxis("Horizontal")>0){
        else if(Input.GetAxis("rotation")>0){
            direction=back*add_angle;
        }
        // else if(Input.GetAxis("Horizontal")<0){
        else if(Input.GetAxis("rotation")<0){
            direction=-1.0f*back*add_angle;
        }
        else {
            direction = 0;
        }

        if (joint.jointType != ArticulationJointType.FixedJoint)
        {
            if (controltype == Unity.Robotics.UrdfImporter.Control.ControlType.PositionControl)
            {
                ArticulationDrive currentDrive = joint.xDrive;
                // float newTargetDelta = direction * Time.fixedDeltaTime * speed;
                float newTargetDelta = direction;

                if (joint.jointType == ArticulationJointType.RevoluteJoint)
                {
                    if (joint.twistLock == ArticulationDofLock.LimitedMotion)
                    {
                        if (newTargetDelta + currentDrive.target > currentDrive.upperLimit)
                        {
                            currentDrive.target = currentDrive.upperLimit;
                            back=back*-1.0f;
                            true_dissappear=!true_dissappear;
                        }
                        else if (newTargetDelta + currentDrive.target < currentDrive.lowerLimit)
                        {
                            currentDrive.target = currentDrive.lowerLimit;
                            back=back*-1.0f;
                            true_dissappear=!true_dissappear;
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
                            back=back*-1.0f;
                            true_dissappear=!true_dissappear;
                        }
                        else if (newTargetDelta + currentDrive.target < currentDrive.lowerLimit)
                        {
                            currentDrive.target = currentDrive.lowerLimit;
                            back=back*-1.0f;
                            true_dissappear=!true_dissappear;
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
