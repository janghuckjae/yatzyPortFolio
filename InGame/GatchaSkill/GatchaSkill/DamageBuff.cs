using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageBuff : GatchaSkill
{
    //캐릭터 버프시 필요한것 
    //가챠스킬의 버프는 한 라운드 동안 유지되는 스킬이다.
    //버프 스킬은 한번에 

    //강화 비율 
    public float enhanceRatio;
    public CharEffectKind charEffect;
    //강화를 받았던 캐릭터의 스텟을 다시 돌려주기 위하여 캐릭터정보를 저장해놓아야한다.
    private List<PVPCharactor> charactors =new List<PVPCharactor>();
    private List<PVPCharactor> rivals =new List<PVPCharactor>();
    //강화 전 데미지 
    private List<float> originDamage = new List<float>();
    private List<float> rivalOriginDamage = new List<float>();

    public override void DoSkill()
    {
        if (InGameInfoManager.Instance.isPVPMode)
        {
            if (!isRivalSkill)
            { 
                //소환되어있는 아군 유닛에게 버프 적용 
                for (int i = 0; i < PVPCharManager.Instance.summonList.Count; i++)
                {
                    //타겟 등록
                    this.pvpTargetNums.Add(PVPCharManager.Instance.summonList[i].unitNum);

                    //강화 받은 유닛 캐싱
                    charactors.Add(PVPCharManager.Instance.summonList[i]);
                    //타겟에게 맞는 버프 적용 
                    originDamage.Add(PVPCharManager.Instance.summonList[i].myDamage);
                    //버프 적용 (현재 데미지 + (초기 데미지 * 강화 비율))
                    PVPCharManager.Instance.summonList[i].myDamage += PVPCharManager.Instance.summonList[i].charData.power*enhanceRatio;
                    //버프 이펙트 활성화 
                    PVPCharManager.Instance.summonList[i].charEffect.EffectOn(charEffect);
                }
                //서버에 버프 적용 메세지를 보낸다.
                BackEndMatchManager.Instance.SendDataToInGame(new Protocol.GatchaSkillActiveMessage(InGameInfoManager.Instance.mySessionID, pvpTargetNums.ToArray()));
                Debug.Log("데미지 버프스킬 작용");
            }
        }
    }
    public override void RivalDoSkill(int[] targetNums)
    {
        //소환되어있는 아군 유닛에게 버프 적용 
        for (int i = 0; i < targetNums.Length; i++)
        {
            //강화 받은 유닛 캐싱
            rivals.Add(PVPInGM.Instance.activeUnits[targetNums[i]]);
            //타겟에게 맞는 버프 적용 
            rivalOriginDamage.Add(PVPInGM.Instance.activeUnits[targetNums[i]].myDamage);
            //버프 적용 (현재 데미지 + (초기 데미지 * 강화 비율))
            PVPInGM.Instance.activeUnits[targetNums[i]].myDamage += PVPInGM.Instance.activeUnits[targetNums[i]].charData.power * enhanceRatio;
            //버프 이펙트 활성화 
            PVPInGM.Instance.activeUnits[targetNums[i]].charEffect.EffectOn(charEffect);
            Debug.Log("라이벌 데미지 버프스킬 작용");
        }
    }

    public override void Initialize()
    {
        if (InGameInfoManager.Instance.isPVPMode)
        {
            //죽은 유닛들은 다시 소환 될 시 자동으로 능력치가 초기화 되지만 살아있는 유닛은 초기화 해주어야한다.
            //버프를 받았던 유닛들의 능력치를 다시 원래대로 돌려준다.
            for (int i = 0; i < charactors.Count; i++)
            {
                for (int j = 0; j < PVPCharManager.Instance.summonList.Count; j++)
                {
                    if (charactors[i] == PVPCharManager.Instance.summonList[j])
                    {
                        charactors[i].myDamage = originDamage[i];
                    }
                }
                charactors[i].charEffect.EffectOff(charEffect);
            }
            //라이벌도 똑같이적용해준다.
            for (int i = 0; i < rivals.Count; i++)
            {
                for (int j = 0; j < RivalManager.Instance.summonList.Count; j++)
                {
                    if (rivals[i] == RivalManager.Instance.summonList[j])
                    {
                        rivals[i].myDamage = rivalOriginDamage[i];
                    }
                }
                rivals[i].charEffect.EffectOff(charEffect);
            }
            //저장해 놓았던 리스트 초기화
            charactors.Clear();
            rivals.Clear();
            originDamage.Clear();
            rivalOriginDamage.Clear();
            this.pvpTargetNums.Clear();
        }
    }
}
