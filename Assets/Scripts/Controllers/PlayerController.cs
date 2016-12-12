using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public delegate void Callback();
    public Callback onObeliskTrigger;

    public bool lockInput = false;
    public float moveSpeed = 0;
    public Weapon weapon = null;
    public Hookshot hookshot = null;
    public Animator bodyAnimator = null;
    public BoxCollider2D attackArea = null;
    public LayerMask attackMask = new LayerMask();

    bool isMoving;
    int directionIndex = 0;
    bool isAttacking = false;
    bool attackPending = false;

    int DirectionToIndex(Vector2 direction)
    {
        Vector2 ndir = direction.normalized;
        if(Mathf.Abs(ndir.y) > 0.8)
        {
            if(ndir.y > 0)
            {
                return 0; //up
            }
            else
            {
                return 1; //down
            }
        }
        else
        {
            if (ndir.x > 0)
            {
                return 2; //right
            }
            else
            {
                return 3; //left
            }
        }
    }

    // Use this for initialization
    void Start ()
    {
        //HACK
        if (hookshot)
        {
            hookshot = Instantiate(hookshot.gameObject).GetComponent<Hookshot>();
            hookshot.transform.parent = this.transform;
            hookshot.transform.localPosition = Vector3.zero;
        }
    }

    // Update is called once per frame
    void Update() {

        Health health = GetComponent<Health>();
        if(health && health.IsDead())
        {
            lockInput = true;
        }

        bool canMove = !lockInput && !isAttacking;
        bool canAttack = !lockInput && !isAttacking;
        bool canHook = !lockInput;
        bool didMove = false;
        bool startedAttack = false;
        bool usedHookShot = false;
        
        if (canHook && hookshot)
        {
            if (Input.GetButtonDown("Fire2"))
            {
                var direction = transform.TransformDirection(0, -1, 0);
                var screenPos = Camera.main.WorldToScreenPoint(transform.position);
                direction = Input.mousePosition - screenPos;
                if (hookshot.Fire(transform, direction))
                {
                    usedHookShot = true;
                    directionIndex = DirectionToIndex(direction);
                }
            }
            if(hookshot.IsInUse())
            {
                canMove = false;
            }
        }

        if (Input.GetButtonDown("Fire1"))
        {
            if (canAttack)
            {
                isAttacking = true;
                startedAttack = true;
                attackPending = false;
            }
            else
            {
                attackPending = true;
            }
        }

        Vector3 facing = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        if (facing.x != 0 || facing.y != 0)
        {
            
            if (facing.magnitude > 1)
            {
                facing.Normalize();
            }
            directionIndex = DirectionToIndex(facing);
        }

        if (canMove)
        {
            var body = GetComponent<Rigidbody2D>();
            if (body && (facing.x != 0 || facing.y != 0))
            {
                var deltaPos = new Vector2(facing.x, facing.y) * moveSpeed * Time.fixedDeltaTime;
                body.MovePosition(body.position + deltaPos);
                didMove = true;
            }
        }

        bool canIdle = lockInput || (canMove && !didMove && !startedAttack && !usedHookShot);
        if (bodyAnimator)
        {
            bodyAnimator.SetInteger("Direction", directionIndex);
            if (!isMoving && didMove)
            {
                CancelAttack();
                bodyAnimator.SetTrigger("OnMove");
            }
            if (startedAttack)
            {
                bodyAnimator.SetTrigger("OnAttack");
            }
            if (usedHookShot)
            {
                CancelAttack();
                bodyAnimator.SetTrigger("OnHook");
            }
            bodyAnimator.SetBool("IsHookRetracting", hookshot && hookshot.state == Hookshot.State.Retracting);
            bodyAnimator.SetBool("CanIdle", canIdle);
        }

        isMoving = didMove;
    }

    private void FixedUpdate()
    {
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if(body)
        {
            body.velocity = Vector2.zero;
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Obelisk"))
        {
            if (onObeliskTrigger != null) onObeliskTrigger();
        }
    }

    private void OnAttackHit()
    {
        //apply damage
        Vector2 size = attackArea.transform.lossyScale;
        size.Scale(attackArea.size);
        Vector2 origin = attackArea.transform.position;
        origin -= size * 0.5f;
        Collider2D[] colliders = Physics2D.OverlapAreaAll(origin, origin + size, attackMask);
        foreach(var col in colliders)
        {
            DealDamage(col.gameObject);
        }
    }

    private void OnAttackEnd()
    {
        if (attackPending)
        {
            attackPending = false;
            isAttacking = true;
        }
        else
        {
            isAttacking = false;
        }
    }

    private void CancelAttack()
    {
        isAttacking = false;
        attackPending = false;
    }

    private void DealDamage(GameObject obj)
    {
        if (obj)
        {
            DamagePacket packet = new DamagePacket();
            packet.value = 1;
            packet.direction = obj.transform.position - transform.position;
            packet.knockback = 10;
            obj.SendMessageUpwards("OnDamage", packet, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnIdling()
    {
        
    }

    private void OnDamage(DamagePacket packet)
    {
        CancelAttack();
    }
}
