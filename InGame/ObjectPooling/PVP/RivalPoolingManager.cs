using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RivalPoolingManager : MonoBehaviour
{
    private static RivalPoolingManager _instance;

    public static RivalPoolingManager Instance // 싱글톤 구현
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(RivalPoolingManager)) as RivalPoolingManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(RivalPoolingManager)) as RivalPoolingManager;
                }
            }
            return _instance;
        }
    }
    //캐릭터 배열
    private GameObject[] charactor;


    //임시 저장할 캐릭터 정보 
    private PVPCharactor[] rivalCharactorInfo;


    private GameObject obj;
    private GameObject pool;

    private GameObject[] cloneObjs;
    private GameObject t_obj;
    private PVPCharactor objInfo;
    private List<IconData> charIconData;
    Quaternion rotation = Quaternion.identity;
    private void Awake()
    {
        //PVP모드 일때
        if (InGameInfoManager.Instance.isPVPMode)
        {
            charIconData = InGameInfoManager.Instance.pvpCharctorDIc[InGameInfoManager.Instance.rivalSessionID];
            charactor = new GameObject[charIconData.Count];
            cloneObjs = new GameObject[charIconData.Count];
            rivalCharactorInfo = new PVPCharactor[charIconData.Count];
            //게임 데이터 매니저의 값을 받아와 적용한다.
            for (int i = 0; i < charIconData.Count; i++)
            {
                //캐릭터 값 적용
                charactor[i] = charIconData[i].deckPrefab;
            }

            pool = new GameObject("RivalPool");
            for (int i = 0; i < charactor.Length; i++)
            {
                //캐릭터 별로 양에 따라 정해진 부모 밑에 생성 
                obj = Instantiate(charactor[i], pool.transform.position, rotation, pool.transform);
                //풀넘버를 지정 해준다.
                rivalCharactorInfo[i] = obj.transform.GetComponent<PVPCharactor>();
                rivalCharactorInfo[i].charPoolNum = i;
                rivalCharactorInfo[i].CHARLEVEL = 0;
                rivalCharactorInfo[i].isRival = true;
                rivalCharactorInfo[i].SetFillImage();

                rivalCharactorInfo[i].myType = charIconData[i].charIconType;
                rivalCharactorInfo[i].subType = charIconData[i].charIconSubType;
                UnitNumberSet(i, rivalCharactorInfo[i]);
                cloneObjs[i] = obj;
                //생성 한 캐릭터 들을 오브젝트 풀에 저장 
                obj.SetActive(false);
            }
        }
    }
    private void UnitNumberSet(int num, PVPCharactor unit)
    {
        if (BackEndMatchManager.Instance.IsHost())
        {
            PVPInGM.Instance.activeUnits.Add(num + 9, unit);
            unit.unitNum = num + 9;
        }
        else
        {
            PVPInGM.Instance.activeUnits.Add(num + 1, unit);
            unit.unitNum = num + 1;
        }
    }

    //오브젝트가 어떤건지 판단후 맞다면 해당 오브젝트풀에 넣기
    public void InsertUnit(GameObject C_obj)
    {
        C_obj.SetActive(false); 
    }
    //비활성화 되어있는 
    public GameObject GetUnit(int rivalNum)
    {
        t_obj = cloneObjs[rivalNum];
        t_obj.SetActive(true);
        return t_obj;
    }
    public PVPCharactor GetUnitInfo(int charNum)
    {
        objInfo = rivalCharactorInfo[charNum];
        return objInfo;
    }
}

