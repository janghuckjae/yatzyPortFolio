using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum PVPCharState
{
    idle,
    walk,
    attack,
    death,
    stun,       //기절 : 행동불가
    freeze,     //빙결 : 행동불가 + 공격받지 않음    //공격을 받지 않는 기절
    snare,      //속박 : 이동불가 //walk 상태일때 해당 효과가 있으면 속박으로 이동
    counfusion, //혼란 : 강제 이동 상태
    banishment, //추방 : 잠시 전투 이탈
    charm,      //매혹 : 아군을 공격한다.
}

//스테이터스로 관리하지 않는 군중제어
//넉백, 침묵, 출혈, 표식, 속도감소

public class PVPCharactor : MonoBehaviour
{
    #region 캐릭터 정보
    public CharData charData;
    [HideInInspector] public CharIconType myType;
    [HideInInspector] public CharIconType subType;
    //캐릭터의 상태
    public PVPCharState pvpCharState;
    public float SHIELD
    {
        get { return myShieldAmount; }
        set
        {
            myShieldAmount = value;
            shieldSlider.value = myShieldAmount;
            //만약 내 쉴드양이 음수라면 그만큼 HP를 깎는다.
            if (myShieldAmount <= 0)
            {
                isShield = false;
                CHARHP += myShieldAmount;
                shieldSlider.gameObject.SetActive(false);
                if (shieldEffect != CharEffectKind.None)
                {
                    charEffect.EffectOff(shieldEffect);
                }
                myShieldAmount = 0;
            }
        }
    }

    //캐릭터의 HP 프로퍼티
    private float currentHP;
    [SerializeField] private float currentMP;
    public float CHARHP
    {
        get
        {
            return currentHP;
        }
        set
        {
            currentHP = value;
            if (currentHP >= hpSlider.maxValue)
            {
                currentHP = hpSlider.maxValue;
                hpSlider.value = currentHP;
            }
            else
            {
                hpSlider.value = currentHP;
            }

            if (PVPInGM.Instance.pvpStageState == PVPStageState.BattleTime && !isRival)
            {
                InGameUIManager.Instance.battleUICharHPSliders[PVPCharManager.Instance.charNumArr[charPoolNum]].maxValue = hpSlider.maxValue;
                InGameUIManager.Instance.battleUICharHPSliders[PVPCharManager.Instance.charNumArr[charPoolNum]].value = currentHP;
            }
        }
    }
    public float CHARMP
    {
        get
        {
            return currentMP;
        }
        set
        {
            currentMP = value;
            if (currentMP >= mpSlider.maxValue)
            {
                currentMP = mpSlider.maxValue;
                mpSlider.value = currentMP;
            }
            else
            {
                mpSlider.value = currentMP;
            }

            if (PVPInGM.Instance.pvpStageState == PVPStageState.BattleTime)
            {
                InGameUIManager.Instance.battleUICharMPSliders[PVPCharManager.Instance.charNumArr[charPoolNum]].maxValue = mpSlider.maxValue;
                InGameUIManager.Instance.battleUICharMPSliders[PVPCharManager.Instance.charNumArr[charPoolNum]].value = currentMP;
            }
        }
    }
    //인게임에서 캐릭터 강화 시 업그레이드 해줄 능력치
    [SerializeField] private int currentEnhanceLevel;
    public int CHARLEVEL
    {
        get { return currentEnhanceLevel; }
        set
        {
            currentEnhanceLevel = value;
            //만약 레벨이 올라간다면 HP,데미지,크리티컬 확률을 올려준다.
            if (currentEnhanceLevel > 0)
            {
                float enhanceRatio = (Mathf.Pow(1.2f, currentEnhanceLevel));
                firstHP = charData.hp * enhanceRatio;
                myDamage = charData.power * enhanceRatio;
                criticalPercent = charData.criPercentage * enhanceRatio;
                //HP바의 최대 체력을 업그레이드 된 HP로 설정 해준다.
                hpSlider.maxValue = firstHP;
                //강화 하면 체력 전체 회복
                CHARHP = firstHP;
                mpSlider.maxValue = maxMP;
                //강화 하면 잃어버린 체력의 30%만큼 회복
                //float healHPValue = (firstHP - CHARHP) * 0.3f;
                //CHARHP += healHPValue;

                //캐릭터 소환 UI 강화 상태(별의 갯수)적용
                if (!isRival)
                {
                    PVPCharManager.Instance.CharEnhance(charPoolNum, currentEnhanceLevel);
                }

            }
            else
            {
                //능력치 캐싱 
                firstHP = charData.hp;
                myDamage = charData.power;
                speed = charData.speed;
                criticalPercent = charData.criPercentage;
                criticalRatio = charData.criRatio;
                attackDelayTime = charData.attackSpeed;
                mpSlider.maxValue = maxMP;

               
            }
        }
    }
    public void SetFillImage()
    {
        if (isRival) { hpFillImage.sprite = charData.rivalHpFillImg; }
        else { hpFillImage.sprite = charData.friendHpFillImg; }
    }
    //캐릭터의 풀넘버
    public int charPoolNum;
    //캐릭터의 HP 슬라이더  
    public Slider hpSlider;
    //이펙트 용 이미지(서서히 다는)
    [SerializeField] private Image hpEffectImage;
    [SerializeField] private Image hpFillImage;
    private float hurtSpeed;


