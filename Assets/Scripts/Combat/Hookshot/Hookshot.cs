using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hookshot : MonoBehaviour
{
    public delegate void Callback();

    public enum State
    {
        None,
        Idle,
        Extending,
        Retracting
    }
    [Serializable]
    public class StateConfig
    {
        public float speed = 0;
        public float pauseDuration = 0;
        public Easing.Method easingMethod;
        public Sprite hookSprite = null;
    }

    [Header("Config")]
    public float minRange = 10;
    public float maxRange = 500;
    public float castWidth = 0;
    public Hookable.Mode defaultMode = Hookable.Mode.None;

    public StateConfig extensionConfig = new StateConfig();
    public StateConfig retractionConfig = new StateConfig();
    public StateConfig hookConfig = new StateConfig();
    public StateConfig pullConfig = new StateConfig();

    public bool fireWhenNoTarget = true;
    public LayerMask hitMask = new LayerMask(); //things we can hit
    public LayerMask hookMask = new LayerMask(); //this we should hook by default

    [Header("Visualisation")]
    public Renderer chain = null;
    public Renderer hook = null;

    [Header("Audio")]
    public AudioClip fireSound = null;
    public AudioClip hookSound = null;
    public AudioClip pullSound = null;
    public AudioClip missSound = null;

    [Header("Exposed")]
    public State state = State.None;
    public Transform owner;

    Hookable.Mode currentMode = Hookable.Mode.None;
    Hookable target = null;
    Vector2 originPosition;
    Vector2 targetPosition;

    float tick = 0;

    public void CancelTarget()
    {
        if (target)
        {
            currentMode = Hookable.Mode.None;
            target.attachedHookshot = null;
            target = null;
        }
    }

    public bool IsInUse()
    {
        return state != State.Idle;
    }

    public bool Fire(Transform _owner, Vector2 _direction)
    {
        if (IsInUse())
        {
            return false;
        }

        owner = _owner;
        _direction.Normalize();

        currentMode = defaultMode;

        originPosition = this.transform.position;
        Vector2 castOffset = _direction * castWidth;
        RaycastHit2D hit = Physics2D.CircleCast(originPosition, castWidth * 0.5f, _direction, maxRange, hitMask.value);
        if(hit.distance <= minRange)
        {
            hit = Physics2D.Raycast(originPosition, _direction, maxRange, hitMask.value);
        }
        if (hit && hit.distance > 0)
        {
            targetPosition = hit.point;
            target = hit.collider.GetComponent<Hookable>();
            if (target && target.IsBlocking())
            {
                currentMode = Hookable.Mode.None;
            }
            else
            {
                if (((1 << hit.collider.gameObject.layer) & hookMask.value) > 0)
                {
                    currentMode = Hookable.Mode.Hook;
                }
            }
        }
        else
        {
            targetPosition = originPosition + _direction * maxRange;
        }

        if (fireWhenNoTarget || target != null)
        {
            SetState(State.Extending);
        }

        return true;
    }

    private void Start()
    {
        SetState(State.Idle);
    }

    void SetState(State newState)
    {
        if(state == newState)
        {
            return;
        }

        state = newState;
        tick = 0;

        switch(state)
        {
            case State.Idle:
                target = null;
                break;
            case State.Extending:
                FAFAudio.Instance.PlayOnce2D(fireSound, transform.position, 1.0f);
                break;
            case State.Retracting:
                var mode = GetHookMode();
                if (mode == Hookable.Mode.Hook)
                {
                    FAFAudio.Instance.PlayOnce2D(hookSound, transform.position, 1.0f, 0.2f);
                }
                else if(mode == Hookable.Mode.Pull)
                {
                    FAFAudio.Instance.PlayOnce2D(pullSound, transform.position, 1.0f, 0.2f);
                }
                else
                {
                    FAFAudio.Instance.PlayOnce2D(missSound, transform.position, 1.0f, 0.2f);
                }
                break;
        }

        //update visualisation
        if (chain)
        {
            chain.enabled = state != State.Idle;
        }
        if (hook)
        {
            hook.enabled = state != State.Idle;
        }
    }

    float GetTForConfig(StateConfig config)
    {
        float distanceToTravel = (targetPosition - originPosition).magnitude;
        float duration = distanceToTravel / config.speed;
        float t = Easing.Ease(Mathf.Clamp(tick - config.pauseDuration, 0, duration), 0, 1, duration, config.easingMethod);
        return t;
    }

    Hookable.Mode GetHookMode()
    {
        Hookable.Mode mode = currentMode;
        if (target)
        {
            mode = target.mode;
        }
        return mode;
    }

    private void Update()
    {
        if (IsInUse())
        {
            Vector2 start = transform.position;
            Vector2 end = targetPosition;

            tick += Time.deltaTime;

            if (state == State.Extending)
            {
                float t = GetTForConfig(extensionConfig);
                if (t >= 1)
                {//begin retracting
                    SetState(State.Retracting);
                }
                else
                {//move end to target
                    end = Vector2.Lerp(originPosition, targetPosition, t);
                }
            }
            else if (state == State.Retracting)
            {
                Hookable.Mode mode = GetHookMode();

                if(mode == Hookable.Mode.Hook)
                {//pull the owner to target
                    float t = GetTForConfig(hookConfig);
                    if (t >= 1)
                    {
                        start = targetPosition;
                        SetState(State.Idle);
                    }
                    else
                    {
                        start = Vector2.Lerp(originPosition, targetPosition, t);
                        start += new Vector2(0, 30) * (t < 0.5f ? t : 1.0f - t) * 0.5f;
                    }
                    if (owner)
                    {
                        //disable body during animation
                        var body = owner.GetComponent<Rigidbody2D>();
                        if (body)
                        {
                            body.simulated = t >= 1;
                        }
                        owner.position = start;
                    }
                }
                else if(mode == Hookable.Mode.Pull)
                {//pull the target to the owner
                    var targetToPull = target; //cache target as SetState sets to null
                    float t = GetTForConfig(pullConfig);
                    if (t >= 1)
                    {
                        end = originPosition;
                        SetState(State.Idle);
                    }
                    else
                    {
                        end = Vector2.Lerp(targetPosition, originPosition, t);
                        end += new Vector2(0, 30) * (t < 0.5f ? t : 1.0f - t) * 0.5f;
                    }
                    end += (targetPosition - originPosition).normalized * 5; //HACK: to avoid enemies passing through player
                    if (targetToPull)
                    {
                        if(targetToPull.IsBlocking())
                        {
                            CancelTarget();
                        }
                        //disable body during animation
                        var body = targetToPull.GetComponent<Rigidbody2D>();
                        if (body)
                        {
                            body.simulated = t >= 1;
                        }
                        targetToPull.transform.position = end;
                    }
                }
                else
                {//just retract the hookshot
                    float t = GetTForConfig(retractionConfig);
                    if (t >= 1)
                    {
                        start = targetPosition;
                        SetState(State.Idle);
                    }
                    else
                    {
                        end = Vector2.Lerp(targetPosition, originPosition, t);
                    }
                }
            }

            //update graphic
            Debug.DrawLine(new Vector3(start.x, start.y), new Vector3(end.x, end.y), Color.magenta);

            Vector2 dir = end - start;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            if (chain)
            {
                const string textName = "_MainTex";
                Vector3 scale = new Vector3(chain.material.mainTexture.width, chain.material.mainTexture.height);
                scale.x = dir.magnitude;
                chain.transform.position = start + dir * 0.5f;
                chain.transform.localScale = scale;
                chain.material.SetTextureScale(textName, new Vector2(scale.x / chain.material.mainTexture.width, 1));
            }
            if(hook)
            {
                hook.transform.position = end;
            }
        }
    }
}
