using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeShieldOn : GatchaSkill
{
    public float cycleTime; //스킬을 사용하는 주기
    public float shieldAmount;//쉴드 양

    public CharEffectKind effectKind;

    private CharIconType type;//스턴을 걸 유닛의 타입
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
                //랜덤으로 타입 정하기 
                int randomValue = Random.Range(0, (int)CharIconType.Support);
                type = (CharIconType)randomValue;

                //타겟 찾기(살아 있는 아군 유닛 중에 정해 놓은 타입이 있다면 쉴드를 준다)
                for (int i = 0; i < PVPCharManager.Instance.summonList.Count; i++)
                {
                    //서포터 이외의 직업일 때
                    if (PVPCharManager.Instance.summonList[i].myType == type)
                    {
                        this.pvpTargetNums.Add(PVPCharManager.Instance.summonList[i].unitNum);
                        PVPCharManager.Instance.summonList[i].OnShield(shieldAmount, effectKind);
                    }
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
