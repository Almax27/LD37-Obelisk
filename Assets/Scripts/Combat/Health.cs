using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {

    public delegate void DamageCallback(DamagePacket packet);
    public DamageCallback onDamage;
    public DamageCallback onDeath;

    [Header("Config")]
    public float max = 1;
    public float current = 0;
    public bool destroyOnDeath = false;
    public float deathKnockbackScale = 5.0f;

    [Header("Visualisation")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.0f;

    DamagePacket lastDamagePacket = null;

    float knockbackTick = float.MaxValue;
    Vector2 knockbackOrigin;
    Vector2 knockbackTarget;

    float flashTick = float.MaxValue;

    public bool IsDead()
    {
        return current <= 0;
    }

    public void Start()
    {
        Reset();
    }

    public void FixedUpdate()
    {
        float knockbackTime = 0.2f;
        if(knockbackTick < knockbackTime)
        {
            knockbackTick += Time.fixedDeltaTime;
            float t = knockbackTick / knockbackTime;

            Vector2 dir = knockbackTarget - knockbackOrigin;
            if(IsDead())
            {
                dir *= deathKnockbackScale;
            }
            float dist = dir.magnitude;

            var position = Vector2.Lerp(knockbackOrigin, knockbackOrigin + dir, Easing.Ease01(t, Easing.Method.QuadOut));
            if(t < 0.5f)
            {
                position += new Vector2(0, dist*0.2f) * Easing.Ease01(t * 2.0f, Easing.Method.QuadOut);
            }
            else
            {
                position += new Vector2(0, dist*0.2f) * Easing.Ease01((1.0f - t) * 2.0f, Easing.Method.BounceIn);
            }

            var body = GetComponent<Rigidbody2D>();
            if(body)
            {
                body.MovePosition(position);
            }

            if(knockbackTick >= knockbackTime)
            {//end of knockback
                if (IsDead())
                {
                    if (current <= 0)
                    {
                        if (onDeath != null) { onDeath(lastDamagePacket); }
                    }
                    if (destroyOnDeath)
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }

        
    }

    private void Update()
    {
        if (flashDuration > 0)
        {
            if (flashTick < flashDuration)
            {
                flashTick += Time.deltaTime;
            }
            float t = Mathf.Clamp01(flashTick / flashDuration);
            var renderers = GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                renderer.material.color = Color.Lerp(flashColor, Color.white, t);
            }
        }
    }

    public void Reset()
    {
        current = max;
    }

    public void OnDamage(DamagePacket packet)
    {
        if(IsDead())
        {
            return;
        }

        current -= packet.value;

        //start knockback
        knockbackTick = 0;
        knockbackOrigin = transform.position;
        knockbackTarget = knockbackOrigin + packet.direction.normalized * packet.knockback;

        lastDamagePacket = packet;

        flashTick = 0;

    }
}
