
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
public enum GatchaSkillType : int
{

    RepeatRangeSkill,      //N초마다 발동 되어 아군 또는 적군 중 전체에게 적용되는 가챠 스킬
    RepeatPartSkill,    //N초마다 발동 되어 아군 또는 적군 중 N 마리에게 적용되는 가챠 스킬
    KeepAllSkill,       //라운드가 끝날 때까지 아군 또는 적군 중 전체에게 적용되는 가챠 스킬 
    KeepPartSkill,      //라운드가 끝날 때까지 아군 또는 적군 중 N 마리에게 적용되는 가챠 스킬 

}
public class SkillGatchaManager : MonoBehaviour
{

    private static SkillGatchaManager _instance;

    public static SkillGatchaManager Instance // 싱글톤 구현
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(SkillGatchaManager)) as SkillGatchaManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(SkillGatchaManager)) as SkillGatchaManager;
                }
            }
            return _instance;
        }
    }

    [HideInInspector] public GatchaSkill rivalGatchaSkill;
    [HideInInspector] public int gatchaTryPoint = 1;
    
    [Header("가챠 비용")]
    //가챠를 시도한 횟수
    //가챠 가격
    [SerializeField] private int firstGatchaCost = 10;
    [SerializeField] private int currentGatchaCost;

    [Header("가챠  UI 관련")]
    //스킬 가챠 UI 
    public GameObject skillGatchaUI;
    //스킬 가챠 오픈버튼 
    [SerializeField] private Button skillGatchaOpenBtn;
    //가챠 버튼 
    [SerializeField] private GameObject skillGatchaBtnObj;
    private Button skillGatchaBtn;
    private TextMeshProUGUI skillGatchaBtnText;
    //선택 버튼
    [SerializeField] private float cardRoateDelayTime;
    WaitForSeconds roateDelayTime;
    [Header("가챠 스킬 정보 관련")]
    //가챠 스킬 카드
    [SerializeField]public GatchaSkillCard[] gatchaSkillCards;
   
    //상단 가챠 이미지
    [SerializeField] private Image myGatchaSkillUIImg;
    [SerializeField] private Image[] rivalGatchaSkillUIImg;
    //임시 선택만 한 가챠 스킬 
    readonly Dictionary<GatchaSkill, int> gatchaSkillDic = new Dictionary<GatchaSkill, int>();

    //획득한 가챠 스킬 
    private GatchaSkill myGatchaSkill;
    private GameObject myGatchaSkillobj;
    private GameObject rivalGatchaSkillobj;
    private Sprite emptyGatchaSkillImg;


    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < GameDataManager.Instance.gatchaSkills.Count; i++)
        {
            gatchaSkillDic.Add(GameDataManager.Instance.gatchaSkills[i], i);
        }

        gatchaTryPoint = 1;

        roateDelayTime = new WaitForSeconds(cardRoateDelayTime);
        //가챠 돌리는 버튼 관련 
        skillGatchaBtn = skillGatchaBtnObj.transform.GetComponent<Button>();
        skillGatchaBtnText = skillGatchaBtnObj.transform.GetComponentInChildren<TextMeshProUGUI>();
        currentGatchaCost = firstGatchaCost;
        

        //상단 UI
        emptyGatchaSkillImg = myGatchaSkillUIImg.sprite; 
        //버튼이벤트 등록 (스킬창 여는 버튼)
        skillGatchaOpenBtn.onClick.AddListener(()=>OnSkillGatcha());
        //버튼이벤트 등록 (스킬가챠 돌리기)
        skillGatchaBtn.onClick.AddListener(() => SkillGatchaRoll());
    }

    //가챠 창 열기 
    public void OnSkillGatcha()
    {
        //만약 선택한 스킬이 있다면 리턴
        if (myGatchaSkill != null)
        {
            InGameUIManager.Instance.SetErrorMessage("이미 스킬을 선택하였습니다.");
            return;
        }
        if (PVPInGM.Instance.isBattleReady)
        {
            InGameUIManager.Instance.SetErrorMessage("이미 준비 완료상태입니다.");
            return;
        }
        if (PVPCharManager.Instance.summonList.Count == 0 && PVPCharManager.Instance.isBuy.Contains(true))
        {
            InGameUIManager.Instance.SetErrorMessage("캐릭터를 골라주세요.");
            return;
        }
        //스킬을 구매할 비용이 없다면 리턴
        if (PVPInGM.Instance.SP < currentGatchaCost * gatchaTryPoint)
        {
            InGameUIManager.Instance.SetErrorMessage("SP가 부족합니다.");
            return;
                
        }
        //스킬 가챠 UI의 active 에 따라 껏다키기
        skillGatchaUI.SetActive(true);
        if (gatchaTryPoint > 1)
        {
            skillGatchaBtnText.text = string.Format("Flip Again\n{0}SP", currentGatchaCost * gatchaTryPoint);
        }
        else
        {
            skillGatchaBtnText.text = string.Format("Flip\n{0}SP", currentGatchaCost * gatchaTryPoint);
        }
        
    }
    #region 가챠스킬 획득
    //가챠 스킬 획득 
    public void SkillGatchaRoll()
    {
        if (InGameInfoManager.Instance.isPVPMode)
        {
            if (!PVPInGM.Instance.isBattleReady)
            {
                if (PVPInGM.Instance.SP >= currentGatchaCost * gatchaTryPoint)
                {
                    PVPInGM.Instance.SP -= currentGatchaCost * gatchaTryPoint;

                    //스킬을 돌린다. 
                    //돌리는 이펙트가 나온 후 3개의 스킬을 랜덤으로 보여준다. 
                    for (int i = 0; i < gatchaSkillCards.Length; i++)
                    {
                        gatchaSkillCards[i].GATCHASKILL = GetGatchaSkill();
                    }
                    //애니메이션 효과 
                    StartCoroutine(RotateCard());
                    gatchaTryPoint++;
                    skillGatchaBtnText.text = string.Format("Flip Again\n{0}SP", currentGatchaCost * gatchaTryPoint);
                }
                else
                {
                    InGameUIManager.Instance.SetErrorMessage("SP가 모자랍니다.");
                    return;
                }
            }
            
            
        }
        else 
        {
            if (InGM.Instance.SP >= currentGatchaCost * gatchaTryPoint)
            {
                InGM.Instance.SP -= currentGatchaCost * gatchaTryPoint;

                //스킬을 돌린다. 
                //돌리는 이펙트가 나온 후 3개의 스킬을 랜덤으로 보여준다. 
                for (int i = 0; i < gatchaSkillCards.Length; i++)
                {
                    gatchaSkillCards[i].GATCHASKILL = GetGatchaSkill();
                }
                //애니메이션 효과 
                StartCoroutine(RotateCard());
                gatchaTryPoint++;
                skillGatchaBtnText.text = string.Format("{0}SP", currentGatchaCost * gatchaTryPoint);
            }
        }
        
    }
    IEnumerator RotateCard()
    {
        for (int i = 0; i < gatchaSkillCards.Length; i++)
        {
            gatchaSkillCards[i].RotateCard();
            yield return roateDelayTime;
        }
    }

    GatchaSkill GetGatchaSkill()
    {
        //가중치 랜덤으로 구성
        int totalweight = 0;
        int weight = 0;
        for (int i = 0; i < GameDataManager.Instance.gatchaSkills.Count; i++)
        {
            //가중치를 전부 더해줍니다.
            totalweight += GameDataManager.Instance.gatchaSkills[i].gatchaSkillInfo.gatchaWeight;
        }
        //통합 가중치에서 랜덤한 넘버를 뽑아줍니다.
        int selectNum = Mathf.RoundToInt(totalweight * Random.Range(0.0f, 1.0f));
        //전체 리스트 중에서 가중치 랜덤에 따라 스킬을 반환한다.
        for (int i = 0; i < GameDataManager.Instance.gatchaSkills.Count; i++)
        {
            weight += GameDataManager.Instance.gatchaSkills[i].gatchaSkillInfo.gatchaWeight;
            if (selectNum <= weight)
            {
                return GameDataManager.Instance.gatchaSkills[i];
            }
        }
        return null;
    }
    #endregion
    #region 가챠스킬 선택정보 관련
    public void ApplyGatchaSkill(GatchaSkill gatchaSkill)
    {
        //게임 데이터 매니저에있는 정보값을 Instantiate로 복사해서 사용해야한다.
        myGatchaSkillobj = Instantiate(gatchaSkill.gameObject);
        myGatchaSkill = myGatchaSkillobj.transform.GetComponent<GatchaSkill>();
        if (InGameInfoManager.Instance.isPVPMode)
        {
            //내 선택 목록 번호 서버에 보내기
            int selectNum = gatchaSkillDic[gatchaSkill];
            BackEndMatchManager.Instance.SendDataToInGame(new Protocol.GatchaSkillInfoMessage(InGameInfoManager.Instance.mySessionID, selectNum));
        }
        //상단 UI에 가챠스킬 이미지 등록
        myGatchaSkillUIImg.sprite = myGatchaSkill.gatchaSkillInfo.skillImg;
        for (int i = 0; i < gatchaSkillCards.Length; i++)
        {
            gatchaSkillCards.Initialize();
        }
        //스킬가챠 창 초기화
        skillGatchaUI.SetActive(false);
    }
    //상대가 가챠 스킬을 선택하면 받는 번호 
    public void RivalSelectGatchaSkill(int num)
    {
        var _key = gatchaSkillDic.FirstOrDefault(x => x.Value == num).Key;
        Debug.Log(_key.gameObject.name);
        rivalGatchaSkillobj = Instantiate(_key.gameObject);
        rivalGatchaSkill = rivalGatchaSkillobj.transform.GetComponent<GatchaSkill>();
        
        rivalGatchaSkillUIImg[0].sprite = rivalGatchaSkill.gatchaSkillInfo.skillImg;
        rivalGatchaSkillUIImg[1].sprite = rivalGatchaSkill.gatchaSkillInfo.skillImg;

        rivalGatchaSkill.isRivalSkill = true;
    }
    //스킬 가챠를 돌린 후 선택을 안한 상태로 캐릭터 배치 시간이 지나면 자동으로 3개 중 랜덤으로 선택하게 하게한다.
    public void AutoSelectGatchaSkill()
    {
        int randomValue = Random.Range(0, gatchaSkillCards.Length);

        if (gatchaTryPoint > 1 && myGatchaSkill ==null)
        {
            //랜덤으로 카드 선택
            gatchaSkillCards[randomValue].SelectGatchaSkill();
            Debug.Log("돈냈으면 스킬 가져가");
        }
    }
    #endregion

    #region 스킬(사용, 종료 , 초기화)
    //가챠 스킬을 사용해라
    public void DoGatchaSkill()
    {
        if (myGatchaSkill != null)
        {
            myGatchaSkill.DoSkill();
        }
        if (rivalGatchaSkill != null)
        {
            rivalGatchaSkill.DoSkill();
        }
        GatchaEndEvent();
    }
    


    //캐릭터 배치 시간이 끝나고 배틀시간에 진입 할 때 스킬 가챠 관련 초기화
    public void GatchaEndEvent()
    {
        
        skillGatchaUI.SetActive(false);
        //가챠 돌린 횟수를 초기화 해준다.
        gatchaTryPoint = 1;
        //가챠 버튼 모양을 다시 초기화 해준다.
        currentGatchaCost = firstGatchaCost;
    }

    //라운드가 업 될때 내가 가지고있는 리스트를 초기화
    public void GatchaSkillInitialize()
    {
        for (int i = 0; i < gatchaSkillCards.Length; i++)
        {
            //이미지 ,텍스트 등록   
            gatchaSkillCards[i].CardInitialized();
        }
        //가챠 스킬 오브젝트 파괴
        if (myGatchaSkill != null)
        {
            myGatchaSkill.Initialize();
            myGatchaSkill = null;
            myGatchaSkillUIImg.sprite = emptyGatchaSkillImg;
            Destroy(myGatchaSkillobj);
            myGatchaSkillobj = null;
        }
        if (rivalGatchaSkill != null)
        {
            rivalGatchaSkill.Initialize();
            rivalGatchaSkill = null;

            rivalGatchaSkillUIImg[0].sprite = emptyGatchaSkillImg;
            rivalGatchaSkillUIImg[1].sprite = emptyGatchaSkillImg;

            Destroy(rivalGatchaSkillobj);
            rivalGatchaSkillobj = null;
        }
    }
    #endregion
}
