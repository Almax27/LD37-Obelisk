using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public delegate void Callback();
    public Callback onObeliskTrigger;

    public float moveSpeed = 0;
    public Hookshot hookshot = null;
    

    // Use this for initialization
    void Start ()
    {
        //HACK
        if (hookshot)
        {
            hookshot = Instantiate(hookshot.gameObject).GetComponent<Hookshot>();
            hookshot.transform.parent = this.transform;
        }

    }

    // Update is called once per frame
    void Update() {

        bool canMove = true;
        if (hookshot)
        {
            if (Input.GetButtonDown("Fire2"))
            {
                var direction = transform.TransformDirection(0, -1, 0);
                var screenPos = Camera.main.WorldToScreenPoint(transform.position);
                direction = Input.mousePosition - screenPos;
                hookshot.Fire(transform, direction);
            }
            if(hookshot.IsInUse())
            {
                canMove = false;
            }
        }

        if (canMove)
        {
            float xMove = Input.GetAxis("Horizontal");
            float yMove = Input.GetAxis("Vertical");
            Vector3 move = new Vector3(xMove, yMove, 0);
            move.Normalize();
            move *= Mathf.Max(xMove, yMove);

            var body = GetComponent<Rigidbody2D>();
            if (body)
            {
                var deltaPos = new Vector2(xMove, yMove) * moveSpeed * Time.fixedDeltaTime;
                body.MovePosition(body.position + deltaPos);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Obelisk"))
        {
            if (onObeliskTrigger != null) onObeliskTrigger();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("Enemy"))
        {
            DamagePacket packet = new DamagePacket();
            packet.value = 1;
            collision.collider.BroadcastMessage("OnDamage", packet);
        }
    }
}
