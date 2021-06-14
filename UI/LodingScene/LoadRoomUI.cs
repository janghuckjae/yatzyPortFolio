using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using BackEnd.Tcp;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadRoomUI : MonoBehaviour
{
    private static LoadRoomUI _instance;

    public static LoadRoomUI Instance // 싱글톤 구현
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(LoadRoomUI)) as LoadRoomUI;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(LoadRoomUI)) as LoadRoomUI;
                }
            }
            return _instance;
        }
    }
    [System.Serializable]
    public struct UserInfoUI
    {
        public TextMeshProUGUI nickNameText;
        public TextMeshProUGUI pointText;
    }
    public UserInfoUI[] userInfos ;

    [SerializeField] private Image loadRoomBG; 
    [SerializeField] private  GameObject gameLodigBar;
    [SerializeField] private Image gameLodigBarImg;
    private int numOfClient = -1;
    void Start()
    {

        gameLodigBar.SetActive(false);

        var matchInstance = BackEndMatchManager.Instance;
        if (matchInstance == null)
        {
            Debug.Log("리턴!!");
            return;
        }

        numOfClient = matchInstance.gameRecords.Count;
        if (numOfClient <= 0)
        {
            Debug.LogError("numOfClient가 0이하입니다.");
            return;
        }
        // 내 정보를 위쪽에 두고 상대방 정보를 아래쪽으로 두고 싶다.
        foreach (var record in matchInstance.gameRecords.OrderByDescending(x => x.Key))
        {

            if (record.Key == InGameInfoManager.Instance.mySessionID)
            {
                ShowUserInfo(record, 0);
            }
            if (record.Key != InGameInfoManager.Instance.mySessionID)
            {
                ShowUserInfo(record, 1);
            }
        }
        loadRoomBG.sprite = InGameInfoManager.Instance.pvpBackGround;
        //1초후 게임씬으로 넘아가게하기
        Invoke("GotoGameScene", 1f);
    }
    //게임 로딩 씬 때 유저의 프로필?, 닉네임, 덱, 마법을 보여준다.
    private void ShowUserInfo(KeyValuePair<SessionId, MatchUserGameRecord> record,int index)
    {
        // 닉네임과 포인트 보여주기
        var data = userInfos[index];
        data.nickNameText.text = record.Value.m_nickname;
        data.pointText.text = string.Format("{0}", record.Value.m_points);
    }
    private void GotoGameScene()
    {
        gameLodigBar.SetActive(true);
        StartCoroutine(LoadGameScene());
    }

    IEnumerator LoadGameScene()
    {
        yield return null;
        AsyncOperation op = SceneManager.LoadSceneAsync("3. GameScene");

        op.allowSceneActivation = false;

        float timer = 0.0f;
        while (!op.isDone)
        {
            yield return null;
            timer += Time.deltaTime;
            if (op.progress < 0.9f)
            {
                gameLodigBarImg.fillAmount = Mathf.Lerp(gameLodigBarImg.fillAmount, op.progress, timer);
                if (gameLodigBarImg.fillAmount >= op.progress)
                {
                    timer = 0f;
                }
            }
            else
            {
                gameLodigBarImg.fillAmount = Mathf.Lerp(gameLodigBarImg.fillAmount, 1f, timer);
                if (gameLodigBarImg.fillAmount == 1.0f)
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }
}
