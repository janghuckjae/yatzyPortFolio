using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using BackEnd.Tcp;
using UnityEngine.SceneManagement;

/*
 * 매치매니저 (인게임 관련 기능)
 * BackEndInGame.cs에서 정의된 기능들
 * 인게임에 필요한 변수들
 * 인게임서버 게임룸 접속하기 (인게임서버 접속은 BackEndMatch.cs에서 정의)
 * 인게임서버 접속종료
 * 게임시작
 * 게임결과값 전송
 * 게임결과값 조합 (1:1 / 개인전 / 팀전)
 * 서버로 데이터 패킷 전송
 */
public partial class BackEndMatchManager : MonoBehaviour
{
    [SerializeField]private bool isSetHost = false;                 // 호스트 세션 결정했는지 여부

    [SerializeField]private MatchGameResult matchGameResult;

    // 게임 로그
    private string FAIL_ACCESS_INGAME = "인게임 접속 실패 : {0} - {1}";
    private string SUCCESS_ACCESS_INGAME = "유저 인게임 접속 성공 : {0}";
    private string NUM_INGAME_SESSION = "인게임 내 세션 갯수 : {0}";


    // 게임 레디 상태일 때 호출됨
    public void OnGameReady()
    {
        if (isSetHost == false)
        {
            // 호스트가 설정되지 않은 상태이면 호스트 설정
            isSetHost = SetHostSession();
        }
        Debug.Log("호스트 설정 완료");

        if (IsHost() == true)
        {
            // 0.5초 후 ReadyToLoadRoom 함수 호출
            Invoke("ReadyToLoadRoom", 0.5f);
        }
        Debug.Log("데이터 세팅");
        //내 세션 ID,상대 세션 I등록
        InGameInfoManager.Instance.mySessionID = Backend.Match.GetMySessionId();
        foreach (var sessionId in BackEndMatchManager.Instance.sessionIdList)
        {
            if (sessionId != InGameInfoManager.Instance.mySessionID)
            {
                InGameInfoManager.Instance.rivalSessionID = sessionId;
            }
        }
        SendDataToInGame(new Protocol.SetPrevGameInfoMessage
            (InGameInfoManager.Instance.mySessionID,
            InGameInfoManager.Instance.pvpCharactorDatas.ToArray(),
            InGameInfoManager.Instance.pvpEmoticonDatas.ToArray()));
    }

    // 현재 룸에 접속한 세션들의 정보
    // 최초 룸에 접속했을 때 1회 수신됨
    // 재접속 했을 때도 1회 수신됨
    private void ProcessMatchInGameSessionList(MatchInGameSessionListEventArgs args)
    {
        Debug.Log("세션 리스트 검사");
        sessionIdList = new List<SessionId>();
        gameRecords = new Dictionary<SessionId, MatchUserGameRecord>();

        
        foreach (var record in args.GameRecords)
        {
            sessionIdList.Add(record.m_sessionId);
            gameRecords.Add(record.m_sessionId, record);
        }
       
        sessionIdList.Sort();
    }

    // 클라이언트 들의 게임 룸 접속에 대한 리턴값
    // 클라이언트가 게임 룸에 접속할 때마다 호출됨
    // 재접속 했을 때는 수신되지 않음
    private void ProcessMatchInGameAccess(MatchInGameSessionEventArgs args)
    {
        Debug.Log(string.Format(SUCCESS_ACCESS_INGAME, args.ErrInfo));

        if (args.ErrInfo != ErrorCode.Success)
        {
            // 게임 룸 접속 실패
            var errorLog = string.Format(FAIL_ACCESS_INGAME, args.ErrInfo, args.Reason);
            Debug.Log(errorLog);
            LeaveInGameRoom();
            return;
        }

        // 게임 룸 접속 성공
        // 인자값에 방금 접속한 클라이언트(세션)의 세션ID와 매칭 기록이 들어있다.
        // 세션 정보는 누적되어 들어있기 때문에 이미 저장한 세션이면 건너뛴다.

        var record = args.GameRecord;
        Debug.Log(string.Format(string.Format("인게임 접속 유저 정보 [{0}] : {1}", args.GameRecord.m_sessionId, args.GameRecord.m_nickname)));
        //만약 세션리스트에 내 세션리스트 말고 다른것이 들어왔다면
        if (!sessionIdList.Contains(args.GameRecord.m_sessionId))
        {
            // 세션 정보, 게임 기록 등을 저장
            sessionIdList.Add(record.m_sessionId);
            gameRecords.Add(record.m_sessionId, record);

            Debug.Log(string.Format(NUM_INGAME_SESSION, sessionIdList.Count));
        }
    }

    // 인게임 룸 접속
    private void AccessInGameRoom(string roomToken)
    {
        Debug.Log("인게임 룸 접속");
        Backend.Match.JoinGameRoom(roomToken);
    }

