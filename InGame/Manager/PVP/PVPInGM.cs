using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Protocol;
using DG.Tweening;


public enum PVPStageState
{
    waitTime,
    RollTime,
    AssignedTime,
    BattleTime,
    Victory,
    Lose,
    Draw,
    Stop,
    GameOut
}
public class PVPInGM : MonoBehaviour 
{

    private static PVPInGM _instance;

    public static PVPInGM Instance // 싱글톤 구현
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(PVPInGM)) as PVPInGM;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(PVPInGM)) as PVPInGM;
                }
            }
            return _instance;
        }
    }
    [Header("SP관련")]

    [SerializeField] private Slider mySPSlider;
    [SerializeField] private Text mySPText;
    [SerializeField] private Slider rivalSPSlider;
    [SerializeField] private Text rivalSPText;
    //SP 관련 프로퍼티 
    private readonly int firstSP = 0;
    int currentSP = 0;
    int rivalSP = 0;
    public int SP
    {
        get { return currentSP; }
        set
        {
            currentSP = value;
            if (currentSP >= rivalSP && pvpStageState == PVPStageState.RollTime)
            {
                mySPSlider.maxValue = currentSP;
                rivalSPSlider.maxValue = currentSP;
            }
            
            PVPCharManager.Instance.NoBuySearch();

            mySPSlider.value = currentSP;
            mySPText.text = currentSP.ToString();
            BackEndMatchManager.Instance.SendDataToInGame(new Protocol.SendSPMessage(InGameInfoManager.Instance.mySessionID, currentSP));
        }
    }
    public void RivalSPShow(SendSPMessage msg)
    {
        rivalSP = msg.sp;
        if (currentSP <= rivalSP && pvpStageState == PVPStageState.RollTime)
        {
            mySPSlider.maxValue = rivalSP;
            rivalSPSlider.maxValue = rivalSP;
        }
        rivalSPSlider.value = msg.sp;
        rivalSPText.text = msg.sp.ToString();

    }


    #region"스테이지 FLOW 관련"
    [Header("스테이지 FLOW 관련")]
    public PVPStageState pvpStageState;

    public bool isBattleReady;

    [SerializeField] private float graceTime;
    private float currentGraceTime=0f;
    [SerializeField]private Text timeCheck_Text;
    [SerializeField]private Text currentRoundText;
    [SerializeField]private Image battleReadyBtnImage;
    [HideInInspector]public bool isWinCheck = false;


    private int myWinPoint;
    private int rivalWinPoint;

    //나와 상대의 승리 표시 별
    [SerializeField] private GameObject[] myWinStars;
    [SerializeField] private GameObject[] rivalWinStars;

    [Header("라운드 관련")]
    public int currentRound;
    private int maxRound;
    //라운드 바뀔때 효과
    [SerializeField] private UIEffect uiEffect;



    #region "게임 Flow별 이벤트(delegate)"
    //타워 공격 타임이 끝났다면 아군 유닛, 적 유닛 회수 
    //아군유닛은 남은 유닛의 체력의 퍼센트를 비교하여 비율 만큼 SP를 되돌려준다.
    //적군 유닛은 따로 리스트에 저장한 뒤 다음 라운드에 가장 앞에 나오게한다.
    //타워 공격 포인트 초기화
    //다이스 관련 초기화
    public delegate void RoundChangeEvent();
    public RoundChangeEvent roundChangeChain;
    #endregion

    #endregion

    #region "해상도 대응 관련"

    [Header("화면 영역 구하기")]
    //게임 화면 영역 구하기 
    public GameObject playArea;
    private RectTransform playAreaRect;
    //캔버스 스케일러의 설정 값 가져오기 
    CanvasScaler canvasScaler;
    //캔버스 스케일러의 기준 Y값 
    float canvasBaseY;
    private float canvasBaseX;

    //현재 캔버스 Y값 
    float currnetResolutionY;
    float currnetResolutionX;

    //해상도 비율 조정
    float resolutionRatio;

    //박스 콜라이더 영역의 최소 최대 xyz 값을 지님
    [HideInInspector] public Vector3 minBound;
    [HideInInspector] public Vector3 maxBound;

    private Vector3[] imgCorners;

    [HideInInspector] public float playAreaWidth;
    [HideInInspector] public float playAreaHeight;

    private float Line1;
    private float Line2;

    #region "아군 적군 포지션 설정"
    [HideInInspector] public List<Vector2> myUnitPos = new List<Vector2>();
    [HideInInspector] public List<Vector2> enemyUnitPos = new List<Vector2>();

    //Layer 나누기 
    [HideInInspector] public readonly string[] lineLayerArr = { "Line0", "Line1", "Line2", "Line3", "Line4", "Line5", "Line6", "Line7" };
    #endregion

    #endregion

    #region PVP 관련 변수 
    //active 정보를 캐싱한 리스트 
    public Dictionary<int, PVPCharactor> activeUnits;
    private bool isReady = false;

    [SerializeField] private GameObject playFuncArea;

    //전투 시간 때 배경을 Rect -500 만큼 내려주고 캐릭터들의 포지션도 world 포지션에서 -500pixel만큼 내려준다.
    private RectTransform pvpBGRect;
    //내려 주기 전 배경 Y 포지션
    private float originBGRectY;
    // 내려 주기 전 과 내려준 후 차이의 월드 포지션 
    private float spreadPosY;

    [SerializeField]private Text hostText;

    #endregion
    private void Awake()
    {
        DOTween.Init();
        //PVP모드가 아니라면 꺼주기
        if (!InGameInfoManager.Instance.isPVPMode)
        {
            this.enabled = false;
        }
        else
        {
            activeUnits = new Dictionary<int, PVPCharactor>();
            //처음은 wait타임 
            pvpStageState = PVPStageState.waitTime;
        }
       
    }
    private void Start()
    {
        currentRound = 1;
        // 변수 선언
        //SP = firstSP;
        myWinPoint = 0;
        rivalWinPoint = 0;
        currentGraceTime = graceTime;
        //유닛들의 위치 정하기
        SetUnitPos();
        maxRound = GameDataManager.Instance.pvpMaxRound;
        isReady = true;
        //맨처음 DiceUI를 켜주고 소환 UI를 꺼준다.
        InGameUIManager.Instance.SwapPlayFunc(true);

        if (BackEndMatchManager.Instance.IsHost()) { hostText.text = "HOST"; }
        else { hostText.text = "NOT HOST"; }


    }
    #region 상대가 나갈 때 이벤트 
    private void RemainPlayerGameOutEvent()
    {
        Debug.Log("게임중 나감1");
        StartCoroutine(RivalGameOutEvent(true));
    }
    private void OutPlayerGameOutEvent()
    {
        Debug.Log("게임중 나감2");
        StartCoroutine(RivalGameOutEvent(false));
    }
    public void RivalGameOut()
    {
        Time.timeScale = 0;

        //로딩씬에서 처음 들어올 때 만약 상대가 나갔다면 Draw로 처리한다.
        if (currentRound == 1 && pvpStageState == PVPStageState.waitTime)
        {

            uiEffect.gameObject.SetActive(false);
            if (InGameInfoManager.Instance.mySessionID != InGameInfoManager.Instance.outSession)
            {
                InGameUIManager.Instance.SetErrorMessage("상대가 떠났습니다.\n 확인 버튼을 누르면 메뉴화면으로 돌아갑니다.");
                //둘이 비겼으면 Draw
                BackEndMatchManager.Instance.MatchGameOver(InGameInfoManager.Instance.rivalSessionID, InGameInfoManager.Instance.mySessionID, true);
            }
            else
            {
                InGameUIManager.Instance.SetErrorMessage("게임을 떠나서 비김 처리 되었습니다.\n 확인 버튼을 누르면 메뉴화면으로 돌아갑니다.");
            }
            //확인 버튼을 누르면 메인메뉴로 돌아가게끔 이벤트를 넣어준다.
            InGameUIManager.Instance.errorBtn.onClick.AddListener(InGameUIManager.Instance.GotoMenuClick);
        }
        else
        {
            if (InGameInfoManager.Instance.mySessionID != InGameInfoManager.Instance.outSession)
            {
                InGameUIManager.Instance.SetErrorMessage("상대가 떠났습니다.\n 2초후 게임 승패를 결정합니다.");
                RemainPlayerGameOutEvent();
            }
            else
            {
                uiEffect.gameObject.SetActive(false);
                OutPlayerGameOutEvent();
                InGameUIManager.Instance.SetErrorMessage("게임을 나가서 패배 처리 되었습니다.");
            }
        }

        pvpStageState = PVPStageState.GameOut;
    }

    IEnumerator RivalGameOutEvent(bool isRemain)
    {
        //내가 남아있던 유저라면?
        if (isRemain)
        {
            //승리
            yield return new WaitForSecondsRealtime(2f);
            InGameUIManager.Instance.errorObj.SetActive(false);
            pvpStageState = PVPStageState.Victory;
            GameEndEvent();
            //승리
            BackEndMatchManager.Instance.MatchGameOver(InGameInfoManager.Instance.mySessionID, InGameInfoManager.Instance.outSession, false);
            InGameUIManager.Instance.winUI.SetActive(true);
        }
        else
        {
            yield return new WaitForSecondsRealtime(.5f);
            InGameUIManager.Instance.errorObj.SetActive(false);
            pvpStageState = PVPStageState.Lose;
            GameEndEvent();
            InGameUIManager.Instance.loseUI.SetActive(true);
        }
    }
    //게임이 끝나면 남아 있는 캐릭터 들을 모두 풀에 돌려준다.
    public void GameEndEvent()
    {
        Debug.Log("게임 엔드 이벤트");
        for (int i = 0; i < 17; i++)
        {
            if (activeUnits.ContainsKey(i))
            {
                activeUnits[i].gameObject.SetActive(false);
            }
        }
    }
    #endregion
    
    #region "게임 진행 과정"
    //타워 움직이는 시간 체크 
    private void Update()
    {
        switch (pvpStageState)
        {
            case PVPStageState.waitTime:
                //만약 로딩중 나간 플레이어가 있다면?
                if (InGameInfoManager.Instance.isGameOut == true)
                {
                    RivalGameOut();
                    return;
                }
                if (isReady)
                {
                    PVPInGM.Instance.pvpStageState = PVPStageState.RollTime;
                    //다이스 미션을 얻는다.
                    DiceManager.Instance.diceMissionManager.GetMisson();
                    //라운드 업 효과
                    uiEffect.OnUIEffect(UIEffectKind.RoundUp);
                    uiEffect.GoDiceTimeEffect();
                    //DiceUI를 켜주고 소환 UI를 꺼준다.
                    InGameUIManager.Instance.SwapPlayFunc(true);

                    //전투 UI 초기화
                    InGameUIManager.Instance.BattleUIInitialize();
                    //호스트만 시간 체크 메세지를 보낸다.
                    if (BackEndMatchManager.Instance.IsHost())
                    {
                        PVPManager.Instance.CountStartMethod(PVPManager.Instance.rollTime, true);
                    }
                    isBattleReady = false;
                    //시각적 효과 제거
                    battleReadyBtnImage.color = Color.white;
                    currentRoundText.text = currentRound.ToString();

                    isReady = false;
                }
                break;
            case PVPStageState.RollTime:
                break;
            case PVPStageState.AssignedTime:
                break;
            case PVPStageState.BattleTime:
                WinCheck();
                break;
            case PVPStageState.Stop:
                GameEndDecision();
                break;
        }
    }
    #endregion
    #region "아군, 적 유닛 배치 관련" 
    void SetUnitPos()
    {
        #region 해상도 비율 조정
        //캔버스 스케일러의 설정 값 가져오기 
        canvasScaler = playArea.transform.GetComponentInParent<CanvasScaler>();
        //캔버스 스케일러의 기준 Y값 
        canvasBaseY = canvasScaler.referenceResolution.y;
        canvasBaseX = canvasScaler.referenceResolution.x;

        currnetResolutionY = Screen.height;
        currnetResolutionX = Screen.width;
        //Bound값에 곱해줄 값
        resolutionRatio =  (canvasBaseY / currnetResolutionY) / (canvasBaseX / currnetResolutionX);
        #endregion

        #region 배경 및 캐릭터 위치 관련
        playAreaRect = playArea.transform.GetComponent<RectTransform>();
        //게임 화면의 bound 구하기 
        imgCorners = new Vector3[4];
        playAreaRect.GetWorldCorners(imgCorners);

        //게임화면의 코너(해상도 비율을 곱하여 어떤 해상도든 1440X 2960 기준으로 위치를 잡게한다.)
        minBound = imgCorners[0] / resolutionRatio;
        maxBound = imgCorners[2] / resolutionRatio;

        //게임화면 전체 폭,높이
        playAreaWidth = (maxBound.x - minBound.x);
        playAreaHeight = (maxBound.y - minBound.y);

        //구역을 1/4로 나누어 1,2,3 번 째 라인에 탱,근,원 으로 x축 좌표를 배정한다.
        float linePointX = playAreaWidth / 3;
        Line1 = maxBound.x - linePointX * 1;
        Line2 = maxBound.x - linePointX * 2;

        #endregion
    }


    //위치 잡기(myList 가 myUnitPos 일때는 아군 , enemyUniyPos 일 때는 적군)
    public void AssignedPos(List<PVPCharactor> summonList, List<Vector2> myList)
    {
        //리스트 초기화
        myList.Clear();
        float Line1X;
        float Line2X;
        if (myList == myUnitPos)
        {
            Line1X = Line1;
            Line2X = Line2;
        }
        else
        {
            Line1X = -Line1;
            Line2X = -Line2;
        }
        //인구수에 따라서 배치 포지션 구하기
        if (summonList.Count > 4)
        {
            TypePosSet(myList, 4, Line1X);
            TypePosSet(myList, summonList.Count - 4, Line2X);
        }
        else
        {
            TypePosSet(myList, summonList.Count, Line1X);
        }
        //캐릭터 배치 
        for (int i = 0; i < summonList.Count; i++)
        {
            summonList[i].transform.position = myList[i];
        }

        SummonLayerSet(summonList);
    }

    private void TypePosSet(List<Vector2> myList, int population, float typeLineX)
    {

        float minusPointY = 0;
        float linePoint = playAreaHeight / 6;


        switch (population)
        {
            case 1:
                for (int i = 3; i < 7; i += 4)
                {
                    minusPointY = (linePoint * i);
                    Vector2 unitPos = new Vector2(typeLineX, (maxBound.y - minusPointY));
                    myList.Add(unitPos);
                }
                break;
            case 2:

                for (int i = 1; i < 7; i += 4)
                {
                    minusPointY = (linePoint * i);
                    Vector2 unitPos = new Vector2(typeLineX, (maxBound.y - minusPointY));
                    myList.Add(unitPos);
                }
                break;
            case 3:
                for (int i = 1; i < 7; i += 2)
                {
                    minusPointY = (linePoint * i);
                    Vector2 unitPos = new Vector2(typeLineX, (maxBound.y - minusPointY));
                    myList.Add(unitPos);
                }
                break;
            case 4:

                for (int i = 0; i < 7; i += 2)
                {
                    minusPointY = (linePoint * i);
                    Vector2 unitPos = new Vector2(typeLineX, (maxBound.y - minusPointY));

                    myList.Add(unitPos);
                }
                break;
        }
    }
    private void SummonLayerSet(List<PVPCharactor> summonList)
    {

        //위치에 따른 내림차순으로 정렬 
        summonList = summonList.OrderByDescending(x => x.transform.position.y).ToList();

        for (int i = 0; i < summonList.Count; i++)
        {
            var mesh = summonList[i].transform.GetComponent<MeshRenderer>();
            mesh.sortingLayerName = PVPInGM.Instance.lineLayerArr[i];
        }
    }
    // 전투가 시작 될 때 배경은 400픽셀 정도 아래로 이동하게 되고 
    // 캐릭터는 Y축으로 400픽셀 정도 아래로 내려가고 X축으로는 640 정도 왼쪽으로 미뤄지게 함 
    // Y축은 따로 계산 해서 
    private void BattleGamePosSet()
    {
        //내리기전 정보 구하기
        pvpBGRect = InGameUIManager.Instance.pvpBackGround.rectTransform;
        originBGRectY = pvpBGRect.anchoredPosition.y;
        Vector3[] originCorners = new Vector3[4];
        pvpBGRect.GetWorldCorners(originCorners);
        // 내린 후 정보 구하기
        pvpBGRect.anchoredPosition -= new Vector2(0, 500);
        Vector3[] battleCorners = new Vector3[4];
        pvpBGRect.GetWorldCorners(battleCorners);

        Vector3 originCorner = originCorners[2];
        Vector3 battleCorner = battleCorners[2];
        spreadPosY = (originCorner.y - battleCorner.y) / resolutionRatio;
        // 캐릭터 위치 조정(게임 씬이 벌어졌기 때문에 캐릭터 사이의 거리를 더 벌려야한다.)
        for (int i = 0; i < PVPCharManager.Instance.summonList.Count; i++)
        {
            Vector2 unitVector = PVPCharManager.Instance.summonList[i].transform.position;
            PVPCharManager.Instance.summonList[i].transform.position = new Vector2(unitVector .x - playAreaWidth, unitVector.y - spreadPosY);
        }
        for (int j = 0; j < RivalManager.Instance.summonList.Count; j++)
        {
            Vector2 unitVector = RivalManager.Instance.summonList[j].transform.position;
            RivalManager.Instance.summonList[j].transform.position = new Vector2(unitVector.x + playAreaWidth, unitVector.y - spreadPosY);
        }
        playFuncArea.SetActive(false);
        InGameUIManager.Instance.BattleUI.SetActive(true);
    }


    #endregion
    
    #region "우승 체크"
    void WinCheck()
    {
        if (isWinCheck)
        {
            //아군이 이겼을 때 
            if (PVPCharManager.Instance.summonList.Count != 0 && RivalManager.Instance.summonList.Count == 0)
            {
                //만약 판정 후 0.5~1 초간의 시간 동안 결과가 바뀔 수 있는 여유를 준다.(무승부 판정 시간)
                if (currentGraceTime >= 0)
                {
                    currentGraceTime -= Time.deltaTime;
                }
                else
                {
                    //만약 유예시간 후 내 유닛 수가 없다면 return 해준다.
                    if (PVPCharManager.Instance.summonList.Count == 0)
                    {
                        Debug.Log("다시!!");
                        currentGraceTime = graceTime;
                        return;
                    }
                    pvpStageState = PVPStageState.Stop;
                   

                    for (int i = 0; i < PVPCharManager.Instance.summonList.Count; i++)
                    {
                        PVPCharManager.Instance.summonList[i].RoundUpEvent();
                    }
                    //스킬 가챠 관련 데이터 초기화
                    SkillGatchaManager.Instance.GatchaSkillInitialize();
                    SkillPoolingManager.Instance.SkillInitialized();
                    Debug.Log("아군 승리");
                    //내 Win카운트 올려주기
                    myWinPoint++;
                    //내 승리 별 켜주기
                    myWinStars[myWinPoint - 1].SetActive(true);
                    //배틀이 끝나서  라운드 업 신호를 보냄
                    if (currentRound != maxRound && myWinPoint != 3)
                    {
                        uiEffect.OnUIEffect(UIEffectKind.Victory);
                        BackEndMatchManager.Instance.SendDataToInGame(new Protocol.RoundUPMessage(true));
                        currentGraceTime = graceTime;
                    }
                    //라운드 넘기기
                    isWinCheck = false;
                }
            }
            //적군이 이겼을 때
            if (PVPCharManager.Instance.summonList.Count == 0 && RivalManager.Instance.summonList.Count != 0)
            {
                //만약 판정 후 0.5~1 초간의 시간 동안 결과가 바뀔 수 있는 여유를 준다.(무승부 판정 시간)
                if (currentGraceTime >= 0)
                {
                    currentGraceTime -= Time.deltaTime;
                }
                else
                {

                    //만약 유예시간 후 내 유닛 수가 없다면 return 해준다.
                    if (RivalManager.Instance.summonList.Count == 0)
                    {
                        Debug.Log("다시!!");
                        currentGraceTime = graceTime;
                        return;
                    }
                    Debug.Log("적군 승리");
                    pvpStageState = PVPStageState.Stop;

                    for (int i = 0; i < RivalManager.Instance.summonList.Count; i++)
                    {
                        RivalManager.Instance.summonList[i].RoundUpEvent();
                    }
                    //스킬 가챠 관련 데이터 초기화
                    SkillGatchaManager.Instance.GatchaSkillInitialize();
                    SkillPoolingManager.Instance.SkillInitialized();
                    //상대 Win카운트 올려주기
                    rivalWinPoint++;
                    rivalWinStars[rivalWinPoint - 1].SetActive(true);
                    //배틀이 끝나서  라운드 업 신호를 보냄
                    if (currentRound != maxRound && rivalWinPoint != 3)
                    {
                        uiEffect.OnUIEffect(UIEffectKind.Lose);
                        BackEndMatchManager.Instance.SendDataToInGame(new Protocol.RoundUPMessage(true));
                        currentGraceTime = graceTime;
                    }
                    //라운드 넘기기
                    isWinCheck = false;
                }
            }
            if (PVPCharManager.Instance.summonList.Count == 0 && RivalManager.Instance.summonList.Count == 0)
            {
                Debug.Log("동점으로 라운드 종료");
                //동점일 때는 둘의 승리포인트 둘다 올려준다.
                //내 Win카운트 올려주기
                myWinPoint++;
                //내 승리 별 켜주기
                myWinStars[myWinPoint - 1].SetActive(true);
                pvpStageState = PVPStageState.Stop;
                //스킬 가챠 관련 데이터 초기화
                SkillGatchaManager.Instance.GatchaSkillInitialize();
                SkillPoolingManager.Instance.SkillInitialized();
                //상대 Win카운트 올려주기
                rivalWinPoint++;
                rivalWinStars[rivalWinPoint - 1].SetActive(true);
                //배틀이 끝나서  라운드 업 신호를 보냄
                if (currentRound != maxRound && rivalWinPoint != 3 && myWinPoint != 3)
                {
                    uiEffect.OnUIEffect(UIEffectKind.Draw);
                    BackEndMatchManager.Instance.SendDataToInGame(new Protocol.RoundUPMessage(true));
                }
                
                //라운드 넘기기
                isWinCheck = false;
            }
            
        }
    }
  
    #endregion
    #region 승패 결정 
    private void GameEndDecision()
    {
        if (!isWinCheck)
        {
            //만약 마지막 라운드에서 무승부가 나왔다면
            if (myWinPoint == 3 && rivalWinPoint == 3)
            {
                pvpStageState = PVPStageState.Draw;
                SendVictoryInfo(pvpStageState);
                return;
            }
            //만약 내가 먼저 3승을 가져갔다면 
            if (myWinPoint == 3)
            {
                //승리
                pvpStageState = PVPStageState.Victory;
                SendVictoryInfo(pvpStageState);
                return;
            }
            //상대가 먼저 3승을 가져갔다면
            if (rivalWinPoint == 3)
            {
                pvpStageState = PVPStageState.Lose;
                SendVictoryInfo(pvpStageState);
                return;
            }
        }
    }


    private void SendVictoryInfo(PVPStageState pvpStage)
    {
        switch (pvpStage)
        {
            case PVPStageState.Victory:
                if (BackEndMatchManager.Instance.IsHost())
                {
                    //승리
                    BackEndMatchManager.Instance.MatchGameOver(InGameInfoManager.Instance.mySessionID, InGameInfoManager.Instance.rivalSessionID, false);
                }
                InGameUIManager.Instance.winUI.SetActive(true);
                break;
            case PVPStageState.Lose:
                if (BackEndMatchManager.Instance.IsHost())
                {
                    //패배
                    BackEndMatchManager.Instance.MatchGameOver(InGameInfoManager.Instance.rivalSessionID, InGameInfoManager.Instance.mySessionID, false);
                }
                InGameUIManager.Instance.loseUI.SetActive(true);
                break;
            case PVPStageState.Draw:
                if (BackEndMatchManager.Instance.IsHost())
                {
                    //둘이 비겼으면 Draw
                    BackEndMatchManager.Instance.MatchGameOver(InGameInfoManager.Instance.rivalSessionID, InGameInfoManager.Instance.mySessionID, true);
                }
                InGameUIManager.Instance.drawUI.SetActive(true);
                break;
        }
    }

    #endregion

    #region 라운드 업 시 이벤트
    public void DelayRoundUp()
    {
        Invoke("RoundUp", 3f);
    }
    public void RoundUp()
    {
        pvpStageState = PVPStageState.waitTime;
        //라운드 변화시 이벤트
        roundChangeChain();
        //라운드 올려주기
        currentRound++;
        //신호 보내기 초기화
        isReady = true;

        InGameUIManager.Instance.BattleUI.SetActive(false);
        Debug.Log("라운드업!");
    }
    #endregion
    #region PVP시 이벤트들
    //시간 체크(true이면 rollTime, false이면 assigendTime)
    public void PVPTimeCheck(int time, bool isRollTime)
    {
        timeCheck_Text.gameObject.SetActive(true);
        if (time != 0)
        {
            

            if (isRollTime) { timeCheck_Text.text = string.Format("주사위 : {0}", time); }
            else { timeCheck_Text.text = string.Format("배치 : {0}", time); }
        }
        else
        {
            //만약 롤타임이 끝났다면(만약 롤타임이 끝났다면 자동으로 선택하게끔한다.)
            if (isRollTime)
            {
                timeCheck_Text.gameObject.SetActive(false);
                DiceManager.Instance.AutoDiceChoice();
            }
            //만약 배치시간이 끝났다면
            else if(!isRollTime )
            {
                BattleReady();
            }
        }
    }
    //캐릭터 배치 때 준비 완료버튼 클릭 
    //만약 준비 완료가 됬다면 SP를 소모하는 기능(캐릭터 소환, 스킬가챠 돌리기)를 못하게 막아야한다.

    public void ReadyButtonClick()
    {
        if (PVPCharManager.Instance.summonList.Count==0 && PVPCharManager.Instance.isBuy.Contains(true))
        {
            InGameUIManager.Instance.SetErrorMessage("캐릭터 소환을 해주세요");
            return;
        }
        BattleReady();
    }
    private void BattleReady()
    {
        if (!isBattleReady)
        {
            //스킬 가챠를 돌린 후 선택을 안한 상태로 캐릭터 배치 시간이 지나면 자동으로 3개 중 랜덤으로 선택하게 하게한다.
            SkillGatchaManager.Instance.AutoSelectGatchaSkill();
            //신호 보내기 
            BackEndMatchManager.Instance.SendDataToInGame(new Protocol.BattleReadyCheckMessage(true));
            //시각적 효과 부여
            battleReadyBtnImage.color = Color.gray;
            SkillGatchaManager.Instance.skillGatchaUI.SetActive(false);
            isBattleReady = true;
        }
    }

   public void DelayGotoBattleTime()
    {
        Debug.Log("시작");
        Invoke("GotoBattleTime", 3.5f);
    }
    public void GotoBattleTime()
    {
        timeCheck_Text.gameObject.SetActive(false);
        //전투 시간으로 바꾸어준다.
        //승리 체크 함수 활성화
        isWinCheck = true;
        //게임 시작시 유닛들의 위치와 배경의 위치를 변경해준다.
        BattleGamePosSet();
        pvpStageState = PVPStageState.BattleTime;
        //가챠스킬이 있다면 스킬을 사용하게 한다.
        SkillGatchaManager.Instance.DoGatchaSkill();
        //전투시 UI에 캐릭터의 HP를 입력해줌
        InGameUIManager.Instance.SetCharStatUIInfo();
        //라이벌 UI꺼줌
        InGameUIManager.Instance.rivalInfoUI.SetActive(false);
    }
    #endregion
    #region "중복없는 랜덤 뽑기(여러개)"
    public int[] GetRandomInt(int length, int min, int max)
    {
        int[] randArray = new int[length];
        bool isSame;

        for (int i = 0; i < length; ++i)
        {
            while (true)
            {
                randArray[i] = Random.Range(min, max);
                isSame = false;

                for (int j = 0; j < i; ++j)
                {
                    if (randArray[j] == randArray[i])
                    {
                        isSame = true;
                        break;
                    }
                }
                if (!isSame) break;
            }
        }
        return randArray;
    }
    #endregion
}