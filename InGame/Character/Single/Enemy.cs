using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EnemyState
{
    idle,
    walk,
    attack,
    hit,
    death
}
public class Enemy : MonoBehaviour
{
    public EnemyData enemyData;
    [HideInInspector] public CharIconType enemyType;
    
    public EnemyState enemyState ;
    
    //캐릭터의 HP 프로퍼티
    private float currentHP;
    public float ENEMYHP
    {
        get { return currentHP; }
        set
        {
            currentHP = value;
            hpSlider.value = currentHP;
        }
    }
    //내가 생성 되었던 풀넘버
    public int enemyPoolNum;
    //캐릭터의 HP 슬라이더  
    [SerializeField] private Slider hpSlider;

    //캐릭터의 애니메이터 
    private Animator anim;
    private float min;

    //가장가까운 적의 트랜스폼
    [SerializeField]protected private Transform nearUnit = null;

    //임시 저장할 캐릭터 스크립트
    private Charactor charInfo;
    protected private Transform playerTowerPos;
    // 캐싱: 반복문에서는, 할당 한 번
    WaitForSeconds waitForDelayTime_Attack;
    WaitForSeconds waitForDelayTime_Die;

    //enemyData에서 캐싱받을 데이터 
    //적의 초기HP 
    [HideInInspector] public float firstHP;
    //적의 공격력
    [HideInInspector] public float myDamage;
    //적의 이동속도
    public float speed;
    //적의 크리티컬 확률 
    [HideInInspector] public float criticalPercent;
    //적의 크리티컬시 곱해줄 비율
    [HideInInspector] public float criticalRatio;

    //적의 공격 속도
    [HideInInspector] public float attackDelayTime;
    
    //탐지거리
    private float detectDistance;
    private float upSpeed;

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();

        //능력치 캐싱 
        enemyType = enemyData.enemyType;
        firstHP = enemyData.hp;
        myDamage = enemyData.power;
        speed = enemyData.speed;
        criticalPercent = enemyData.criticalPercent;
        criticalRatio = enemyData.criticalMultiple;
        attackDelayTime = enemyData.attackSpeed;

        playerTowerPos = InGM.Instance.towerPlayer.transform;
        //detectDistance = enemyData.attackDistance * 4;
        detectDistance = 5f;
        // 딜레이 타임
        waitForDelayTime_Attack = new WaitForSeconds(attackDelayTime);
        
        waitForDelayTime_Die = new WaitForSeconds(enemyData.dieDelayTime);

