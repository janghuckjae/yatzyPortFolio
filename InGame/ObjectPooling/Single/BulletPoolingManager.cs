using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPoolingManager : MonoBehaviour
{
    private static BulletPoolingManager _instance;
    public static BulletPoolingManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(BulletPoolingManager)) as BulletPoolingManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(BulletPoolingManager)) as BulletPoolingManager;
                }
            }
            return _instance;
        }
    }

    //넣어줄 오브젝트 풀
    public GameObject projectilePoolObj;
    //생성할 투사체 목록 
    [SerializeField]private List<GameObject> bullets;
    //생성할 양 
    

    //투사체 별 풀
    public Queue<GameObject>[] poolBullet_Queue;

    //타워 총알 
    public GameObject enemyTowerBullet;
    public GameObject PlayerTowerBullet;
    
    //임시 저장할 총알 스크립트 정보
    private Bullet bulletInfo;
    //총알 오브젝트를 임시로 저장할 변수
    private GameObject bulletObj;
    private GameObject[] projectilePool;
    //총알 양을 임서로 저장할 변수 배열
    [SerializeField]private int bulletAmount;
    // Start is called before the first frame update
    void Awake()
    {
        //만약 PVP모드 일때 꺼준다.
        if (InGameInfoManager.Instance.isPVPMode)
        {
            this.enabled = false;
        }
        else
        {
            bullets = new List<GameObject>();
            //총알과 총알 데이터를 가져온다.
            //내 캐릭터 중 원거리 딜러가 있다면?
            for (int i = 0; i < InGameInfoManager.Instance.charactorDatas.Count; i++)
            {
                
                //만약 데이터에있는 캐릭터가 원거리 딜러라면? 
                if (InGameInfoManager.Instance.charactorDatas[i].charIconType == CharIconType.ADCharactor)
                {
                    //ADCharactor에 있는 bullet 을 가져와 내bullets 리스트에 넣어준다. 
                    bulletObj = InGameInfoManager.Instance.charactorDatas[i].deckPrefab.GetComponent<ADCharactor>().bullet;
                    bullets.Add(bulletObj);
                }
            }
            //적 캐릭터 중 원거리 딜러가 있다면?
            //n라운드의
            for (int j = 0; j < InGameInfoManager.Instance.selectStageData.roundDatas.Length; j++)
            {
                //해당 되는 에너미 숫자 만큼 
                for (int k = 0; k < InGameInfoManager.Instance.selectStageData.roundDatas[j].enemies.Length; k++)
                {
                    //원거리 딜러여부체크 
                    //만약 라운드 캐릭터안의 에너미가 ADEnemy스크립트를 가지고있다면 
                    if (InGameInfoManager.Instance.selectStageData.roundDatas[j].enemies[k].GetComponent<ADEnemy>() != null)
                    {
                        //bullet을 캐싱하고 생성할 리스트에 넣어준다. 
                        bulletObj = InGameInfoManager.Instance.selectStageData.roundDatas[j].enemies[k].GetComponent<ADEnemy>().bullet;
                        bullets.Add(bulletObj);
                    }
                }
            }


            //타워 총알 프리팹 bullet 리스트에 저장
            bullets.Add(enemyTowerBullet);
            bullets.Add(PlayerTowerBullet);
            poolBullet_Queue = new Queue<GameObject>[bullets.Count];
            projectilePool = new GameObject[bullets.Count];
            //총알의 수 만큼 오브젝트 풀을 생성해준다.
            for (int i = 0; i < bullets.Count; i++)
            {
                projectilePool[i] = Instantiate(projectilePoolObj);
                poolBullet_Queue[i] = new Queue<GameObject>();
                //풀넘버를 지정 해준다.
                bulletInfo = bullets[i].transform.GetComponent<Bullet>();
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
