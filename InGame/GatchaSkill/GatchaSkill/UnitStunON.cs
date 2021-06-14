using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStunON : GatchaSkill
{
    public float cycleTime; //스킬을 사용하는 주기
    public float time;      //스턴 시간
    public int targetAmount;//스턴을 걸 유닛 수 
    public CharEffectKind effectKind;

    private WaitForSeconds cycletime_Delay;
    public override void DoSkill()
    {
        cycletime_Delay = new WaitForSeconds(this.cycleTime);
        //int[] a = PVPInGM.Instance.GetRandomInt(targetAmount, 0, RivalManager.Instance.summonList.Count);
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
            if (InGameInfoManager.Instance.isPVPMode)
            {
                yield return cycletime_Delay;
                if (RivalManager.Instance.summonList.Count == 0 )
                {
                    yield break;
                }

                //타겟 찾기(살아 있는 적 유닛중에 골라 스턴을 걸리게 한다.)

                //만약 정해 놓은 타겟 넘버 보다 소환 된 유닛이 적으면 소환된 유닛에 맞춰서 타겟 넘버를 조정해준다,
                if (RivalManager.Instance.summonList.Count <= targetAmount) { targetAmount = RivalManager.Instance.summonList.Count; }

                int[] targets = PVPInGM.Instance.GetRandomInt(targetAmount, 0, RivalManager.Instance.summonList.Count);
                for (int i = 0;i< targets.Length; i++)
                {
                    this.pvpTargetNums.Add(RivalManager.Instance.summonList[targets[i]].unitNum);
                    //스턴 적용
                    RivalManager.Instance.summonList[targets[i]].StunOn(time,effectKind);
                }

                //서버에 스킬 사용 정보 (내 세션,타겟 리스트)보내주기
                BackEndMatchManager.Instance.SendDataToInGame(new Protocol.GatchaSkillActiveMessage(InGameInfoManager.Instance.mySessionID, pvpTargetNums.ToArray()));

                

                this.pvpTargetNums.Clear();
            }
        }
    }
    public override void RivalDoSkill(int[] targetNums)
    {
        for (int i = 0; i < targetNums.Length; i++)
        {
            PVPInGM.Instance.activeUnits[targetNums[i]].StunOn(time, effectKind);
        }
    }
    public override void Initialize()
    {
        if (InGameInfoManager.Instance.isPVPMode)
        {
            foreach (PVPCharactor charactor in PVPInGM.Instance.activeUnits.Values)
            {
                charactor.StunOff();
            }
            this.pvpTargetNums.Clear();
        }
    }
}
