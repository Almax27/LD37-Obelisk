using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour {

    public List<GameObject> objectsToSpawn = new List<GameObject>();
    public float spawnDelay = 0;
    public bool waitForNull = true;

    GameObject lastSpawnedObject = null;
    List<GameObject> liveObjects = new List<GameObject>();
    float delayTick = 0;

    public bool IsExhasted()
    {
        return objectsToSpawn.Count <= 0 && liveObjects.Count <= 0;
    }

    void SpawnNext()
    {
        var obj = objectsToSpawn[0];
        objectsToSpawn.RemoveAt(0);

        //random starting location
        var spawnPosition = Vector2.zero;
        var points = GetComponentsInChildren<Transform>();
        if (points.Length > 0)
        {
            spawnPosition = points[Random.Range(0, points.Length - 1)].position;
        }

        var instance = Instantiate(obj, spawnPosition, Quaternion.identity);
        instance.transform.parent = this.transform;
        instance.SendMessage("OnSpawn", transform, SendMessageOptions.DontRequireReceiver);

        lastSpawnedObject = instance;
        liveObjects.Add(instance);

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
        liveObjects.RemoveAll(item => item == null);
    }
}
