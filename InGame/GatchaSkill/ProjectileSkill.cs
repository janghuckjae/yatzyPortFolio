using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSkill : MonoBehaviour
{
    public int poolnum;
    public float range;
    //타겟 
    public List<int> targets;
    public float damage;
    public float speed;
    public float attackDistance;
    public bool isRival;
    public int targetAmount;
    public GatchaSkillType gatchaSkillType;

    private Vector3 target;
    private float distance;
    private GameObject e_obj;
    private Effect effect;

    public void SKillOn()
    {
        StartCoroutine(OnProjectileSkill());
    }
    IEnumerator OnProjectileSkill()
    {
        //targets 방향으로 순서대로 이동한다. 
        //타겟의 숫자로 메테오 처럼 단일 타겟의 유닛을 지정하거나 체인 라이트닝 처럼 순서대로 이동도 가능하다.
        for (int i = 0; i < targets.Count;i++)
        { 
            target = PVPInGM.Instance.activeUnits[targets[i]].transform.position;
            distance = float.MaxValue;
            //거리가 attackDistance까지 올때까지 반복해라
            while (distance >= attackDistance)
            {
                //타워가 게임이 끝난 후 남아있던 총알 때문에 오류가 생겨 방어 코드 작성
                if (PVPInGM.Instance.pvpStageState != PVPStageState.BattleTime)
                {
                    Debug.Log("남아있었어");
                    target = Vector3.zero;
                    SkillPoolingManager.Instance.InsertSkillObj(gameObject, poolnum);
                    yield break;
                }
                
                //타겟 방향으로 보게하기
                LookTarget();
                //이동
                transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
                //거리 계산
                distance = (target - transform.position).sqrMagnitude;
                yield return null;
            }
            //도달
            //도달 했을 때 해당 스킬의 포지션 주위에 있는 유닛을 탐지하여 해당 되는 유닛에게 데미지를 준다.
            //호스트 일때 데미지 신호르 보내줌 
            if (BackEndMatchManager.Instance.IsHost())
            {
                switch (gatchaSkillType)
                {
                    case GatchaSkillType.RepeatPartSkill:

                        PVPInGM.Instance.activeUnits[targets[i]].PVPOnDamageProcess(damage, false);
                        Debug.Log("타겟 공격 : " + targets[i]);
                        break;
                    case GatchaSkillType.RepeatRangeSkill:
                        //탐색 
                        if (isRival == true)
                        {
                            for (int j = 0; j < PVPCharManager.Instance.summonList.Count; j++)
                            {
                                float damageDistance = (PVPCharManager.Instance.summonList[j].transform.position - transform.position).sqrMagnitude;
                                if (damageDistance <= range)
                                {
                                    PVPCharManager.Instance.summonList[j].PVPOnDamageProcess(damage, false);
                                }
                            }

                        }
                        else
                        {
                            for (int k = 0; k < RivalManager.Instance.summonList.Count; k++)
                            {
                                float damageDistance = (RivalManager.Instance.summonList[k].transform.position - transform.position).sqrMagnitude;

                                if (damageDistance <= range)
                                {
                                    RivalManager.Instance.summonList[k].PVPOnDamageProcess(damage, false);
                                }
                            }
                        }
                        break;
                }
                
            }
            ArriveTargetPos();
        }
        Initialize();
        yield return null;
    }
    //최종 목표까지 도달 했을 때 주는 이벤트
    void ArriveTargetPos()
    {
        //나는 풀로 돌려줘야한다.
        //이펙트 풀에서 가져오기
        e_obj= SkillPoolingManager.Instance.GetSkillEffect(poolnum);
        //이펙트 한테 정보 전달
        effect = e_obj.transform.GetComponent<Effect>();
        effect.poolNum = poolnum;
        //이펙트 위치 지정 
        e_obj.transform.position = target;
        //이펙트 활성화
        e_obj.SetActive(true);
        target = Vector2.zero;

    }
    void Initialize()
    {
        SkillPoolingManager.Instance.InsertSkillObj(gameObject, poolnum);
        //targets.Clear();
    }
    // 총알이 타겟이 위치한 방향으로 기울어서 이동
    void LookTarget()
    {
        Vector2 dir = target - transform.position;

        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