    // 인게임 서버 접속 종료
    public void LeaveInGameRoom()
    {
        Debug.Log("인게임 서버 접속 종료");
        isConnectInGameServer = false;
        Backend.Match.LeaveGameServer();
    }

    // 서버에서 게임 시작 패킷을 보냈을 때 호출
    // 모든 세션이 게임 룸에 참여 후 "콘솔에서 설정한 시간" 후에 게임 시작 패킷이 서버에서 온다
    private void GameSetup()
    {
        Debug.Log("게임 시작 메시지 수신. 게임 설정 시작");
        
        // 게임 시작 메시지가 오면 게임을 레디 상태로 변경
        if (GameManager.Instance.GetGameState() != GameManager.GameState.Ready)
        {
            Debug.Log("OnGameReady");
            isHost = false;
            isSetHost = false;
            
            OnGameReady();
           
        }
    }

    private void ReadyToLoadRoom()
    {
        Debug.Log("1초 후 룸 씬 전환 메시지 송신");
        if (!InGameInfoManager.Instance.isGameOut) { Invoke("SendChangeRoomScene", 1f); }
        
    }
    //게임 로딩씬으로 이동
    private void SendChangeRoomScene()
    {
        if (!InGameInfoManager.Instance.isGameOut)
        {
            Debug.Log("게임 로딩씬 신호 보내기");
            SendDataToInGame(new Protocol.LoadRoomSceneMessage());
        }
    }

    #region 게임 결과 전송
    public void MatchGameOver(SessionId winSession, SessionId loseSession, bool isDraw)
    {
        matchGameResult = OneOnOneRecord(winSession,loseSession,isDraw);
        Backend.Match.MatchEnd(matchGameResult);
        Debug.Log("결과 보내기 완료");
    }
    private MatchGameResult OneOnOneRecord(SessionId winSession, SessionId loseSession, bool isDraw)
    {
        MatchGameResult nowGameResult = new MatchGameResult();

        if (isDraw)
        {
            nowGameResult.m_draws = new List<SessionId>();
            nowGameResult.m_winners = null;
            nowGameResult.m_losers = null;

            nowGameResult.m_draws.Add(winSession);
            nowGameResult.m_draws.Add(loseSession);
            Debug.Log("동점 집계 완료");
        }
        else
        {
            nowGameResult.m_winners = new List<SessionId>();

            nowGameResult.m_winners.Add(winSession);

            nowGameResult.m_losers = new List<SessionId>();

            nowGameResult.m_losers.Add(loseSession);

            nowGameResult.m_draws = null;
            Debug.Log("승패 집계 완료");
        }

        return nowGameResult;
    }
    #endregion


    // 서버로 데이터 패킷 전송
    // 서버에서는 이 패킷을 받아 모든 클라이언트(패킷 보낸 클라이언트 포함)로 브로드캐스팅 해준다.
    public void SendDataToInGame<T>(T msg)
    {
        if (!InGameInfoManager.Instance.isGameOut)
        { 
            var byteArray = DataParser.DataToJsonData<T>(msg);
            Backend.Match.SendDataToInGameRoom(byteArray);
        }
    }
    //만약에 상대가 나갔을 경우 호출되는 메소드
    private void ProcessSessionOffline(SessionId sessionId)
    {
        Debug.Log("들어옴");
        
        if (hostSession.Equals(sessionId))
        {
            switch (SceneManager.GetActiveScene().name)
            {
                case "1. MenuScene":
                    InGameInfoManager.Instance.isGameOut = true;
                    InGameInfoManager.Instance.outSession = sessionId;
                    BackEndMatchManager.Instance.GameOutInitialize(true);
                    LobbyUI.Instance.SetErrorObject("상대 방이 게임을 떠났습니다.",true);
                    Debug.Log("상대가 떠났습니다.");
                    break;
                case "2. GameLoadRoom":
                    InGameInfoManager.Instance.isGameOut = true;
                    InGameInfoManager.Instance.outSession = sessionId;
                    break;
                //만약 게임 씬으로 들어왔을 때 상대방이 나간다면?
                //나간 상대는 패배로 처리한다.
                case "3. GameScene":
                    //인게임의 TimeScale을 멈추고 에러메세지 호출
                    InGameInfoManager.Instance.isGameOut = true;
                    InGameInfoManager.Instance.outSession = sessionId;
                    PVPInGM.Instance.RivalGameOut();
                    break;
            }
        }
        else
        {
            Debug.Log(SceneManager.GetActiveScene().name);
            switch (SceneManager.GetActiveScene().name)
            {
                case "1. MenuScene":
                    InGameInfoManager.Instance.isGameOut = true;
                    InGameInfoManager.Instance.outSession = sessionId;
                    BackEndMatchManager.Instance.GameOutInitialize(true);
                    LobbyUI.Instance.SetErrorObject("상대 방이 게임을 떠났습니다.", true);
                    Debug.Log("상대가 떠났습니다.");
                    break;
                case "2. GameLoadRoom":
                    InGameInfoManager.Instance.isGameOut = true;
                    InGameInfoManager.Instance.outSession = sessionId;
                    break;
                //만약 게임 씬으로 들어왔을 때 상대방이 나간다면?
                //나간 상대는 패배로 처리한다.
                case "3. GameScene":
                    //인게임의 TimeScale을 멈추고 에러메세지 호출
                    InGameInfoManager.Instance.isGameOut = true;
                    InGameInfoManager.Instance.outSession = sessionId;
                    //게임을 나갈 시에 이벤트
                    PVPInGM.Instance.RivalGameOut();
                    break;
            }
        }
    }
    

