using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq ;

public class RepeatHeal : GatchaSkill
{
    public float cycleTime;                 //스킬을 사용하는 주기
    public float ratio;                     //회복할 비율
    public CharEffectKind effectKind;
    private WaitForSeconds cycletime_Delay;
    public override void DoSkill()
    {
        cycletime_Delay = new WaitForSeconds(this.cycleTime);
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

            if (InGameInfoManager.Instance.isPVPMode)
            {
                yield return cycletime_Delay;
                //라운드 동안 cycletime_Delay마다 전체 체력의 5%를 회복하게 해준다.
                for (int i = 0; i < PVPCharManager.Instance.summonList.Count; i++)
                {
                    this.pvpTargetNums.Add(PVPCharManager.Instance.summonList[i].unitNum);
                    PVPCharManager.Instance.summonList[i].CHARHP += PVPCharManager.Instance.summonList[i].firstHP * ratio;
                    InGameUIManager.Instance.textPopUpManager.GetTextMesh(PVPCharManager.Instance.summonList[i].transform.position,
                                                                  ("+" + PVPCharManager.Instance.summonList[i].firstHP * ratio).ToString(),
                                                                  PopUpType.Heal);
                    PVPCharManager.Instance.summonList[i].charEffect.EffectOn(effectKind);
                }
                BackEndMatchManager.Instance.SendDataToInGame(new Protocol.GatchaSkillActiveMessage(InGameInfoManager.Instance.mySessionID, pvpTargetNums.ToArray()));
                this.pvpTargetNums.Clear();
            }
        }
    }
    //상대가 스킬 공격 신호를 보낼 때마다 실행해줄 함수
    public override void RivalDoSkill(int[] targetNums)
    {        
        //라운드 동안 cycletime_Delay마다 전체 체력의 5%를 회복하게 해준다.
        for (int i = 0; i < targetNums.Length; i++)
        {
            PVPInGM.Instance.activeUnits[targetNums[i]].CHARHP += PVPInGM.Instance.activeUnits[targetNums[i]].firstHP * ratio;
            InGameUIManager.Instance.textPopUpManager.GetTextMesh(PVPInGM.Instance.activeUnits[targetNums[i]].transform.position,
                                                                  ("+" + PVPInGM.Instance.activeUnits[targetNums[i]].firstHP * ratio).ToString(),
                                                                  PopUpType.Heal);

            PVPInGM.Instance.activeUnits[targetNums[i]].charEffect.EffectOn(effectKind);
        }
    }
    public override void Initialize()
    {
        if (InGameInfoManager.Instance.isPVPMode)
        {
            foreach (var charactor in PVPInGM.Instance.activeUnits.Values)
            {
                charactor.charEffect.EffectOff(effectKind);
            }
            this.pvpTargetNums.Clear();
        }
        else
        {
        }
    }
}
