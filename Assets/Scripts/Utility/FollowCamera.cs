using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {

    public float dampTime = 0.3f; //offset from the viewport center to fix damping
    public Vector2 deadZone = new Vector2(0.2f,0.2f);
    public Transform target;
    public Vector2 targetOffset = Vector2.zero;

    private Vector3 velocity = Vector3.zero;
    private Vector3 destination = Vector3.zero;

	// Use this for initialization
	void Start () 
    {
        destination = transform.position;
	}
	
	// Update is called once per frame
	void LateUpdate () 
    {
        if(target) 
        {
            Vector3 point = GetComponent<Camera>().WorldToViewportPoint(target.position);
            Rect deadArea = new Rect((0.5f - deadZone.x*0.5f), (0.5f - deadZone.y*0.5f), deadZone.x, deadZone.y);

            destination = target.position;

            if(targetOffset != Vector2.zero)
            {
                destination += (Vector3)targetOffset;
            }
            else if(!deadArea.Contains(point))
            {
                destination += transform.position - GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
            }


            if(transform.position != destination)
            {
                destination.z = transform.position.z; //lock z
                transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
            }
        }
	}
}