    private void ProcessSessionOnline(SessionId sessionId, string nickName)
    {
        //재접속
        Debug.Log("재접속");
        // 호스트가 아니면 아무 작업 안함 (호스트가 해줌)
        if (isHost)
        {
            // 재접속 한 클라이언트가 인게임 씬에 접속하기 전 게임 정보값을 전송 시 nullptr 예외가 발생하므로 조금
            // 2초정도 기다린 후 게임 정보 메시지를 보냄
        }
    }

    public bool PrevGameMessage(MatchRelayEventArgs args)
    {
        Protocol.Message msg = DataParser.ReadJsonData<Protocol.Message>(args.BinaryUserData);
        if (msg == null)
        {
            return false;
        }

        // 게임 설정 사전 작업 패킷 검사 
        switch (msg.type)
        {
            case Protocol.Type.SetPrevGameInfo:
                Protocol.SetPrevGameInfoMessage setPrevGameInfo = DataParser.ReadJsonData<Protocol.SetPrevGameInfoMessage>(args.BinaryUserData);
                ProcessPrevPlayerData(setPrevGameInfo);
                return true;
            case Protocol.Type.LoadRoomScene:

                LobbyUI.Instance.ChangeRoomLoadScene();
                return true;
        }
        return false;
    }

    //인게임 접속 시 플레이어의 정보 세팅 작업
    private void ProcessPrevPlayerData(Protocol.SetPrevGameInfoMessage setPrevGameInfo)
    {
        var gamers = BackEndMatchManager.Instance.sessionIdList;
        List<IconData> deckIconList = new List<IconData>();
        List<EmoticonData> EmoticonList = new List<EmoticonData>();
        int size = gamers.Count;
        if (size <= 0)
        {
            Debug.Log("플레이어 없음");
            return;
        }
        if (size > 2)
        {
            Debug.Log("플레이어 초과");
            return;
        }
        
        if (InGameInfoManager.Instance.mySessionID == setPrevGameInfo.playerSession)
        {
            AddPvpChar(setPrevGameInfo, setPrevGameInfo.playerSession,deckIconList);
            AddPVPEmoticon(setPrevGameInfo, setPrevGameInfo.playerSession,EmoticonList);
        }
        if(InGameInfoManager.Instance.mySessionID != setPrevGameInfo.playerSession)
        {
            AddPvpChar(setPrevGameInfo, setPrevGameInfo.playerSession,deckIconList);
            AddPVPEmoticon(setPrevGameInfo, setPrevGameInfo.playerSession, EmoticonList);
        }

    }
    private void AddPvpChar(Protocol.SetPrevGameInfoMessage setPrevGameInfo, SessionId sessionId, List<IconData> deckIconList)
    {
        //만약 게임 데이터매니저의 캐릭터 아이콘데이터의 고유넘버가 playerDecks의 넘버와 같은게 있다면 
        for (int i = 0; i < setPrevGameInfo.playerDecks.Length; i++)
        {
            for (int j = 0; j < GameDataManager.Instance.charIconDatas.Length; j++)
            {
                if (setPrevGameInfo.playerDecks[i] == GameDataManager.Instance.charIconDatas[j].uniqueNumber)
                {
                    //임시리스트에 등록
                    deckIconList.Add(GameDataManager.Instance.charIconDatas[j]);
                }
            }
        }
        InGameInfoManager.Instance.pvpCharctorDIc.Add(sessionId, deckIconList);
    }
    private void AddPVPEmoticon(Protocol.SetPrevGameInfoMessage setPrevGameInfo, SessionId sessionId,List<EmoticonData> EmoticonList)
    {
        //만약 게임 데이터매니저의 이모티콘 데이터의 고유넘버가 playerEmoticons의 넘버와 같은게 있다면 
        
            
        for (int i = 0; i < setPrevGameInfo.playerEmoticons.Length; i++)
        {
            for (int j = 0; j < GameDataManager.Instance.EmoticonDatas.Length; j++)
            {
                if (setPrevGameInfo.playerEmoticons[i] == GameDataManager.Instance.EmoticonDatas[j].uniqueNumber)
                {
                    //임시리스트에 등록
                    EmoticonList.Add(GameDataManager.Instance.EmoticonDatas[j]);
                }
            }
        }
        InGameInfoManager.Instance.pvpEmoticonDic.Add(sessionId, EmoticonList);
    }
}


