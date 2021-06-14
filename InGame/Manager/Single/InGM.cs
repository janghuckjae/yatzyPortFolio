using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public enum StageState
{
    waitTime,
    RollTime,
    AssignedTime,
    BattleTime,
    TowerAttackTime,
    Victory,
    Lose,
    Draw,
    Stop
}
public class InGM : MonoBehaviour
{

    private static InGM _instance;

    public static InGM Instance // 싱글톤 구현
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(InGM)) as InGM;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(InGM)) as InGM;
                }
            }
            return _instance;
        }
    }

    //적, 아군 풀링 매니저에서 setActive된 유닛들을 각자의 배열에 넣어준다. 
    public List<Transform> activeEnemy = new List<Transform>();
    public List<Transform> activeFriend = new List<Transform>();


    [Header("SP관련")]
    //SP 관련 프로퍼티 
    private readonly int firstSP = 0;
    [SerializeField] private Text spUI;
    int currentSP = 0;


    public int SP
    {
        get { return currentSP; }
        set
        {
            currentSP = value;

            CharManager.Instance.NoBuySearch();
            //배치 타임 일때
            if (stageState == StageState.AssignedTime)
            {
                DiceManager.Instance.ScoreBarValue = SP;
                spUI.text = currentSP.ToString();
            }

        }
    }
    #region "해상도 대응 관련"

    [Header("화면 영역 구하기")]
    //게임 화면 영역 구하기 
    public GameObject background;
    public GameObject mobileScreen;
    public Camera mainCam;
    private RectTransform backgroundRect;

    //박스 콜라이더 영역의 최소 chleo xyz 값을 지닌ㅁ 

    [HideInInspector] public Vector3 minBound;
    [HideInInspector] public Vector3 maxBound;

    private Vector3[] imgCorners;
    
    [Header("타워 관련")]
    // 타워
    public GameObject objTowerPlayer; // 나의 타워 오브젝트
    public GameObject objTowerEnemy; // 상대 타워 오브젝트
    [HideInInspector] public PlayerTower towerPlayer; // 나의 타워 스크립트
    [HideInInspector] public EnemyTower towerEnemy; // 상대 타워 스크립트
    
    //[Range(-3.0f, 3.0f)]
    private readonly float towersYRange =0.5f;
    //[Range(-3.0f, 3.0f)]
    private readonly float towersXRange = 2f;

    [HideInInspector] public float towerPosX;
    [HideInInspector] public float towerPosY;

    [HideInInspector] public float bgWidth;
    [HideInInspector] public float bgHeight;
    #endregion
    #region"스테이지 FLOW 관련"
    [Header("스테이지 FLOW 관련")]
    public StageState stageState;
    public float assignedTime = 10f;
    [HideInInspector] public float currentTime = 0f;
    //최대 유닛 숫자
    [HideInInspector]public int maxUnitCnt = 15;

    //타워 공격 타임 카운트 => 살아있는 유닛의 카운트와 공격 판정 카운트가 갔다면 타워 공격 Start 
    public int towerAttackPoint = 0;
    public Text timeCheck_Text;
    private CameraMovement camInfo;
    bool isWinCheck = false;
    public bool isTowerAttackCheck;

    [Header("라운드 관련")]
    public int maxRound;
    public int currentRound;
    //라운드 바뀔때 효과
    [SerializeField] private UIEffect roundUI;

    //누가 이긴지 체크 (true면 아군이 이김,false면 상대가 이김 )
    private bool isFriendWin;
    #region "게임 Flow별 이벤트(delegate)"
    //타워 공격 타임이 끝났다면 아군 유닛, 적 유닛 회수 
    //아군유닛은 남은 유닛의 체력의 퍼센트를 비교하여 비율 만큼 SP를 되돌려준다.
    //적군 유닛은 따로 리스트에 저장한 뒤 다음 라운드에 가장 앞에 나오게한다.
    //타워 공격 포인트 초기화
    public delegate void RoundChangeEvent();
    public RoundChangeEvent roundChangeChain;
    #endregion

    #endregion
    #region "아군 적군 포지션 설정"
    private float playWidth;
    private float widthPoint;
    private float behindX;
    private float frontX;
    //임시로 값을 받을 변수 
    private float front_X;
    private float width_P;
    [HideInInspector] public List<Vector2> myUnitPos = new List<Vector2>();
    [HideInInspector]public List<Vector2> enemyUnitPos = new List<Vector2>();

    //가운데 맵의 X축 Y축의 최대,최소 값
    [HideInInspector] public float playAreaMaxY;
    [HideInInspector] public float playAreaMinY;
    [HideInInspector] public float playAreaMaxX;
    [HideInInspector] public float playAreaMinX;
    
    //게임 구역의 Y 축 길이
    private float playAreaHeight;
    
    //Y축을 나눈것 
    private float heightPoint;
    
    //Layer 나누기 
    [HideInInspector] public readonly string[] line1_3LayerArr = { "Line0", "Line2", "Line4", "Line6", "Line8"};
    [HideInInspector] public readonly string[] line2LayerArr = { "Line1", "Line3", "Line5", "Line7", "Line9"};
    #endregion

    #region 멈춤 관련 변수
    [Header("멈춤 관련변수")]
    public bool isPause;
    public GameObject pauseButtonOBJ;
    public GameObject inPauseUI;

    private Button pauseBtn;
    private Button pauseCancelBtn;
    private CanvasScaler canvasScaler;
    private float canvasBaseY;
    private float currnetResolutionY;
    private float resolutionRatio;

    #endregion
    private void Awake()
    {
        //PVP모드라면 꺼주기
        if (InGameInfoManager.Instance.isPVPMode)
        {
            objTowerPlayer.SetActive(false);
            objTowerEnemy.SetActive(false);
            this.enabled = false;
        }
        else
        {
            #region 해상도 비율 조정
            //캔버스 스케일러의 설정 값 가져오기 
            canvasScaler = background.transform.GetComponentInParent<CanvasScaler>();
            //캔버스 스케일러의 기준 Y값 
            canvasBaseY = canvasScaler.referenceResolution.y;
            currnetResolutionY = Screen.height;
            //Bound값에 곱해줄 값
            resolutionRatio = canvasBaseY / currnetResolutionY;
            #endregion

            //처음은 wait타임 
            stageState = StageState.waitTime;
            currentRound = 1;
            // 변수 선언
            SP = firstSP;
            #region 배경 및 캐릭터 위치 관련
            backgroundRect = background.transform.GetComponent<RectTransform>();
            camInfo = mobileScreen.transform.GetComponent<CameraMovement>();
            //게임 화면의 bound 구하기 
            imgCorners = new Vector3[4];
            backgroundRect.GetWorldCorners(imgCorners);

            //게임화면의 코너
            minBound = imgCorners[0]/ resolutionRatio;
            maxBound = imgCorners[2]/ resolutionRatio;

            //게임화면 전체 폭,높이+
            bgWidth = maxBound.x - minBound.x;
            bgHeight = maxBound.y - minBound.y;
            //아군 유닛 구역의 최소 범위
            playAreaMinX = minBound.x / 3;
            //적 유닛 구역의 최소 범위
            playAreaMaxX = maxBound.x / 3;

            //유닛들의 위치 정하기
            SetUnitPos();
            isTowerAttackCheck = true;
            //타워 위치 지정
            towerPosX = towersXRange * (bgWidth / 6f);
            towerPosY = towersYRange * (bgHeight / 6f);

            objTowerPlayer.transform.position = new Vector2(-towerPosX, towerPosY);
            objTowerEnemy.transform.position = new Vector2(towerPosX, towerPosY);
            towerPlayer = objTowerPlayer.transform.GetChild(0).GetComponent<PlayerTower>();
            towerEnemy = objTowerEnemy.transform.GetChild(0).GetComponent<EnemyTower>();

            #endregion

            pauseBtn = pauseButtonOBJ.transform.GetComponent<Button>();
            pauseCancelBtn = inPauseUI.transform.GetComponentInChildren<Button>();
            inPauseUI.SetActive(false);


            //최대 라운드 설정
            maxRound = InGameInfoManager.Instance.selectStageData.roundDatas.Length;

            //pause버튼 이벤트 등록
            pauseBtn.onClick.AddListener(SGPauseEvent);
            pauseCancelBtn.onClick.AddListener(SGPauseCancelEvent);
            //맨처음 DiceUI를 켜주고 소환 UI를 꺼준다.
            InGameUIManager.Instance.SwapPlayFunc(true);
        }
        
    }

    #region "게임 진행 과정"
    //타워 움직이는 시간 체크 
    private void Update()
    {
        switch (stageState)
        {
            case StageState.waitTime:

                //마지막 라운드라면 타워 이동
                if (currentRound == maxRound) { TowerWalk(); }
                camInfo.ComBackCamPos();
                //roundUI.RoundChangeEffect(currentRound);
                EnemyManager.Instance.SummonEnemy();
                //다이스 미션을 얻는다.
                DiceManager.Instance.diceMissionManager.GetMisson();
                stageState = StageState.RollTime;
                //스킬 발동
                //DiceUI를 켜주고 소환 UI를 꺼준다.
                InGameUIManager.Instance.SwapPlayFunc(true);
                break;
            case StageState.RollTime:
                
                break;
            case StageState.AssignedTime:
                //PVP모드 일때

                currentTime -= Time.deltaTime;
                timeCheck_Text.text = "남은 배치시간 : " + Mathf.Ceil(currentTime).ToString();

                if (currentTime <= 0)
                {
                    stageState = StageState.BattleTime;
                    timeCheck_Text.gameObject.SetActive(false);
                    //승리 체크 ON
                    isWinCheck = true;
                }

                break;
            case StageState.BattleTime:

                WinCheck();
                StartTowerAttackCheck();
                GameEndDecision();
                break;

            case StageState.TowerAttackTime:
                GameEndDecision();
                break;
        }
    }
    #endregion

    
    #region "아군, 적 유닛 배치 관련" 
    void SetUnitPos()
    {
        playAreaMaxY = bgHeight * 0.2862f;
        playAreaMinY = bgHeight * -0.06544f;

        //Y축의 길이 구하기
        playAreaHeight = playAreaMaxY - playAreaMinY;
        heightPoint = playAreaHeight / 32;

        // 우리가 구할 부분은 전체(4800)의 1/3 인 1440이다.
        playWidth = bgWidth * 0.3f;
        behindX = playWidth * 0.434f;
        frontX = playWidth * 0.156f;
        
        //배치 구역의 폭
        float assignedWidth = behindX - frontX;
        widthPoint = assignedWidth / 8f;
        //Y축의 최소 최대 값은 그대로 heightPoint 
    }

    //위치 잡기(myList 가 myUnitPos 일때는 아군 , enemyUniyPos 일 때는 적군)
    public void AssignedPos(int population,List<Vector2> myList)
    {
        //리스트 초기화
        myList.Clear();
        //받은 리스트에 따라 적군 아군 별 따로 적용
        if (myList == myUnitPos)
        {
            front_X = frontX;
            width_P = widthPoint;
        }
        else if (myList == enemyUnitPos)
        {
            front_X = frontX * -1;
            width_P = widthPoint * -1;
        }
        switch (population)
        { 
            case 1: GetAssignedPos1(myList, front_X, width_P, 16, 33, 18);
            break;
            case 2: GetAssignedPos1(myList, front_X, width_P, 10, 33, 12);
            break;
            case 3: GetAssignedPos2(myList, front_X, width_P, 16, 33, 18, 10, 33, 12);
            break;
            case 4: GetAssignedPos2(myList, front_X, width_P, 6, 30, 12, 12, 33, 12);
            break;
            case 5: GetAssignedPos2(myList, front_X, width_P, 3, 33, 13, 8, 33, 16);
            break;
            case 6: GetAssignedPos2(myList, front_X, width_P, 4, 33, 10, 9, 33, 10);
            break;
            case 7: GetAssignedPos2(myList, front_X, width_P, 2, 33, 9, 7, 30, 9);
            break;
            case 8: GetAssignedPos2(myList, front_X, width_P, 0, 33, 9, 2, 33, 9);
            break;
            case 9: GetAssignedPos2(myList, front_X, width_P, 2, 33, 7, 5, 30, 7);
            break;
            case 10: GetAssignedPos2(myList, front_X, width_P, 0, 33, 7, 2, 33, 7);
            break;
            case 11: GetAssignedPos3(myList, front_X, width_P, 2, 33, 9, 6, 33, 9);
            break;
            case 12: GetAssignedPos3(myList, front_X, width_P, 2, 33, 9, 4, 33, 9);
            break;
            case 13: GetAssignedPos3(myList, front_X, width_P, 0, 33, 9, 3, 33, 7);
            break;
            case 14: GetAssignedPos3(myList, front_X, width_P, 2, 33, 7, 5, 33, 7);
            break;
            case 15: GetAssignedPos3(myList, front_X, width_P, 0, 33, 7, 3, 33, 7);
            break;
        }
    }

    //값이 1~2일때 배치하는 함수
    void GetAssignedPos1(List<Vector2> posList,float frontX,float widthPoint,int firstPos,int maxPos,int posDistance)
    {
        for (int i = 1; i < 9; i += 3)
        {
            if (i == 4)
            {
                for (int j = firstPos; j < maxPos; j += posDistance)
                {
                    Vector2 myPos = new Vector2(-frontX - (widthPoint * i), playAreaMaxY - (heightPoint * j));
                    posList.Add(myPos);
                }
            }
        }
    }
    //값이 3~10일 때 배치하는 함수 
    void GetAssignedPos2(List<Vector2> posList, float frontX, float widthPoint, int firstPos1, int maxPos1, int posDistance1, int firstPos2, int maxPos2, int posDistance2)
    {
        for (int i = 0; i < 9; i += 2)
        {
            if (i == 2)
            {
                for (int j = firstPos1; j < maxPos1; j += posDistance1)
                {
                    Vector2 myPos = new Vector2(-frontX - (widthPoint * i), playAreaMaxY - (heightPoint * j));
                    posList.Add(myPos);
                }
            }
            else if (i == 6)
            {
                for (int j = firstPos2; j < maxPos2; j += posDistance2)
                {
                    Vector2 myPos = new Vector2(-frontX - (widthPoint * i), playAreaMaxY - (heightPoint * j));
                    posList.Add(myPos);
                }
            }
        }
    }
    void GetAssignedPos3(List<Vector2> posList, float frontX, float widthPoint, int firstPos1, int maxPos1, int posDistance1, int firstPos2, int maxPos2, int posDistance2)
    {
        for (int i = 1; i < 9; i += 3)
        {
            if (i == 1 || i == 7)
            {
                for (int j = firstPos1; j < maxPos1; j += posDistance1)
                {
                    Vector2 myPos = new Vector2(-frontX - (widthPoint * i), playAreaMaxY - (heightPoint * j));
                    posList.Add(myPos);
                }
            }
            else if (i == 4)
            {
                for (int j = firstPos2; j < maxPos2; j += posDistance2)
                {
                    Vector2 myPos = new Vector2(-frontX - (widthPoint * i), playAreaMaxY - (heightPoint * j));
                    posList.Add(myPos);
                }
            }
        }
    }
    #endregion
    #region "우승 체크"
    void WinCheck()
    {
        if (isWinCheck)
        { 
            //아군이 이겼을 때 
            if (activeFriend.Count != 0 && activeEnemy.Count == 0)
            {
                float min = float.MaxValue;
                foreach (Transform friendPos in activeFriend)
                {
                    float distance = (towerEnemy.transform.position - friendPos.position).sqrMagnitude;

                    if (distance <= min)
                    {
                        min = distance;
                        camInfo.camTarget = friendPos;
                    }
                }
                isFriendWin = true;
                isWinCheck = false;
            }
            //적군이 이겼을 때
            if (activeFriend.Count == 0 && activeEnemy.Count != 0)
            {
                float min = float.MaxValue;
                foreach (Transform enemyPos in activeEnemy)
                {
                    float distance = (towerPlayer.transform.position - enemyPos.position).sqrMagnitude;

                    if (distance <= min)
                    {
                        min = distance;
                        camInfo.camTarget = enemyPos;
                    }
                }
                isFriendWin = false;
                isWinCheck = false;
            }
        }
    }

    void StartTowerAttackCheck()
    {
        //승리 체크가 마무리 되고나서
        //배틀 시간 중 이긴쪽이 타워 공격 준비가 끝난다면 StageStage를 타워공격시간으로 바꿔준다.
        if (isWinCheck == false)
        {
            //아군이 승리했을 때 
            if (isFriendWin)
            {
                if (activeFriend.Count == towerAttackPoint)
                {
                    stageState = StageState.TowerAttackTime;
                    towerEnemy.towerState = TowerState.attack;
                }
            }
            else
            {
                if (activeEnemy.Count == towerAttackPoint)
                {
                    stageState = StageState.TowerAttackTime;
                    towerPlayer.towerState = TowerState.attack;

                }
            }
        }
    }
    #endregion

    #region 라운드 업 시 이벤트
    public void RoundUp()
    {
        towerEnemy.towerState = TowerState.idle;
        towerPlayer.towerState = TowerState.idle;

        towerAttackPoint = 0;
        //라운드 변화시 이벤트
        roundChangeChain();
        //라운드 올려주기
        currentRound++;
        //마지막 라운드라면 타워 이동
        if (currentRound == maxRound) { TowerWalk(); }
        stageState = StageState.waitTime;
        Debug.Log("라운드업!");
    }
    #endregion
    #region 승패 결정 
    private void GameEndDecision()
    {
        //마지막 라운드 때는 타워 공격 시간이 무제한이다.
        if (currentRound != maxRound)
        {
            //적이 아예 없을 때까지 타워 공격 타임
            if (activeFriend.Count == 0 && activeEnemy.Count == 0)
            {
                RoundUp();
            }
        }
        else
        {
            //마지막 라운드 타워 공격 시간 때 적 아군 둘다 없다면 체력이 적은쪽 패배
            if (activeFriend.Count == 0 && activeEnemy.Count == 0)
            {
                towerEnemy.towerState = TowerState.idle;
                towerPlayer.towerState = TowerState.idle;

                if (InGM.Instance.towerPlayer.TOWERHP > InGM.Instance.towerEnemy.TOWERHP)
                {
                    //승리
                    stageState = StageState.Victory;
                    GameEndEvent();
                    InGameUIManager.Instance.winUI.SetActive(true);
                }
                else if (InGM.Instance.towerPlayer.TOWERHP < InGM.Instance.towerEnemy.TOWERHP)
                {
                    GameEndEvent();
                    stageState = StageState.Lose;
                    InGameUIManager.Instance.loseUI.SetActive(true);
                }
                else
                {
                    GameEndEvent();
                    stageState = StageState.Draw;
                    InGameUIManager.Instance.drawUI.SetActive(true);
                }
            }
        }
    }
    #endregion
    void TowerWalk()
    {
        //타워가 움직이게한다.
        towerPlayer.towerState = TowerState.walk;
        towerEnemy.towerState = TowerState.walk;
    }
    //싱글 게임일 때 Pause 이벤트
    public void SGPauseEvent()
    {
        if (!isPause)
        {
            Debug.Log("Pause");
            pauseButtonOBJ.SetActive(false);
            Time.timeScale = 0;
            isPause = true;
            //멈춤 UI나오게하기
            inPauseUI.SetActive(true);
        }
    }
    public void SGPauseCancelEvent()
    {
        if(isPause)
        {
            Debug.Log("PauseCancel");
            isPause = false;
            Time.timeScale = 1;
            inPauseUI.SetActive(false);
            pauseButtonOBJ.SetActive(true);
        }
        
    }    
    //게임이 끝나면 남아 있는 캐릭터 들을 모두 풀에 돌려준다.
    public void GameEndEvent()
    {
        if (isFriendWin == true)
        {
            for (int i = 0; i < activeFriend.Count; i++)
            {
                activeFriend[i].gameObject.SetActive(false);
                Debug.Log("아군 사라짐");
            }
        }
        else
        {
            for (int i = 0; i < activeEnemy.Count; i++)
            {
                activeEnemy[i].gameObject.SetActive(false);
                Debug.Log("적군 사라짐");
            }
        }
    }
}
