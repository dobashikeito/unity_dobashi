//hingejointを扱う
//FrontFlipper用
using UnityEngine;

public class hingejoint : MonoBehaviour
{
    private float angle = 0;
    void Start()
    {
        // var hinge = GetComponent<HingeJoint>();

        // // Make the hinge motor rotate with 90 degrees per second and a strong force.
        // var motor = hinge.motor;
        // motor.force = 100;
        // motor.targetVelocity = 20;
        // motor.freeSpin = false;
        // hinge.motor = motor;
        // hinge.useMotor = true;
        HingeJoint hinge = GetComponent<HingeJoint>();
        JointLimits limits = hinge.limits;
        limits.min = -1;
        limits.max = 1;
        hinge.limits = limits;

        
    }

    void Update(){
        // var hinge = GetComponent<HingeJoint>();
        HingeJoint hinge = GetComponent<HingeJoint>();
        JointLimits limits = hinge.limits;
        // limits.min = 90;
        limits.bounciness = 0;//ジョイントが限界に達したときに跳ね返るサイズを決定
        // limits.max = -90;
        limits.bounceMinVelocity = 0;//跳ね返るときにジョイントが発生する最小速度
        // hinge.limits = limits;//限界値を設定
        hinge.useLimits = true;//ジョイントの制限を有効にする
        // Make the hinge motor rotate with 90 degrees per second and a strong force.
        if(Input.GetKey(KeyCode.Q)){
            var motor = hinge.motor;
            motor.force = 1000;
            motor.targetVelocity = -100;
            motor.freeSpin = false;
            hinge.motor = motor;
            hinge.useMotor = true;
            angle++;
            if(angle>90){
                angle=90;
            }
            limits.min = angle;
            limits.max = angle-1;
            hinge.limits = limits;
        }
        if(Input.GetKey(KeyCode.W)){
            var motor = hinge.motor;
            motor.force = 1000;
            motor.targetVelocity = 100;
            motor.freeSpin = false;
            hinge.motor = motor;
            hinge.useMotor = true;
            angle--;
            if(angle<-90){
                angle=-90;
            }
            limits.min = angle;
            limits.max = angle+1;
            hinge.limits = limits;
        }
        // if(Input.GetKey("up")){
        //     hinge.useMotor=false;
        // }
        
    }
}