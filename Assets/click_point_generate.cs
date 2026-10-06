using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class click_point_generate : MonoBehaviour
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
    public List<GameObject> go_list = new List<GameObject>();
    public List<Vector3> tf_list = new List<Vector3>();
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
                    Cube.transform.parent=this.transform;
                    go_list.Add(Cube);
                    Cube.transform.name = "click_point"+go_list.Count;
                    click_point_next = new Vector3 (hit.point.x,hit.point.y, hit.point.z);
                    tf_list.Add(click_point_next);
                    Vector3 vec = click_point_next - click_point_past;
                    
                    plane_next=Vector3.ProjectOnPlane(vec,plane_normal);
                    angle = Vector3.SignedAngle(plane_next,zero_axis,plane_normal);
                    
                    Cube.transform.localScale = new Vector3(1.5f,1.0f,0.01f);
                    Cube.transform.position = click_point_next;
                    Cube.transform.localEulerAngles = new Vector3(0.0f,-1.0f*angle,0.0f);
                    plane_past = plane_next;
                    click_point_past = click_point_next;

                }
        }
    }
}
