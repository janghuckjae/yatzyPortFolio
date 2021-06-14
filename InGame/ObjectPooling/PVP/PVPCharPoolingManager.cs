using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PVPCharPoolingManager : MonoBehaviour
{
    private static PVPCharPoolingManager _instance;

    public static PVPCharPoolingManager Instance // 싱글톤 구현
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(PVPCharPoolingManager)) as PVPCharPoolingManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(PVPCharPoolingManager)) as PVPCharPoolingManager;
                }
            }
            return _instance;
        }
    }
    //캐릭터 배열
    [SerializeField]private GameObject[] charactor;
    //임시 저장할 캐릭터 정보 
    private PVPCharactor[] charactorInfo;
    private GameObject[] cloneObjs;
    private PVPCharactor unitInfo;
    private GameObject obj;
    private GameObject pool;
    private GameObject t_obj;
    [SerializeField]private List<IconData> charIconData;
    // Start is called before the first frame update
    void Awake()
    {
        //PVP모드가 아닐때 기능 꺼줌
        if (!InGameInfoManager.Instance.isPVPMode)
        {
            this.enabled = false;
        }
        else
        {
            charIconData = InGameInfoManager.Instance.pvpCharctorDIc[InGameInfoManager.Instance.mySessionID];

            charactor = new GameObject[charIconData.Count];
            cloneObjs = new GameObject[charIconData.Count];
            charactorInfo = new PVPCharactor[charIconData.Count];
            //게임 데이터 매니저의 값을 받아와 적용한다.
            for (int i = 0; i < charIconData.Count; i++)
            {
                //캐릭터 값 적용
                charactor[i] = charIconData[i].deckPrefab;
            }

            pool = new GameObject("CharPool");

            for (int i = 0; i < charactor.Length; i++)
            {
                //캐릭터 별로 양에 따라 정해진 부모 밑에 생성 
                obj = Instantiate(charactor[i], pool.transform);

                //풀넘버를 지정 해준다.
                charactorInfo[i] = obj.transform.GetComponent<PVPCharactor>();
                charactorInfo[i].charPoolNum = i;
                charactorInfo[i].CHARLEVEL = 0;
                charactorInfo[i].isRival = false;
                charactorInfo[i].SetFillImage();

                charactorInfo[i].myType = charIconData[i].charIconType;
                charactorInfo[i].subType = charIconData[i].charIconSubType;
                UnitNumberSet(i, charactorInfo[i]);
                cloneObjs[i] = obj;
                //생성 한 캐릭터 들을 오브젝트 풀에 저장 
                obj.SetActive(false);
            }
        }
    }
    private void Start()
    {
        
    }
    private void UnitNumberSet(int num, PVPCharactor unit)
    {
        if (BackEndMatchManager.Instance.IsHost())
        {
            PVPInGM.Instance.activeUnits.Add(num + 1, unit);
            unit.unitNum = num + 1;
        }
        else
        {
            PVPInGM.Instance.activeUnits.Add(num + 9, unit);
            unit.unitNum = num + 9;
        }
    }

    public void InsertUnit(GameObject C_obj)
    {
        C_obj.SetActive(false);
    }
    //비활성화 되어있는 
    public GameObject GetUnit(int charNum)
    {
        t_obj = cloneObjs[charNum];
        t_obj.SetActive(true);
        return t_obj;
    }
    public PVPCharactor GetUnitInfo(int charNum)
    {
        unitInfo = charactorInfo[charNum];
        return unitInfo;
    }
}
