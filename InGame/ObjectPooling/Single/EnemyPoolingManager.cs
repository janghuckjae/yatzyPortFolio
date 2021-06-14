using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPoolingManager : MonoBehaviour
{
    private static EnemyPoolingManager _instance;
    public static EnemyPoolingManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(EnemyPoolingManager)) as EnemyPoolingManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(EnemyPoolingManager)) as EnemyPoolingManager;
                }
            }
            return _instance;
        }
    }


    //인스펙터 창에서 정리하기 위한 obj
    private Enemy enemyInfo;
    //캐릭터 별 오브젝트 풀  
    public Queue<GameObject>[] round_Queue;

    private GameObject obj;
    private GameObject[] roundPool;
    private GameObject e_obj;
    // Start is called before the first frame update
    void Awake()
    {
        //PVP 때는 에너미 Pooling 작업을 하지않는다.
        if (!InGameInfoManager.Instance.isPVPMode)
        { 
            round_Queue = new Queue<GameObject>[InGameInfoManager.Instance.selectStageData.roundDatas.Length];
            roundPool = new GameObject[InGameInfoManager.Instance.selectStageData.roundDatas.Length];
            //라운드 만큼 오브젝트 풀 생성 
            for (int i = 0; i < InGameInfoManager.Instance.selectStageData.roundDatas.Length; i++)
            {
                //큐 선언 
                //풀 복사
                round_Queue[i] = new Queue<GameObject>();
                roundPool[i] = new GameObject(string.Format("EnemyPool{0}",i));
                //라운드 별 캐릭터 오브젝트 풀안에 넣고 풀번호 할당
                for (int j = 0; j < InGameInfoManager.Instance.selectStageData.roundDatas[i].enemies.Length; j++)
                {
                    obj = Instantiate(InGameInfoManager.Instance.selectStageData.roundDatas[i].enemies[j], roundPool[i].transform);
                
                    enemyInfo = obj.transform.GetComponent<Enemy>();
                    enemyInfo.enemyPoolNum = i;

                    obj.SetActive(false);
                    round_Queue[i].Enqueue(obj);
                }
            }
        }
    }
    //오브젝트가 어떤건지 판단후 맞다면 해당 오브젝트풀에 넣기
    public void InsertPool(GameObject C_obj, int queueNum)
    {
        round_Queue[queueNum].Enqueue(C_obj);
        C_obj.SetActive(false);
        
    }
    //비활성화 되어있는 
    public GameObject GetPool(int queueNum)
    {
        e_obj = round_Queue[queueNum].Dequeue();
        InGM.Instance.activeEnemy.Add(e_obj.transform);
        e_obj.SetActive(true);
        return e_obj;
    }
}
