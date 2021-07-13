using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{

    public GameObject[] Joints = new GameObject[8];

    //rotate on y-axis for base
    [Range(0.0f, 360.0f)]
    public float BaseAngle = 0.0f;
    
    //rotate on x-axis for shoulderjoints
    [Range(0.0f, 159.0f)]
    public float ShoulderAngle = 0.0f;

    //rotate on x-axis for joint2
    [Range(0.0f, -80.0f)]
    public float ElbowAngle = 0.0f;

    //rotate on z-axis for forearmjoint
    [Range(-90f, 90f)]
    public float ForearmRotation = -90.0f;

    //transform position on x-axis for grippers
    [Range(0.0f,10.0f)]
    public float Gripper = 10.0f;

    //rotate PivotBase on y-axis
    [Range(-260.0f,85.0f)]
    public float CameraPivotBaseRotation = -90.0f;

    //pitch CameraBox on the x-axis
    [Range(-35.0f,35.0f)]
    public float CameraBoxPitch = 0.0f;


    private float calcGripperLeft()
    {
        return (-0.4333f/10.0f)*Gripper - 0.1f;
    }

    private float calcGripperRight()
    {
        return (0.4333f/10.0f)*Gripper + 0.1f;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Joints[0].transform.localRotation = Quaternion.Euler(0.0f,BaseAngle,0.0f);
        Joints[1].transform.localRotation = Quaternion.Euler(ShoulderAngle,0.0f,0.0f);
        Joints[2].transform.localRotation = Quaternion.Euler(ElbowAngle,0.0f,-90.0f);
        Joints[3].transform.localRotation = Quaternion.Euler(0.0f,ForearmRotation,-180);
        //Right Gripper
        Joints[4].transform.localPosition = new Vector3(calcGripperRight(),-2.58f,0.0f);
        //left Gripper
        Joints[5].transform.localPosition = new Vector3(calcGripperLeft(),-2.58f,0.0f);
        Joints[6].transform.localRotation = Quaternion.Euler(0.0f,CameraPivotBaseRotation,0.0f);
        Joints[7].transform.localRotation = Quaternion.Euler(0.0f,CameraBoxPitch,90.0f);
    }
}