    [SerializeField] private Slider shieldSlider;
    public Slider mpSlider;

    //캐릭터의 애니메이터 
    [HideInInspector] public Animator anim;


    #endregion
    #region 캐릭터 스테이터스
    WaitForSeconds waitForDelayTime_Attack;
    WaitForSeconds waitForDelayTime_Die;


    //CharData에서 캐싱받을 데이터 
    //캐릭터의 초기HP 
    public float firstHP;
    //캐릭터의 공격력
    public float myDamage;
    //캐릭터의 이동속도
    public float speed;
    //적의 크리티컬 확률 
    public float criticalPercent;
    //적의 크리티컬시 곱해줄 비율
    public float criticalRatio;
    //캐릭터의 공격 속도
    public float attackDelayTime;
    [HideInInspector] public CharEffect charEffect;
    //탐지 거리 
    [HideInInspector] public float detectDistance;
    //쉴드 상태 유무
    private bool isShield;
    //스턴 상태 유무
    private float myStunTime;
    //쉴드 양
    private float myShieldAmount;

    //스턴 효과가 있는 상태 (스턴, 석화 등)의 이펙트 
    private CharEffectKind stunEffect;
    //캐릭터, 스킬 마다 쉴드의 이펙트가 다르기 때문에 
    private CharEffectKind shieldEffect;
    #endregion
    #region PVP시 쓰는 변수
    //캐릭터가 내 캐릭터인지 rival인지 확인하는 함수 
    [HideInInspector] public bool isRival;
    //서버의 구별 때문에 캐릭터가 생성하고 나서 번호가 필요하다.(1~16까지)
    [HideInInspector] public int unitNum;

    //MP 최대값
    private float maxMP = 30;

    //크리티컬이 터졌을 때 활성화 되는 bool함수
    [HideInInspector] public bool isCritical = false;
    #endregion


    //가장가까운 적의 트랜스폼
    public PVPCharactor nearUnit = null;
    //가장 최신의 NearUnit을 저장(exNearUnit이 바뀌었을 때 호스트라면 서버로 타겟정보를 올려준다.)
     private PVPCharactor exNearUnit;
    [HideInInspector] public float min;



    #region 스킬시스템
    private CharactorSkill specialSkill;

    ////스킬1,2,3
    //private CharactorSkill skill1;
    //private CharactorSkill skill2;
    //private CharactorSkill skill3;

    private List<CharactorBuff> buffList = new List<CharactorBuff>();
    private List<CharactorAbnormalState> debuffList = new List<CharactorAbnormalState>();

    #endregion

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        if (!InGameInfoManager.Instance.isPVPMode)
        {
            this.enabled = false;
        }
        else
        {
            anim = GetComponent<Animator>();
            charEffect = GetComponent<CharEffect>();
            isShield = false;
            shieldEffect = CharEffectKind.None;

            //초기 능력치 캐싱 
            firstHP = charData.hp;
            myDamage = charData.power;
            speed = charData.speed;
            criticalPercent = charData.criPercentage;
            criticalRatio = charData.criRatio;
            attackDelayTime = charData.attackSpeed;

            detectDistance = 6f;
            //공격 속도 제어를 위해 딜레이를 준다.
            waitForDelayTime_Attack = new WaitForSeconds(attackDelayTime);
            //죽는 애니메이션이 다 나오게끔 딜레이를 준다.
            waitForDelayTime_Die = new WaitForSeconds(charData.dieDelayTime);
        }
        hurtSpeed = 0.8f;
        GameObject newObj = new GameObject("MainSkill");
        newObj.transform.parent = this.transform;
        newObj.transform.localPosition = Vector3.zero;

