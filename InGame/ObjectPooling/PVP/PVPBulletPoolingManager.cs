using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PVPBulletPoolingManager : MonoBehaviour
{
    private static PVPBulletPoolingManager _instance;
    public static PVPBulletPoolingManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(PVPBulletPoolingManager)) as PVPBulletPoolingManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(PVPBulletPoolingManager)) as PVPBulletPoolingManager;
                }
            }
            return _instance;
        }
    }

    //생성할 양 
    [SerializeField] private int bulletAmount;
    //생성할 투사체 목록 
    private List<GameObject> bullets = new List<GameObject>();


    //투사체 별 풀
    public Queue<GameObject>[] poolBullet_Queue;

    //임시 저장할 총알 스크립트 정보
    private PVPBullet bulletInfo;
    //총알 오브젝트를 임시로 저장할 변수
    private GameObject bulletObj;
    private GameObject[] projectilePool;
    // Start is called before the first frame update
    void Awake()
    {
        //만약 PVP모드가 아닐떄 멈춰준다.
        if (!InGameInfoManager.Instance.isPVPMode)
        {
            this.enabled = false;
        }
        else
        {
            //총알과 총알 데이터를 가져온다.
            //내 원거리 캐릭터의 총알 오브젝트 풀링
            for (int i = 0; i < InGameInfoManager.Instance.pvpCharctorDIc[InGameInfoManager.Instance.mySessionID].Count; i++)
            {
                //만약 데이터에있는 캐릭터가 원거리 딜러라면? 
                if (InGameInfoManager.Instance.pvpCharctorDIc[InGameInfoManager.Instance.mySessionID][i].charIconType == CharIconType.ADCharactor)
                {
                    //ADCharactor에 있는 bullet 을 가져와 내bullets 리스트에 넣어준다. 
                    bulletObj = InGameInfoManager.Instance.pvpCharctorDIc[InGameInfoManager.Instance.mySessionID][i].deckPrefab.GetComponent<PVPADCharactor>().bullet;
                    bullets.Add(bulletObj);
                }
            }
            //상대 원거리 캐릭터의 총알 오브젝트 풀링
            for (int i = 0; i < InGameInfoManager.Instance.pvpCharctorDIc[InGameInfoManager.Instance.rivalSessionID].Count; i++)
            {
                //만약 데이터에있는 캐릭터가 원거리 딜러라면? 
                if (InGameInfoManager.Instance.pvpCharctorDIc[InGameInfoManager.Instance.rivalSessionID][i].charIconType == CharIconType.ADCharactor)
                {
                    //ADCharactor에 있는 bullet 을 가져와 내bullets 리스트에 넣어준다. 
                    bulletObj = InGameInfoManager.Instance.pvpCharctorDIc[InGameInfoManager.Instance.rivalSessionID][i].deckPrefab.GetComponent<PVPADCharactor>().bullet;
                    bullets.Add(bulletObj);
                }
            }
            poolBullet_Queue = new Queue<GameObject>[bullets.Count];
            projectilePool = new GameObject[bullets.Count];
            //총알의 수 만큼 오브젝트 풀을 생성해준다.
            for (int i = 0; i < bullets.Count; i++)
            {
                projectilePool[i] = new GameObject(string.Format("ProjectilePool{0}", i));
                poolBullet_Queue[i] = new Queue<GameObject>();
                //풀넘버를 지정 해준다.
                bulletInfo = bullets[i].transform.GetComponent<PVPBullet>();
                bulletInfo.bulletPoolNum = i;
                //총알 리스트에서 총알의 양 정보를 가져온다.
                for (int j = 0; j < bulletAmount; j++)
                {
                    //투사체를 투사체 오브젝트 풀의 자식으로 생성 
                    bulletObj = Instantiate(bullets[i], projectilePool[i].transform);


                    bulletObj.SetActive(false);
                    poolBullet_Queue[i].Enqueue(bulletObj);
                }
            }
        }
    }
    private void Start()
    {
        
    }
    //오브젝트 풀에 넣기
    public void InsertPool(GameObject b_Obj, int myNum)
    {
        poolBullet_Queue[myNum].Enqueue(b_Obj);
        b_Obj.SetActive(false);
    }
    //오브젝트 풀에서 꺼내기 
    public GameObject GetPool(int myNum)
    {
        bulletObj = poolBullet_Queue[myNum].Dequeue();
        bulletObj.SetActive(true);
        return bulletObj;
    }
}
