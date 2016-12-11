using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    [Header("Prefabs")]
    public PlayerController player = null;
    PlayerController playerInstance = null;

    public List<BaseLevel> levels = new List<BaseLevel>();
    int levelIndex = -1;
    BaseLevel levelInstance = null;

    [Header("Music")]
    public AudioClip calmMusic = null;
    public AudioClip fightMusic = null;

    enum GameState
    {
        None,
        Fighting,
        Calm
    }
    GameState gameState = GameState.None;

    void TransitionToLevel(int _levelIndex)
    {
        if(levelInstance)
        {
            Destroy(levelInstance.gameObject);
        }
        if(_levelIndex >= 0 && _levelIndex < levels.Count)
        {
            var levelPrefab = levels[_levelIndex];
            levelInstance = Instantiate(levelPrefab.gameObject).GetComponent<BaseLevel>();
            levelInstance.transform.parent = this.transform;

            levelInstance.onComplete += () =>
            {
                SetGameState(GameState.Calm);
            };

            SetGameState(GameState.Fighting);

            levelIndex = _levelIndex;

            Debug.Log(string.Format("Transitioning to level {0}", _levelIndex));
        }
        else
        {
            Debug.LogError(string.Format("Failed to transition to level {0}", _levelIndex));
        }
    }

    void SetGameState(GameState newState)
    {
        switch(newState)
        {
            case GameState.Calm:
                FAFAudio.Instance.PlayMusic(calmMusic);
                break;
            case GameState.Fighting:
                FAFAudio.Instance.PlayMusic(fightMusic, 0.2f, 0.2f, true);
                break;
        }
        gameState = newState;
    }

    void SpawnPlayer()
    {
        if (playerInstance == null)
        {
            playerInstance = Instantiate(player.gameObject).GetComponent<PlayerController>();
            playerInstance.transform.parent = this.transform;

            playerInstance.onObeliskTrigger = () =>
            {
                if (gameState == GameState.Calm)
                {
                    TransitionToLevel(levelIndex + 1);
                }
            };
        }
        else
        {
            Debug.LogError("Failed to spawn player: player already exists");
        }
    }

    // Use this for initialization
    void Start ()
    {
        TransitionToLevel(0);
        SpawnPlayer();
    }
	
	// Update is called once per frame
	void Update ()
    {
        
    }
}
