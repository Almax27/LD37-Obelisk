using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {

    public delegate void DamageCallback(DamagePacket packet);
    public DamageCallback onDamage;
    public DamageCallback onDeath;

    public float max = 1;
    public float current = 0;
    public bool destroyOnDeath = false;

    public void Start()
    {
        Reset();
    }

    public void Reset()
    {
        current = max;
    }

    public void OnDamage(DamagePacket packet)
    {
        current -= packet.value;

        if (onDamage != null) { onDamage(packet); }
        if (current <= 0)
        {
            if (onDeath != null) { onDeath(packet); }
            if(destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }
}
