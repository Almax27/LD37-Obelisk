using UnityEngine;
using System.Collections;

public class SimpleAnimation : MonoBehaviour 
{

    public bool playFadeAnim = false;
    public float fadeAnimTick = 0;
    public AnimationCurve fadeAnim;

    public bool playRotAnim = false;
    public float rotAnimTick = 0;
    public AnimationCurve rotAnim;

    public bool playScaleAnim = false;
    public float scaleAnimTick = 0;
    public AnimationCurve scaleAnim;

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	    if (playFadeAnim)
        {
            fadeAnimTick += Time.deltaTime;
            Color color = GetComponent<Renderer>().material.color;
            color.a = fadeAnim.Evaluate(fadeAnimTick);
            GetComponent<Renderer>().material.color = color;
        }
        if (playRotAnim)
        {
            rotAnimTick += Time.deltaTime;
            Vector3 eulerAngles = transform.eulerAngles;
            eulerAngles.z = rotAnim.Evaluate(rotAnimTick);
            transform.eulerAngles = eulerAngles;
        }
        if (playScaleAnim)
        {
            scaleAnimTick += Time.deltaTime;
            float scale = scaleAnim.Evaluate(scaleAnimTick);
            transform.localScale = new Vector3(scale,scale,scale);
        }	
    }
}
