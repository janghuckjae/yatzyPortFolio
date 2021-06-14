using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class InventoryManager : MonoBehaviour
{
    private static InventoryManager _instance;
    public static InventoryManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(InventoryManager)) as InventoryManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(InventoryManager)) as InventoryManager;
                }
            }
            return _instance;
        }
    }
    [Header("캐릭터 인벤토리")]
    //전체 캐릭터 정보를 담아줄 리스트
    [SerializeField] private IconData[] charIconDatas;
    //해금 된 캐릭터 리스트 
    private List<IconData> releaseCharData = new List<IconData>();
    //해금 안된 캐릭터 리스트 
    private List<IconData> lockCharData = new List<IconData>();
    //캐릭터 슬롯
    public GameObject[] charSlots;
    //가장 최근에 클릭한 아이콘 
    public Icon recentIcon;
    //슬롯 안에있는 아이콘 스크립트
    private List<Icon> charIconInfo;

    private int charDataAmount = 0;
    //가장 최근에 쓴 정렬기준을 가져온다.
    enum SortKind
    {
        GradeSort,
        LevelSort,
        TypeSort
    }
    private SortKind sortKind = SortKind.GradeSort;


    [Header("Swap 버튼 기능")]
    public GameObject charDeck;
    public GameObject charInven;

    [Header("아이콘 클릭 관련 ")]
    public GameObject selectWindow;
    public Image selectImg;
    //-값 찾기 
    public float selectImgY;
    public Button infoBtn;
    public Button UseBtn;
    public Button RemoveBtn;

    // Start is called before the first frame update
    void Awake()
    {
        charIconDatas = GameDataManager.Instance.charIconDatas;
        GetIconInfo();
    }
    private void Start()
    {
        AssignInvenData();
    }
    #region "Icon 스크립트 가져오기"
    //마법과 캐릭터의 Icon 배열선언과 게임오브젝트의 Icon 스크립트 가져오기; 
    void GetIconInfo()
    {
        //캐릭터
        charIconInfo = new List<Icon>(charSlots.Length);
        for (int i = 0; i < charSlots.Length; i++)
        {
            charIconInfo.Add(charSlots[i].transform.GetComponent<Icon>());
            //이때 슬롯 넘버를 배정해준다.
            charIconInfo[i].charSlotNum = i;
        }
    }
    #endregion
    #region "처음 시작시 인벤토리 데이터 관리" 
    //데이터 배열의 길이 만큼만 게임오브젝트를 켜주고 나머지는 꺼준다.
    void AssignInvenData()
    {
        //해금 여부 판정
        CheckRelease();
        //캐릭터
        for (int j = 0; j < charSlots.Length; j++)
        {
            if (j < charIconDatas.Length) { charSlots[j].SetActive(true); }

            else { charSlots[j].SetActive(false); }
        }
    }
    //해금 여부 검출 
    void CheckRelease()
    {
        //전체 데이터중 해금 되있는 데이터는 release...Data에 , 해금 안되어있는 데이터는 lock...Data에 넣어준다.

        //캐릭터 
        for (int i = 0; i < charIconDatas.Length; i++)
        {
            if (charIconDatas[i].isRelease)
            {
                releaseCharData.Add(charIconDatas[i]);

            }
            else { lockCharData.Add(charIconDatas[i]); }
        }
        
        //처음에는 등급별로 배정한다.
        GradeSort();
    }
    #endregion
    #region "정렬" 
    //오름 차순으로 정렬 
    //각자의 버튼에 배정
    //등급별 정렬
    public void GradeSort()
    {
        selectWindow.SetActive(false);
        sortKind = SortKind.GradeSort;
        //캐릭터
        releaseCharData = releaseCharData.OrderByDescending(x => x.itemGrade).ToList();
        lockCharData = lockCharData.OrderByDescending(x => x.itemGrade).ToList();
        AssignData();
    }


    //레벨 별 정렬
    //내림차순 정렬
    public void LevelSort()
    {
        selectWindow.SetActive(false);
        sortKind = SortKind.LevelSort;
        //캐릭터
        releaseCharData = releaseCharData.OrderByDescending(x => x.itemLevel).ToList();
        lockCharData = lockCharData.OrderByDescending(x => x.itemLevel).ToList();
        AssignData();
    }
    //타입별 정렬(캐릭터:근딜,원딜,탱커;;마법:?,?,?)
    public void TypeSort()
    {
        selectWindow.SetActive(false);
        sortKind = SortKind.TypeSort;
        //캐릭터
        releaseCharData = releaseCharData.OrderBy(x => (int)x.charIconType).ToList();
        lockCharData = lockCharData.OrderBy(x => (int)x.charIconType).ToList();
        AssignData();
    }

    #endregion
    #region "배정"
    //정리한 데이터를 배정 
    private void AssignData()
    {
        int j = 0;
        //마법과 캐릭터의 총 양은 해금 안된 데이터양와 잠긴 데이터 양의 합
        charDataAmount = releaseCharData.Count + lockCharData.Count;
        //캐릭터
        //덱에 먼저 해금된게 나오고 나중에 잠긴게 나오기 위한 기능
        for (int i = 0; i < charDataAmount; i++)
        {
            if (i < releaseCharData.Count)
            {
                //Icon 스크립트 배열에 해금된 데이터를 먼저 넣어준다.
                charIconInfo[i].isRelease = releaseCharData[i].isRelease;
                charIconInfo[i].myData = releaseCharData[i];
            }
            else
            {
                // 해금된 데이터가 들어간 다음 잠긴 데이터를 넣어준다.
                charIconInfo[i].isRelease = lockCharData[j].isRelease;
                charIconInfo[i].myData = lockCharData[j];
                j++;
            }
            charIconInfo[i].DataSetEvent();
        }
    }

    #endregion

    #region "덱에 넣을 때"
    //덱에 넣을 때 
    //아이콘 함수에서는 InventoryManager.Instance.DeckIn(this, myData); 이런식으로 사용한다.
    //캐릭터 IconInfo 배열에서 Icon스크립트(나 자신)을 뺀다.
    //덱리스트 판별(null이 된것 중)
    public void DeckIn(Icon icon)
    {
        if (icon.myData.charIconSubType == CharIconType.Support)
        {
            DeckinCost(icon, 7);
            return;
        }
        else
        {
            switch (icon.myData.charIconType)
            {
                case CharIconType.MeleeCharactor:
                    DeckinCost(icon, 1);
                    break;
                case CharIconType.ADCharactor:
                    DeckinCost(icon, 3);
                    break;
                case CharIconType.TankCharactor:
                    DeckinCost(icon, 5);
                    break;

            }
        }
    }
    private void DeckinCost(Icon icon, int maxCost)
    {
        if (DeckManager.Instance.isCharDeck[maxCost] && DeckManager.Instance.isCharDeck[maxCost - 1]) 
        {
            LobbyUI.Instance.SetErrorObject("해당 직업군은 꽉찼습니다.", false);
            return; 
        }
        else
        {
            for (int i = maxCost-1; i < maxCost+1; i++)
            {
                if (!DeckManager.Instance.isCharDeck[i])
                {
                    //해금된 캐릭터 데이터 리스트에서 빼주고
                    releaseCharData.Remove(icon.myData);
                    charIconInfo.Remove(icon);
                    //게임데이터에 아이콘을 배정해주어야한다.
                    DeckManager.Instance.isCharDeck[i] = true;
                    DeckManager.Instance.myDeck[i] = icon.myData;
                    DeckManager.Instance.RegistDeckData();
                    
                    icon.charDeckNum = i;
                    icon.transform.SetParent(DeckManager.Instance.charDeckArea[i]);
                    icon.gameObject.transform.position = DeckManager.Instance.charDeckArea[i].position;
                    icon.isDeckIn = true;
                    break;
                }
            }
        }   
    }
    #endregion
    #region "덱에서 뺼 때"
    //덱에서 뺐을 때 
    //아이콘 함수에서는 InventoryManager.Instance.DeckOut(this, myData); 이런식으로 사용한다.
    //나 자신이 해금됬는지 안됬는지에 따라서 release...Data나 lock....Data에 더해준다.
    //Iconinfo
    public void DeckOut(Icon icon)
    {
        //해금된 캐릭터 데이터 리스트에 더해주고
        releaseCharData.Add(icon.myData);
        charIconInfo.Add(icon);
        //iconInfo의 정렬이 필요
        charIconInfo = charIconInfo.OrderBy(x => x.charSlotNum).ToList();
        //덱 리스트중 적용 되어있는지 안되어 있는지 검사 

        //덱안에 정보가 있다면 
        if (icon.isDeckIn)
        {
           
            //아이콘이 덱안에 있다는 bool함수
            DeckManager.Instance.isCharDeck[icon.charDeckNum] = false;
            //덱에서 뺼 때  데이터 매니저의 정보에서도 빼주고 난 다음 정렬한다.
            //InGameInfoManager.Instance.charactorDatas.Remove(iconData);
            DeckManager.Instance.myDeck[icon.charDeckNum] = null;

            //만약 덱에서 뺀 아이콘이 윗 부분(0,2,4,6)이고 내 아래 부분의 유닛이 있다면 그 유닛을 위로 올려준다.
            if (icon.charDeckNum % 2 == 0 && DeckManager.Instance.isCharDeck[icon.charDeckNum + 1] == true)
            {
                Debug.Log("아래있는데 윗부분 제거"+ icon.charDeckNum);
                //게임데이터에 아이콘을 배정해주어야한다.
                DeckManager.Instance.isCharDeck[icon.charDeckNum] = true;
                DeckManager.Instance.isCharDeck[icon.charDeckNum + 1] = false;

                DeckManager.Instance.myDeck[icon.charDeckNum] = DeckManager.Instance.myDeck[icon.charDeckNum+1];
                DeckManager.Instance.myDeck[icon.charDeckNum + 1] = null;
                DeckManager.Instance.RegistDeckData();

                Icon swapIcon = DeckManager.Instance.charDeckArea[icon.charDeckNum + 1].GetComponentInChildren<Icon>();
                swapIcon.charDeckNum = icon.charDeckNum;
                swapIcon.transform.SetParent(DeckManager.Instance.charDeckArea[icon.charDeckNum]);
                swapIcon.gameObject.transform.position = DeckManager.Instance.charDeckArea[icon.charDeckNum].position;
            }

            //Char슬롯구역의 자식으로 해준다.
            icon.transform.SetParent(DeckManager.Instance.charSlotParent.transform);
            DeckManager.Instance.RegistDeckData();
            //캐릭터 아이콘의 Hierachy위치 정렬
            for (int i = 0; i < DeckManager.Instance.charSlotParent.transform.childCount; i++)
            {
                charIconInfo[i].transform.SetSiblingIndex(i);
            }
            //정렬필요
            //가장 최근에 시도한 정렬로 정렬한다.
            switch (sortKind)
            {
                case SortKind.GradeSort:
                    GradeSort();
                    break;
                case SortKind.LevelSort:
                    LevelSort();
                    break;
                case SortKind.TypeSort:
                    TypeSort();
                    break;
            }
            icon.isDeckIn = false;
        }
    }
    #endregion
    #region "캐릭터 가챠 후 인벤토리 적용"
    //가챠 후 해금이 됬다면
    public void ReleaseChar(List<IconData> gatchaResultList)
    {
        //해금 됬는지 안됬는지 확인 
        for (int i = 0; i < gatchaResultList.Count; i++)
        {
            //해금 안됬다면
            if (!gatchaResultList[i].isRelease)
            {
                gatchaResultList[i].isRelease = true;
                //잠금된 데이터 리스트에 뺴주기
                lockCharData.Remove(gatchaResultList[i]);
                //해금 된 데이터 리스트에 더해주기
                releaseCharData.Add(gatchaResultList[i]);
            }
            //이미 해금 되있던 것을 뽑으면 어떻게 할건지는 기획에 따라 변경
            else { }
        }
        //중복 제거
        releaseCharData.Distinct();
        //정렬필요
        //가장 최근에 실행한 정렬 방식으로 정렬
        switch (sortKind)
        {
            case SortKind.GradeSort:
                GradeSort();
                break;
            case SortKind.LevelSort:
                LevelSort();
                break;
            case SortKind.TypeSort:
                TypeSort();
                break;
        }
    }
    #endregion


    //임시로 넣은 덱고르기 기능
    public void DeckSelect()
    {
        for (int i = 0; i < charIconInfo.Count; i++)
        {
            if (charIconInfo[i].myData != null)
            {
                charIconInfo[i].UseBtnClick();
            }
        }
    }
}
