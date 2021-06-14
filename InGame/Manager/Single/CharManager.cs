using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CharManager : MonoBehaviour
{
    private static CharManager _instance;
    public static CharManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(CharManager)) as CharManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(CharManager)) as CharManager;
                }
            }
            return _instance;
        }
    }
    [Header("받아온 데이터 관련")]
    [HideInInspector] public IconData[] CharDatas;

    //버튼들의 CharButton스크립트 배열
    [SerializeField] private Button[] charBtns;

    [Header("캐릭터별 SP")]
    //캐릭터 별SP
    [SerializeField] private Text[] spText;
    private int[] charSP;

    [Header("캐릭터 소환, 배치 관련")]
    //캐릭터 최대 소환 
    public int maxFriendCnt = 15;
    [HideInInspector] public int currentFriendCnt = 0;
    //소환한 아군 리스트
    [SerializeField]private List<Charactor> summonList = new List<Charactor>();
    private MeshRenderer[] summonRenderer;

    //버튼 이미지
    [SerializeField] private Image[] buttonImg;
    //임시로 지정한 칼라값
    [SerializeField] private Color noBuyColor;
    private GameObject t_obj;

    private int line1SortPoint=0;
    private int line2SortPoint=0;
    private int line3SortPoint=0;

    // Start is called before the first frame update
    void Awake()
    {
        //게임데이터매니저에서 데이터를 받아오고 적용해준다.
        //PVP모드 일때 꺼준다
        if (InGameInfoManager.Instance.isPVPMode)
        {
            this.enabled = false;
        }
        else
        {
            CharDatas = InGameInfoManager.Instance.charactorDatas.ToArray();
            summonRenderer = new MeshRenderer[maxFriendCnt];
            charSP = new int[CharDatas.Length];
            //덱 위치에 따라 이미지 및 정보 값 대입 
            for (int i = 0; i < CharDatas.Length; i++)
            {
                switch (CharDatas[i].itemSP)
                {
                    case 5:
                        SummonButtonDataSet(i, 1);
                        break;
                    case 10:
                        SummonButtonDataSet(i, 3);
                        break;
                    case 15:
                        SummonButtonDataSet(i, 5);
                        break;
                    case 20:
                        SummonButtonDataSet(i, 7);
                        break;
                }

            }
            //라운드 변화시 이벤트 추가
            InGM.Instance.roundChangeChain += CharInitialization;
        }
    }
    private void Start()
    {
        
    }
    //버튼에 관련된거만 해당 코스트 라인의 버튼에 할당해준다.
    void SummonButtonDataSet(int i,int maxBtnNum)
    {
        //만약 해당 버튼의 이미지가 할당이 되었는데 할당하려 하는경우
        //(덱 구성 시 동일 코스트 유닛을 3마리 이상 가져온 경우 이므로 버그가 생긴거다.)
        if (buttonImg[maxBtnNum].sprite != null)
        {
            Debug.Log("코스트 중복이 있는데...");
            return;
        }
        else
        {
            int _i = i;
            for (int j = maxBtnNum-1; j < maxBtnNum+1; j++)
            {
                if (buttonImg[j].sprite == null)
                { 
                    //SP, 쿨타임, 아이콘이미지 값을 보내준다.
                    charSP[i] = CharDatas[i].itemSP;
                    buttonImg[j].color = CharDatas[i].color;
                    buttonImg[j].sprite = CharDatas[i].itemImage;
                    // 버튼클릭 이벤트 등록
                    charBtns[j].onClick.AddListener(() => SummonEvent(_i));
                    //SP 텍스트에 표시
                    spText[j].text = charSP[i].ToString() + " SP";
                    break;
                }
            }
        }
    }    


    //평소에 살수 있는지 없는지 탐지
    public void NoBuySearch()
    {
        for (int i = 0; i < CharDatas.Length; i++)
        {
            //만약 쿨타임이 활성화가 안됬다면 구매 가능 여부 탐색
            if (charSP[i] > InGM.Instance.SP) { buttonImg[i].color = noBuyColor; }
         
            else { buttonImg[i].color = CharDatas[i].color; }
        }
    }

    //캐릭터 버튼을 클릭할 때 알맞은 캐릭터 생성 
    //클릭 할 때 캐릭터버튼에 맞는 SP 소모 
    public void SummonEvent(int poolNum)
    {
        if (InGM.Instance.stageState != StageState.AssignedTime || InGM.Instance.isPause)
        {
            return;
        }
        //클릭한 버튼에 맞는 SP 차감 
        // 구매가 가능한 SP 일 때 , 배치 시간일 때 , 최대 유닛이 넘지 않았을 때 , 멈춤 상태가 아닐 때
        if (InGM.Instance.SP >= charSP[poolNum] && currentFriendCnt != maxFriendCnt)
        {
            //게임 씬에서 캐릭터의  생성 위치를 3분할 하여 배정한다.
            t_obj = CharPoolingManager.Instance.GetPool(poolNum);

            //캐릭터를 리스트에 저장하여 직업별로 정렬해 배치
            CharSetPos(t_obj);

            //소환 횟수 증가
            currentFriendCnt++;
            //SP 차감 
            InGM.Instance.SP -= charSP[poolNum];
        }
    }

    void CharSetPos(GameObject charObj)
    {
        //캐릭터 스크립트 저장
        summonList.Add(charObj.transform.GetComponent<Charactor>());

        //정렬(탱,근,원 순으로 정렬)
        summonList = summonList.OrderBy(x => (int)x.myType).ToList();
        
        //소환할 숫자에 따라 배치 변경
        InGM.Instance.AssignedPos(summonList.Count, InGM.Instance.myUnitPos);

        //캐릭터 배치 (SortLayer => 아래부터 먼저 출력 되는순으로 5 3 1 6 4 2 라인 순으로 된다. )
        line1SortPoint = 0;
        line2SortPoint = 0;
        line3SortPoint = 0;
        for (int i = 0; i < summonList.Count; i++)
        {
            // 현재는 전부가 스파인 오브젝트가 아니므로 방어 코드 작성함
            if (summonList[i].transform.GetComponent<MeshRenderer>() != null)
            {
                summonRenderer[i] = summonList[i].transform.GetComponent<MeshRenderer>();
                //아군 Layer구분
                SortLayerAssigned(i); 
            }

            //순차적으로 배치
            summonList[i].transform.position = InGM.Instance.myUnitPos[i];
        }
    }

    void SortLayerAssigned(int i)
    {
        if (summonList.Count == 3)
        {
            //유닛의 위치가 1번째 라인이라면 (x축이 -1.1보다 크다면)
            if (InGM.Instance.myUnitPos[i].x > -1.1f)
            {
                summonRenderer[i].sortingLayerName = InGM.Instance.line2LayerArr[line1SortPoint];
                line1SortPoint++;

            }
            //유닛의 위치가 2번째 라인이라면?(x축이 -1.1보다 작고 -1.78보다 크다면)
            else if (InGM.Instance.myUnitPos[i].x > -1.78f && InGM.Instance.myUnitPos[i].x < -1.1f)
            {
                summonRenderer[i].sortingLayerName = InGM.Instance.line1_3LayerArr[line2SortPoint];
                line2SortPoint++;
            }
            //유닛의 위치가 3번째 라인이라면 (x축이 -1.78보다 작다면)
            else if (InGM.Instance.myUnitPos[i].x < -1.78f)
            {
                summonRenderer[i].sortingLayerName = InGM.Instance.line2LayerArr[line3SortPoint];
                line3SortPoint++;
            }
        }
        else
        {
            //유닛의 위치가 1번째 라인이라면 (x축이 -1.1보다 크다면)
            if (InGM.Instance.myUnitPos[i].x > -1.1f)
            {
                summonRenderer[i].sortingLayerName = InGM.Instance.line1_3LayerArr[line1SortPoint];
                line1SortPoint++;

            }
            //유닛의 위치가 2번째 라인이라면?(x축이 -1.1보다 작고 -1.78보다 크다면)
            else if (InGM.Instance.myUnitPos[i].x > -1.78f && InGM.Instance.myUnitPos[i].x < -1.1f)
            {
                summonRenderer[i].sortingLayerName = InGM.Instance.line2LayerArr[line2SortPoint];
                line2SortPoint++;
            }
            //유닛의 위치가 3번째 라인이라면 (x축이 -1.78보다 작다면)
            else if (InGM.Instance.myUnitPos[i].x < -1.78f)
            {
                summonRenderer[i].sortingLayerName = InGM.Instance.line1_3LayerArr[line3SortPoint];
                line3SortPoint++;
            }
        }

    }

    //타워 공격이 끝나고 라운드가 바뀔때 초기화

    void CharInitialization()
    {
        summonList.Clear();
        currentFriendCnt = 0;
    }
}