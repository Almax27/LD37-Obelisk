using UnityEngine;
using System.Collections;

public class AutoDestruct : MonoBehaviour {

    public float delay;
    private float tick;
	
	// Update is called once per frame
	void Update () 
    {
        tick += Time.deltaTime;
        if (tick > delay)
        {
            Destroy(gameObject);
        }
	}
}
