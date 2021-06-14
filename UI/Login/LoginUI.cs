using UnityEngine;
using UnityEngine.UI;
using Battlehub.Dispatcher;

public class LoginUI : MonoBehaviour
{
    private static LoginUI _instance;

    public static LoginUI Instance // 싱글톤 구현
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(LoginUI)) as LoginUI;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(LoginUI)) as LoginUI;
                }
            }
            return _instance;
        }
    }

    public GameObject touchStart;
    public GameObject startObj;
    public GameObject loginObject;
    public GameObject customLoginObject;
    public GameObject signUpObject;
    public GameObject errorObject;
    public GameObject nicknameObject;
    public Toggle customPWToggle;
    public Text errorBtnText;
    public Button errorbtn;

    private InputField[] loginField;
    private InputField[] signUpField;
    private InputField nicknameField;
    private Text touchStartText;
    private Text errorText;
    private GameObject loadingObject;

    private const byte ID_INDEX = 0;
    private const byte PW_INDEX = 1;

    private Text showPWtext;
    private string pwString;
    
    void Start()
    {
        startObj.SetActive(false);

        loginField = customLoginObject.GetComponentsInChildren<InputField>();
        signUpField = signUpObject.GetComponentsInChildren<InputField>();
        nicknameField = nicknameObject.GetComponentInChildren<InputField>();
        errorText = errorObject.GetComponentInChildren<Text>();
        touchStartText = touchStart.GetComponentInChildren<Text>();
        showPWtext = loginField[PW_INDEX].transform.GetChild(1).GetComponent<Text>();
        loadingObject = GameObject.FindGameObjectWithTag("Loading");
        loadingObject.SetActive(false);

        //백엔드 토큰이 있다면 자동 로그인
        AutoLogin();
    }

    public void TouchStart()
    {
        loadingObject.SetActive(true);
        //만약 로그인 된 상태 라면?
        if (BackEndServerManager.Instance.isLogin )
        {
            //만약 내가 로그인 한 상태에서 다른 기기에서 로그인을 한 상태라면?
            if (!BackEndServerManager.Instance.ISAccessTokenAlive())
            {
                loadingObject.SetActive(false);
                return;
            }
            if (BackEndServerManager.Instance.myNickName != string.Empty) { ChangeLobbyScene(); }
            //로그인을 했는데 닉네임이 없다면?
            else {
                BackEndServerManager.Instance.LogOut();
                loadingObject.SetActive(false);
                startObj.SetActive(false);
                //customLoginObject.SetActive(true);
                loginObject.SetActive(true);
            }
            
        }
        else
        {
            loadingObject.SetActive(false);
            startObj.SetActive(false);
            //customLoginObject.SetActive(true);
            loginObject.SetActive(true);
        }
    }

    public void Login()
    {
        if (errorObject.activeSelf)
        {
            return;
        }
        string id = loginField[ID_INDEX].text;
        string pw = loginField[PW_INDEX].text;

        if (id.Equals(string.Empty) || pw.Equals(string.Empty))
        {
            errorText.text = "ID 혹은 PW 를 먼저 입력해주세요.";
            errorObject.SetActive(true);
            return;
        }

        loadingObject.SetActive(true);
        
        BackEndServerManager.Instance.CustomLogin(id, pw, (bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                
                if (!result)
                {
                    loadingObject.SetActive(false);
                    errorText.text = "로그인 에러\n\n" + error;
                    errorObject.SetActive(true);
                    return;
                }
                ShowStartMenu();
            });
        });
    }

    public void SignUp()
    {
        if (errorObject.activeSelf)
        {
            return;
        }
        string id = signUpField[ID_INDEX].text;
        string pw = signUpField[PW_INDEX].text;

        if (id.Equals(string.Empty) || pw.Equals(string.Empty))
        {
            errorText.text = "ID 혹은 PW 를 먼저 입력해주세요.";
            errorObject.SetActive(true);
            return;
        }

        loadingObject.SetActive(true);
        BackEndServerManager.Instance.CustomSignIn(id, pw, (bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                if (error != null)
                {
                    Debug.Log(error);
                }
                if (!result)
                {
                    loadingObject.SetActive(false);
                    errorText.text = "회원가입 에러\n\n" + error;
                    errorObject.SetActive(true);
                    return;
                }
                ActiveNickNameObject();
            });
        });
    }
    //로그인 했을 때 
    public void ActiveNickNameObject()
    {
        Dispatcher.Current.BeginInvoke(() =>
        {
            startObj.SetActive(false);
            loginObject.SetActive(false);
            customLoginObject.SetActive(false);
            signUpObject.SetActive(false);
            errorObject.SetActive(false);
            loadingObject.SetActive(false);
            nicknameObject.SetActive(true);
        });
    }

    public void UpdateNickName()
    {
        if (errorObject.activeSelf)
        {
            return;
        }
        string nickname = nicknameField.text;
        if (nickname.Equals(string.Empty))
        {
            errorText.text = "닉네임을 먼저 입력해주세요";
            errorObject.SetActive(true);
            return;
        }
        loadingObject.SetActive(true);
        
        BackEndServerManager.Instance.UpdateNickname(nickname, (bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                if (!result)
                {
                    loadingObject.SetActive(false);
                    errorText.text = "닉네임 생성 오류\n\n" + error;
                    errorObject.SetActive(true);
                    return;
                }
                ShowStartMenu();
            });
        });
    }

    public void GoogleFederation()
    {
        if (errorObject.activeSelf)
        {
            return;
        }

        loadingObject.SetActive(true);
        BackEndServerManager.Instance.GoogleAuthorizeFederation((bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                if (!result)
                {
                    loadingObject.SetActive(false);
                    errorText.text = "로그인 에러\n\n" + error;
                    errorObject.SetActive(true);
                    return;
                }
                ShowStartMenu();
            });
        });
    }

    public void AccountChange()
    {
        //로그아웃
        BackEndServerManager.Instance.LogOut();
        //시작 창 비활성화
        startObj.SetActive(false);
        //로그인 UI 활성화
        loginObject.SetActive(true);
    }

    //백앤드 토큰이 있다면 로그인 하고 스타트 버튼 텍스트에 닉네임을 뿌려줌('닉네임'님 환영합니다.) 
    void AutoLogin()
    {
        BackEndServerManager.Instance.BackendTokenLogin((bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                if (result)
                {
                    ShowStartMenu();
                    return;
                }
                if (!error.Equals(string.Empty))
                {
                    errorText.text = "유저 정보 불러오기 실패\n\n" + error;
                    errorObject.SetActive(true);
                    return;
                }
                startObj.SetActive(true);
            });
        });
        
    }
    public void ShowStartMenu()
    {
        if (BackEndServerManager.Instance.isLogin)
        {
            if (BackEndServerManager.Instance.myNickName != string.Empty)
            {
                touchStartText.text = BackEndServerManager.Instance.myNickName + " 님 환영합니다.";
            }
            else
            {
                touchStartText.text = "Touch to Start";
            }
            loginObject.SetActive(false);
            customLoginObject.SetActive(false);
            signUpObject.SetActive(false);
            errorObject.SetActive(false);
            loadingObject.SetActive(false);
            nicknameObject.SetActive(false);
            startObj.SetActive(true);

        }
    }
    //비밀 번호 감추기 
    public void HideCustomPW()
    {
        //감추기 활성화
        if (customPWToggle.isOn) 
        {
            loginField[PW_INDEX].contentType = InputField.ContentType.Password;
            loginField[PW_INDEX].ActivateInputField();
        }
        //감추기 비활성화
        else
        {
            loginField[PW_INDEX].contentType = InputField.ContentType.Standard;
            loginField[PW_INDEX].ActivateInputField();
        }
    }
    //중복 접속 오류 발생 시 호출되는 함수 
    public void DuplicateConnectError(bool AddOutEvent)
    {
        errorText.text = "다른 기기에서 로그인해서 로그아웃 되었습니다.";
        if (AddOutEvent)
        {
            //에러 버튼에 게임 종료 이벤트를 추가해준다. 
            errorText.text = "다른 기기에서 로그인 했습니다.";
            errorbtn.onClick.AddListener(GameManager.Instance.OutGame);
            errorBtnText.text = "나가기";
            touchStartText.text = "Touch to Start";
        }
        errorObject.SetActive(true);
    }


    void ChangeLobbyScene()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.MenuScene);
    }
}
