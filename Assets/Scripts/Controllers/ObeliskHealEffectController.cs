using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObeliskHealEffectController : MonoBehaviour {

    public Easing.Helper moveEasing = new Easing.Helper();
    public bool moveToPlayer = true;

    Vector2 originPosition;
    
    // Use this for initialization
    void Start ()
    {
        originPosition = this.transform.position;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (moveToPlayer)
        {
            var player = FindObjectOfType<PlayerController>();
            if (player)
            {
                Vector3 position = player.transform.position;
                moveEasing.Update(Time.deltaTime, originPosition, player.transform.position, out position);
                transform.position = position;
            }
        }
	}
}
