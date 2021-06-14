using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class SelectStageManager : MonoBehaviour
{
    [System.Serializable]
    private struct StageStruct
    {
        //스테이지 별 정보 
        public StageData[] stageDatas;
    }
    [SerializeField]
    private StageStruct[] arrStageStruct;
    [SerializeField]private StageImgStruct[] backGrounds;

    [Header("맵 정보 UI관련")]
    public Image[] InfoImages;
    private Image[] enemyImage;
    //스테이지를 선택하면 나오는 정보창
    public GameObject mapInfoUI;
    [SerializeField] private Image backGroundInfo;

    [Header("경고창 관련")]
    [SerializeField]private GameObject warningWindow;
    [SerializeField] private Text warningText;
    private readonly float warnningTime=1.5f;
    WaitForSeconds warningWaitTime;

    private string mapName;
    private string[] splitName;
    readonly char sp = '-';
    string s_name;

    //스테이지 버튼을 눌러 맵과 배경을 골라주는 기능 
    private void Start()
    {
        backGrounds = GameDataManager.Instance.inGameBGImgs;
           arrStageStruct = new StageStruct[GameDataManager.Instance.arrStageStruct.Length];
        for (int j = 0; j < GameDataManager.Instance.arrStageStruct.Length; j++)
        {
            arrStageStruct[j].stageDatas = new StageData[GameDataManager.Instance.arrStageStruct[j].stageDatas.Length];
            for (int k = 0; k < GameDataManager.Instance.arrStageStruct[j].stageDatas.Length; k++)
            {
                arrStageStruct[j].stageDatas[k] = GameDataManager.Instance.arrStageStruct[j].stageDatas[k];
            }
        }

        warningWaitTime = new WaitForSeconds(warnningTime);
        enemyImage = new Image[InfoImages.Length];
        for (int i = 0; i < InfoImages.Length; i++)
        {
            enemyImage[i] = InfoImages[i].transform.GetChild(0).GetComponent<Image>();
        }
    }
    //싱글 스테이지 일때
    public void MapSelect()
    {
        mapName = EventSystem.current.currentSelectedGameObject.name;
        s_name = mapName.Substring(5);
        // '-'기준으로 나누기 
        splitName = s_name.Split(sp);

        //스테이지 데이터는 챕터 넘버-1,스테이지 넘버-1 =>ex) 0,0배열에 있는 스테이지 데이터
        InGameInfoManager.Instance.selectChapterNum = int.Parse(splitName[0]);
        InGameInfoManager.Instance.selectStageNum = int.Parse(splitName[1]);
        InGameInfoManager.Instance.selectStageData = arrStageStruct[InGameInfoManager.Instance.selectChapterNum - 1].stageDatas[InGameInfoManager.Instance.selectStageNum - 1];

        InGameInfoManager.Instance.selectBackGround = backGrounds[InGameInfoManager.Instance.selectChapterNum - 1];
        MapInfoChange(InGameInfoManager.Instance.selectChapterNum - 1);
        mapInfoUI.SetActive(true);
        InventoryManager.Instance.selectWindow.SetActive(false);
    }
    //버튼 클릭시 선택한 스테이지의 맵,적 종류 등에 따라 이미지,정보를 교체 해준다.
    private void MapInfoChange(int chapterNum)
    {
        //배경 적용
        backGroundInfo.sprite = backGrounds[chapterNum].sampleImg;
        //이미지 배열과 선택한 적 배열의 크기가 맞지 않는다면
        //이미지 배열에서 적 배열 크기까지의 이미지는 켜주고 그 밖의 이미지는 꺼준다.

        if (InfoImages.Length != InGameInfoManager.Instance.selectStageData.enemiesImg.Length)
        {
            for (int i = 0; i < InGameInfoManager.Instance.selectStageData.enemiesImg.Length; i++)
            {
                enemyImage[i].sprite = InGameInfoManager.Instance.selectStageData.enemiesImg[i];
                InfoImages[i].gameObject.SetActive(true);
            }
            for (int j = InGameInfoManager.Instance.selectStageData.enemiesImg.Length; j < InfoImages.Length; j++)
            {
                InfoImages[j].transform.gameObject.SetActive(false);
            }
        }
    }
    //맵 정보 UI에서 게임 시작 버튼을 누르면 게임 씬으로 이동한다.
    public void GoGameBtnClick()
    {
        //덱에 캐릭터 정보가 하나라도 있어야 게임 씬으로 갈수 있다.
        if (InGameInfoManager.Instance.charactorDatas.Count != 0)
        {
            MenuLoadingSceneManager.LoadingtoNextScene("3. GameScene");
        }
        else 
        {
            //만약 덱에 캐릭터가 없다면 경고창이 뜨게한다.
            StartCoroutine(WarningTime());
        }
    }
    //Cancel 버튼을 누르면 지정한 버튼의 SetActive를 꺼준다.
    public void CancelBtnClick(GameObject gameObject )
    {
        gameObject.SetActive(false);
    }

    IEnumerator WarningTime()
    {
        warningText.text = "덱에 아무것도 없습니다.\n덱을 구성해주세요.";
        warningWindow.SetActive(true);
        yield return warningWaitTime;
        warningWindow.SetActive(false);
        mapInfoUI.SetActive(false);
    }


}
