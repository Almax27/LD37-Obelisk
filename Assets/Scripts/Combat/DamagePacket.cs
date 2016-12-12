using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class DamagePacket
{
    public float value = 0;
    public Vector2 direction;
    public float knockback = 0;
    public AudioClip hitSound;
}
