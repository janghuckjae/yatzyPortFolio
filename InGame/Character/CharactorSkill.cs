using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region skill enum
public enum SKILL
{
    NONE,
    SPECIAL,
    ACTIVE,
    PASSIVE,
}


public enum SKILL_TYPE
{
    NONE,
    ATTACK,
    SUPPORT,
}

public enum TARGET_TYPE
{
    NONE,
    SINGLE,     //단일타겟
    MULTI,      //2인 이상
}

public enum TARGET_MULTI
{
    GROUP,      //정해진 그룹 넘버
    RANDOM,     //숫자 수 만큼
    FULL,       //숫자에 상관없이 전체
}

public enum ATTACK_TYPE
{
    MILLE,      //가까운 타겟 = 현재 공격중 타겟
    RANGE,      //떨어진 타겟 = 새로운 탐색을 해야 함
    GRAB,       //강제이동과 관련된 타입 = 떨어진 타겟 탐색 + 강제이동 + 디버프
}

public enum SUPPORT_TYPE
{
    HEAL,
    HEAL_REPEAT,
    BUFF_ATK,
    BUFF_CRITICAL,
    BUFF_CRITICAL_DAMAGE,
    BUFF_ATTACK_SPEED,
    BUFF_SHIELD,
    BUFF_SHIELD_AREA,
    BUFF_CURE,
    BUFF_RESURRECTION,
}


public enum SKILL_SPECIAL
{
    GRAB,       //끌고오기
    STURN,      //스턴
    BLOOD,      //출혈 (지속대미지 계열)
    POISON,     //독
    TELEPORT,   //순간이동

}
#endregion
public class CharactorSkill : MonoBehaviour
{
    private TARGET_TYPE targetType;
    private TARGET_MULTI targetMultiType;
    private CharIconType targetInfo;

    private bool isGet;         //스킬 습득 여부

    private float coolTime;     //쿨타임
    private bool isActive;      //활성화 여부

    private int unitNum;

    public void SetSkillType(TARGET_TYPE t1, TARGET_MULTI t2, CharIconType t3)
    {
        targetType = t1;
        targetMultiType = t2;
        targetInfo = t3;
    }

    public void OnCharSkill(int unitNum, PVPCharactor nearUnit, bool isCritical, float criticalPercent)
    {
        this.unitNum = unitNum;
        //각 캐릭터가 가지고 있는 고유 스킬을 발동한다.
        //스킬에는 각 타입이 존재해서 타입에 맞는 행동을 한다
        SKILL_TYPE type = SKILL_TYPE.SUPPORT;
        switch (type)
        {
            case SKILL_TYPE.NONE:
                break;
            case SKILL_TYPE.ATTACK:
                Skill_AttackType(nearUnit.transform, isCritical, criticalPercent);
                break;
            case SKILL_TYPE.SUPPORT:
                Skill_SupportType(nearUnit.transform);
                break;
            default:
                break;
        }
    }

    private void Skill_AttackType(Transform nearUnit,bool isCritical, float criticalPercent)
    {
        //공격타입 스킬
        //호스트일 때는 크리티컬 확률과 포지션을 계산후 서버에 전송한다음 공격한다.
        if (BackEndMatchManager.Instance.IsHost())
        {
            //타겟 탐색 알고리즘 가동
            List<int> targets = FindSkillTarget(nearUnit, true);

            float randomValue = Random.Range(0f, 100f);
            if (randomValue <= criticalPercent) { isCritical = true; }
            else { isCritical = false; }
            Vector2 myPos;
            myPos = new Vector2(-transform.position.x, transform.position.y);

            if (targets != null && targets.Count > 0)
                BackEndMatchManager.Instance.SendDataToInGame(new Protocol.UnitSkillAttackMessage(targets.ToArray(), myPos, isCritical));
        }
    }

    private void Skill_SupportType(Transform nearUnit)
    {
        //타겟 탐색 알고리즘 가동
        List<int> targets = FindSkillTarget(nearUnit, false);

        Vector2 myPos;
        myPos = new Vector2(-transform.position.x, transform.position.y);

        //host에서 처리 //실제 효과가 데이터로 저장
        foreach (int i in targets)
        {
            PVPInGM.Instance.activeUnits[i].CHARHP += 10f;
        }

        //스킬 효과 전달
        if (targets != null && targets.Count > 0)
        {
            BackEndMatchManager.Instance.SendDataToInGame(new Protocol.UnitSkillSupportMessage(targets.ToArray(), 0f, 10f, myPos));
        }
    }