        //적은 HP를 유지 시켜야 하므로 처음에만 선언한다.
        hpSlider.maxValue = firstHP;
        ENEMYHP = firstHP;
    }

    //캐릭터가 SetActive 되었을 때 여러가지 요소를 초기화해준다.
    protected virtual void OnEnable()
    {
        min = detectDistance;
        speed = enemyData.speed;
        upSpeed = speed * 3;
        enemyState = EnemyState.idle;
        StartCoroutine(UpdateCoroutine());
    }
    #region EnemyFSM

    IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            //Idle 상태일 때 
            if (enemyState == EnemyState.idle)
            {
                yield return null;
                if (InGM.Instance.stageState == StageState.BattleTime || InGM.Instance.stageState == StageState.TowerAttackTime)
                {
                    if (nearUnit != null)
                    {
                        //만약 플레이어 타워를 공격하게 될 때 (승리시)
                        if (nearUnit.CompareTag("PlayerTower"))
                        {
                            //모든 유닛이 공격 상태가 되었다면
                            if (InGM.Instance.stageState  == StageState.TowerAttackTime)
                            {
                                enemyState = EnemyState.attack;
                            }
                        }
                        else { enemyState = EnemyState.attack; }
                    }
                    else
                    {
                        enemyState = EnemyState.walk;
                        anim.SetTrigger("walk");
                    }
                }
            }
            //Run상태일때
            else if (enemyState == EnemyState.walk)
            {
                yield return null;
                AttackDetect();
               

                if (nearUnit != null)
                {
                    float distance = (nearUnit.position - transform.position).sqrMagnitude;
                    //만약 적이 공격 사거리 안으로 들어온다면
                    if (distance <= enemyData.attackDistance)
                    {
                        enemyState = EnemyState.attack;
                    }
                    //적에게 이동
                    transform.position = Vector2.MoveTowards(transform.position, nearUnit.position, speed * Time.deltaTime);
                }
                else
                {
                    transform.position = Vector2.MoveTowards(transform.position, 
                        new Vector2(playerTowerPos.position.x,transform.position.y), speed * Time.deltaTime);
                }
            }
            //Attack 상태일 때
            else if (enemyState == EnemyState.attack)
            {
                //공격 사거리를 벗어나면 다시 공격 하게 처리
                if (nearUnit != null)
                {
                    //만약 공격 타겟이 타워라면 타워 공격 포인트를 하나 올리고 CharState를 Idle로 옮긴다.
                    if (nearUnit.CompareTag("PlayerTower") && InGM.Instance.stageState !=StageState.TowerAttackTime )
                    {
                        InGM.Instance.towerAttackPoint++;
                        enemyState = EnemyState.idle;
                        anim.SetTrigger("idle");
                    }
                    else if (InGM.Instance.stageState == StageState.TowerAttackTime)
                    {
                        anim.SetTrigger("attack");
                        yield return waitForDelayTime_Attack;
                    }
                    else
                    {
                        charInfo = nearUnit.GetComponent<Charactor>();
                        //만약 적과의 거리가 멀어지거나 적이 죽은 경우 상태를 Idle로 해준다.
                        if (charInfo.charState ==CharState.death)
                        {
                            min = detectDistance;
                            nearUnit = null;
                            
                            enemyState = EnemyState.idle;
                        }
                        else
                        { 
                            anim.SetTrigger("attack");
                            yield return waitForDelayTime_Attack;
                        }
                    }
                }
            }
            //Damage 상태일 때
            else if (enemyState == EnemyState.hit)
            {
                yield return null;
                enemyState = EnemyState.idle;
            }
            yield return null;
        }

    }

    #endregion
    #region "탐지 기능"
    //거리 비교
    void AttackDetect()
    {
        //게임 필드에 적이 있다면
        if (InGM.Instance.activeFriend.Count > 0)
        {
            //activeFriend리스트에 있는 적을 검출
            foreach (Transform player in InGM.Instance.activeFriend)
            {
                float distance = (player.position - transform.position).sqrMagnitude;
                //공격 사거리 안에있는 가장 가까운 유닛을 타겟으로 정한다.
                if (distance <= detectDistance && distance <= min)
                {
                    min = distance;
                    nearUnit = player;
                }
            }
        }
        //게임 필드에 적이 없다면 타워 공격
        else if(InGM.Instance.activeFriend.Count <= 0)
        {
            nearUnit = null;
            //적이 없다면
            //스피드를 빠르게
            speed = upSpeed;
            //타워로 이동
            //transform.position = new Vector2(playerTowerPos.position.x + enemyData.attackDistance, playerTowerPos.position.y);
            
            float towerDistance = (new Vector2(playerTowerPos.position.x, 0) - new Vector2(transform.position.x, 0)).sqrMagnitude;
            if (towerDistance <= 3)
            {
                min = towerDistance;
                nearUnit = playerTowerPos;
            }
        }
    }
    #endregion
    #region "공격 이벤트"
    protected virtual void OnAttackProcess()
    {
        if (nearUnit != null)
        {
            float randomValue = Random.Range(0f, 100f);
            if (nearUnit.CompareTag("PlayerTower"))
            {
                if (randomValue <= criticalPercent)
                {
                    InGM.Instance.towerPlayer.TowerDamagedProcess(myDamage * criticalRatio);
                }
                else
                {
                    InGM.Instance.towerPlayer.TowerDamagedProcess(myDamage);
                }
            }
            else
            {
                //만약 크리티컬 발생 시 hit 애니메이션 출력 
                if (randomValue <= criticalPercent)
                {
                    charInfo.OnDamageProcess(myDamage*criticalRatio, true);
                }
                else
                {
                    charInfo.OnDamageProcess(myDamage, false);
                }
                    
            }
        }
    }
    #endregion
    #region "데미지 받을 시 이벤트"
    public void OnDamageProcess(float playerDamage, bool getCritical)
    {
        if (enemyState != EnemyState.death)
        {
            //hp감소
            ENEMYHP -= playerDamage;
            if (ENEMYHP > 0)
            {
                //만약 크리티컬 발생 시 hit 애니메이션 출력 
                if (getCritical)
                {
                    enemyState = EnemyState.hit;
                    anim.SetTrigger("hit");
                }
            }
            //hp가 0보다 작으면 Die상태
            if (ENEMYHP <= 0)
            {
                enemyState = EnemyState.death;
                StopAllCoroutines();
                StartCoroutine(Death());
            }
        }
    }
    IEnumerator Death()
    {
        InGM.Instance.activeEnemy.Remove(transform);
        anim.SetTrigger("death");
        //애니메이션을 끝까지 재생하기 위하여 딜레이 후 해당하는 오브젝트풀에 넣어준다.
        yield return waitForDelayTime_Die;
        //타겟 초기화
        nearUnit = null;
        EnemyPoolingManager.Instance.InsertPool(gameObject, enemyPoolNum);
        yield break;
    }
    #endregion
   
}
