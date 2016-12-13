using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheVoid : MonoBehaviour {

    public delegate void Callback();
    private Callback onActiveCallback;
    private Callback onInactiveCallback;

    public enum State
    {
        FadingIn,
        Active,
        FadingOut,
        Inactive
    }

    public SpriteRenderer darkness = null;
    public Easing.Helper darknessEaseIn = new Easing.Helper();
    public Easing.Helper darknessEaseOut = new Easing.Helper();

    State state = State.Inactive;

    void MoveToLayer(MonoBehaviour behaviour, string name)
    {
        if (behaviour)
        {
            foreach (var renderer in behaviour.GetComponentsInChildren<SpriteRenderer>(true))
            {
                renderer.sortingLayerName = name;
            }
            foreach (var renderer in behaviour.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                renderer.sortingLayerName = name;
            }
        }
    }

    public void EnterTheVoid(Callback callback = null)
    {
        if (state != State.FadingIn && state != State.Active)
        {
            darknessEaseIn.Reset();
            state = State.FadingIn;
            MoveToLayer(FindObjectOfType<PlayerController>(), "Void");
            MoveToLayer(FindObjectOfType<Obelisk>(), "Void");
            onActiveCallback = callback;
        }
        else if(state == State.Active)
        {
            if (callback != null) callback();
        }
    }

    public void ExitTheVoid(Callback callback = null)
    {
        if (state != State.FadingOut && state != State.Inactive)
        {
            darknessEaseOut.Reset();
            state = State.FadingOut;
            onInactiveCallback = callback;
        }
        else if (state == State.Inactive)
        {
            if (callback != null) callback();
        }
    }

    private void Start()
    {
        EnterTheVoid();
    }

    // Update is called once per frame
    void Update ()
    {
        if (darkness)
        {
            Color color = darkness.color;
            if (state == State.FadingIn)
            {
                if (darknessEaseIn.Update(Time.deltaTime, 0, 1, out color.a) == false)
                {
                    state = State.Active;
                    if (onActiveCallback != null) onActiveCallback();
                }
            }
            else if (state == State.FadingOut)
            {
                if (darknessEaseOut.Update(Time.deltaTime, 1, 0, out color.a) == false)
                {
                    state = State.Inactive;
                    MoveToLayer(FindObjectOfType<PlayerController>(), "Default");
                    MoveToLayer(FindObjectOfType<Obelisk>(), "Default");
                    if (onInactiveCallback != null) onInactiveCallback();
                }
            }
            darkness.color = color;
        }
	}
}
