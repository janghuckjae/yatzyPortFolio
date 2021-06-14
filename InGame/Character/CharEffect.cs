using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharEffectKind
{ 
    None,
    //버프
    DamageUP,
    AttackSpeedUP,
    CriticalPercentageUP,
    CriticalRatioUp,
    HPMAXUP,
    RepeatHeal,
    Shield,

    //디버프
    Stun,
    AttackDown,
    AttackSpeedDown,
    CriticalPercentageDown,
    CriticalRatioDown,
    Max
}
//캐릭터가 강화, 디버프 ,스킬 등 이펙트나 상태를 표시해야 할 때의 기능 관리를 하는 스크립트
public class CharEffect : MonoBehaviour
{

    [SerializeField]private SkillEffectData skillEffectData;
    private Dictionary<CharEffectKind, GameObject> effectDic = new Dictionary<CharEffectKind, GameObject>();
    private GameObject effectObj;
    private void Start()
    {
        //스킬 이펙트를 자기 자신이 있는 캐릭터의 자식으로 생성한다.
        EffctInstantiate(skillEffectData.atkSpeedUpBuff_GS_Effect, CharEffectKind.AttackSpeedUP);
        EffctInstantiate(skillEffectData.criPercentBuff_GS_Effect, CharEffectKind.CriticalPercentageUP);
        EffctInstantiate(skillEffectData.criRaitioBuff_GS_Effect, CharEffectKind.CriticalRatioUp);
        EffctInstantiate(skillEffectData.damageBuff_GS_Effect, CharEffectKind.DamageUP);
        EffctInstantiate(skillEffectData.HPMAXUpBuff_GS_Effect, CharEffectKind.HPMAXUP);
        EffctInstantiate(skillEffectData.repeatHeal_GS_Effect, CharEffectKind.RepeatHeal);
        EffctInstantiate(skillEffectData.shield_Effect, CharEffectKind.Shield);
        EffctInstantiate(skillEffectData.stun_Effect, CharEffectKind.Stun);
    }

    private void EffctInstantiate(GameObject effect,CharEffectKind charEffectKind)
    {
        //스킬 이펙트를 자기 자신이 있는 캐릭터의 자식으로 생성한다.
        effectObj = Instantiate(effect, this.transform);
        effectDic.Add(charEffectKind, effectObj);
        effectObj.transform.position = this.transform.position;
        effectObj.SetActive(false);
    }
    public void EffectOn(CharEffectKind effectKind)
    {
        //효과에 알맞은 이펙트가 캐릭터가 실행하게함
        effectDic[effectKind].SetActive(true);
    }
    public void EffectOff(CharEffectKind effectKind)
    {
        //효과가 없어지게되면 이펙트가 사라지게함
        effectDic[effectKind].SetActive(false);
    }

}
