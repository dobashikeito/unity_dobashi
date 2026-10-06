using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class click_create_cp : MonoBehaviour
{
    // Start is called before the first frame update
    public Camera cam;
    private Vector3 plane_normal;
    private Vector3 plane_next;
    private Vector3 plane_past;
    private Vector3 click_point_past;
    private Vector3 click_point_next;
    private Vector3 zero_axis;
    private float angle;
    void Start()
    {
        plane_normal=Vector3.up;
        zero_axis=new Vector3(0.0f,0.0f,1.0f);
        click_point_past =this.transform.position;
        plane_past=Vector3.ProjectOnPlane(zero_axis,plane_normal);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
             RaycastHit hit;
             Ray ray = cam.ScreenPointToRay(Input.mousePosition);
             if (Physics.Raycast(ray, out hit))
                {
                    GameObject Cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    click_point_next = new Vector3 (hit.point.x, 0.0f, hit.point.z);
                    Vector3 vec = click_point_next - click_point_past;
                    
                    plane_next=Vector3.ProjectOnPlane(vec,plane_normal);
                    angle = Vector3.SignedAngle(plane_next,zero_axis,plane_normal);
                    
                    Cube.transform.localScale = new Vector3(1.5f,1.0f,0.01f);
                    Cube.transform.position = click_point_next;
                    // Debug.LogError(click_point_past);
                    // Debug.LogError(click_point_next);
                    // Debug.LogWarning(vec);
                    // Debug.Log(-1.0f*angle);
                    Cube.transform.localEulerAngles = new Vector3(0.0f,-1.0f*angle,0.0f);
                    plane_past = plane_next;
                    click_point_past = click_point_next;

                }
        }
    }
}
