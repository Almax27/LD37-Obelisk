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

    [Header("References")]
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
        if (levelIndex < 0)
        {
            LoadLevel(_levelIndex);
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

    // Use this for initialization
    void Start ()
    {
        TransitionToLevel(0);
        SpawnPlayer();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(Input.GetKeyDown(KeyCode.N))
        {
            TransitionToLevel(levelIndex + 1);
        }
    }
}
