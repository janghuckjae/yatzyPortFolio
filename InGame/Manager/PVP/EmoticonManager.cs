using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Protocol;
using Spine.Unity;

public class EmoticonManager : MonoBehaviour
{
    private static EmoticonManager _instance;
    public static EmoticonManager Instance // 싱글톤 구현
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(EmoticonManager)) as EmoticonManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(EmoticonManager)) as EmoticonManager;
                }
            }
            return _instance;
        }
    }


    private readonly Dictionary<int, SkeletonDataAsset> emoticonDic = new Dictionary<int, SkeletonDataAsset>();
    [SerializeField] private float emoticonTime;
    private WaitForSeconds emoticonDelayTime;
    
    [Space(10f)]
    [SerializeField] private GameObject emoticonUI;
    [SerializeField] private GameObject playerSpeechBubble;
    [SerializeField] private GameObject rivalSpeechBubble;
    private SkeletonGraphic playerSpeechAsset;
    private SkeletonGraphic rivalSpeechAsset;
    
    [Space(10f)]
    [SerializeField] private GameObject[] myEmoticonObj;
    [SerializeField] private Button[] myEmoticonBtn;
    private Image[] myEmoticonImg;

    // Start is called before the first frame update
    void Start()
    {
        if (InGameInfoManager.Instance.isPVPMode)
        {
            myEmoticonBtn = new Button[myEmoticonObj.Length];
            myEmoticonImg = new Image[myEmoticonObj.Length];
            for (int i = 0; i < myEmoticonObj.Length; i++)
            {
                myEmoticonBtn[i] = myEmoticonObj[i].transform.GetComponent<Button>();
                myEmoticonImg[i] = myEmoticonObj[i].transform.GetComponent<Image>();
            }
            playerSpeechAsset = playerSpeechBubble.transform.GetComponent<SkeletonGraphic>();
            rivalSpeechAsset = rivalSpeechBubble.transform.GetComponent<SkeletonGraphic>();

            playerSpeechBubble.SetActive(false);
            rivalSpeechBubble.SetActive(false);

            emoticonDelayTime = new WaitForSeconds(emoticonTime); 
            //이모티콘 세팅
            SetEmoticon();
        }
    }
    //이모티콘 세팅
    private void SetEmoticon()
    {
        if (BackEndMatchManager.Instance.IsHost())
        {
            for (int i = 0; i < InGameInfoManager.Instance.pvpEmoticonDic[InGameInfoManager.Instance.mySessionID].Count; i++)
            {
                emoticonDic.Add(i + 1, InGameInfoManager.Instance.pvpEmoticonDic[InGameInfoManager.Instance.mySessionID][i].emoticonSpineAsset);
                //이미지 넣어주기
                myEmoticonImg[i].sprite = InGameInfoManager.Instance.pvpEmoticonDic[InGameInfoManager.Instance.mySessionID][i].EmoticonImage;
                //버튼 이벤트 넣어주기
                int emoticonNum = i + 1;
                myEmoticonBtn[i].onClick.AddListener(() => SelectEmoticon(emoticonNum));
            }
            for (int j = 0; j < InGameInfoManager.Instance.pvpEmoticonDic[InGameInfoManager.Instance.rivalSessionID].Count; j++)
            {
                emoticonDic.Add(j + 9, InGameInfoManager.Instance.pvpEmoticonDic[InGameInfoManager.Instance.rivalSessionID][j].emoticonSpineAsset);
            }
        }
        else
        {
            for (int i = 0; i < InGameInfoManager.Instance.pvpEmoticonDic[InGameInfoManager.Instance.mySessionID].Count; i++)
            {
                emoticonDic.Add(i + 9, InGameInfoManager.Instance.pvpEmoticonDic[InGameInfoManager.Instance.mySessionID][i].emoticonSpineAsset);
                //이미지 넣어주기
                myEmoticonImg[i].sprite = InGameInfoManager.Instance.pvpEmoticonDic[InGameInfoManager.Instance.mySessionID][i].EmoticonImage;
                //버튼 이벤트 넣어주기
                int emoticonNum = i + 9;
                myEmoticonBtn[i].onClick.AddListener(() => SelectEmoticon(emoticonNum));
            }
            for (int j = 0; j < InGameInfoManager.Instance.pvpEmoticonDic[InGameInfoManager.Instance.rivalSessionID].Count; j++)
            {
                emoticonDic.Add(j + 1, InGameInfoManager.Instance.pvpEmoticonDic[InGameInfoManager.Instance.rivalSessionID][j].emoticonSpineAsset);
            }
        }
    }


    private void SelectEmoticon(int emoticonNum)
    {
        //내 이모티콘 박스가 활성화 중이라면 다른 이모팀콘은 못나오게 
        if (!playerSpeechBubble.activeSelf)
        {
            //이모티콘을 담아두던 UI제거
            emoticonUI.SetActive(false);
            //서버에 전송해준다.
            BackEndMatchManager.Instance.SendDataToInGame(new Protocol.EmoticonMessage(InGameInfoManager.Instance.mySessionID, emoticonNum));
            //이모티콘 말풍선의 이미지에 선택한이미지를 눌러준다.
            playerSpeechAsset.skeletonDataAsset = emoticonDic[emoticonNum];
            playerSpeechAsset.Initialize(true);
            var anims=  playerSpeechAsset.AnimationState.Data.SkeletonData.Animations.ToArray();
            
            //말풍선 활성화
            StartCoroutine(EmoticonEffect(playerSpeechBubble,playerSpeechAsset,anims[0]));
        }
    }
   
    public void ReceiveEmoticon(EmoticonMessage msg)
    {
        if (msg.SessionId != InGameInfoManager.Instance.mySessionID)
        {
            //이모티콘 말풍선의 이미지에 선택한이미지를 눌러준다.
            rivalSpeechAsset.skeletonDataAsset = emoticonDic[msg.EmoticonNum];
            rivalSpeechAsset.Initialize(true);
            var anims = rivalSpeechAsset.AnimationState.Data.SkeletonData.Animations.ToArray();

            //이모티콘 나타나는 효과
            StartCoroutine(EmoticonEffect(rivalSpeechBubble,rivalSpeechAsset, anims[1]));
        }
    }
    //이모티콘 나타나는 효과
    IEnumerator EmoticonEffect(GameObject speechBubble, SkeletonGraphic skeletonGraphic, Spine.Animation anim)
    {
        speechBubble.SetActive(true);

        skeletonGraphic.AnimationState.SetAnimation(0, anim, false);

        yield return emoticonDelayTime;
        speechBubble.SetActive(false);
    }
    
}
