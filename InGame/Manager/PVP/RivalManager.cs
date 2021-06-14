using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BackEnd;
using BackEnd.Tcp;

public class RivalManager : MonoBehaviour
{
    private static RivalManager _instance;
    public static RivalManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(RivalManager)) as RivalManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(RivalManager)) as RivalManager;
                }
            }
            return _instance;
        }
    }

    //만약 신호를 받는다면 받은 번호를 이용해 캐릭터를 생성, 배치, 리스트 등록까지 마친다.

    [Header("받아온 데이터 관련")]
    [HideInInspector] public IconData[] CharDatas;


    [Header("캐릭터 소환, 배치 관련")]
    //소환한 아군 리스트
    public List<PVPCharactor> summonList = new List<PVPCharactor>();
    private GameObject t_obj;
    private PVPCharactor t_char;

    private void Awake()
    {
        if (InGameInfoManager.Instance.isPVPMode)
        {
            PVPInGM.Instance.roundChangeChain += RivalRoundUpEvent;
        }
    }
    //PVP시 상대가 소환한 유닛을 받아와 동기화해준다.
    public void RivalCharSummon(SessionId session, int poolNum)
    {
        if (session == InGameInfoManager.Instance.rivalSessionID)
        {
            //라이벌 캐릭터 동기화
            t_obj = RivalPoolingManager.Instance.GetUnit(poolNum);

            //위치 배정
            RivalSetPos(t_obj);
        }
    }
    public void RivalUnitLevelUP(SessionId session , int poolNum)
    {
        if (session == InGameInfoManager.Instance.rivalSessionID)
        {

            t_char = RivalPoolingManager.Instance.GetUnitInfo(poolNum);
            t_char.CHARLEVEL++;
            InGameUIManager.Instance.SetRivalIngameLevel(t_char);
        }
    }

    private void RivalSetPos(GameObject rivalObj)
    {
        //소환 리스트 추가
        summonList.Add(rivalObj.transform.GetComponent<PVPCharactor>());
        //정렬
        summonList = summonList.OrderBy(x => (int)x.myType).ToList();
        //오른쪽에 배치
        PVPInGM.Instance.AssignedPos(summonList, PVPInGM.Instance.enemyUnitPos);

       
    }
   
    //적군이 이겼을 때 이벤트 
    private void RivalRoundUpEvent()
    {
        if (summonList.Count != 0)
        {
            for (int i = 0; i < summonList.Count; i++)
            {
                summonList[i].gameObject.SetActive(false);
            }
        }
    }
    public void SetRival()
    {
        if (summonList.Count != 0)
        {
            //정렬(탱,근,원 순으로 정렬)
            summonList = summonList.OrderBy(x => (int)x.myType).ToList();

            PVPInGM.Instance.AssignedPos(summonList, PVPInGM.Instance.enemyUnitPos);
            for (int i = 0; i < summonList.Count; i++)
            {
                summonList[i].gameObject.SetActive(true);
            }
        }
    }

}
