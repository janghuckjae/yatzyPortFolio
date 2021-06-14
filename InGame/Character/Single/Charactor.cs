using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CharState
{
    idle,
    walk,
    attack,
    hit,
    death
}
public class Charactor : MonoBehaviour
{
    #region 캐릭터 정보
    public CharData charData;
    [HideInInspector]public CharIconType myType;
    [HideInInspector]public int mySP;
    //캐릭터의 상태
    public CharState charState;
    
    
    //캐릭터의 HP 프로퍼티
    private float currentHP;
    public float CHARHP
    {
        get { return currentHP; }
        set
        {
            currentHP = value;
            hpSlider.value = currentHP;
        }
    }
    //캐릭터의 풀넘버
    public int charPoolNum;
    //캐릭터의 HP 슬라이더  
    [SerializeField] private Slider hpSlider;

    //캐릭터의 애니메이터 
    protected private Animator anim;
    #endregion
    #region 캐릭터 스테이터스
    WaitForSeconds waitForDelayTime_Attack;
    WaitForSeconds waitForDelayTime_Die;

    
    //CharData에서 캐싱받을 데이터 
    //캐릭터의 초기HP 
    [HideInInspector] public float firstHP;
    //캐릭터의 공격력
    [HideInInspector] public float myDamage;
    //캐릭터의 이동속도
    public float speed;
    //적의 크리티컬 확률 
    [HideInInspector] public float criticalPercent;
    //적의 크리티컬시 곱해줄 비율
    [HideInInspector] public float criticalRatio;

    //캐릭터의 공격 속도
    [HideInInspector] public float attackDelayTime;

    //탐지 거리 
    private float detectDistance;
    #endregion
    //가장가까운 적의 트랜스폼
    public Transform nearUnit = null;
    //가장 최신의 NearUnit을 저장(exNearUnit이 바뀌었을 때 호스트라면 서버로 타겟정보를 올려준다.)
    [HideInInspector]public bool isCritical = false;
    private bool isUpSpeed=true;
    protected private float min;
    //임시 저장할 에너미 
    protected private Enemy enemyInfo;
    protected private Charactor rivalInfo;
    [HideInInspector]public Transform enemyTowerPos;
    private float upSpeed;
    
    // Start is called before the first frame update
    protected virtual void Awake()
    {
        if (InGameInfoManager.Instance.isPVPMode)
        {
            this.enabled = false;
        }
        else
        { 
            anim = GetComponent<Animator>();
        
            //능력치 캐싱 
            firstHP = charData.hp;
            myDamage = charData.power;
            speed = charData.speed;
            criticalPercent = charData.criPercentage;
            criticalRatio = charData.criRatio;
            attackDelayTime = charData.attackSpeed;

            //detectDistance = charData.attackDistance * 4;
            detectDistance = 6f;
            //공격 속도 제어를 위해 딜레이를 준다.
            waitForDelayTime_Attack = new WaitForSeconds(attackDelayTime);
            //죽는 애니메이션이 다 나오게끔 딜레이를 준다.
            waitForDelayTime_Die = new WaitForSeconds(charData.dieDelayTime);

            //pvp 캐릭터 배치 테스트 때문에 주석처리함
            //enemyTowerPos = InGM.Instance.towerEnemy.transform; 
        }
        
    }

