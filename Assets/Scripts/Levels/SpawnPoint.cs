using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour {

    public List<GameObject> objectsToSpawn = new List<GameObject>();
    public float spawnDelay = 0;
    public bool waitForNull = true;

    GameObject lastSpawnedObject = null;
    float delayTick = 0;

    public bool IsExhasted()
    {
        return objectsToSpawn.Count <= 0 && lastSpawnedObject == null;
    }

    void SpawnNext()
    {
        var obj = objectsToSpawn[0];
        objectsToSpawn.RemoveAt(0);

        var instance = Instantiate(obj);
        instance.transform.position = this.transform.position;

        lastSpawnedObject = instance;

        delayTick = 0;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(objectsToSpawn.Count > 0)
        {
            if(delayTick < spawnDelay)
            {
                delayTick += Time.deltaTime;
            }
            if ((!waitForNull || lastSpawnedObject == null) && delayTick >= spawnDelay)
            {
                SpawnNext();
            }
        }
	}
}
