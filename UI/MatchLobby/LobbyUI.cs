using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BackEnd.Tcp;
using Battlehub.Dispatcher;

public partial class LobbyUI : MonoBehaviour
{
    private static LobbyUI _instance;

    public static LobbyUI Instance // 싱글톤 구현
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(LobbyUI)) as LobbyUI;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(LobbyUI)) as LobbyUI;
                }
            }
            return _instance;
        }
    }

    public GameObject nickNameObject;
    public GameObject MatchProgressObject;
    public GameObject gameModeUI;

    public bool isMatchDone = false;
    [HideInInspector]public string matchDoneStr = "매칭 입장 중";
    [HideInInspector]public string matchOriginStr = "매치 상대를 검색하는 중...";
    [SerializeField]private GameObject matchCancelBtn;
    [SerializeField]private GameObject loadingObject;
    private Text matchInfoText;


    public GameObject errorObject;
    private Text errorText;
    public Text errorBtnText;
    public Button errorbtn;

    void Awake()
    {
        // 재접속 로직 제외
        BackEndMatchManager.Instance.IsMatchGameActivate();
    }
    void Start()
    {
        if (BackEndMatchManager.Instance == null)
        {
            return;
        }

        SetNickName();
        errorText = errorObject.GetComponentInChildren<Text>();
        matchInfoText = MatchProgressObject.GetComponentInChildren<Text>();

        errorObject.SetActive(false);
        MatchProgressObject.SetActive(false);
        loadingObject.SetActive(false);
        //readyRoomObject.SetActive(false);
    }
    //닉네임 세팅
    private void SetNickName()
    {
        var name = BackEndServerManager.Instance.myNickName;
        if (name.Equals(string.Empty))
        {
            Debug.LogError("닉네임 불러오기 실패");
            name = "test123";
        }
        Text nickname = nickNameObject.GetComponentInChildren<Text>();

        nickname.text = name;
    }
    //매칭 취소 버튼
    public void RequestCancel()
    {
        Debug.Log("매치메이킹 요청취소");
        if (errorObject.activeSelf || isMatchDone)
        {

            return;
        }
        BackEndMatchManager.Instance.CancelRegistMatchMaking();
    }
    //매치 검색하는중 UI 이벤트
    public void MatchRequestCallback(bool result)
    {
        if (!result)
        {
            MatchProgressObject.SetActive(false);
            loadingObject.SetActive(false);
            return;
        }

        MatchProgressObject.SetActive(true);
        loadingObject.SetActive(true);
    }
    //매치 완료시 UI 이벤트 (이 때 매칭 취소 버튼 없애기)
    public void MatchDoneCallback(string str)
    {
        Debug.Log("매치 완료");
        isMatchDone = true;
        matchInfoText.text = str;
        matchCancelBtn.SetActive(false);
        loadingObject.SetActive(false);
        MatchProgressObject.SetActive(true);
    }
    //매칭 취소 시 UI 이벤트 
    public void MatchCancelCallback()
    {
        isMatchDone = false;
        SetErrorObject("매칭이 취소되었습니다.",true);
    }
    //인게임 로딩 씬으로 게임씬을 옮김
    public void ChangeRoomLoadScene()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Ready);
    }
    //에러 메세지를 날릴때 보여줄 오브젝트
    public void SetErrorObject(string error,bool clearPVPInfo)
    {
        MatchProgressObject.SetActive(false);
        gameModeUI.SetActive(true);
        errorObject.SetActive(true);
        matchCancelBtn.SetActive(true);
        if (clearPVPInfo) { ClearPVPInfo(); }
        errorText.text = error;
    }
    //매칭이 취소 됬을 때 초기화할 변수들 
    public void ClearPVPInfo()
    {
        //VS모드 비활성화
        InGameInfoManager.Instance.isPVPMode = false;
        //PVP리스트들 초기화
        InGameInfoManager.Instance.pvpCharactorDatas.Clear();
        InGameInfoManager.Instance.pvpEmoticonDatas.Clear();
        InGameInfoManager.Instance.pvpCharctorDIc.Clear();
        InGameInfoManager.Instance.pvpEmoticonDic.Clear();
        InGameInfoManager.Instance.isGameOut = false;

        //배경 초기화
        InGameInfoManager.Instance.pvpBackGround = null;
    }
    public void JoinMatchProcess()
    {
        BackEndMatchManager.Instance.JoinMatchServer();
    }

    public bool IsLoadingObjectActive()
    {
        return loadingObject.activeSelf;
    }

    public bool IsErrorObjectActive()
    {
        return errorObject.activeSelf;
    }

    //중복 접속 오류 발생 시 호출되는 함수 
    public void MatchDuplicateConnectError()
    {
        //에러 버튼에 게임 종료 이벤트를 추가해준다. 
        errorText.text = "다른 기기에서 로그인해서 연결이 끊겼습니다.";
        errorbtn.onClick.AddListener(GameManager.Instance.OutGame);
        errorBtnText.text = "나가기";
        errorObject.SetActive(true);
    }

}
