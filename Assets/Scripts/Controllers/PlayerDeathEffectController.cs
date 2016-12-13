using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeathEffectController : MonoBehaviour {

    public float moveTime = 1.0f;
    public Vector2 targetPos = Vector2.zero;

    float tick = 0;
    Vector2 originPos;

	void Start () {
        originPos = transform.position;
    }
	
	void Update () {
	    if(tick < moveTime)
        {
            tick = Mathf.Min(tick + Time.deltaTime, moveTime);
            transform.position = Easing.Ease(tick, originPos, targetPos, moveTime, Easing.Method.QuadInOut);
        }
	}
}
