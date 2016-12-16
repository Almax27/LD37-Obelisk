using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeEnemyController : MonoBehaviour {

    [System.Serializable]
    public class MinMax
    {
        public float min = 0;
        public float max = 0;
        public float RandomInRange()
        {
            return Random.Range(min, max);
        }
    }
    [Header("Config")]
    public MinMax attackDelay = new MinMax();
    public MinMax attackDuration = new MinMax();
    public LayerMask attackMask = new LayerMask();
    public float moveSpeed = 0;
    public float stunTimeOnHit = 0;

    [Header("References")]
    public Animator bodyAnimator = null;
    public CircleCollider2D attackArea = null;
    public Transform moveTargetPoints = null;

    bool isAttacking = false;
    float attackTick = 0;
    float nextAttackDelay = 0;
    float nextAttackDuration = 0;
    List<Collider2D> hitCache = new List<Collider2D>();

    Transform moveTarget = null;
    Vector2 moveVelocity = Vector2.zero;

    int directionIndex = 0;
    float stunOnHitTick = float.MaxValue;

    int DirectionToIndex(Vector2 direction)
    {
        Vector2 ndir = direction.normalized;
        if (Mathf.Abs(ndir.y) > 0.8)
        {
            if (ndir.y > 0)
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
        nextAttackDelay = attackDelay.RandomInRange();
        PickMovementPoint();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(stunOnHitTick < stunTimeOnHit)
        {
            stunOnHitTick += Time.deltaTime;
            return;
        }

        attackTick += Time.deltaTime;
        if (isAttacking)
        {
            ApplyDamage();
            if(attackTick > nextAttackDuration)
            {
                StopAtacking();
            }
        }
        else
        {
            if (moveTarget)
            {
                var pos = Vector2.SmoothDamp(transform.position, moveTarget.position, ref moveVelocity, 0.2f, moveSpeed, Time.deltaTime);
                Rigidbody2D body = GetComponent<Rigidbody2D>();
                if (body && body.simulated)
                {
                    body.MovePosition(pos);
                }
                if(moveVelocity.magnitude > 0)
                {
                    directionIndex = DirectionToIndex(moveVelocity);
                }
            }
            if (attackTick > nextAttackDelay)
            {
                moveVelocity = Vector2.zero;
                StartAttacking();
            }
        }

        if(bodyAnimator)
        {
            bodyAnimator.SetBool("IsAttacking", isAttacking);
            bodyAnimator.SetInteger("Direction", directionIndex);
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

    void StartAttacking()
    {
        attackTick = 0;
        isAttacking = true;
        nextAttackDuration = attackDuration.RandomInRange();
    }

    void StopAtacking()
    {
        attackTick = 0;
        isAttacking = false;
        nextAttackDelay = attackDelay.RandomInRange();
        directionIndex = 0;
        hitCache.Clear();
        PickMovementPoint();
    }

    void PickMovementPoint()
    {
        moveTarget = null;
        if (moveTargetPoints)
        {
            var points = moveTargetPoints.GetComponentsInChildren<Transform>();
            if (points.Length > 0)
            {
                moveTarget = points[Random.Range(0, points.Length - 1)];
            }
        }
    }

    void ApplyDamage()
    {
        if (attackArea)
        {
            var colliders = Physics2D.OverlapCircleAll(attackArea.transform.position, attackArea.radius, attackMask);
            foreach (var col in colliders)
            {
                if (!hitCache.Contains(col))
                {
                    DamagePacket packet = new DamagePacket();
                    packet.value = 1;
                    packet.direction = col.transform.position - attackArea.transform.position;
                    packet.knockback = 10;
                    col.SendMessageUpwards("OnDamage", packet, SendMessageOptions.DontRequireReceiver);
                    hitCache.Add(col);
                }
            }
        }
    }

    void OnDamage(DamagePacket packet)
    {
        StopAtacking();
        stunOnHitTick = 0;
    }

    void OnSpawn(Transform spawnPoint)
    {
        if (spawnPoint)
        {
            moveTargetPoints = spawnPoint;
        } 
    }
}
