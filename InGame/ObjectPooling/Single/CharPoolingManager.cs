using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharPoolingManager : MonoBehaviour
{
    private static CharPoolingManager _instance;

    public static CharPoolingManager Instance // 싱글톤 구현
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(CharPoolingManager)) as CharPoolingManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(CharPoolingManager)) as CharPoolingManager;
                }
            }
            return _instance;
        }
    }

    [Header("저장할 풀의 위치")]
    //인스펙터 창에서 정리하기 위한 obj
    public GameObject poolObj;


    //캐릭터 배열
    private GameObject[] charactor;
    //캐릭터 별로 생성할 양
    private int charAmount;
    
    
    //임시 저장할 캐릭터 정보 
    private Charactor charactorInfo;

    //캐릭터 별 오브젝트 풀  
    public Queue<GameObject>[] poolChar_Queue ;

    private GameObject obj;
    private GameObject[] pool;
    private GameObject t_obj;
    private List<IconData> charIconData;
    // Start is called before the first frame update
    void Awake()
    {
        //PVP모드 일때 기능 정지
        if (InGameInfoManager.Instance.isPVPMode)
        {
            this.enabled = false;
        }
        else
        { 
            charIconData = InGameInfoManager.Instance.charactorDatas;
            charactor = new GameObject[charIconData.Count];
            charAmount = InGM.Instance.maxUnitCnt;

            //게임 데이터 매니저의 값을 받아와 적용한다.
            for (int i = 0; i < charIconData.Count; i++)
            {
                //캐릭터 값 적용
                charactor[i] = charIconData[i].deckPrefab;
            }


            poolChar_Queue = new Queue<GameObject>[charactor.Length];
            pool = new GameObject[charactor.Length];


            for (int i = 0; i < charactor.Length; i++)
            {
                pool[i] = Instantiate(poolObj);

                poolChar_Queue[i] = new Queue<GameObject>();
                //풀넘버를 지정 해준다.
                charactorInfo = charactor[i].transform.GetComponent<Charactor>();
                charactorInfo.charPoolNum = i;
                charactorInfo.myType = charIconData[i].charIconType;
                charactorInfo.mySP = charIconData[i].itemSP;

                for (int j = 0; j < charAmount; j++)
                {
                    //캐릭터 별로 양에 따라 정해진 부모 밑에 생성 
                    obj = Instantiate(charactor[i], pool[i].transform);

                    //생성 한 캐릭터 들을 오브젝트 풀에 저장 
                    obj.SetActive(false);
                    poolChar_Queue[i].Enqueue(obj);
                }
            }
        }
    }
    private void Start()
    {
        
    }
    //오브젝트가 어떤건지 판단후 맞다면 해당 오브젝트풀에 넣기
    public void InsertPool(GameObject C_obj,int queueNum)
    {
        poolChar_Queue[queueNum].Enqueue(C_obj);
        
        C_obj.SetActive(false);
    }
    //비활성화 되어있는 
    public GameObject GetPool(int queueNum)
    {
        t_obj = poolChar_Queue[queueNum].Dequeue();
        InGM.Instance.activeFriend.Add(t_obj.transform);
        t_obj.SetActive(true);
        return t_obj;
    }
}