    //캐릭터가 SetActive 되었을 때 여러가지 요소를 초기화해준다.
    protected virtual void OnEnable()
    {
        hpSlider.maxValue = firstHP;
        CHARHP = firstHP;
        min = detectDistance;
        speed = charData.speed;
        upSpeed = speed * 3;
        isUpSpeed = true;
        charState = CharState.idle;
        StartCoroutine(UpdateCoroutine());
    }
    #region "캐릭터 FSM"
    IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            //Idle 상태일 때 
            if (charState == CharState.idle)
            {
                yield return null;
                if (InGM.Instance.stageState == StageState.BattleTime || InGM.Instance.stageState == StageState.TowerAttackTime)
                {
                    if (nearUnit != null)
                    {
                        //아군 승리시 
                        if (nearUnit == enemyTowerPos)
                        {
                            if (InGM.Instance.stageState == StageState.TowerAttackTime)
                            { 
                                charState = CharState.attack;
                            }
                        }
                        else { charState = CharState.attack; }
                    }
                    else 
                    {
                        charState = CharState.walk;
                        anim.SetTrigger("walk");
                    }
                }
            }
            //Run상태일때
            else if (charState == CharState.walk)
            {
                yield return null;
              
                AttackDetect(InGM.Instance.activeEnemy); 

                if (nearUnit != null)
                {
                    float distance = (nearUnit.position - transform.position).sqrMagnitude;
                    //만약 적이 공격 사거리 안으로 들어온다면
                    if (distance <= charData.attackDistance) {
                            charState = CharState.attack; 
                    }

                    //적에게 이동
                    transform.position = Vector2.MoveTowards(transform.position, nearUnit.position, speed * Time.deltaTime);
                }
                else
                {
                    transform.position = Vector2.MoveTowards(transform.position,
                        new Vector2(enemyTowerPos.position.x,transform.position.y), speed * Time.deltaTime);
                }
            }
            //Attack 상태일 때
            else if (charState == CharState.attack)
            {
                yield return null;
                if (nearUnit != null)
                {
                    //만약 공격 타겟이 타워라면 타워 공격 포인트를 하나 올리고 CharState를 Idle로 옮긴다.
                    if (nearUnit == enemyTowerPos && InGM.Instance.stageState != StageState.TowerAttackTime)
                    {
                        InGM.Instance.towerAttackPoint++;
                        charState = CharState.idle;
                        anim.SetTrigger("idle");
                    }
                    else if (nearUnit == enemyTowerPos && InGM.Instance.stageState == StageState.TowerAttackTime)
                    {

                        float randomValue = Random.Range(0f, 100f);
                        if (randomValue <= criticalPercent) { isCritical = true; }
                        else { isCritical = false; }
                        anim.SetTrigger("attack");
                        yield return waitForDelayTime_Attack;
                    }
                    else
                    {
                        enemyInfo = nearUnit.GetComponent<Enemy>();
                        //만약 적과의 거리가 멀어지거나 적이 죽은 경우 상태를 Idle로 해준다.
                        if (enemyInfo.enemyState == EnemyState.death)
                        {
                            min = detectDistance;
                            nearUnit = null;
                            charState = CharState.idle;
                        }
                        else
                        {
                            float randomValue = Random.Range(0f, 100f);
                            if (randomValue <= criticalPercent) { isCritical = true; }
                            else { isCritical = false; }
                            anim.SetTrigger("attack");
                            yield return waitForDelayTime_Attack;
                        }
                    }
                }
                else
                {
                    Debug.Log("이게 검출되네");
                }
            }
            //Damage 상태일 때
            else if (charState == CharState.hit)
            {
                yield return null;
                charState = CharState.idle;
            }
        }
    }
    #endregion
    #region "캐릭터 탐색 기능"
    //거리 비교
    void AttackDetect(List<Transform> activeUnit)
    {
        if (activeUnit.Count > 0)
        {
            //해쉬셋에 있는 적을 검출
            foreach (Transform enemy in activeUnit)
            {
                float distance = (enemy.position - transform.position).sqrMagnitude;
                //탐지 사거리 안에있는 가장 가까운 유닛을 타겟으로 정한다.
                if (distance <= detectDistance && distance <= min)
                {
                    min = distance;
                    nearUnit = enemy;
                }
            }
        }
        else if(activeUnit.Count <= 0)
        {
            if (isUpSpeed)
            {
                nearUnit = null;
                //스피드를 빠르게
                speed = upSpeed;
                isUpSpeed = false;
            }
            //타워로 이동
            float towerDistance = (new Vector2(enemyTowerPos.position.x, 0) - new Vector2(transform.position.x, 0)).sqrMagnitude;
            if (towerDistance < 3)
            {
                min = towerDistance;
                nearUnit = enemyTowerPos;
            }
        }
    }
    #endregion
    #region "캐릭터 공격 이벤트"
    protected virtual void OnAttackProcess()
    {
        
        if (!InGameInfoManager.Instance.isPVPMode)
        {
            AttackEvent();
        }
    }
    private void AttackEvent()
    {
        if (nearUnit != null)
        {
            if (nearUnit == enemyTowerPos)
            {
                //크리티컬 발생시
                if (isCritical == true) { InGM.Instance.towerEnemy.TowerDamagedProcess(myDamage * criticalRatio); }
                else { InGM.Instance.towerEnemy.TowerDamagedProcess(myDamage); }
            }
            else
            {
                //크리티컬 발생시
                if (isCritical == true) { enemyInfo.OnDamageProcess(myDamage * criticalRatio, isCritical); }
                else { enemyInfo.OnDamageProcess(myDamage, isCritical); }
            }
        }
    }
    #endregion
    #region "캐릭터 데미지 이벤트"
    public void OnDamageProcess(float enemyDamage,bool getCritical)
    {
        if (charState != CharState.death)
        {
            //hp감소
            CHARHP -= enemyDamage;
            if (CHARHP > 0)
            {
                //만약 크리티컬 발생 시 hit 애니메이션 출력 
                if (getCritical)
                {
                    charState = CharState.hit;
                    anim.SetTrigger("hit");
                }
            }
            //hp가 0보다 작으면 Die상태
            if (CHARHP <= 0)
            {
                charState = CharState.death;
                StopAllCoroutines();
                StartCoroutine(Death());
            }
        }
    }

    IEnumerator Death()
    {
        InGM.Instance.activeFriend.Remove(transform);
        anim.SetTrigger("death");
        //애니메이션을 끝까지 재생하기 위하여 딜레이 후 해당하는 오브젝트풀에 넣어준다.
        yield return waitForDelayTime_Die;
        //타겟 초기화
        nearUnit = null;
        isUpSpeed = true;
        CharPoolingManager.Instance.InsertPool(gameObject, charPoolNum);
        yield break;
    }
    #endregion

    
}


