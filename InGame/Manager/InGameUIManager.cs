using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BackEnd;
using BackEnd.Tcp;


public class InGameUIManager : MonoBehaviour
{
    private static InGameUIManager _instance;
    public static InGameUIManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(InGameUIManager)) as InGameUIManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(InGameUIManager)) as InGameUIManager;
                }
            }
            return _instance;
        }
    }
    [Header("SP 별 테두리")]
    public Sprite[] spBorderline;

    [Header("배경 세팅")]
    public Image pvpBackGround;
    public RectTransform funcAreaRect;

     private RectTransform bg;
     private Image bg_Sky;
     private Image bg_BackObj;
     private Image bg_GameField;
     private Image bg_FrontObj;
        

    [Header("에러 관련")]
     public GameObject errorObj;
     public Button errorBtn;
    [SerializeField] private Text errorText;
    [SerializeField] private Text errorBtnText;

    [Header("승패UI")]
    public GameObject winUI;
    public GameObject loseUI;
    public GameObject drawUI;

    [Header("플레이 기능 스왑")]
    public GameObject diceArea;
    public GameObject summon_SKill_Area;
    public GameObject BattleUI;

    [Header("상대 정보 UI")]
    public GameObject rivalInfoUI;
    [SerializeField] private GameObject rivalInfoBtn;
    
    [SerializeField] private Text playerNickNameText;
    [SerializeField] private Text rivalPointText;
    [SerializeField] private Text[] rivalNickNameText;
    private int[] rivalNumArr;
    private bool[] isRegist;
    [System.Serializable]
    struct RivalUIinfo
    {
        public Image rivalCharImage;
        public Image spBorderLine;
        public Text spText;
        public Text outGameLevelText;
        public Image[] inGameLevelStar;
    }
    [SerializeField] private RivalUIinfo[] rivalUIInfos;
    [Header("전투 시 UI 관련")]
    [SerializeField]private BattleUIInfo[] battleUIInfos;

    [System.Serializable]
    struct BattleUIInfo
    {
        public Image charImage;
        public GameObject gaugeBar;
        public Image spBorderLine;
        public Text spText;
        public Text outGameLevelText;
        public Image[] inGameLevelStar;
        public GameObject dieEffect;
    }


    [HideInInspector] public Slider[] battleUICharHPSliders;
    [HideInInspector] public Slider[] battleUICharMPSliders;
    [Header("텍스트 팝업 관련 ")]
    public TextPopUpManager textPopUpManager;
    // Start is called before the first frame update
    void Start()
    {
        //유저 정보세팅
        if (InGameInfoManager.Instance.isPVPMode)
        {
            pvpBackGround.sprite = InGameInfoManager.Instance.pvpBackGround;
            isRegist = new bool[rivalUIInfos.Length];
            rivalNumArr = new int[rivalUIInfos.Length];
            SetUserInfo();
            battleUICharHPSliders = new Slider[battleUIInfos.Length];
            battleUICharMPSliders = new Slider[battleUIInfos.Length];
            for (int i = 0; i < battleUIInfos.Length; i++)
            {
                battleUICharHPSliders[i] = battleUIInfos[i].gaugeBar.transform.GetChild(0).GetComponent<Slider>();
                battleUICharMPSliders[i] = battleUIInfos[i].gaugeBar.transform.GetChild(1).GetComponent<Slider>();
            }
           
        }
        else
        {
            //배경 세팅
            bg_Sky.sprite = InGameInfoManager.Instance.selectBackGround.bg_Sky;
            bg_BackObj.sprite = InGameInfoManager.Instance.selectBackGround.bg_BackObj;
            bg_GameField.sprite = InGameInfoManager.Instance.selectBackGround.bg_GameField;
            bg_FrontObj.sprite = InGameInfoManager.Instance.selectBackGround.bg_FrontObj;
        }
        rivalInfoUI.SetActive(false);
    }
    #region "상대의 정보(닉네임,덱 정보)입력" 
    private void SetUserInfo()
    {
        foreach (var record in BackEndMatchManager.Instance.gameRecords)
        {
            if (record.Key == InGameInfoManager.Instance.mySessionID)
            {
                playerNickNameText.text = record.Value.m_nickname;
            }
            //상대 덱 정보 보여주기
            if (record.Key == InGameInfoManager.Instance.rivalSessionID)
            {
                for (int i = 0; i < rivalNickNameText.Length; i++)
                {
                    //닉네임 세팅
                    rivalNickNameText[i].text = record.Value.m_nickname;
                }
                //라이벌 포인트 세팅
                rivalPointText.text = string.Format("포인트 : {0}", record.Value.m_points);
                for (int j = 0; j < InGameInfoManager.Instance.pvpCharctorDIc[record.Key].Count; j++)
                {
                    if (InGameInfoManager.Instance.pvpCharctorDIc[record.Key][j].charIconSubType == CharIconType.Support) 
                    {
                        RivalDeckInfoSet(j, 7, record.Key);
                    }
                    else
                    {
                        switch (InGameInfoManager.Instance.pvpCharctorDIc[record.Key][j].charIconType)
                        {
                            case CharIconType.MeleeCharactor:
                                RivalDeckInfoSet(j, 1, record.Key);
                                break;
                            case CharIconType.ADCharactor:
                                RivalDeckInfoSet(j, 3, record.Key);
                                break;
                            case CharIconType.TankCharactor:
                                RivalDeckInfoSet(j, 5, record.Key);
                                break;
                        }
                    }
                }
            }
        }
    }
    private void RivalDeckInfoSet(int poolnum,int maxImgNum,SessionId sessionId)
    {
        for (int j = maxImgNum - 1; j < maxImgNum + 1; j++)
        {
            if (!isRegist[j])
            {
                rivalNumArr[poolnum] = j;
                //덱 이미지 세팅
                rivalUIInfos[j].rivalCharImage.sprite = InGameInfoManager.Instance.pvpCharctorDIc[sessionId][poolnum].itemImage;
                int borderlineNum = (int)InGameInfoManager.Instance.pvpCharctorDIc[sessionId][poolnum].itemGrade;
                rivalUIInfos[j].spBorderLine.sprite = spBorderline[borderlineNum];
                //SP 정보 설정
                rivalUIInfos[j].spText.text = InGameInfoManager.Instance.pvpCharctorDIc[sessionId][poolnum].itemSP.ToString();
                int outgameLevel = InGameInfoManager.Instance.pvpCharctorDIc[sessionId][poolnum].itemLevel;
                rivalUIInfos[j].outGameLevelText.text = string.Format("LV. {0}", outgameLevel);
                //별 UI변경
                for (int k = 0; k < rivalUIInfos[j].inGameLevelStar.Length; k++)
                {
                    rivalUIInfos[j].inGameLevelStar[k].sprite = PVPCharManager.Instance.noLevelStar;
                }
                rivalUIInfos[j].inGameLevelStar[0].transform.parent.gameObject.SetActive(true);
                isRegist[j] = true;
                break;
            }
        }
    }
    //라이벌 레벨 세팅
    public void SetRivalIngameLevel(PVPCharactor rivalChar)
    {
        int uiNum = rivalNumArr[rivalChar.charPoolNum];
        for (int i = 0; i < rivalUIInfos[uiNum].inGameLevelStar.Length; i++)
        {
            if (i < rivalChar.CHARLEVEL)
            {
                rivalUIInfos[uiNum].inGameLevelStar[i].sprite = PVPCharManager.Instance.levelStar;
            }
            else
            {
                rivalUIInfos[uiNum].inGameLevelStar[i].sprite = PVPCharManager.Instance.noLevelStar;
            }
        }
    }
    
   
    #endregion

    #region UI전환 
    //diceUIOn 가 False일 때 다이스 UI(가이드라인)를 꺼주고 캐릭터 생성창을 켜준다.
    //diceUIOn 가 True일 때 캐릭터 생성창을 꺼주고 다이스 UI를 켜준다.
    public void SwapPlayFunc(bool diceUIOn)
    {
        if (diceUIOn==false)
        {
            diceArea.SetActive(false);
            summon_SKill_Area.SetActive(true);
        }
        else 
        {
            diceArea.SetActive(true);
            summon_SKill_Area.SetActive(false);
        }
    }
    #endregion

    #region "Battle UI 관련"
   
    //BattleUI의 캐릭터 정보 등록
    public void SetBattleUIInfo(int num,IconData icondata)
    {
        battleUIInfos[num].charImage.sprite = icondata.itemImage;
        battleUIInfos[num].outGameLevelText.text = string.Format("LV. {0}", icondata.itemLevel);
        battleUIInfos[num].spText.text = icondata.itemSP.ToString();
        battleUIInfos[num].spBorderLine.sprite = spBorderline[(int)icondata.itemGrade];
    }
    //BattleUI가 활성화 하기 전 캐릭터의 정보(인게임 레벨, 소환 여부) 등록
    public void SetCharStatUIInfo()
    {
        for (int i = 0; i < PVPCharManager.Instance.summonList.Count; i++)
        {
            int poolnum = PVPCharManager.Instance.summonList[i].charPoolNum;
            int uiNum = PVPCharManager.Instance.charNumArr[poolnum];
            //이미 소환된 상태 
            if (PVPCharManager.Instance.summonList[i].gameObject.activeSelf)
            {
                //별 갯수 부여 
                for (int j = 0; j < battleUIInfos[i].inGameLevelStar.Length; j++)
                {
                    if (j < PVPCharManager.Instance.summonList[i].CHARLEVEL)
                    {
                        battleUIInfos[uiNum].inGameLevelStar[j].sprite = PVPCharManager.Instance.levelStar;
                    }
                    else
                    {
                        battleUIInfos[uiNum].inGameLevelStar[j].sprite = PVPCharManager.Instance.noLevelStar;
                    }
                }
                battleUIInfos[uiNum].inGameLevelStar[0].transform.parent.gameObject.SetActive(true);

                battleUICharHPSliders[uiNum].maxValue = PVPCharManager.Instance.summonList[i].hpSlider.maxValue;
                battleUICharHPSliders[uiNum].value = PVPCharManager.Instance.summonList[i].CHARHP;

                battleUIInfos[uiNum].gaugeBar.SetActive(true);
                battleUIInfos[uiNum].spBorderLine.color = Color.white;
                battleUIInfos[uiNum].charImage.color = Color.white;
            }
            else
            {
                battleUIInfos[uiNum].inGameLevelStar[0].transform.parent.gameObject.SetActive(false);
                battleUIInfos[uiNum].gaugeBar.SetActive(false);
                battleUIInfos[uiNum].spBorderLine.color = PVPCharManager.Instance.noBuyColor;
                battleUIInfos[uiNum].charImage.color = PVPCharManager.Instance.noBuyColor;
            }
        }
    }
    //전투 중 아군 캐릭터가 죽었을 때 효과
    public void CharDieUIEffect(int num)
    {
        battleUIInfos[num].dieEffect.SetActive(true);
        battleUIInfos[num].gaugeBar.SetActive(false);
    }
    //BattleUI에 적용한 정보 초기화
    public void BattleUIInitialize()
    {
        for (int i =0; i < battleUIInfos.Length; i++)
        {
            battleUIInfos[i].inGameLevelStar[0].transform.parent.gameObject.SetActive(false);
            battleUIInfos[i].spBorderLine.color = PVPCharManager.Instance.noBuyColor;
            battleUIInfos[i].charImage.color = PVPCharManager.Instance.noBuyColor;
            battleUIInfos[i].dieEffect.SetActive(false);
            battleUIInfos[i].gaugeBar.SetActive(false);
        }
    }

    #endregion
    //캐릭터 텍스트 팝업 (데미지(일반, 크리티컬), 힐 , 기타 상태 표시)
    //에러메세지 입력
    public void SetErrorMessage(string msg)
    {
        errorText.text = msg;
        errorObj.SetActive(true);
    }
    //메뉴로 돌아가기 버튼 기능
    public void GotoMenuClick()
    {
        Debug.Log("메뉴씬으로");
        if (Time.timeScale != 1)
        {
            Time.timeScale = 1;
        }
        GameManager.Instance.gameState = GameManager.GameState.MenuScene;
        BackEndMatchManager.Instance.GameOutInitialize(false);

        Destroy(InGameInfoManager.Instance.transform.gameObject);
        MenuLoadingSceneManager.LoadingtoNextScene("1. MenuScene");
    }
    //게임 종료 버튼
    public void ExitBtnClick()
    {
        Application.Quit();
    }
}
