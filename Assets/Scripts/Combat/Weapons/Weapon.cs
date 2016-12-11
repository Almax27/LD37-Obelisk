using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour {

    public abstract bool BeginUse(Vector2 direction);
    public abstract bool EndUse();
    public abstract bool CanUse();
}