        //스킬 정보에 따라서 추가되는 스킬이 다르다
        specialSkill = newObj.AddComponent<CharactorSkill>();

        specialSkill.SetSkillType(TARGET_TYPE.SINGLE, TARGET_MULTI.GROUP, CharIconType.MeleeCharactor);

    }
    //캐릭터가 SetActive 되었을 때 여러가지 요소를 초기화해준다.
    protected virtual void OnEnable()
    {
        //체력은 처음 유지한 거대로 
        hpSlider.maxValue = firstHP;
        //쉴드 바 끄기
        shieldSlider.gameObject.SetActive(false);
        CHARHP = firstHP;
        min = detectDistance;
        speed = charData.speed;
        pvpCharState = PVPCharState.idle;
        CHARLEVEL = currentEnhanceLevel;
        CHARMP = 0f;
        hpEffectImage.fillAmount = (CHARHP / hpSlider.maxValue);

        if (BackEndMatchManager.Instance.IsHost()) { StartCoroutine(UpdateCoroutine()); }
    }

    #region "캐릭터 FSM"
    IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            if (PVPInGM.Instance.pvpStageState == PVPStageState.BattleTime)
            {
                //Idle 상태일 때 
                if (pvpCharState == PVPCharState.idle)
                {
                    if (nearUnit != null)
                    {
                        Debug.Log("타겟있음");
                    }
                    else
                    {
                        pvpCharState = PVPCharState.walk;
                        anim.SetTrigger("walk");
                        exNearUnit = null;
                        BackEndMatchManager.Instance.SendDataToInGame(new Protocol.UnitWalkMessage(unitNum));
                    }

                }
                //Run상태일때
                else if (pvpCharState == PVPCharState.walk)
                {
                    if (!isRival) { AttackDetect(RivalManager.Instance.summonList); }
                    else { AttackDetect(PVPCharManager.Instance.summonList); }
                    if (nearUnit != null)
                    {
                        //적에게 이동
                        transform.position = Vector2.MoveTowards(transform.position, nearUnit.transform.position, speed * Time.deltaTime);

                        float distance = (nearUnit.transform.position - transform.position).sqrMagnitude;
                        //만약 적이 공격 사거리 안으로 들어온다면
                        if (distance <= charData.attackDistance)
                        {
                            pvpCharState = PVPCharState.attack;
                        }

                    }
                    else
                    {
                        float meMove = speed * Time.deltaTime;
                        transform.Translate(Vector2.left * meMove);
                    }
                }
                //Attack 상태일 때
                else if (pvpCharState == PVPCharState.attack)
                {
                    if (nearUnit != null)
                    {
                        
                        //만약 적과의 거리가 멀어지거나 적이 죽은 경우 상태를 Idle로 해준다.
                        float distance  = (nearUnit.transform.position - transform.position).sqrMagnitude;

                        if (nearUnit.pvpCharState == PVPCharState.death || distance >= charData.attackDistance)
                        {
                            min = detectDistance;
                            //타겟이 죽어 null이 되었을때 신호를 보내준다.
                            nearUnit = null;
                            pvpCharState = PVPCharState.idle;
                            anim.SetTrigger("idle");
                            //타겟이 Null 이면 신호를 보낸다.
                            BackEndMatchManager.Instance.SendDataToInGame(new Protocol.ReturnIdleMessage(unitNum));
                        }
                        else
                        {


                            float randomValue = Random.Range(0f, 100f);
                            if (randomValue <= criticalPercent) { isCritical = true; }
                            else { isCritical = false; }
                            Vector2 myPos;
                            myPos = new Vector2(-transform.position.x, transform.position.y);
                            BackEndMatchManager.Instance.SendDataToInGame(new Protocol.UnitAttackMessage(unitNum, myPos, isCritical));
                            anim.SetTrigger("attack");
                            yield return waitForDelayTime_Attack;
                        }
                    }
                }
                //Damage 상태일 때
                else if (pvpCharState == PVPCharState.stun)
                {
                    if (myStunTime > 0)
                    {
                        myStunTime -= Time.deltaTime;
                    }
                    //스턴시간이 끝났을 때
                    else
                    {
                        //타겟이 Null 이면 신호를 보낸다.
                        Debug.Log("스턴 종료");
                        BackEndMatchManager.Instance.SendDataToInGame(new Protocol.ReturnIdleMessage(unitNum));
                        StunOff();
                    }
                }
            }
            yield return null;
        }
    }
    #endregion
    #region "캐릭터 탐색 기능"
    //거리 비교
    void AttackDetect(List<PVPCharactor> activeUnit)
    {
        if (activeUnit.Count > 0)
        {
            //해쉬셋에 있는 적을 검출
            foreach (PVPCharactor enemy in activeUnit)
            {
                float distance = (enemy.transform.position - transform.position).sqrMagnitude;
                //탐지 사거리 안에있는 가장 가까운 유닛을 타겟으로 정한다.
                if (distance <= detectDistance && distance <= min)
                {
                    min = distance;
                    nearUnit = enemy;
                }
            }
            if (nearUnit != null && exNearUnit != nearUnit)
            {
                //Debug.Log(this.name + " : " + nearUnit + " , " + exNearUnit);
                exNearUnit = nearUnit;
                //서버에다가 타겟의 넘버와 내 넘버,포지션을 보내준다.(타겟이 Null이거나,상대 타워 일경우 )
                if (nearUnit.pvpCharState != PVPCharState.death)
                {
                    int rivalNum = nearUnit.unitNum;
                    BackEndMatchManager.Instance.SendDataToInGame(new Protocol.SetTargetMessage(unitNum, rivalNum));
                }
                else
                {
                    //타겟이 Null 이면 신호를 보낸다.
                    Debug.Log("걷는 중 타겟이 없어짐");
                    min = detectDistance;
                    //타겟이 죽어 null이 되었을때 신호를 보내준다.
                    nearUnit = null;
                    pvpCharState = PVPCharState.idle;
                    anim.SetTrigger("idle");
                    BackEndMatchManager.Instance.SendDataToInGame(new Protocol.ReturnIdleMessage(unitNum));
                }
            }
        }
    }
    #endregion
    #region "호스트가 아닌 쪽의 캐릭터 FSM"
    private void Update()
    {
        if (PVPInGM.Instance.pvpStageState == PVPStageState.BattleTime || PVPInGM.Instance.pvpStageState == PVPStageState.Stop)
        {
            if (hpEffectImage.fillAmount > (CHARHP / hpSlider.maxValue))
            {
                hpEffectImage.fillAmount -= hurtSpeed * Time.deltaTime;
            }
            else
            {
                hpEffectImage.fillAmount = (CHARHP / hpSlider.maxValue);
            }
        }
        if (pvpCharState == PVPCharState.walk && !BackEndMatchManager.Instance.IsHost() && PVPInGM.Instance.pvpStageState == PVPStageState.BattleTime)
        {
            if (nearUnit == null)
            {
                float meMove = speed * Time.deltaTime;
                transform.Translate(Vector2.left * meMove);
            }
            else
            {
                //적에게 이동
                transform.position = Vector2.MoveTowards(transform.position, nearUnit.transform.position, speed * Time.deltaTime);
            }
        }
    }

    #endregion

    #region "캐릭터 공격 이벤트"
    protected virtual void PVPOnAttackProcess()
    {
        //PVP 모드 일때는 호스트만 적에게 데미지를 입힐수 있도록한다.
        //호스트가 아닌 유저는 서버에서 받은 정보로 데미지를 입는다.
        if (BackEndMatchManager.Instance.IsHost())
        {
            if (pvpCharState == PVPCharState.death)
            {
                return;
            }
            if (nearUnit != null && PVPInGM.Instance.isWinCheck == true)
            {
                //크리티컬 발생시
                if (isCritical == true) { nearUnit.PVPOnDamageProcess(myDamage * criticalRatio, isCritical); }
                else { nearUnit.PVPOnDamageProcess(myDamage, isCritical); }
            }
        }
    }
    #endregion

    #region "캐릭터 데미지 이벤트"
    public void PVPOnDamageProcess(float enemyDamage, bool getCritical)
    {
        if (pvpCharState != PVPCharState.death && PVPInGM.Instance.isWinCheck == true)
        {
            //호스트에 경우 크리티컬 여부 받은 데미지를 서버에 보내준다.
            if (BackEndMatchManager.Instance.IsHost())
            {
                Vector2 myPos = new Vector2(-transform.position.x, transform.position.y);
                BackEndMatchManager.Instance.SendDataToInGame(new Protocol.UnitDamegedMessage(unitNum, myPos, enemyDamage, getCritical));
            }
            //hp감소
            if (isShield == true)
            {
                SHIELD -= enemyDamage;
                InGameUIManager.Instance.textPopUpManager.GetTextMesh(transform.position, (-enemyDamage).ToString(), PopUpType.SheildDamage);
            }
            else {
                
                if (getCritical == true)
                {
                    InGameUIManager.Instance.textPopUpManager.GetTextMesh(transform.position, (-enemyDamage).ToString(), PopUpType.Critical);
                }
                else
                {
                    InGameUIManager.Instance.textPopUpManager.GetTextMesh(transform.position, (-enemyDamage).ToString(), PopUpType.Damage);
                }
                CHARHP -= enemyDamage; 
            
            }

            //mp 증가
            CHARMP += 10f;

            //hp가 0보다 작으면 Die상태
            if (CHARHP <= 0)
            {
                pvpCharState = PVPCharState.death;
                StopAllCoroutines();
                StartCoroutine(Death());
            }

            if (CHARHP > 0 && CHARMP >= maxMP)
            {
                //스킬 발동 가능
                if (!isRival)
                {
                    //하단 배틀 UI에 스킬 활성화
                    //InGameUIManager.Instance.battleUICharMPSliers[PVPCharManager.Instance.charNumDic[charPoolNum]].maxValue = mpSlider.maxValue;                    
                }
            }
        }
    }

    IEnumerator Death()
    {
        if (!isRival)
        {
            myStunTime = 0;
            SHIELD = 0;
            PVPCharManager.Instance.summonList.Remove(this);
            //만약 캐릭터가 죽었다면 다시 소환 할 수 있게 해준다.
            PVPCharManager.Instance.CharSummonOn(charPoolNum);
            shieldEffect = CharEffectKind.None;
            anim.SetTrigger("death");
            //애니메이션을 끝까지 재생하기 위하여 딜레이 후 해당하는 오브젝트풀에 넣어준다.
            yield return waitForDelayTime_Die;
            //타겟 초기화
            nearUnit = null;
            PVPCharPoolingManager.Instance.InsertUnit(gameObject);
            //죽을 때 전투 UI효과 부여 
            InGameUIManager.Instance.CharDieUIEffect(PVPCharManager.Instance.charNumArr[charPoolNum]);
            yield break;
        }
        else
        {
            myStunTime = 0;
            SHIELD = 0;
            shieldEffect = CharEffectKind.None;
            RivalManager.Instance.summonList.Remove(this);
            anim.SetTrigger("death");
            //애니메이션을 끝까지 재생하기 위하여 딜레이 후 해당하는 오브젝트풀에 넣어준다.
            yield return waitForDelayTime_Die;
            //타겟 초기화
            nearUnit = null;
            RivalPoolingManager.Instance.InsertUnit(gameObject);
            yield break;
        }

    }
    #endregion

    #region "캐릭터 특수 상태(스턴,쉴드)"
    public void StunOn(float stunTime, CharEffectKind charEffectKind)
    {
        charEffect.EffectOn(charEffectKind);
        pvpCharState = PVPCharState.stun;
        stunEffect = charEffectKind;
        if (BackEndMatchManager.Instance.IsHost())
        {
            //스턴 시간은 가산되지않고 남아있는 스턴 시간이 새로운 스턴 시간보다 적으면 스턴시간을 갱신하게 해준다.
            if (myStunTime <= stunTime)
            {
                myStunTime = stunTime;
            }
        }
        anim.SetTrigger("idle");
        min = detectDistance;
        //타겟을 다시 찾게끔 하자
        nearUnit = null;
    }
    public void StunOff()
    {
        if (stunEffect != CharEffectKind.None)
        {
            myStunTime = 0;
            charEffect.EffectOff(stunEffect);
            stunEffect = CharEffectKind.None;
            pvpCharState = PVPCharState.idle;
        }
    }
    public void OnShield(float shledAmount, CharEffectKind charEffectKind)
    {
        isShield = true;
        shieldSlider.maxValue = shledAmount;
        SHIELD = shledAmount;
        shieldSlider.gameObject.SetActive(true);
        charEffect.EffectOn(charEffectKind);
        shieldEffect = charEffectKind;
    }
    #endregion


    public void SetSkillEnable(bool onoff)
    {
        //스킬 사용 가능 불가능
        
    }

    //스킬 발동
    public void ActiveSkill()
    {
        if (BackEndMatchManager.Instance.IsHost())
        {
            OnCharSkill();
        }
        else
        {
            BackEndMatchManager.Instance.SendDataToInGame(new Protocol.UnitSkillActiveMessage(InGameInfoManager.Instance.mySessionID, unitNum));
        }
    }
    public void OnCharSkill()
    {
        specialSkill.OnCharSkill(unitNum, nearUnit, isCritical, criticalPercent);
    }

    //스킬 연출 - 서버로부터 값을 받으면 실행
    //호스트의 경우 스킬 발동에 성공했는지 아닌지 여부 받아와서 실행
    public void SetSkillInfo()
    {
        //스킬 연출 전 시작, 끝지점을 알기 위해서 미리 정보를 받아둔다
        //스킬타입, 타겟타입에 따라서 다르다.


    }

    //연출파트
    public void PlaySkillAnimation()
    {
        //anim.SetTrigger("skill");

        //스킬에 맞는 이펙트와 사운드 출력

        //스킬 연출이 끝날때까지 다른 스텝으로 진입을 막아야 하는가?
    }

    private void FindSkillSet()
    {
        //데이터에 스킬 정보를 긁어와서 각 스킬에 맞춰서 등록해두기
    }

    CharactorBuff totalBuff = new CharactorBuff();
    public void SetBuff(int unitNum, int buffType, float buffValue)
    {
        //buffType = 버프의 효과
        //공격력증가, 최대체력증가, 체력회복, 등등등

        //해당 버프를 unitNum과 같이 해서 관리한다.

        //해당 Unit이 전장에서 사라지면 버프효과를 제거할 수 있게 된다.

    }

    public void CheckBuff()
    {
        //리스트 전체 탐색 돌면서 duration 남은 양을 확인한다.
        //duration이 0이 되면 버프효과를 제거한다

        for (int i = buffList.Count - 1; i >= 0; i--)
        {
            if (buffList[i].duration <= 0f)
            {
                //버프 효과 제거 = 리스트에서 제거
                totalBuff.normalAttack -= buffList[i].normalAttack;
            }
            buffList.Remove(buffList[i]);
        }
    }

    public void RemoveBuff(int buffIndex)
    {
        //해당 버프를 지움, 따로 인덱스가 지정 안되면 제일 마지막 버프를 지움?
    }

    public CharactorBuff GetBuffEffect()
    {
        CharactorBuff result = new CharactorBuff();
        foreach (CharactorBuff buff in buffList)
        {
            result.normalAttack += buff.normalAttack;
        }

        //합산해서 결과 반영

        return result;
    }


    public void SetDebuff(int unitNum, int debuffType, float debuffValue)
    {
        //디버프의 인덱스를 보고 추가효과가 있는걸 따로 기록해둔다

    }

    public void CheckDebuff()
    {
        //리스트 전체 탐색 돌면서 duration 남은 양을 확인한다.
        //duration이 0이 되면 디버프효과를 제거한다

        //남아있는 디버프 효과 중 제일 강력한 것을 스테이터스에 반영한다.
    }

    public void RemoveDebuff(int buffIndex)
    {
        //해당 디버프를 지움, 따로 인덱스가 지정 안되면 제일 마지막 버프를 지움?
    }

    //스테이터스 변화 없는 제어기    
    //Knockback -> 별도의 상태이상은 없으나, 거리계산을 다시 해서 공격할 수 없는 상황이 되면 다시 타겟을 찾아 이동해야 한다
    public void OnKnockback(Vector2 dir)
    {
        //날아가는 연출 실행

        //종료되면 타겟 다시 찾는 로직 실행
    }
    //Silience -> host이면 클라이언트로 해당 유닛 스킬 발동을 비활성화 시킨다
    public void OnSilence(bool onoff)
    {
        //침묵 활성 비활성

        //스킬 발동을 막는다
    }
    //Bloody -> 지속적 hp 감소
    //Target -> 받는 대미지 증가
    //Slow -> 이동 속도 감소

    //만약 이 캐릭터가 살아남아서 다음 라운드에도 유지된다면 할 행동 들 
    public void RoundUpEvent()
    {
        myStunTime = 0;
        SHIELD = 0;
        shieldEffect = CharEffectKind.None;
        //idle로 변환 
        pvpCharState = PVPCharState.idle;
        anim.SetTrigger("idle");
        nearUnit = null;
        hpEffectImage.fillAmount = (CHARHP / hpSlider.maxValue);
        //초기화
        min = detectDistance;
    }
}