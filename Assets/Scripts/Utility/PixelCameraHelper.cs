using UnityEngine;
using System.Collections;

public class PixelCameraHelper : MonoBehaviour
{
    public enum Mode
    {
        PixelPerfect,
        FitHeight,
    }
    public Mode mode = Mode.PixelPerfect;
    public float worldHeight = 240;
    public float pixelsPerUnit = 1;
    public float pixelScale = 1;

    public float smoothTime = 0.2f;

    new Camera camera = null;

    float velocity = 0;

    // Use this for initialization
    void Start()
    {
        camera = GetComponent<Camera>();
        camera.orthographicSize = CalculateOrthSize();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (smoothTime == 0)
        {
            camera.orthographicSize = CalculateOrthSize();
        }
        else
        {
            camera.orthographicSize = Mathf.SmoothDamp(camera.orthographicSize, CalculateOrthSize(), ref velocity, smoothTime);
        }
    }

    float CalculateOrthSize()
    {
        float size = 0;
        switch(mode)
        {
            case Mode.PixelPerfect:
                size = Screen.height;
                break;
            case Mode.FitHeight:
                size = worldHeight;
                break;
        }
        float orthographicSize = 0.5f * (size / pixelsPerUnit );
        orthographicSize /= pixelScale;
        return orthographicSize;
    }
}