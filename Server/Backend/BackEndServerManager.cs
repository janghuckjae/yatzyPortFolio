using System;
using System.Collections.Generic;
using UnityEngine;
// Include Backend
using BackEnd;
using static BackEnd.SendQueue;
//  Include GPGS namespace
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using System.Collections;
#if UNITY_IOS
using UnityEngine.SignInWithApple;
#endif
/*
 * Backend-4.2.1
 * 뒤끝의 기본 기능을 정의
 * 뒤끝 초기화
 * 커스텀 회원가입
 * 커스텀 로그인
 * 유저정보 불러오기
 */
public class BackEndServerManager : MonoBehaviour
{
    private static BackEndServerManager _instance;

    public static BackEndServerManager Instance // 싱글톤 구현
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(BackEndServerManager)) as BackEndServerManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(BackEndServerManager)) as BackEndServerManager;
                }
            }
            return _instance;
        }
    }
    bool mePause;
    public bool isRivalPause;   //만약 상대가 잠시 자리를 비웠을 경우 

    public bool isLogin { get; private set; }   // 로그인 여부

    private string tempNickName;                        // 설정할 닉네임 (id와 동일)
    public string myNickName { get; private set; } = string.Empty;  // 로그인한 계정의 닉네임
    public string myIndate { get; private set; } = string.Empty;    // 로그인한 계정의 inDate
    private Action<bool, string> loginSuccessFunc = null;

    private const string BackendError = "statusCode : {0}\nErrorCode : {1}\nMessage : {2}";


    void Awake()
    {
       
        // 모든 씬에서 유지
        DontDestroyOnLoad(this.gameObject);

    }

   

    /*
	 * 서버 초기화
	 */
    void Start()
    {
#if UNITY_ANDROID
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration
            .Builder()
            .RequestServerAuthCode(false)
            .RequestIdToken()
            .Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;

        PlayGamesPlatform.Activate();
#endif
        isLogin = false;
        try
        {
            Backend.Initialize(() =>
            {
                if (Backend.IsInitialized)
                {
#if UNITY_ANDROID
                    Debug.Log("GoogleHash - " + Backend.Utils.GetGoogleHash());
#endif
                    // 비동기 함수 큐 초기화
                    StartSendQueue(true);
                }
                else
                {
                    Debug.Log("뒤끝 초기화 실패");
                }
            });
        }
        catch (Exception e)
        {
            Debug.Log("[예외]뒤끝 초기화 실패\n" + e.ToString());
        }
    }

    // 게임 종료, 에디터 종료 시 호출
    // 비동기 큐 쓰레드를 중지시킴
    // 해당 함수는 실제 안드로이드, iOS 환경에서 호출이 안될 수도 있다 (각 os의 특징 때문)
    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        StopSendQueue();
    }

    // 게임 시작, 게임 종료, 백그라운드로 돌아갈 때(홈버튼 누를 때) 호출됨
    // 위의 종료함수와는 달리 무조건 호출됨
    // 비동기 큐 종료, 재시작
#if UNITY_EDITOR
    void OnApplicationPause(bool isPause)
    {
        Debug.Log("OnApplicationPause : " + isPause);
        if (isPause)
        {
            mePause = true;
            PauseSendQueue();
        }
        else
        {
            if (mePause)
            {
                mePause = false;
                ResumeSendQueue();
            }
        }
    }

