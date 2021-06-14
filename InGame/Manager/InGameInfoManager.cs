using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using BackEnd;
using BackEnd.Tcp;


public class InGameInfoManager : MonoBehaviour
{
    private static InGameInfoManager _instance;
    public static InGameInfoManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(InGameInfoManager)) as InGameInfoManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(InGameInfoManager)) as InGameInfoManager;
                }
            }
            return _instance;
        }
    }

    #region SingleStage
    //선택한 챕터의 번호
    [HideInInspector] public int selectChapterNum;
    //선택한 스테이지의 번호
    [HideInInspector] public int selectStageNum;
    //선택한 스테이지의 데이터
    public StageData selectStageData;

    //선택한 챕터의 배경

    public StageImgStruct selectBackGround;
    public Sprite pvpBackGround;

    //멀티 때 가져갈 목록 덱 정보(캐릭터, 마법)

    //게임으로 가져갈 캐릭터, 마법 덱 데이터 
    public List<IconData> charactorDatas;
    public List<EmoticonData> EmoticonDatas;
    #endregion

    #region PVP 
    //PvP 때 가져갈 덱정보(고유번호로 되어있음)
    public List<string> pvpCharactorDatas;
    public List<string> pvpEmoticonDatas;
    
    
    public Dictionary<SessionId, List<IconData>> pvpCharctorDIc = new Dictionary<SessionId, List<IconData>>();
    public Dictionary<SessionId, List<EmoticonData>> pvpEmoticonDic = new Dictionary<SessionId, List<EmoticonData>>();
    //VSMode 여부
    public bool isPVPMode;
    public SessionId mySessionID;
    public SessionId rivalSessionID;

    //게임을 나간 플레이어가 있을 때 사용하는 변수
    public bool isGameOut= false;
    public SessionId outSession;
    #endregion
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

   
}
