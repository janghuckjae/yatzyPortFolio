using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BackEnd.Tcp;
using Battlehub.Dispatcher;
using BackEnd;

public partial class LobbyUI : MonoBehaviour
{
    private List<string> readyUserList = null;
    public void StartVSMode()
    {
        if (InGameInfoManager.Instance.charactorDatas.Count == 0)
        {
            SetErrorObject("덱을 구성해주세요", false);
            return;
        }
        //만약 덱 리스트에 근,원,탱,서폿이 각각 1마리 이상 씩 없다면 오류메세지와 함께 return해준다.
        for (int i = 0;i< InGameInfoManager.Instance.charactorDatas.Count; i+=2)
        {
            if (InGameInfoManager.Instance.charactorDatas[i] == null)
            {
                if (i < 2) { SetErrorObject("근접 유닛을 추가해주세요", false); }

                else if (i >= 2 && i < 4) { SetErrorObject("원거리 유닛을 추가해주세요", false); }
                
                else if (i >= 4 && i < 6) { SetErrorObject("탱커 유닛을 추가해주세요", false); }
                
                else if (i >= 6 && i < 8) { SetErrorObject("서폿 유닛을 추가해주세요", false); }
                return;
            }
        }
        // 매치 서버에 대기방 생성 요청
        if (BackEndMatchManager.Instance.CreateMatchRoom() == true)
        {
            if (errorObject.activeSelf) { errorObject.SetActive(false); }
            MatchProgressObject.SetActive(true);
            gameModeUI.SetActive(false);
            matchInfoText.text = matchOriginStr;
            loadingObject.SetActive(true);
            //덱정보 보내기
            DeckParsing();
            //이모티콘 정보 보내기
            EmoticonParsing();
            
        }
    }
    //매칭 취소 버튼을 누를 시 
    public void CancelVSMode()
    {
        Debug.Log("매칭 취소 버튼 누름");
        if (errorObject.activeSelf) { errorObject.SetActive(false); }
        //매칭을 캔슬 해주고 
        RequestCancel();
        //대기방에서 나간다.
        LeaveReadyRoom();
        //초기화
        ClearPVPInfo();
    }


    //덱정보를 들고간다음 서버에 통신
    private void DeckParsing()
    {
        //만약 캐릭터 정보 중에 null이 있다면 99999을 추가해준다.
        //멀티 덱정보에도 고유번호로 바꿔서 보내준다.
        for (int i = 0; i < InGameInfoManager.Instance.charactorDatas.Count; i++)
        {
            if (InGameInfoManager.Instance.charactorDatas[i] != null)
            {
                InGameInfoManager.Instance.pvpCharactorDatas.Add(InGameInfoManager.Instance.charactorDatas[i].uniqueNumber);
            }
            else
            {
                InGameInfoManager.Instance.pvpCharactorDatas.Add("99999");
            }
        }
        //맵 같은 경우 전체리스트 중 랜덤으로 뽑아 등록해준다.
    }
    private void EmoticonParsing()
    {
        //이모티콘 정보 GameDataManager의 이모티콘 순서로 결정
        for (int i = 0; i < InGameInfoManager.Instance.EmoticonDatas.Count; i++)
        {
            InGameInfoManager.Instance.pvpEmoticonDatas.Add(InGameInfoManager.Instance.EmoticonDatas[i].uniqueNumber);
        }

    }
    
    public void CreateRoomResult(bool isSuccess, List<MatchMakingUserInfo> userList = null)
    {
        // 대기 방 생성에 성공 시 
        if (isSuccess == true)
        {
            Debug.Log("대기방 생성에 성공");
            if (userList == null)
            {
                //만약 userList에 아무것도 없다면 내 닉네임을 리스트에 추가해주고
                SetReadyUserList(BackEndServerManager.Instance.myNickName);
                //매칭을 신청한다.
                RequestMatch();
            }
            else
            {
                //있는 UserList를 세팅하고 
                SetReadyUserList(userList);
                //매칭을 신청한다.
                RequestMatch();
            }
        }
        // 대기 방 생성에 실패 시 에러를 띄움
        else
        {
            SetErrorObject("대기방 생성에 실패했습니다.\n\n잠시 후 다시 시도해주세요.",true);
        }
    }
    //대기방에서 나가기
    public void LeaveReadyRoom()
    {
        Debug.Log("매치 대기방에서 나가기");
        BackEndMatchManager.Instance.LeaveMatchLoom();
    }

    //매칭 방법을 문의 한다.
    public void RequestMatch()
    {
        if (errorObject.activeSelf || isMatchDone)
        {
            Debug.Log("매칭 방법 문의 실패 " + errorObject.activeSelf + ":" + isMatchDone);
            return;
        }
        //멀티 모드 bool 함수 활성화
        Debug.Log("매칭 방법 문의 하기");
        //PVP 모드 진입
        InGameInfoManager.Instance.isPVPMode = true;
        //배경화면 설정(임시로 첫번째 배경으로 함)
        InGameInfoManager.Instance.pvpBackGround = GameDataManager.Instance.pvpGameBGImgs[0];
        BackEndMatchManager.Instance.RequestMatchMaking(0);
    }
    // 대기방 인원 설정
    public void SetReadyUserList(List<MatchMakingUserInfo> userList)
    {
        ClearReadyUserList();

        if (userList == null)
        {
            Debug.LogError("ready user list is null");
            return;
        }
        if (userList.Count <= 0)
        {
            Debug.LogError("ready user list is empty");
            return;
        }

        foreach (var user in userList)
        {
            InsertReadyUserPrefab(user.m_nickName);
        }
        Debug.Log("대기방 인원 설정");
    }

    public void SetReadyUserList(string nickName)
    {
        ClearReadyUserList();

        if (string.IsNullOrEmpty(nickName))
        {
            Debug.LogError("ready user list is empty");
            return;
        }

        InsertReadyUserPrefab(nickName);
        Debug.Log("유저리스트에 아무것도없어서 내닉네임 리스트에 추가");
    }

    public void InsertReadyUserPrefab(string nickName)
    {
        if (readyUserList == null)
        {
            return;
        }

        if (readyUserList.Contains(nickName))
        {
            return;
        }
        readyUserList.Add(nickName);
    }

    public void DeleteReadyUserPrefab(string nickName)
    {
        if (readyUserList == null)
        {
            return;
        }

        if (readyUserList.Contains(nickName) == false)
        {
            return;
        }

        for (int i = 0; i < readyUserList.Count; ++i)
        {
            if (nickName.Equals(readyUserList[i]) == false)
            {
                continue;
            }
        }
        readyUserList.Remove(nickName);
    }

    private void ClearReadyUserList()
    {
        readyUserList = new List<string>();
    }
}

