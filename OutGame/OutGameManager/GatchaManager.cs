using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GatchaManager : MonoBehaviour
{
    private static GatchaManager _instance;
    public static GatchaManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(GatchaManager)) as GatchaManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(GatchaManager)) as GatchaManager;
                }
            }
            return _instance;
        }
    }
    //캔슬버튼 활성화 여부(false 이면 Skip버튼 활성화, true이면 Cancel버튼 활성화)
    private bool onCancel = false;
    public bool ONCANCEL
    {
        get { return onCancel; }
        set
        {
            onCancel = value;
            if (!onCancel)
            {
                //이벤트를 초기화 시켜주고
                skipORCancelBtn.onClick.RemoveAllListeners();
                //버튼에 스킵버튼 이벤트를 넣어준다.
                skipORCancelBtn.onClick.AddListener(skipBtnClick);
                skipORCancelText.text = "SKIP";
            }
            else
            {
                //이벤트를 초기화 시켜주고
                skipORCancelBtn.onClick.RemoveAllListeners();
                //버튼에 스킵버튼 이벤트를 넣어준다.
                skipORCancelBtn.onClick.AddListener(cancelBtnClick);
                skipORCancelText.text = "Cancel";
            }
        }
    }

    [Header("가챠 버튼")]
    [SerializeField] private Button oneGatchaBtn;
    [SerializeField] private Button tenGatchaBtn;
    [Header("스킵 닫기 버튼")]
    [SerializeField] private Button skipORCancelBtn;
    [SerializeField] private Text skipORCancelText;

    [Header("가챠 관련")]
    //가챠를 돌릴 데이터 목록
    [SerializeField] private List<IconData> gatchaList;
    private int totalWeight = 0;
    //총 가중치
    //랜덤하게 선택된 카드 데이터
    private List<IconData> resultData = new List<IconData>();
    //가챠 돌리는 횟수 
    [HideInInspector]public int gatchaCnt = 0;
    //가챠 구역
    public GameObject cardArea;
    [Header("카드")]
    //카드들  
    public Card[] cards;
    //카드를 뒤집은 숫자 체크
    [HideInInspector] public int rotatePoint=0;


    // Start is called before the first frame update
    void Start()
    {
        //가챠 돌린것 체크하는 카운트들을 초기화해준다.
        rotatePoint = 0;
        gatchaCnt = 0;

        //가챠를 돌릴 목록의 가중치들을 가져와 총 가중치를 구합니다.
        for (int i = 0; i < gatchaList.Count; i++)
        {
            totalWeight += gatchaList[i].gatchaWeight;
        }
        //카드 목록의 SetActive를 꺼준다.
        cardArea.SetActive(false);

        //카드들의 setActive를 꺼준다.
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].gameObject.SetActive(false);
        }
        //버튼 이벤트 할당
        //1번
        oneGatchaBtn.onClick.AddListener(()=>GatchaBtnClick(1));
        //10번
        tenGatchaBtn.onClick.AddListener(() => GatchaBtnClick(10));
        //ONCANCEL프로퍼티 초기화
        ONCANCEL = false;
    }
    #region "가챠 버튼 클릭 이벤트"
    void GatchaBtnClick(int cnt)
    {
        //가챠 버튼을 비활성화 시켜준다.
        oneGatchaBtn.gameObject.SetActive(false);
        tenGatchaBtn.gameObject.SetActive(false);
        gatchaCnt = cnt;
        //카드 목록의 SetActive를 켜준다.
        cardArea.SetActive(true);
        //가챠횟수 만큼 돌림
        for (int i = 0; i < cnt; i++)
        {
            //가챠해서 뽑은 데이터를 결과 데이터에 넣어준다.
            resultData.Add(Gatcha());
            //카드에 뽑은 데이터를 넣어준다.
            cards[i].CARDDATA = resultData[i];
            cards[i].gameObject.SetActive(true);
        }
        //다 돌렸다면 resultData 리스트에 있는 데이터의 해금 상태를 검사하여 인벤토리에 적용해준다.
        InventoryManager.Instance.ReleaseChar(resultData);
        GatchaReset();
    }
    //가챠 기능
    IconData Gatcha( )
    {
        int weight = 0;
        int selectNum = 0;
        //통합 가중치 값을 0에서 1사이의 랜덤한 float값 과 곱해주고 그걸 반올림 하여 Int로 바꾸어준다.
        selectNum = Mathf.RoundToInt(totalWeight * Random.Range(0.0f, 1.0f));
        //랜덤 값을 뽑아준 뒤 가챠 리스트의 숫자 만큼 검출 기능을 돌려줍니다.
        for (int i = 0; i < gatchaList.Count; i++)
        {
            //임시로 둔 가중치에 가챠 리스트에 각 데이터 마다 있는 데이터 값을 더해줍니다.
            weight += gatchaList[i].gatchaWeight;
            //만약 검출한 랜덤 값이 임시로둔 가중치 보다 작을 경우 데이터를 반환합니다.
            if (selectNum <= weight)
            {
                return gatchaList[i];
            }
        }
        return null;
    }
    //가챠를 뽑는 순간 바로 초기화시킨다.
    void GatchaReset()
    { 
        //저장한 데이터 만큼 반복
        for(int i=0;i<resultData.Count; i++)
        {
            //카드를 null로 
            cards[i].CARDDATA = null;

        }
        resultData.Clear();
    }
    #endregion

    //캔슬 버튼 클릭 이벤트
    void cancelBtnClick()
    {
        //카드 전체의 애니메이션을 초기화 해준다.
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].anim.Rebind();
            cards[i].gameObject.SetActive(false);
        }
        //카드 에리어를 꺼준다.
        cardArea.SetActive(false);
        //가챠 돌린것 체크하는 카운트들을 초기화해준다.
        rotatePoint = 0;
        gatchaCnt = 0;
        //가챠 버튼을 활성화 시켜준다.
        oneGatchaBtn.gameObject.SetActive(true);
        tenGatchaBtn.gameObject.SetActive(true);
        ONCANCEL = false;
    }
    //스킵 버튼 클릭 이벤트
    void skipBtnClick()
    {
        for (int i = 0; i < gatchaCnt; i++)
        {
            //안까본 카드들에게 카드를 까라고 시킨다.
            cards[i].CardClickEvent();

        }
        ONCANCEL = true;
    }
}