#endif


    //로그아웃 하기 
    public void LogOut()
    {
        Enqueue(Backend.BMember.Logout, callback =>
        {
            if (!callback.IsSuccess())
            {
                Debug.LogError("로그아웃 실패\n" + callback);
                return;
            }
            //로그인 상태 해제
            Debug.Log("로그아웃 성공");
            isLogin = false;
            myNickName = string.Empty;
            myIndate = string.Empty;
            loginSuccessFunc = null;
            tempNickName = null;
        });
    }
    // 뒤끝 토큰으로 로그인
    public void BackendTokenLogin(Action<bool, string> func)
    {
        Enqueue(Backend.BMember.LoginWithTheBackendToken, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("토큰 로그인 성공");
                loginSuccessFunc = func;

                OnPrevBackendAuthorized();
                return;
            }

            Debug.Log("토큰 로그인 실패\n" + callback.ToString());
            //다른기기에서 로그인 해서 실패;
            if (callback.GetErrorCode().Equals("BadUnauthorizedException"))
            { 
                LoginUI.Instance.DuplicateConnectError(false);
            }
            func(false, string.Empty);
        });
    }

    // 커스텀 로그인
    public void CustomLogin(string id, string pw, Action<bool, string> func)
    {
        Enqueue(Backend.BMember.CustomLogin, id, pw, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("커스텀 로그인 성공");
                loginSuccessFunc = func;

                OnPrevBackendAuthorized();
                return;
            }

            Debug.Log("커스텀 로그인 실패\n" + callback);
            func(false, string.Format(BackendError,
                callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
        });
    }
    public bool ISAccessTokenAlive()
    {

        if (!Backend.BMember.IsAccessTokenAlive().IsSuccess())
        {

            Debug.Log("Access토큰이 활성화 되있지않습니다.");
            LoginUI.Instance.DuplicateConnectError(true);
            return false;
        }
        else
        {
            Debug.Log("Access토큰이 활성화 되있습니다.");
            return true;

        }
    }


    // 커스텀 회원가입
    public void CustomSignIn(string id, string pw, Action<bool, string> func)
    {
        tempNickName = id;
        Enqueue(Backend.BMember.CustomSignUp, id, pw, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("커스텀 회원가입 성공");
                loginSuccessFunc = func;

                OnPrevBackendAuthorized();
                return;
            }

            func(false, string.Format(BackendError,
                callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
        });
    }

    // 구글 페더레이션 로그인/회원가입
    public void GoogleAuthorizeFederation(Action<bool, string> func)
    {
#if UNITY_ANDROID
        // 이미 gpgs 로그인이 된 경우
        if (Social.localUser.authenticated == true)
        {
            var token = GetFederationToken();
            if (token.Equals(string.Empty))
            {
                Debug.LogError("GPGS 토큰이 존재하지 않습니다.");
                func(false, "GPGS 인증을 실패하였습니다.\nGPGS 토큰이 존재하지 않습니다.");
                return;
            }

            Enqueue(Backend.BMember.AuthorizeFederation, token, FederationType.Google, "gpgs 인증", callback =>
            {
                if (callback.IsSuccess())
                {
                    Debug.Log("GPGS 인증 성공");
                    loginSuccessFunc = func;

                    OnPrevBackendAuthorized();
                    return;
                }

                Debug.LogError("GPGS 인증 실패\n" + callback.ToString());
                func(false, string.Format(BackendError,
                    callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
            });
        }
        // gpgs 로그인을 해야하는 경우
        else
        {
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    var token = GetFederationToken();
                    if (token.Equals(string.Empty))
                    {
                        Debug.LogError("GPGS 토큰이 존재하지 않습니다.");
                        func(false, "GPGS 인증을 실패하였습니다.\nGPGS 토큰이 존재하지 않습니다.");
                        return;
                    }

                    Enqueue(Backend.BMember.AuthorizeFederation, token, FederationType.Google, "gpgs 인증", callback =>
                    {
                        if (callback.IsSuccess())
                        {
                            Debug.Log("GPGS 인증 성공");
                            loginSuccessFunc = func;

                            OnPrevBackendAuthorized();
                            return;
                        }

                        Debug.LogError("GPGS 인증 실패\n" + callback.ToString());
                        func(false, string.Format(BackendError,
                            callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
                    });
                }
                else
                {
                    func(false, "GPGS 인증을 실패하였습니다.\nGPGS 로그인을 실패하였습니다.");
                    return;
                }
            });
        }
#endif
    }

    private string GetFederationToken()
    {
#if UNITY_ANDROID
        if (!PlayGamesPlatform.Instance.localUser.authenticated)
        {
            Debug.LogError("GPGS에 접속되어있지 않습니다. PlayGamesPlatform.Instance.localUser.authenticated :  fail");
            return string.Empty;
        }
        // 유저 토큰 받기
        string _IDtoken = PlayGamesPlatform.Instance.GetIdToken();
        tempNickName = PlayGamesPlatform.Instance.GetUserDisplayName();
        Debug.Log(tempNickName);
        return _IDtoken;
#endif
    }

    public void UpdateNickname(string nickname, Action<bool, string> func)
    {
        Enqueue(Backend.BMember.UpdateNickname, nickname, bro =>
        {
            // 닉네임이 없으면 매치서버 접속이 안됨
            if (!bro.IsSuccess())
            {
                //Debug.LogError("닉네임 생성 실패\n" + bro.ToString());
                func(false, string.Format(BackendError,
                    bro.GetStatusCode(), bro.GetErrorCode(), bro.GetMessage()));
                return;
            }
            loginSuccessFunc = func;
            OnBackendAuthorized();
        });
    }

    // 유저 정보 불러오기 사전작업
    private void OnPrevBackendAuthorized()
    {
        isLogin = true;

        OnBackendAuthorized();
    }

    // 실제 유저 정보 불러오기
    private void OnBackendAuthorized()
    {
        Enqueue(Backend.BMember.GetUserInfo, callback =>
        {
            if (!callback.IsSuccess())
            {
                Debug.LogError("유저 정보 불러오기 실패\n" + callback);
                loginSuccessFunc(false, string.Format(BackendError,
                callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
                return;
            }
            Debug.Log("유저정보\n" + callback);
            var info = callback.GetReturnValuetoJSON()["row"];
            //만약 닉네임이 없다면 로그인 UI의 닉네임창을 활성화
            if (info["nickname"] == null)
            {
                if (LoginUI.Instance.signUpObject.activeSelf || LoginUI.Instance.customLoginObject.activeSelf)
                {
                    LoginUI.Instance.ActiveNickNameObject();
                }
                else { LoginUI.Instance.ShowStartMenu(); }
                
                return;
            }
            myNickName = info["nickname"].ToString();
            myIndate = info["inDate"].ToString();

            if (loginSuccessFunc != null)
            {
                BackEndMatchManager.Instance.GetMatchList(loginSuccessFunc);
                // loginSuccessFunc(true, string.Empty);
            }
        });
    }
   


    void Update()
    {
        SendQueue.Poll();
    }

}