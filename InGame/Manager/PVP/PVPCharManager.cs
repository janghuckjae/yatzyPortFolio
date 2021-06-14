using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PVPCharManager : MonoBehaviour
{
    private static PVPCharManager _instance;
    public static PVPCharManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(PVPCharManager)) as PVPCharManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(PVPCharManager)) as PVPCharManager;
                }
            }
            return _instance;
        }
    }
    [Header("받아온 데이터 관련")]
    public IconData[] charDatas;

    [Header("캐릭터 소환 버튼 관련")]
    public Color noBuyColor;
    [SerializeField] private int maxLevel;
    [System.Serializable]
    public struct SummonButton
    {
        public GameObject resurrectionUI;
        public Text resurrectionSPText;
        public Text spText;
        public Text outGameLevelText;
        public Button charBtn;
        public Image spBorderLine;
        public Image charBtnImage;
        public Image[] charLevelStars;
        public int charSP;
        [HideInInspector]public bool isRegist;
    }
    [SerializeField]SummonButton[] summonButtons;
    //소환 가능 여부 
    //캐릭터 버튼은 캐릭터 소환 후  소환한 캐릭터의 강화 버튼으로 사용 되기 때문에 bool함수로 소환 여부 체크
    [HideInInspector] public bool[] isSummon;
    //소환 가능 불가능 여부 
    [HideInInspector] public bool[] isBuy;
    //소환한 아군 리스트
    [HideInInspector] public List<PVPCharactor> summonList = new List<PVPCharactor>();

    [HideInInspector] public int[] charNumArr;
    private GameObject t_obj;
    //임시로 저장할 캐릭터 스크립트
    private PVPCharactor myChar;
    private PVPCharactor noBuyChar;
    
    
    [Header("캐릭터 강화 관련")]
    public Sprite levelStar;
    public Sprite noLevelStar;



    

    // Start is called before the first frame update
    void Awake()
    {
        //PVP모드가 아니면 활동정지
        if (!InGameInfoManager.Instance.isPVPMode)
        {
            this.enabled = false;
        }
        else
        {
            //게임데이터매니저에서 데이터를 받아오고 적용해준다.
            charDatas = InGameInfoManager.Instance.pvpCharctorDIc[InGameInfoManager.Instance.mySessionID].ToArray();
            isSummon = new bool[summonButtons.Length];
            isBuy = new bool[summonButtons.Length];
            charNumArr = new int[summonButtons.Length];
            //덱 위치에 따라 이미지 및 정보 값 대입 
            for (int i = 0; i < charDatas.Length; i++)
            {
                if (charDatas[i].charIconSubType == CharIconType.Support)
                {
                    SummonButtonDataSet(i, 7);
                }
                else
                {
                    switch (charDatas[i].charIconType)
                    {
                        case CharIconType.MeleeCharactor:
                            SummonButtonDataSet(i, 1);
                            break;
                        case CharIconType.ADCharactor:
                            SummonButtonDataSet(i, 3);
                            break;
                        case CharIconType.TankCharactor:
                            SummonButtonDataSet(i, 5);
                            break;
                    }
                }
               
            }
            
        }
       
    }

    private void Start()
    {
        //델리게이트에 이벤트 등록 
        PVPInGM.Instance.roundChangeChain += FriendRoundUpEvent;
    }
    //버튼에 관련된거만 해당 코스트 라인의 버튼에 할당해준다.
    // j는 버튼 넘버이다.
    void SummonButtonDataSet(int charPoolNum, int maxBtnNum)
    {
        int _i = charPoolNum;
        for (int j = maxBtnNum - 1; j < maxBtnNum + 1; j++)
        {
            if (!summonButtons[j].isRegist)
            {
                //SP, 쿨타임, 아이콘이미지 값을 보내준다.
                //부활 SP적용
                charNumArr[charPoolNum] = j;

                //Debug.Log("풀넘버 " + charPoolNum + ",,값 " + j);
                summonButtons[j].charSP = charDatas[charPoolNum].itemSP;
                summonButtons[j].resurrectionSPText.text = string.Format("{0} SP", charDatas[charPoolNum].itemSP);
                summonButtons[j].resurrectionUI.SetActive(false);
                summonButtons[j].charBtnImage.color = charDatas[charPoolNum].color;
                summonButtons[j].charBtnImage.sprite = charDatas[charPoolNum].itemImage;
                summonButtons[j].outGameLevelText.text = string.Format("LV. {0}",charDatas[charPoolNum].itemLevel);
                summonButtons[j].spBorderLine.sprite = InGameUIManager.Instance.spBorderline[(int)charDatas[charPoolNum].itemGrade];
                summonButtons[j].charLevelStars[0].transform.parent.gameObject.SetActive(true);
                //battleUI이미지 적용
                InGameUIManager.Instance.SetBattleUIInfo(j,charDatas[charPoolNum]);
                // 버튼클릭 이벤트 등록
                summonButtons[j].charBtn.onClick.AddListener(() => SummonEvent(_i));
                summonButtons[j].spText.text = string.Format("{0}", charDatas[charPoolNum].itemSP);
                isSummon[j] = true;
                summonButtons[j].isRegist = true;
                CharEnhance(charPoolNum, 0);
                break;
            }
        }
    }

    //평소에 살수 있는지 없는지 탐지
    public void NoBuySearch()
    {
        if (PVPInGM.Instance.pvpStageState == PVPStageState.RollTime || PVPInGM.Instance.pvpStageState == PVPStageState.AssignedTime)
        {
            for (int i = 0; i < charDatas.Length; i++)
            {
                noBuyChar = PVPCharPoolingManager.Instance.GetUnitInfo(i);

                //소환 된 유닛이라면 
                if (!isSummon[charNumArr[i]])
                {
                    if (((summonButtons[charNumArr[i]].charSP * 0.4) + (2 * (noBuyChar.CHARLEVEL - 1))) > PVPInGM.Instance.SP)
                    {
                        summonButtons[charNumArr[i]].charBtnImage.color = noBuyColor;
                        summonButtons[charNumArr[i]].spBorderLine.color = noBuyColor;
                        isBuy[charNumArr[i]] = false;
                    }

                    else
                    {
                        summonButtons[charNumArr[i]].charBtnImage.color = Color.white;
                        summonButtons[charNumArr[i]].spBorderLine.color = Color.white;
                        isBuy[charNumArr[i]] = true;
                    }
                }
                //아직 소환 하기 전 이라면 
                else
                {
                    if ((summonButtons[charNumArr[i]].charSP > PVPInGM.Instance.SP))
                    {
                        summonButtons[charNumArr[i]].charBtnImage.color = noBuyColor;
                        summonButtons[charNumArr[i]].spBorderLine.color = noBuyColor;
                        isBuy[charNumArr[i]] = false;
                    }

                    else
                    {
                        summonButtons[charNumArr[i]].charBtnImage.color = Color.white;
                        summonButtons[charNumArr[i]].spBorderLine.color = Color.white;
                        isBuy[charNumArr[i]] = true;
                    }

                }
            }
        }
    }

    //캐릭터 버튼을 클릭할 때 알맞은 캐릭터 생성 
    //클릭 할 때 캐릭터버튼에 맞는 SP 소모 
    public void SummonEvent(int charPoolNum)
    {
        //배치 시간일 때 시도 하면 바로 리턴
        if (PVPInGM.Instance.pvpStageState != PVPStageState.AssignedTime || PVPInGM.Instance.isBattleReady)
        {
            return;
        }

        //유닛의 정보를 얻는다.
        myChar = PVPCharPoolingManager.Instance.GetUnitInfo(charPoolNum);
        int enhanceSP = (int)((summonButtons[charNumArr[charPoolNum]].charSP * 0.4)+(2 * (myChar.CHARLEVEL - 1)));
        //클릭한 버튼에 맞는 SP 차감 
        // 캐릭터 부활 , 캐릭터 소환 시 
        //부활, 캐릭터 소환 비용은 캐릭터의 초기 소환 비용이다.
        if (PVPInGM.Instance.SP >= summonButtons[charNumArr[charPoolNum]].charSP && isSummon[charNumArr[charPoolNum]] == true)
        {
            //게임 씬에서 캐릭터의  생성 위치를 3분할 하여 배정한다.
            t_obj = PVPCharPoolingManager.Instance.GetUnit(charPoolNum);
            //캐릭터를 리스트에 저장하여 직업별로 정렬해 배치
            CharSetPos(t_obj);
            //소환 후 소환 창을 강화 창으로 만들기 위하여 bool 함수를 사용하였다.
            isSummon[charNumArr[charPoolNum]] = false;
            //만약 부활 UI가 켜져있다면 꺼준다.
            if (summonButtons[charNumArr[charPoolNum]].resurrectionUI.activeSelf)
            {
                summonButtons[charNumArr[charPoolNum]].resurrectionUI.SetActive(false);
            }
            //처음 캐릭터 소환 할 때 
            else
            {
                BackEndMatchManager.Instance.SendDataToInGame(new Protocol.UnitLevelUPMessage(InGameInfoManager.Instance.mySessionID, charPoolNum));
                myChar.CHARLEVEL++;
                //강화 비용을 보여 주기위해 텍스트의 SP값 변경
                summonButtons[charNumArr[charPoolNum]].spText.text = ((summonButtons[charNumArr[charPoolNum]].charSP * 0.4) + (2 * (myChar.CHARLEVEL-1))).ToString();
            }
            //SP 차감 (소환 초기 비용)
            PVPInGM.Instance.SP -= summonButtons[charNumArr[charPoolNum]].charSP;
            //서버에 캐릭터 소환 정보 보내기
            BackEndMatchManager.Instance.SendDataToInGame(new Protocol.AssignedCharactorMessage(InGameInfoManager.Instance.mySessionID, charPoolNum));

        }
        //만약 캐릭터가 이미 소환 되어있고 소지SP가 레벨별 강화 비용보다 높다면 
        else if (isSummon[charNumArr[charPoolNum]] == false && PVPInGM.Instance.SP >= enhanceSP && myChar.CHARLEVEL <maxLevel)
        {
           
            //캐릭터 강화 정보 보내기
            BackEndMatchManager.Instance.SendDataToInGame(new Protocol.UnitLevelUPMessage(InGameInfoManager.Instance.mySessionID, charPoolNum));
            myChar.CHARLEVEL++;
            PVPInGM.Instance.SP -= enhanceSP;
            if (myChar.CHARLEVEL == maxLevel)
            {
                summonButtons[charNumArr[charPoolNum]].spText.text = "MAX";
            }
            else { summonButtons[charNumArr[charPoolNum]].spText.text = ((summonButtons[charNumArr[charPoolNum]].charSP * 0.4) + (2 * (myChar.CHARLEVEL - 1))).ToString(); }
        }
    }

    void CharSetPos(GameObject charObj)
    {
        //캐릭터 스크립트 저장
        summonList.Add(charObj.transform.GetComponent<PVPCharactor>());

        //정렬(탱,근,원 순으로 정렬)
        summonList = summonList.OrderBy(x => (int)x.myType).ToList();

        //소환할 숫자에 따라 배치 변경
        PVPInGM.Instance.AssignedPos(summonList, PVPInGM.Instance.myUnitPos);
        
    }
    //아군이 이겼을 때 이벤트 
    private void FriendRoundUpEvent()
    {
        if (summonList.Count != 0)
        {
            for(int i=0; i<summonList.Count; i++ )
            {
                summonList[i].gameObject.SetActive(false);
            }
        }
    }
    public void SetFriend()
    {
        if (summonList.Count != 0)
        {
            //정렬(탱,근,원 순으로 정렬)
            summonList = summonList.OrderBy(x => (int)x.myType).ToList();

            PVPInGM.Instance.AssignedPos(summonList, PVPInGM.Instance.myUnitPos);
            for (int i = 0; i < summonList.Count; i++)
            {
                summonList[i].gameObject.SetActive(true);
            }
        }
    }
    //만약 캐릭터가 죽었을 때 UI및 상태 이벤트
    //
    public void CharSummonOn(int charPoolNum)
    {
        isSummon[charNumArr[charPoolNum]] = true;
        //재 소환을 위해 UI수정
        summonButtons[charNumArr[charPoolNum]].resurrectionUI.SetActive(true);
        
    }
    //캐릭터 강화 시 UI및 상태 이벤트 (별의 갯수 등)
    public void CharEnhance(int charPoolNum,int charLevel)
    {
        for (int i = 0; i < maxLevel; i++)
        {
            if (i < charLevel)
            {
                summonButtons[charNumArr[charPoolNum]].charLevelStars[i].sprite = levelStar;
            }
            else
            {
                summonButtons[charNumArr[charPoolNum]].charLevelStars[i].sprite = noLevelStar;
            }
        }
    }
}
