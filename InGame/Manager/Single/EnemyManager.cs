using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private static EnemyManager _instance;
    public static EnemyManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(EnemyManager)) as EnemyManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(EnemyManager)) as EnemyManager;
                }
            }
            return _instance;
        }
    }

    

    private MeshRenderer[] enemyRenderer;
    private int line1SortPoint;
    private int line2SortPoint;
    private int line3SortPoint;

    //캐릭터 최대 소환 
    private int maxEnemyCnt;
    [HideInInspector] public int currentEnemyCnt = 0;
    private GameObject m_obj;
    //라운드 별 에너미생성 기능
    private List<Enemy> enemySummonList = new List<Enemy>();

    //최종적으로 배치할 리스트 
    [SerializeField]private List<Enemy> setEnemyList = new List<Enemy>();
    [HideInInspector] public bool enemiesSummon = true;

    private void Awake()
    {
        //PVP 모드 일때는 에너미 소환을 하지않는다.
        if (!InGameInfoManager.Instance.isPVPMode)
        { 
            maxEnemyCnt = InGM.Instance.maxUnitCnt; 

            enemyRenderer = new MeshRenderer[maxEnemyCnt];
            //라운드 변화시 이벤트 추가
            InGM.Instance.roundChangeChain += EnemyInitialization;
        }
    }

    public void  SummonEnemy()
    {
        //PVP 모드 일때는 에너미 소환을 하지않는다.
        if (enemiesSummon && !InGameInfoManager.Instance.isPVPMode)
        {
            //현재 라운드에 해당하는 에너미 풀을 가져온다.
            for (int j = 0; j < InGameInfoManager.Instance.selectStageData.roundDatas[InGM.Instance.currentRound - 1].enemies.Length; j++)
            {
                m_obj = EnemyPoolingManager.Instance.GetPool(InGM.Instance.currentRound - 1);
                enemySummonList.Add(m_obj.transform.GetComponent<Enemy>());
            }
            //정렬
            enemySummonList = enemySummonList.OrderBy(x => x.enemyType).ToList();
            setEnemyList.AddRange(enemySummonList);

            //맞는 위치에 세팅
            EnemySetPos();
            enemiesSummon = false;

            //소환 후 리스트 초기화
            enemySummonList.Clear();
            enemyRenderer.Initialize();
        }
    }

    void EnemySetPos()
    {
        line1SortPoint = 0;
        line2SortPoint = 0;
        line3SortPoint = 0;
        //세팅할 유닛에 맞춰 적군 배치(만약 세팅되있는 에너미 숫자가 최대 에너미 숫자를 넘었다면 값은 15로 고정)
        if (setEnemyList.Count <= maxEnemyCnt) { InGM.Instance.AssignedPos(setEnemyList.Count, InGM.Instance.enemyUnitPos); }
        
        else { InGM.Instance.AssignedPos(15, InGM.Instance.enemyUnitPos); }

        //적은 최대 15마리 까지만 나와야한다.
        for (int i = 0; i < setEnemyList.Count; i++)
        {
            //만약 setEnemyList가 15마리 이하이라면 계속 진행하고
            if (i <= maxEnemyCnt-1)
            {
                //현재 적은 전부 SpriteRenderer를 사용하므로 스프라이트 랜더러 검색 후 적용
                enemyRenderer[i] = setEnemyList[i].transform.GetComponent<MeshRenderer>();
                //적군 Layer구분
                SortLayerAssigned(i);

                //순차적으로 배치 
                setEnemyList[i].transform.position = InGM.Instance.enemyUnitPos[i];
            }
            //15마리 이상이라면 넘치는 부분은 다시 큐로 되돌려준다.
            else
            {
                InGM.Instance.activeEnemy.Remove(setEnemyList[i].transform);
                EnemyPoolingManager.Instance.InsertPool(setEnemyList[i].gameObject, InGM.Instance.currentRound - 1);
            }
        }
        setEnemyList.Clear();
    }

    //적군 Layer구분
    void SortLayerAssigned(int i)
    {
        if (setEnemyList.Count == 3)
        {
            if (InGM.Instance.enemyUnitPos[i].x < 1.1f)
            {
                enemyRenderer[i].sortingLayerName = InGM.Instance.line2LayerArr[line1SortPoint];
                line1SortPoint++;

            }
            //유닛의 위치가 2번째 라인이라면?(x축이 1.1보다 크고 1.78보다 작다면)
            else if (InGM.Instance.enemyUnitPos[i].x < 1.78f && InGM.Instance.enemyUnitPos[i].x > 1.1f)
            {
                enemyRenderer[i].sortingLayerName = InGM.Instance.line1_3LayerArr[line2SortPoint];
                line2SortPoint++;
            }
            //유닛의 위치가 3번째 라인이라면 (x축이 1.78보다 크다면)
            else if (InGM.Instance.enemyUnitPos[i].x > 1.78f)
            {
                enemyRenderer[i].sortingLayerName = InGM.Instance.line2LayerArr[line3SortPoint];
                line3SortPoint++;
            }
        }
        else 
        {
            if (InGM.Instance.enemyUnitPos[i].x < 1.1f)
            {
                enemyRenderer[i].sortingLayerName = InGM.Instance.line1_3LayerArr[line1SortPoint];
                line1SortPoint++;

            }
            //유닛의 위치가 2번째 라인이라면?(x축이 1.1보다 크고 1.78보다 작다면)
            else if (InGM.Instance.enemyUnitPos[i].x < 1.78f && InGM.Instance.enemyUnitPos[i].x > 1.1f)
            {
                enemyRenderer[i].sortingLayerName = InGM.Instance.line2LayerArr[line2SortPoint];
                line2SortPoint++;
            }
            //유닛의 위치가 3번째 라인이라면 (x축이 1.78보다 크다면)
            else if (InGM.Instance.enemyUnitPos[i].x > 1.78f)
            {
                enemyRenderer[i].sortingLayerName = InGM.Instance.line1_3LayerArr[line3SortPoint];
                line3SortPoint++;
            }
        }
        
    }


    //타워 이벤트가 끝나고 라운드 변화시 이벤트
    void EnemyInitialization()
    {
        currentEnemyCnt = 0;
        enemiesSummon = true;
    }
}
