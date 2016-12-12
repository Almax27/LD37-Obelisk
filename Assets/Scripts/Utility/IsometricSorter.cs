using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsometricSorter : MonoBehaviour {

    public Transform pointOfReference = null;

    private void LateUpdate()
    {
        var renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach(var renderer in renderers)
        {
            if(pointOfReference)
            {
                renderer.sortingOrder = -(int)pointOfReference.position.y;
            }
            else
            {
                renderer.sortingOrder = -(int)renderer.transform.position.y;
            }
            
        }
    }
}
