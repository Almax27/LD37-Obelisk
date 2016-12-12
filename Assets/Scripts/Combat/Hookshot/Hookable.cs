using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hookable : MonoBehaviour {

    public enum Mode
    {
        None,
        Hook, //pull this player towards this
        Pull //pull this towards the player
    }
    public Mode mode = Mode.None;
    public Hookshot attachedHookshot = null;

    public bool blocking = false;

    public bool IsBlocking()
    {
        return blocking;
    }

    public void OnDamage(DamagePacket packet)
    {
        if(attachedHookshot)
        {
            attachedHookshot.CancelTarget();
        }
    }
}
