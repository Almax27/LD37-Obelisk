﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    [Header("Prefabs")]
    public PlayerController player = null;
    PlayerController playerInstance = null;

    public List<BaseLevel> levels = new List<BaseLevel>();
    int levelIndex = -1;
    BaseLevel levelInstance = null;

    [Header("References")]
    public Obelisk obelisk = null;
    public TheVoid theVoid = null;

    [Header("UI")]
    public Yarn.Unity.DialogueRunner obeliskDialogueRunner = null;
    public ObeliskDialogueUI obeliskDialogueUI = null;

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
        if (_levelIndex == 0)
        {
            ShowDialogue("start", 0);
        }
        else
        {
            SetGameState(GameState.Calm);
            theVoid.EnterTheVoid(() =>
            {
                if (levelInstance)
                {
                    levelInstance.gameObject.SetActive(false);
                    ShowDialogue(levelInstance.endDialogueNode, _levelIndex);
                }
            });
        }
    }

    void LoadLevel(int _levelIndex)
    {
        if (levelInstance)
        {
            Destroy(levelInstance.gameObject);
            levelInstance = null;
        }
        if (_levelIndex >= 0 && _levelIndex < levels.Count)
        {
            var levelPrefab = levels[_levelIndex];
            levelInstance = Instantiate(levelPrefab.gameObject).GetComponent<BaseLevel>();
            levelInstance.transform.parent = this.transform;

            levelInstance.onComplete += () =>
            {
                SetGameState(GameState.Calm);
            };

            SetGameState(GameState.Fighting);

            theVoid.ExitTheVoid();

            levelIndex = _levelIndex;

            Debug.Log(string.Format("Loading to level {0}", _levelIndex));
        }
        else
        {
            Debug.LogError(string.Format("Failed to load level {0}", _levelIndex));
        }
    }

    void SetGameState(GameState newState)
    {
        switch(newState)
        {
            case GameState.Calm:
                FAFAudio.Instance.TryPlayMusic(calmMusic);
                break;
            case GameState.Fighting:
                FAFAudio.Instance.TryPlayMusic(fightMusic, 0.2f, 0.2f, true);
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

    void ShowDialogue(string startNode, int nextLevel)
    {
        if (obeliskDialogueRunner && obeliskDialogueUI)
        {
            obeliskDialogueUI.onCommand = (Yarn.Command command) =>
            {
                if(command.text == "nextlevel")
                {
                    LoadLevel(nextLevel);
                }
                else if(command.text == "restart")
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
            };
            obeliskDialogueUI.onStarted = () =>
            {
                if(playerInstance)
                {
                    playerInstance.lockInput = true;
                }
            };
            obeliskDialogueUI.onComplete = () =>
            {
                if (playerInstance)
                {
                    playerInstance.lockInput = false;
                }
            };
            obeliskDialogueRunner.StartDialogue(startNode);
        }
    }

    private void Awake()
    {
        SpawnPlayer();
    }

    // Use this for initialization
    void Start ()
    {
        TransitionToLevel(0);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(Input.GetKeyDown(KeyCode.N))
        {
            TransitionToLevel(levelIndex + 1);
        }
        if(obelisk)
        {
            if(playerInstance)
            {
                var health = playerInstance.GetComponent<Health>();
                if (health)
                {
                    obelisk.SetHealth((int)health.current);
                }
            }
            else
            {
                //player died
                if(gameState != GameState.Calm)
                {
                    SetGameState(GameState.Calm);
                    ShowDialogue("death", 0);
                }
            }
        }
    }
}
