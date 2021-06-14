using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    public int poolNum;

    private void OnEnable()
    {
        StartCoroutine(Finsh());
    }
    //이펙트에서 데미지를 주어야할 함수 
    public void OnEffectAttack(float Damage)
    {
            

    }
    //우선 코루틴으로 진행()
    IEnumerator Finsh()
    {
        yield return new WaitForSeconds(1f);
        FinishEffect();
    }
    //이펙트가 끝났을 때 
    public void FinishEffect()
    {
        if (!this.gameObject.activeSelf)
        {
            return;
        }
        SkillPoolingManager.Instance.InsertSkillEffect(gameObject, poolNum);
    }
}
