using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hookshot : MonoBehaviour
{

    public delegate void Callback();

    public enum State
    {
        Idle,
        Extending,
        Retracting
    }
    [Header("Config")]
    public float minRange = 10;
    public float maxRange = 500;
    public float castWidth = 0;
    public float extensionSpeed = 1.0f;
    public float retractionSpeed = 1.0f;
    public bool fireWhenNoTarget = true;
    public LayerMask hookMask = new LayerMask();
    public LayerMask hitMask = new LayerMask();

    [Header("Visualisation")]
    public Renderer chain = null;
    public Renderer hook = null;

    [Header("Exposed")]
    public State state;
    public Transform owner;

    Collider2D target = null;
    Vector2 targetPosition;
    float extensionTime = 0;
    float retractionTime = 0;

    float tick = 0;

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

        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Vector2 origin = this.transform.position;
        Vector2 castOffset = _direction * castWidth;
        RaycastHit2D hit = Physics2D.CircleCast(origin + castOffset, castWidth * 0.5f, _direction, maxRange, hookMask.value);
        if (hit && hit.distance > minRange)
        {
            targetPosition = hit.point;
            target = hit.collider;
        }
        else
        {
            hit = Physics2D.CircleCast(origin + castOffset, castWidth * 0.5f, _direction, maxRange, hitMask.value);
            if (hit && hit.distance > minRange)
            {
                targetPosition = hit.point;
            }
            else
            {
                targetPosition = origin + _direction * maxRange;
                Vector2 start = origin + castOffset * 0.5f;
                Debug.DrawLine(new Vector3(start.x, start.y), new Vector3(targetPosition.x, targetPosition.y), Color.blue, 0.5f);
            }
        }

        //calculate anim times
        float distanceToTravel = (targetPosition - origin).magnitude;
        extensionTime = distanceToTravel / extensionSpeed;
        retractionTime = distanceToTravel / retractionSpeed;

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
        state = newState;
        tick = 0;
        if (state == State.Idle)
        {
            target = null;
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

    private void Update()
    {
        if (IsInUse())
        {
            Vector2 start = new Vector2();
            Vector2 end = new Vector2();

            if (state == State.Extending)
            {
                start = transform.position;
                tick += Time.deltaTime;
                if (tick > extensionTime)
                {//begin retracting
                    end = targetPosition;
                    SetState(State.Retracting);
                }
                else
                {//move end to target
                    float t = Easing.Ease(tick, 0, 1, extensionTime, Easing.Method.QuadOut);
                    end = Vector2.Lerp(transform.position, targetPosition, t);
                }
            }
            else if (state == State.Retracting)
            {
                end = targetPosition;
                tick += Time.deltaTime;
                if (tick > retractionTime)
                {
                    start = targetPosition;
                    SetState(State.Idle);
                }
                else
                {
                    float t = 0;
                    if (target)
                    {//move start towards target
                        t = Easing.Ease(tick, 0, 1, retractionTime, Easing.Method.QuadInOut);
                        start = Vector2.Lerp(transform.position, targetPosition, t);
                    }
                    else
                    {//return end to self
                        t = Easing.Ease(tick, 1, 0, retractionTime, Easing.Method.QuadOut);
                        start = transform.position;
                        end = Vector2.Lerp(transform.position, targetPosition, t);
                    }
                }

                //pull owner along with hook start
                if (owner && target)
                {
                    //using physics body if present
                    var body = owner.GetComponent<Rigidbody2D>();
                    if (body)
                    {
                        body.MovePosition(start);
                    }
                    else
                    {
                        owner.position = start;
                    }
                }
            }

            //update graphic
            Debug.DrawLine(new Vector3(start.x, start.y), new Vector3(end.x, end.y), Color.magenta);

            Vector2 dir = end - start;
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
