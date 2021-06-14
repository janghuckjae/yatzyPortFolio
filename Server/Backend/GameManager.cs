using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(GameManager)) as GameManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(GameManager)) as GameManager;
                }
            }
            return _instance;
        }
    }
    private static bool isCreate = false;

    #region Scene
    private const string StartScene = "0. StartScene";
    private const string MenuScene = "1. MenuScene";
    private const string GameLoadRoom = "2. GameLoadRoom";
    private const string INGAME = "3. GameScene";
    #endregion


    #region Actions-Events
    //public static event Action OnRobby = delegate { };
    //public static event Action OnGameReady = delegate { };
    //public static event Action OnGameStart = delegate { };
    public static event Action InGame = delegate { };
    public static event Action AfterInGame = delegate { };
    public static event Action OnGameReconnect = delegate { };

    private string asyncSceneName = string.Empty;
    private IEnumerator InGameUpdateCoroutine;

    public enum GameState { StartScene, MenuScene, Ready, Start, InGame, Over, Result, Reconnect };
    public GameState gameState;
    #endregion

    
    void Awake()
    {
        // 60프레임 고정
        Application.targetFrameRate = 60;
        // 게임중 슬립모드 해제
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        DontDestroyOnLoad(this.gameObject);

    }
   

    void Start()
    {
        //만약 게임이 끝난 후 다시 메뉴씬 등으로 돌아왔을 때 게임매니저가 2개가 생기니까 만약 게임매니저가 있다면 파괴시킨다.

        if (isCreate)
        {
            DestroyImmediate(gameObject, true);
            return;
        }
        gameState = GameState.StartScene;
        isCreate = true;
    }
    private void Login()
    {
        // OnLogin();
        // ChangeScene(LOGIN);
    }
    private void GotoMenuScene()
    {
        //메뉴로 넘어가기전에 자료를 세팅해준다.

        GameDataManager.Instance.GetEnemyChartContents();
        GameDataManager.Instance.GetCharctorChartContents();
        GameDataManager.Instance.GetEmoticonChartContents();

        MenuLoadingSceneManager.LoadingtoNextScene(MenuScene);
    }

    private void GameReady()
    {
        Debug.Log("게임 레디 상태 돌입");
        ChangeScene(GameLoadRoom);
    }

    //private void GameStart()
    //{
    //    Debug.Log("게임 스타트");
    //    //delegate 초기화
    //    InGame = delegate { };
    //    AfterInGame = delegate { };

    //    //OnGameStart();
    //    // 게임씬이 로드되면 Start에서 OnGameStart 호출
    //    //LoadRoomUI.Instance.GotoGameScene();
    //}


    public GameState GetGameState()
    {
        return gameState;
    }

    public void ChangeState(GameState state)
    {
        gameState = state;
        switch (gameState)
        {
            case GameState.StartScene:
                Login();
                break;
            case GameState.MenuScene:
                GotoMenuScene();
                break;
            case GameState.Ready:
                GameReady();
                break;
            case GameState.Start:
                //GameStart();
                break;
            default:
                Debug.Log("알수없는 스테이트입니다. 확인해주세요.");
                break;
        }
    }

    public bool IsLobbyScene()
    {
        return SceneManager.GetActiveScene().name == MenuScene;
    }

    private void ChangeScene(string scene)
    {
        if (scene != StartScene && scene != INGAME && scene != MenuScene && scene != GameLoadRoom)
        {
            Debug.Log("알수없는 씬 입니다.");
            return;
        }

        SceneManager.LoadScene(scene);
    }
    

    public void OutGame()
    {
        Application.Quit();
    }
}
