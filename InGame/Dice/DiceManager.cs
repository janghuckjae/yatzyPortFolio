using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using BackEnd;
using Protocol;


//다이스의 족보 
public enum HandRank
{
    noScore,
    OnePair,
    TwoPair,
    ThreeKind,
    FullHouse,
    FourKind,
    S_Straight,
    L_Straight,
    Yacht,
    Max
}

public class DiceManager : MonoBehaviour
{
    private static DiceManager _instance;

    public static DiceManager Instance // 싱글톤 구현
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(DiceManager)) as DiceManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(DiceManager)) as DiceManager;
                }
            }
            return _instance;
        }
    }

    HandRank myHandRank;
    [HideInInspector]public DiceMissionManager diceMissionManager;
    #region 다이스 롤 횟수 및 스코어바 프로퍼티
    //다이스 횟수 프로퍼티 
    int currentRollCnt;
    public int DICEROLL_CNT
    {
        get
        {
            return currentRollCnt;
        }
        set
        {
            currentRollCnt = value;
            for (int i = 0; i < rollCountObj.Length; i++)
            {
                if (i < currentRollCnt)
                {
                    rollCountObj[i].SetActive(true);
                }
                else
                {
                    rollCountObj[i].SetActive(false);
                }
            }
        }
    }
    private int currentScore;
    public int ScoreBarValue
    {
        get { return currentScore; }

        set
        {
            currentScore = value;
            //스코어바에 값 전송
            spSlider.value = currentScore;
        }
    }
    #endregion

    [Header("족보별 점수")]
    public int[] handRankScore;
    public Slider spSlider;
    [SerializeField] private RectTransform sliderFillArea;
    
    [SerializeField] private float autoDiceChoiceTime;
    public float spGetDelayTime;

    WaitForSeconds delayTime_autoDiceChoice;
    WaitForSeconds delayTime_spGet;

    [Header("가이드라인 관련")]
    [SerializeField] private RectTransform[] guideLine;
    private Text[] guideLineSpText;

    [SerializeField]private RectTransform myHandRankFlagObj;
    [SerializeField] private RectTransform rivalHandRankFlagObj;
    
    private Image myHandRankFlagImg;
    private Image rivalHandRankFlagImg;

    [SerializeField] private Sprite[] myHandRankSprite;
    [SerializeField] private Sprite[] rivalHandRankSprite;

    [SerializeField] private GameObject ruleUIObj;

    #region "다이스 표시 관련 변수"
    [Header("다이스 표시 관련")]
    [SerializeField] private Dice[] dices;
    //다이스 Roll초기화 상태 표시 
    [SerializeField] private Color selectColor;
    [HideInInspector]public bool[] onDiceChoose;
    public float diceRollTime;
    [HideInInspector] public List<int> keepDiceList = new List<int>();
    //현재 keepDiceList중 해당되는 State 정보
    [HideInInspector]public List<HandRank> currentRankList = new List<HandRank>();

    [Header("PVP관련")]
    [SerializeField] private GameObject myDiceUI;
    [SerializeField] private GameObject rivalDiceUI;
    [SerializeField] private GameObject rivalNoRollImg;
    [SerializeField] private Image diceUISwapBtnImg;
    [SerializeField] private Sprite[] swapBtnSprites;

    [SerializeField] private Image[] rivalImgs;
    [SerializeField] private Sprite[] rivalDiceSprite;
    [HideInInspector] public bool isDiceUISwap;
    #endregion

    [Header("Roll카운트 관련")]
    //다이스를 돌릴 수 있는 횟수
    [SerializeField] private int firstRollCnt = 3;
    //Roll 버튼의 텍스트 
    [SerializeField] private GameObject[] rollCountObj;

    #region "다이스 검사 관련 변수"
    //주사위의 눈을 최종 저장할 리스트
    private List<int> diceNum_List;

    //초기화 리스트
    private readonly List<int> first_list = new List<int>() { 0, 0, 0, 0, 0 };

    //임시 저장할 다이스 리스트
    //DiceNumList는 초기화를 시켜야하기 때문에 Add를 못한다. 그래서 대신 Add를 해서 대입해주기 위해 사용
    private readonly List<int> storage_List = new List<int>();
    //다이스를 돌릴수 있는 상태 
    [HideInInspector] public bool isDiceRoll = true;

    [HideInInspector] public int pair_C = 0;
    [HideInInspector] public int threekind_C = 0;
    [HideInInspector]public int straight_C = 0;
    //예외처리할 다이스 족보 
    private readonly List<int> execptList = new List<int>() { 1, 2, 4, 5, 6 };
    //랜덤 한 변수
    int rNumber;

    #endregion


    private void Start()
    {
        if (InGameInfoManager.Instance.isPVPMode)
        {
            //라운드 업 이벤트 등록
            PVPInGM.Instance.roundChangeChain += DiceRoundUPEvent;
        }
        else
        {
            InGM.Instance.roundChangeChain += DiceRoundUPEvent;
        }
        onDiceChoose = new bool[dices.Length];
        guideLineSpText = new Text[guideLine.Length];
        diceMissionManager = transform.GetComponent<DiceMissionManager>();
        myHandRankFlagImg = myHandRankFlagObj.GetComponentInChildren<Image>();
        rivalHandRankFlagImg = rivalHandRankFlagObj.GetComponentInChildren<Image>();
        myHandRankFlagObj.gameObject.SetActive(false);
        rivalHandRankFlagObj.gameObject.SetActive(false);
        myHandRank = HandRank.noScore;
        spSlider.maxValue = handRankScore[handRankScore.Length - 1];
        ScoreBarValue = handRankScore[(int)myHandRank];

        GuideLinePosSet();
        isDiceRoll = true;
        isDiceUISwap = true;

        delayTime_autoDiceChoice = new WaitForSeconds(autoDiceChoiceTime);
        delayTime_spGet = new WaitForSeconds(spGetDelayTime);

        diceNum_List = first_list;
        DICEROLL_CNT = firstRollCnt;

        //컴포넌트 할당 및 버튼 이벤트 등록
        for (int i = 0; i < dices.Length; i++)
        {
            dices[i].readyText.SetActive(true);

            int btnNum = i;
            dices[btnNum].myButton.onClick.AddListener(() => OnDiceBtnClick(btnNum));
            dices[btnNum].buttonNum = btnNum;
        }
    }
    #region 가이드라인 위치 맞추기
    private void GuideLinePosSet()
    {
        //야찌의 값이 스코어바의 Value에 마지막 값이 되기 때문에 
        float halfValue = handRankScore[handRankScore.Length - 1] / 2;
        //슬라이더의 Right값의 길이를 야찌의 스코어 점수로 나눈것
        float sliderpos1 = (sliderFillArea.rect.xMax- sliderFillArea.rect.xMin) / (float)handRankScore[handRankScore.Length - 1];

        for (int i = 1; i < handRankScore.Length; i++)
        {
            guideLine[i - 1].anchoredPosition = new Vector2(sliderpos1 * (handRankScore[i] - halfValue), guideLine[i - 1].anchoredPosition.y);
            guideLineSpText[i - 1] = guideLine[i - 1].GetComponentInChildren<Text>();
            guideLineSpText[i - 1].text = handRankScore[i].ToString();
        }
    }
    #endregion
    #region "Rule 버튼 클릭"
    public void RuleButtonClick()
    {
        ruleUIObj.SetActive(!ruleUIObj.activeSelf);
    }
    #endregion

    #region Roll 버튼을 클릭했을 때
    public void DiceRollButtonClick()
    {
        if (InGameInfoManager.Instance.isPVPMode)
        {
            if (PVPInGM.Instance.pvpStageState != PVPStageState.RollTime) { return; }
            //상대 UI를 볼때는 못돌리게 막아놈

            if (rivalDiceUI.activeSelf) { return; }
        }
        else
        {
            if (InGM.Instance.stageState != StageState.RollTime) { return; }
        }
        if (isDiceRoll)
        {
            switch (currentRollCnt)
            {
                case 3:
                    //주사위의 숫자 배정
                    for (int i = 0; i < diceNum_List.Count; i++)
                    {
                        //다이스 레디 상태 꺼주기
                        dices[i].readyText.SetActive(false);
                        rNumber = Random.Range(1, 7);
                        //랜덤한 숫자를 storage_List 저장해준다.
                        storage_List.Add(rNumber);
                        
                        diceNum_List[i] = storage_List[i];
                    }
                    //다이스 롤 가능 횟수  감소
                    DICEROLL_CNT--;
                    //다이스 화면 표시 변환 
                    DiceRollEffect();
                    break;
                   
                case 2:
                    //선택이 안된 다이스만 돌림
                    for (int i = 0; i < onDiceChoose.Length; i++)
                    {
                        if (onDiceChoose[i]==false)
                        {
                            rNumber = Random.Range(1, 7);
                            storage_List[i] = rNumber;
                        }
                        diceNum_List[i] = storage_List[i];
                    }
                    //다이스 롤 가능 횟수  감소
                    DICEROLL_CNT--;
                    //다이스 화면 표시 변환 
                    DiceRollEffect();
                    break;
                case 1:
                    
                    //선택이 안된 다이스만 돌림
                    for (int i = 0; i < onDiceChoose.Length; i++)
                    {
                        if (onDiceChoose[i]==false)
                        {
                            rNumber = Random.Range(1, 7);
                            storage_List[i] = rNumber;
                        }
                        diceNum_List[i] = storage_List[i];
                    }
                    //다이스 롤 가능 횟수  감소
                    DICEROLL_CNT--;
                    //다이스 화면 표시 변환 
                    DiceRollEffect();
                    isDiceRoll = false;
                    //마지막 롤을 돌린 뒤 선택이 안된 다이스들은 자동으로 선택 된다.
                    AutoDiceChoice();
                    break;
            }
            if (InGameInfoManager.Instance.isPVPMode)
            {
                if (keepDiceList.Count != 0)
                {
                    List<int> selectDiceList = new List<int>();
                    List<int> unselectDiceList = new List<int>();

                    selectDiceList.AddRange(keepDiceList);
                    for (int i = 0; i < onDiceChoose.Length; i++)
                    {
                        if (onDiceChoose[i] == false)
                        {
                            unselectDiceList.Add(diceNum_List[i]);
                        }
                    }
                    unselectDiceList.Sort();
                    selectDiceList.AddRange(unselectDiceList);
                    //선택된 주사위가 없어야 롤을 돌릴 때 리스트를 전송한다.
                    BackEndMatchManager.Instance.SendDataToInGame(new Protocol.SendDiceArrMessage(InGameInfoManager.Instance.mySessionID, selectDiceList.ToArray()));
                }
                else
                {
                    //롤 할 때 주사위 정보 보내주기
                    BackEndMatchManager.Instance.SendDataToInGame(new Protocol.SendDiceArrMessage(InGameInfoManager.Instance.mySessionID, diceNum_List.ToArray()));
                }
            }
        }
    }
    #endregion
    #region 오토 다이스 선택 기능
    public void AutoDiceChoice()
    {   //시간 내에 만약 롤을 돌리지 않았다면 그냥 넘기도록한다.
        if (currentRollCnt == 3)
        {
            if (InGameInfoManager.Instance.isPVPMode)
            {
                if (PVPInGM.Instance.pvpStageState == PVPStageState.RollTime)
                {
                    BackEndMatchManager.Instance.SendDataToInGame(new Protocol.StartAssignedTimeMessage(true));
                }
                return;
            }
        }
        //마지막 롤을 돌린 뒤 선택이 안된 다이스들은 자동으로 선택 된다.
        for (int j = 0; j < onDiceChoose.Length; j++)
        {
            if (onDiceChoose[j] == false)
            {
                StartCoroutine(AutoDiceChoice(j));
            }
        }
    }
    IEnumerator AutoDiceChoice(int btnNum)
    {
        yield return delayTime_autoDiceChoice;
        
        if (currentRollCnt != 3) { OnDiceBtnClick(btnNum); }
        
    }
    #endregion
    #region 다이스 선택기능 
    //다이스 버튼을 클릭 했을 때 
    public void OnDiceBtnClick(int diceBtnNum)
    {
        if (InGameInfoManager.Instance.isPVPMode)
        {
            if (PVPInGM.Instance.pvpStageState != PVPStageState.RollTime) { return; }

        }
        else
        {
            if (InGM.Instance.stageState != StageState.RollTime) { return; }
        }

        //롤을 한번이라도 돌려야 다이스 선택 가능  
        if (currentRollCnt < firstRollCnt && isDiceRoll)
        {

            List<int> selectDiceList = new List<int>();
            List<int> unselectDiceList = new List<int>();
            //버튼을 누르면 각 버튼의 선택한 bool함수를 켜준다.
            if (onDiceChoose[diceBtnNum]==false)
            {

                //선택한 목록에 다이스 이미지를 넣어준다.
                onDiceChoose[diceBtnNum] = true;
                //dices[diceBtnNum].myImage.color = selectColor;

                keepDiceList.Add(diceNum_List[diceBtnNum]);
                keepDiceList.Sort();

                selectDiceList.AddRange(keepDiceList);
                for (int i = 0; i < onDiceChoose.Length; i++)
                {
                    if (onDiceChoose[i] == false)
                    {
                        unselectDiceList.Add(diceNum_List[i]);
                    }
                }
                unselectDiceList.Sort();
                selectDiceList.AddRange(unselectDiceList);
                for (int j = 0; j < diceNum_List.Count; j++)
                {
                    if (j < keepDiceList.Count) { 
                        onDiceChoose[j] = true;
                        dices[j].myImage.color = selectColor;
                        
                    }
                    else { onDiceChoose[j] = false; }
                    diceNum_List[j] = selectDiceList[j];
                    dices[j].ImageChange(diceNum_List[j]);
                    
                }
                

            }
            //검사 
            DiceCount(keepDiceList,myHandRank);

            //PVP모드일 때 서버에 Keep데이터를 보내준다.
            if (InGameInfoManager.Instance.isPVPMode)
            {
                
                //선택된 주사위가 없어야 롤을 돌릴 때 리스트를 전송한다.
                BackEndMatchManager.Instance.SendDataToInGame(new Protocol.SendDiceArrMessage(InGameInfoManager.Instance.mySessionID, selectDiceList.ToArray()));

                //세션 아이디와 KeepDice목록을 부여해준다.
                BackEndMatchManager.Instance.SendDataToInGame(new Protocol.DiceSelectMessage(InGameInfoManager.Instance.mySessionID,keepDiceList.Count,(int)myHandRank));
            }

            //만약 모든 버튼을 다 선택 하였다면 자동으로 Choice되게함
            int diceChooseCnt = 0;
            for (int i = 0; i < onDiceChoose.Length; i++)
            {
                if (onDiceChoose[i]==true)
                {
                    diceChooseCnt++;
                }
                if (diceChooseCnt == dices.Length)
                {
                    isDiceRoll = false;

                    StartCoroutine(OnChoice());
                    diceChooseCnt = 0;
                }
            }
        }
    }
    #endregion

    #region Dice 목록 검사 
    //diceNum_List 에 있는 숫자의 중복 횟수를 검사. 
    private void DiceCount(List<int> keepList,HandRank diceState)
    {
        //돌린 다이스의 숫자를 카운트 한 값을 저장하는 딕셔너리.(pair 종류와 풀하우스 체크에 사용)
        Dictionary<int, int> count_Dic = new Dictionary<int, int>();
        foreach (int num in keepList)
        {
            if (!count_Dic.ContainsKey(num))
            {
                count_Dic.Add(num, 1);
            }
            else count_Dic[num]++;
        }
        RankApply(keepList, count_Dic,diceState);
    }

    // 족보 검색 및 적용 (체크 순서를 지정하여 결과적으로는 가장 높은 값이 diceState에 입력되도록 하였다.)
    private void RankApply(List<int> keepList, Dictionary<int, int> count_Dic, HandRank diceState)
    {
        //처음에 스코어를 noScore로 정한다음 뒤의 검사 과정중 해당이 없을 때 State를 noScore로 유지 
        diceState = HandRank.noScore;

        //족보 검색 전 키와 value 검출 
        //원페어,투페어,3Kind,4Kind,야츠 검색
        foreach (int value in count_Dic.Values)
        {
            switch (value)
            {
                case 2:
                    //페어 검사
                    pair_C++;
                    if (pair_C == 1)
                    {
                        diceState = HandRank.OnePair;
                        if (!currentRankList.Contains(diceState)) { currentRankList.Add(diceState); }
                    }
                    else if (pair_C == 2)
                    {
                        diceState = HandRank.TwoPair;
                        if (!currentRankList.Contains(diceState)) { currentRankList.Add(diceState); }
                    }
                        
                    break;
                case 3:
                    //3카인드 검사
                    threekind_C++;
                    if (threekind_C == 1)
                    {
                        diceState = HandRank.ThreeKind;
                        if (!currentRankList.Contains(diceState)) { currentRankList.Add(diceState); }
                    }
                        
                    break;
                case 4:
                    //포카인드 검사
                    diceState = HandRank.FourKind;
                    if (!currentRankList.Contains(diceState)) { currentRankList.Add(diceState); }
                    break;
                case 5:
                    //야츠 검사
                    diceState = HandRank.Yacht;
                    if (!currentRankList.Contains(diceState)) { currentRankList.Add(diceState); }
                    break;
            }
        }
        //풀하우스 검사
        //페어 1개와 3카인드 1개가 있으면 풀하우스
        if (pair_C == 1 && threekind_C == 1)
        {
            diceState = HandRank.FullHouse;
            if (!currentRankList.Contains(diceState)) { currentRankList.Add(diceState); }
        }

        for (int i = 0; i < keepList.Count - 1; i++)
        {
            //만약 정렬한 리스트 중 현재 요소와 다음요소의 차이가 1일 때 straight_C를 올려준다.
            if (keepList[i + 1] - keepList[i] == 1)
            {
                straight_C++;
            }
        }
        if (straight_C == 3)
        {
            diceState = HandRank.S_Straight;
            if (!currentRankList.Contains(diceState)) { currentRankList.Add(diceState); }
        }
        if (straight_C == 4)
        {
            diceState = HandRank.L_Straight;
            if (!currentRankList.Contains(diceState)) { currentRankList.Add(diceState); }
        }
        if (keepList.SequenceEqual(execptList))
        {
            diceState = HandRank.noScore;
        }
        //가이드라인
        myHandRank = diceState;
        //미션 클리어 체크
        diceMissionManager.MissionClearCheck();
        //검사 값 적용
        ScoreBarValue = handRankScore[(int)diceState];
        //만약 dicestate의 값에따라 해당되는 가이드라인 포인트의 색이 바뀐다. 
        //내 가이드 라인의 위치를 조정해준다.

        if ((int)diceState > 0)
        {
            myHandRankFlagObj.anchoredPosition = guideLine[(int)diceState - 1].anchoredPosition;
            myHandRankFlagImg.sprite = myHandRankSprite[(int)diceState - 1];
            if (myHandRankFlagObj.anchoredPosition == rivalHandRankFlagObj.anchoredPosition)
            {
                rivalHandRankFlagImg.fillAmount = 0.5f;
            }
            else { rivalHandRankFlagImg.fillAmount = 1.0f; }

            if (!myHandRankFlagObj.gameObject.activeSelf) { myHandRankFlagObj.gameObject.SetActive(true); }
        }
        //초기화 해줘야할 값 : count_Dic , pair_C,threekind_C,straight_C
        straight_C = 0;
        pair_C = 0;
        threekind_C = 0;
        count_Dic.Clear();
    }
    #endregion
    
    #region 초이스 이벤트
    //choice 버튼 클릭시 
    IEnumerator OnChoice()
    {
        yield return delayTime_spGet;
        ChoiceEvent((int)myHandRank);
    }
    //choice버튼 클릭시 각 State마다 이벤트 발생 
    public void ChoiceEvent(int stateNum)
    {
        if (!InGameInfoManager.Instance.isPVPMode)
        {
            //SP 추가
            InGM.Instance.SP += handRankScore[stateNum];
            //dice미션 보상 주기 
            diceMissionManager.MissonReward();
            DiceInfoClearDelay(spGetDelayTime);
        }
        //전부 다 선택하면 배치시간으로 넘어갈 준비가 됬다고 서버에 보내줌
        else 
        {

            //SP 추가
            PVPInGM.Instance.SP += handRankScore[stateNum];
            //dice미션 보상 주기 
            diceMissionManager.MissonReward();
            //영상용
            PVPInGM.Instance.SP += 200;
            if (PVPInGM.Instance.pvpStageState == PVPStageState.RollTime)
            { 
                BackEndMatchManager.Instance.SendDataToInGame(new Protocol.StartAssignedTimeMessage(true));
            }
        }
    }
  
    #endregion

    #region 다이스 화면 변화
    //다이스 버튼 이미지에 선택한 주사위 리스트의 숫자에 맞는 이미지를 전송
    //일단 text에 string전송으로 함
    private void DiceRollEffect()
    {
        for (int i = 0; i < diceNum_List.Count; i++)
        {
            dices[i].RollSelf(diceNum_List[i]);
        }
    }
    public void DiceUISwap()
    {
        //주사위를 돌릴수 있는 상태가 되야 버튼을 누를 수 있게한다.
        if (isDiceUISwap)
        {
            if (myDiceUI.activeSelf)
            {
                rivalDiceUI.SetActive(true);
                myDiceUI.SetActive(false);
                diceUISwapBtnImg.sprite = swapBtnSprites[1];
                
            }
            else
            {
                rivalDiceUI.SetActive(false);
                myDiceUI.SetActive(true);
                diceUISwapBtnImg.sprite = swapBtnSprites[0];
            }
        }
    }
    #endregion

    #region PVP 시 상대 다이스 정보 등록 이벤트
    public void SetRivalDiceArr(SendDiceArrMessage msg)
    {
        //상대 다이스 스코어만 받음
        if (msg.playerSession != InGameInfoManager.Instance.mySessionID)
        {
            if (rivalNoRollImg.activeSelf) { rivalNoRollImg.SetActive(false); }

            int[] rivalDiceArr = msg.diceArr;
            //이미지 적용 
            for (int i = 0; i < rivalDiceArr.Length; i++)
            {
                rivalImgs[i].sprite = rivalDiceSprite[rivalDiceArr[i] - 1];
            }
        }
    }
    //받은 상대의 다이스 정보를 상대 다이스 UI에 뿌려준다.
    public void RivalSelectDiceInfo(DiceSelectMessage msg)
    {
        //상대 다이스 스코어만 받음
        if (msg.playerSession != InGameInfoManager.Instance.mySessionID)
        {
            //상대가 주사위를 선택할 시 그 주사위 넘버에 선택 효과 부여 
            for (int i = 0; i < msg.keepDiceCount; i++)
            { 
                rivalImgs[i].color = selectColor;   
            }
            //만약 주사위를 고른 뒤 나온 족보가 원페어 이상일 때 가이드라인에 표시
            if (msg.diceStateNum != 0)
            {
                rivalHandRankFlagObj.anchoredPosition = guideLine[msg.diceStateNum - 1].anchoredPosition;
                rivalHandRankFlagImg.sprite = rivalHandRankSprite[msg.diceStateNum - 1];
                if ((int)myHandRank == msg.diceStateNum)
                {
                    rivalHandRankFlagImg.fillAmount = 0.5f;
                }
                else
                {
                    rivalHandRankFlagImg.fillAmount = 1.0f;
                }
                if (!rivalHandRankFlagObj.gameObject.activeSelf) { rivalHandRankFlagObj.gameObject.SetActive(true); }

            }
        }
    }
    
    //상대 다이스 정보를 보여주기위해 딜레이를 준다.
    public void DiceInfoClearDelay(float delaytime)
    {
        Invoke("DiceInfoClear", delaytime);
    }
    //게임 스테이트가 넘어갈 때 내 주사위 정보와 상대방 주사위 정보를 초기화 해준다.
    public void DiceInfoClear()
    {
    
        //다이스 배열 및 회전 횟수 초기화
        diceNum_List = first_list;
        storage_List.Clear();
        keepDiceList.Clear();
        currentRankList.Clear();
        DICEROLL_CNT = firstRollCnt;


        //다이스 저장상태 및 효과 초기화
        for (int i = 0; i < dices.Length; i++)
        {
            dices[i].myImage.color = Color.white;
            dices[i].isClick = true;

            //다이스 ready이미지 활성화
            dices[i].readyText.SetActive(true);
            //선택 초기화
            onDiceChoose[i]=false;
        }
        //족보 상황과 족보 표시 텍스트 초기화
        myHandRank = HandRank.noScore;
        isDiceRoll = true;
        //다이스 가이드라인 초기화
        for (int i = 0; i < guideLine.Length; i++)
        {
            guideLine[i].gameObject.SetActive(true);
        }
        //다이스 미션 값 초기화
        diceMissionManager.DiceMissionInitialize();

        if (InGameInfoManager.Instance.isPVPMode)
        {
            //이미지 초기화
            for (int i = 0; i < rivalImgs.Length; i++)
            {
                //할당해준 이미지 초기화  
                rivalImgs[i].sprite = null;
                rivalImgs[i].color = Color.white;
            }
            myHandRankFlagObj.gameObject.SetActive(false);
            rivalHandRankFlagObj.gameObject.SetActive(false);

        }
        else
        {   //State 변경
            InGM.Instance.timeCheck_Text.gameObject.SetActive(true);
            InGM.Instance.currentTime = InGM.Instance.assignedTime;
            InGM.Instance.stageState = StageState.AssignedTime;

            //소환 UI를 켜주고 DiceUI를 꺼준다.
            InGameUIManager.Instance.SwapPlayFunc(false);
        }
    }
    #endregion

    #region 라운드가 업될때 다이스 이벤트 
    private void DiceRoundUPEvent()
    {
        //SP초기화
        if (InGameInfoManager.Instance.isPVPMode) 
        {
            //PVPInGM.Instance.SP = 0;
            rivalNoRollImg.SetActive(true);
            rivalHandRankFlagImg.fillAmount = 1.0f;
            rivalDiceUI.SetActive(false);
            myDiceUI.SetActive(true);
            
            //스왑 버튼 UI P1으로 바꿔줌
            diceUISwapBtnImg.sprite = swapBtnSprites[0];
        }
        else { InGM.Instance.SP = 0; }


        //룰UI가 활성화 되어있다면 꺼준다.
        if (ruleUIObj.activeSelf) { ruleUIObj.SetActive(false); }
        //SPMax값 초기화
        spSlider.maxValue = handRankScore[handRankScore.Length - 1];
        ScoreBarValue = 0;
    }

    #endregion


}