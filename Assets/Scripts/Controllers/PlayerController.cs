using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public delegate void Callback();
    public Callback onObeliskTrigger;

    public enum MoveAttackMode
    {
        MoveAndAttack,
        MoveAfterHit,
        MoveAfterAttack
    }

    public bool lockInput = false;
    public float moveSpeed = 0;
    public float attackDashSpeed = 0;
    public MoveAttackMode moveAttackMode = MoveAttackMode.MoveAfterHit;
    public Weapon weapon = null;
    public Hookshot hookshot = null;
    public Animator bodyAnimator = null;
    public BoxCollider2D attackArea = null;
    public LayerMask attackMask = new LayerMask();
    public AudioClip attackSound = null;

    bool isMoving;
    Vector2 currentFacing = Vector2.zero;
    int directionIndex = 0;
    int hookshotDirectionIndex = 0;
    int attackDirectionIndex = 0;

    bool isAttacking = false;
    bool hasAttackHit = false;
    bool attackPending = false;
    List<GameObject> attackHitCache = new List<GameObject>();

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

    void Awake ()
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

        if(lockInput)
        {
            CancelAttack();
        }

        //------------------------------------------------
        //FACING
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (moveInput.x != 0 || moveInput.y != 0)
        {
            if (moveInput.magnitude > 1)
            {
                moveInput.Normalize();
            }
            directionIndex = DirectionToIndex(moveInput);
            currentFacing = moveInput.normalized;
        }

        //------------------------------------------------
        //COMBAT
        bool isUsingHookshot = false;
        if (hookshot)
        {
            if (!lockInput && Input.GetButtonDown("Fire2"))
            {
                var direction = transform.TransformDirection(0, -1, 0);
                var screenPos = Camera.main.WorldToScreenPoint(transform.position);
                direction = Input.mousePosition - screenPos;
                if (hookshot.Fire(transform, direction))
                {
                    if (bodyAnimator)
                    {
                        CancelAttack();
                        bodyAnimator.SetTrigger("OnHook");
                    }
                    directionIndex = hookshotDirectionIndex = DirectionToIndex(direction);
                }
            }
            bodyAnimator.SetBool("IsHookRetracting", hookshot.state == Hookshot.State.Retracting);
            isUsingHookshot = hookshot.IsInUse();
        }

        bool canAttack = !lockInput && !isAttacking;
        if (!lockInput && Input.GetButtonDown("Fire1"))
        {
            if (canAttack)
            {
                StartAttack();
            }
            else
            {
                attackPending = true;
            }
        }
        else if(canAttack && attackPending)
        {
            StartAttack();
        }
        if (!lockInput && isAttacking && !hasAttackHit)
        {
            //apply damage
            if (attackArea)
            {
                Vector2 size = attackArea.transform.lossyScale;
                size.Scale(attackArea.size);
                Vector2 origin = attackArea.transform.position;
                origin -= size * 0.5f;
                Collider2D[] colliders = Physics2D.OverlapAreaAll(origin, origin + size, attackMask);
                foreach (var col in colliders)
                {
                    DealDamage(col.gameObject);
                }
            }

            //apply attack dash
            if (!isUsingHookshot)
            {
                var body = GetComponent<Rigidbody2D>();
                if (body)
                {
                    var deltaPos = new Vector2(currentFacing.x, currentFacing.y) * attackDashSpeed * Time.deltaTime;
                    body.MovePosition(body.position + deltaPos);
                }
            }
        }

        //------------------------------------------------
        //MOVEMENT
        bool canMove = !lockInput && !isUsingHookshot;
        if (canMove)
        {
            if (moveAttackMode == MoveAttackMode.MoveAfterHit && isAttacking && !hasAttackHit)
            {
                canMove = false;
            }
            else if (moveAttackMode == MoveAttackMode.MoveAfterAttack && isAttacking)
            {
                canMove = false;
            }
        }
        if (canMove)
        {
            var body = GetComponent<Rigidbody2D>();
            if (body && (moveInput.x != 0 || moveInput.y != 0))
            {
                var deltaPos = new Vector2(moveInput.x, moveInput.y) * moveSpeed * Time.deltaTime;
                body.MovePosition(body.position + deltaPos);

                if (!isMoving && !isAttacking)
                {
                    isMoving = true;
                    if (bodyAnimator)
                    {
                        CancelAttack();
                        bodyAnimator.SetTrigger("OnMove");
                    }
                }
            }
            else
            {
                isMoving = false;
            }
        }
        else
        {
            isMoving = false;
        }

        if (bodyAnimator)
        {
            bodyAnimator.SetInteger("Direction", isUsingHookshot ? hookshotDirectionIndex : directionIndex);

            bool canIdle = lockInput || (!isMoving && !isAttacking && !isUsingHookshot);
            bodyAnimator.SetBool("CanIdle", canIdle);
        }
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
        hasAttackHit = true;
    }

    private void OnAttackEnd()
    {
        isAttacking = false;
        if (attackPending)
        {
            if(directionIndex == attackDirectionIndex)
            {
                isAttacking = true;
                attackPending = false;
                OnAttack();
            }
            else
            {
                StartAttack();
            }
        }
        hasAttackHit = false;
        attackHitCache.Clear();
    }

    private bool StartAttack()
    {
        if(!isAttacking)
        {
            isAttacking = true;
            attackPending = false;
            hasAttackHit = false;
            attackDirectionIndex = directionIndex;
            if (bodyAnimator)
            {
                bodyAnimator.SetTrigger("OnAttack");
            }
            OnAttack();
            return true;
        }
        return false;
    }

    private void CancelAttack()
    {
        isAttacking = false;
        attackPending = false;
        hasAttackHit = false;
    }

    private void DealDamage(GameObject obj)
    {
        if (obj && !attackHitCache.Contains(obj))
        {
            attackHitCache.Add(obj);

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

    private void OnAttack()
    {
        FAFAudio.Instance.PlayOnce2D(attackSound, transform.position, 1.0f, 0.2f);
    }
}
