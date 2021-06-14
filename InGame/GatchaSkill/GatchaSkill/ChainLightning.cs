using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainLightning : GatchaSkill
{
    public float cycleTime;                 //스킬을 사용하는 주기
    public float range;                     //스킬을 사용하는 범위
    public float damage;                    //스킬 데미지
    public float speed;                     //투사체의 스피드
    public float attackDistance;            //공격 범위
    public int targetAmount;                //타겟 수         
    private WaitForSeconds cycletime_Delay;
    private GameObject s_obj;
    private ProjectileSkill projectileSkill;
    private int m_Value;

    public override void DoSkill()
    {

        cycletime_Delay = new WaitForSeconds(this.cycleTime);
        //가챠 스킬 매니저에 가챠스킬 오브젝트와 가챠 스킬 이펙트를 풀링 하도록시킨다.
        //그리고 리턴값으로 poolNum을 받는다.
        this.gatchaSkillPoolNum = SkillPoolingManager.Instance.CreateObjPool(this.gatchaSkillObj, this.gatchaSkillEffectObj, this.skillObjAmount, this.isRivalSkill);

        //라이벌 스킬이 아닌 상태에만 스킬 사용
        if (!isRivalSkill)
        {
            StartCoroutine(SkillUse());
        }
    }
    IEnumerator SkillUse()
    {

        while (true)
        {
            yield return null;

            if (InGameInfoManager.Instance.isPVPMode )
            {
                yield return cycletime_Delay; 
                if (RivalManager.Instance.summonList.Count == 0)
                {
                    yield break;
                }
                //타겟 찾기(랜덤으로 한 타겟을 설정한 후 그 타겟과 가장 가까운 유닛에게 전파된다.(targetAmount가 다 채워질 때 까지))
                //내가 쓰는 경우 하고 상대가 쓰는 경우가 필요하다.
                int value = Random.Range(0, RivalManager.Instance.summonList.Count);
                this.pvpTargetNums.Add(RivalManager.Instance.summonList[value].unitNum);
                
                //만약 정해 놓은 타겟 넘버 보다 소환 된 유닛이 적으면 소환된 유닛에 맞춰서 타겟 넘버를 조정해준다,
                if (RivalManager.Instance.summonList.Count <= targetAmount) { targetAmount = RivalManager.Instance.summonList.Count; }

                while (pvpTargetNums.Count < targetAmount)
                {
                    float min = float.MaxValue;
                    for (int i = 0; i < RivalManager.Instance.summonList.Count; i++)
                    {
                        //최근에 얻은 타겟을 제외하고 가장 가까운 유닛을 찾자
                        if (!this.pvpTargetNums.Contains(RivalManager.Instance.summonList[i].unitNum)&&i!=value)
                        {
                            float distance = (RivalManager.Instance.summonList[i].transform.position 
                                              - RivalManager.Instance.summonList[value].transform.position).sqrMagnitude;
                            if (distance <= min)
                            {
                                min = distance;
                                m_Value = i;
                            }
                        }
                    }
                    value = m_Value;
                    this.pvpTargetNums.Add(RivalManager.Instance.summonList[value].unitNum);
                    yield return null;
                }

                //오브젝트를 가져온다.
                s_obj = SkillPoolingManager.Instance.GetSkillObj(this.gatchaSkillPoolNum);
                //내가 공격 했다는 걸 보내준다.
                BackEndMatchManager.Instance.SendDataToInGame(new Protocol.GatchaSkillActiveMessage(InGameInfoManager.Instance.mySessionID, pvpTargetNums.ToArray()));
                //투사체 스킬에게 타겟 정보 보내기 
                projectileSkill = s_obj.transform.GetComponent<ProjectileSkill>();
                projectileSkill.poolnum = this.gatchaSkillPoolNum;
                projectileSkill.range = range;
                projectileSkill.damage = damage;
                projectileSkill.speed = speed;
                projectileSkill.targetAmount = targetAmount;
                projectileSkill.isRival = this.isRivalSkill;
                projectileSkill.attackDistance = attackDistance;
                projectileSkill.gatchaSkillType = this.gatchaSkillInfo.skillType;
                projectileSkill.targets.AddRange(this.pvpTargetNums);

                //오브젝트 위치 배정 
                Vector2 skiilSpawnPoint = new Vector2(PVPInGM.Instance.activeUnits[pvpTargetNums[0]].transform.position.x
                                                     , PVPInGM.Instance.activeUnits[pvpTargetNums[0]].transform.position.y);
                s_obj.transform.position = skiilSpawnPoint;
                s_obj.SetActive(true);
                projectileSkill.SKillOn();
                this.pvpTargetNums.Clear();

            }
            //else
            //{
            //    if (InGM.Instance.stageState != StageState.BattleTime)
            //    {
            //        Debug.LogError("배틀 타임 아님");
            //        break;
            //    }
            //    yield return cycletime_Delay;
            //    Debug.Log("스킬 사용");
            //    //타겟 찾기
            //    TargetSearch();
            //    //오브젝트를 가져온다.
            //    s_obj = SkillPoolingManager.Instance.GetSkillObj(this.gatchaSkillPoolNum);

            //    //투사체 스킬에게 타겟 정보 보내기 
            //    projectileSkill = s_obj.transform.GetComponent<ProjectileSkill>();
            //    projectileSkill.poolnum = this.gatchaSkillPoolNum;
            //    projectileSkill.range = range;
            //    projectileSkill.damage = damage;
            //    projectileSkill.isRival = this.isRivalSkill;
            //    projectileSkill.targets.AddRange(this.pvpTargetNums);

            //    //오브젝트 위치 배정 
            //    Vector2 skiilSpawnPoint = new Vector2(PVPInGM.Instance.activeUnits[pvpTargetNums[0]].transform.position.x
            //                                         + 3f, PVPInGM.Instance.activeUnits[pvpTargetNums[0]].transform.position.y + 6f);
            //    s_obj.transform.position = skiilSpawnPoint;
            //    s_obj.SetActive(true);
            //    this.pvpTargetNums.Clear();
            //}

        }
    }
    //상대가 스킬 공격 신호를 보낼 때마다 실행해줄 함수
    public override void RivalDoSkill(int[] targetNums)
    {
        //오브젝트를 가져온다.
        s_obj = SkillPoolingManager.Instance.GetSkillObj(this.gatchaSkillPoolNum);
        //타겟 정보 전달
        //투사체 스킬에게 타겟 정보 보내기 
        projectileSkill = s_obj.transform.GetComponent<ProjectileSkill>();
        projectileSkill.poolnum = this.gatchaSkillPoolNum;
        projectileSkill.range = range;
        projectileSkill.damage = damage;
        projectileSkill.speed = speed;
        projectileSkill.targetAmount = targetAmount;
        projectileSkill.isRival = this.isRivalSkill;
        projectileSkill.attackDistance = attackDistance;
        projectileSkill.gatchaSkillType = this.gatchaSkillInfo.skillType;
        projectileSkill.targets.AddRange(targetNums);
        //오브젝트 위치 배정 
        Vector2 skiilSpawnPoint = new Vector2(PVPInGM.Instance.activeUnits[targetNums[0]].transform.position.x
                                              , PVPInGM.Instance.activeUnits[targetNums[0]].transform.position.y);
        s_obj.transform.position = skiilSpawnPoint;
        s_obj.SetActive(true);
        projectileSkill.SKillOn();
    }
    public override void Initialize()
    {
        if (InGameInfoManager.Instance.isPVPMode)
        {
            this.pvpTargetNums.Clear();
        }
        else
        {
            this.targets.Clear();
        }
    }
}
