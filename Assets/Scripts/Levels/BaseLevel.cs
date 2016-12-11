using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseLevel : MonoBehaviour {

    List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
    public int enemiesRemaining = 0;

    public delegate void OnComplete();
    public OnComplete onComplete;
    bool wasComplete = false;

    PlayerController playerController;

    public bool IsComplete()
    {
        bool enemiesRemain = false;
        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            if(!spawnPoint.IsExhasted())
            {
                enemiesRemain = true;
                break;
            }
        }
        return !enemiesRemain;
    }

	// Use this for initialization
	void Start ()
    {
        playerController = FindObjectOfType<PlayerController>();
        spawnPoints.AddRange(this.GetComponentsInChildren<SpawnPoint>());
	}
	
	// Update is called once per frame
	void Update ()
    {
        //trigger completion event
        bool isComplete = IsComplete();
        if(!wasComplete && isComplete)
        {
            wasComplete = isComplete;
            if (onComplete != null) { onComplete(); }
        }
	}
}
