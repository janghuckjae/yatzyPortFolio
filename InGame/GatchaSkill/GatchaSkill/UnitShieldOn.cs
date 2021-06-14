using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitShieldOn :GatchaSkill
{
    public float cycleTime; //스킬을 사용하는 주기
    public float shieldAmount;//쉴드 양
    public int targetAmount;//스턴을 걸 유닛의 타입

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
                if (PVPCharManager.Instance.summonList.Count == 0)
                {
                    yield break;
                }

                //타겟 찾기(살아 있는 아군 유닛에서 타겟 넘버 만큼 쉴드를 준다.)

                //만약 정해 놓은 타겟 넘버 보다 소환 된 유닛이 적으면 소환된 유닛에 맞춰서 타겟 넘버를 조정해준다,
                if (PVPCharManager.Instance.summonList.Count <= targetAmount) { targetAmount = PVPCharManager.Instance.summonList.Count; }

                int[] targets = PVPInGM.Instance.GetRandomInt(targetAmount, 0, PVPCharManager.Instance.summonList.Count);
                for (int i = 0; i < targets.Length; i++)
                {
                    this.pvpTargetNums.Add(PVPCharManager.Instance.summonList[targets[i]].unitNum);
                    //스턴 적용
                    PVPCharManager.Instance.summonList[targets[i]].OnShield(shieldAmount, effectKind);
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
            PVPInGM.Instance.activeUnits[targetNums[i]].OnShield(shieldAmount, effectKind);
        }
    }
    public override void Initialize()
    {
        if (InGameInfoManager.Instance.isPVPMode)
        {
            
            foreach (PVPCharactor charactor in PVPInGM.Instance.activeUnits.Values)
            {
                charactor.SHIELD = 0;
            }
            this.pvpTargetNums.Clear();
        }
    }
}