    //아군 타겟 탐색
    private List<int> FindSkillTarget(Transform nearUnit, bool bFindRival)
    {
        List<int> result = new List<int>();
        //targets에 원하는 결과 찾아서 입력해주기
        //활성화된 transform
        //PVPInGM.Instance.activeEnemy

        List<Transform> list = null;
        //아군을 찾는지 적군을 찾는지 확인
        //나의 아군인지 적의 아군인지를 알아야 한다
        //if (unitNum > 15)
        //{
        //    //클라의 요청 : 클라의 아군은 호스트한테는 activeFriend에 들어있다.
        //    if (bFindRival)
        //        list = PVPInGM.Instance.activeFriend;
        //    else
        //        list = PVPInGM.Instance.activeEnemy;
        //}
        //else
        //{
        //    if (bFindRival)
        //        list = PVPInGM.Instance.activeEnemy;
        //    else
        //        list = PVPInGM.Instance.activeFriend;
        //}
        List<PVPCharactor> data = new List<PVPCharactor>();

        foreach (Transform trans in list)
        {
            data.Add(trans.GetComponent<PVPCharactor>());
        }
        if (data.Count > 0)
        {
            switch (targetType)
            {
                case TARGET_TYPE.NONE:
                    break;
                case TARGET_TYPE.SINGLE:
                    {
                        //0이면 제일 가까운 타겟
                        //1~4로 그룹지정하면 해당 그룹 중 가까운 타겟
                        if (targetInfo == CharIconType.No)
                        {
                            result.Add(nearUnit.GetComponent<PVPCharactor>().unitNum);
                        }
                        else
                        {
                            float targetDist = 3000f;
                            PVPCharactor targetChar = null;
                            foreach (PVPCharactor pChar in data)
                            {
                                if (pChar.myType == targetInfo)
                                {
                                    float dist = (pChar.transform.position - transform.position).sqrMagnitude;
                                    //탐지 사거리 안에있는 가장 가까운 유닛을 타겟으로 정한다.
                                    if (dist <= targetDist)
                                    {
                                        targetDist = dist;
                                        targetChar = pChar;
                                    }
                                }
                            }

                            if (targetChar != null)
                                result.Add(targetChar.unitNum);
                        }
                    }
                    break;
                case TARGET_TYPE.MULTI:
                    switch (targetMultiType)
                    {
                        case TARGET_MULTI.GROUP:
                            //1~4 근딜, 원딜, 탱커, 서폿
                            foreach (PVPCharactor pChar in data)
                            {
                                if (pChar.myType == targetInfo)
                                {
                                    result.Add(pChar.unitNum);
                                }
                            }
                            break;
                        case TARGET_MULTI.RANDOM:
                            //활성화 유닛 중 값만큼 탐색
                            //중복 허용

                            //중복 허용 안함
                            int maxValue = 3;
                            if(data.Count < maxValue)
                            {
                                //정해진 타겟 수 보다 적다.
                                //1. 있는 타겟만 확보
                                foreach (PVPCharactor pChar in data)
                                {
                                    result.Add(pChar.unitNum);
                                }


                                //2. 중복타겟으로 변경
                                //??
                            }
                            else
                            {
                                //랜덤 뽑기
                                int[] randArray = new int[data.Count];
                                for (int i = 0; i < data.Count; i++)
                                    randArray[i] = i;
                                for(int i = 0; i< 50; i++)
                                {
                                    int rand1 = Random.Range(0, data.Count);
                                    int rand2 = Random.Range(0, data.Count);
                                    int temp = randArray[rand1];
                                    randArray[rand1] = randArray[rand2];
                                    randArray[rand2] = temp;
                                }

                                for(int i = 0; i< maxValue; i++)
                                    result.Add(data[randArray[i]].unitNum);
                            }
                            break;
                        case TARGET_MULTI.FULL:
                            //활성화 된 유닛 전체
                            foreach (PVPCharactor pChar in data)
                            {
                                result.Add(pChar.unitNum);
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        return result;
    }
}
