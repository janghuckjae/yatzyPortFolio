using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DiceMissionProtocol;
//다이스 미션 종류 
public enum DiceMissonKind :int
{ 
    StateMission,       //특정 주사위 족보 맞추기 미션
    ContinuousMission,  //연속된 숫자 미션
    OddNumMission,      //최종 선택 값이 홀수로만 이루어져있을 때 
    EvenNumMission,     //최종 선택 값이 짝수로만 이루어져있을 때 
    LessthanNumMission, //최종 선택 값이 n의 숫자 이하로만 있을 때 
    MorethanNumMission, //최종 선택 값이 n의 숫자 이상으로만 있을 때 
    IncludeNumMission,  //최종 선택 값이 랜덤한 2가지의 숫자를 포함했을 때 
    ExceptionNumMission,//최종 선택 값이 랜덤한 2가지의 숫자를 제외했을 때 
    AmountMission,      //최종 선택 값의 합이 정해진 숫자 일 때 
    BelowTotalMission,  //최종 선택 값의 합이  정해진 숫자 아래일 때 
    MorethanTotalMission,//최종 선택 값의 합이  정해진 숫자 위일 때 
    
    Max
}

public class DiceMissionManager : MonoBehaviour
{
    [SerializeField] private Text[] missionTexts;
    [SerializeField] private GameObject[] successImgs;

    [SerializeField] private DiceMission[] diceMissions;
    [SerializeField] private bool[] isDiceClear;
    [SerializeField] private int[] clearRewards;
    private void Start()
    {
        diceMissions = new DiceMission[missionTexts.Length];
       
        for (int i = 0; i < successImgs.Length; i++)
        {
            successImgs[i].SetActive(false);
        }
    }

    //waitTime때 개인에게 무작위로 3가지의 미션을 준다.
    //미션 창에 적용해준다.
    public void GetMisson()
    {
        //0~10까지의 다이스 미션 종류중 하나를 고름
        isDiceClear = new bool[missionTexts.Length];
        clearRewards = new int[missionTexts.Length];
        int[] randomKindValue = PVPInGM.Instance.GetRandomInt(diceMissions.Length, 0, (int)DiceMissonKind.Max);
        for (int i = 0; i < randomKindValue.Length; i++)
        {
            //미션 적용
            AssignedMissonKind(i, randomKindValue[i]);
        }
    }
    private void AssignedMissonKind(int i, int diceMissionKindNum)
    {
        DiceMissonKind diceMisson = (DiceMissonKind)diceMissionKindNum;
        int randomMissionValue1;
        int randomMissionValue2;
        int amountValue;
        switch (diceMisson)
        {
            case DiceMissonKind.StateMission:
                //onePair부터 
                randomMissionValue1 = Random.Range(1, (int)HandRank.Max);
                diceMissions[i] = new StateMission(randomMissionValue1);
                missionTexts[i].text = diceMissions[i].title;
                break;
            case DiceMissonKind.ContinuousMission:
                randomMissionValue1 = Random.Range(1, 5);
                diceMissions[i] = new ContiuonsMission(randomMissionValue1);
                missionTexts[i].text = diceMissions[i].title;
                break;
            case DiceMissonKind.OddNumMission:
                diceMissions[i] = new OddNumMission();
                missionTexts[i].text = diceMissions[i].title;
                break;
            case DiceMissonKind.EvenNumMission:
                diceMissions[i] = new EvenNumMission();
                missionTexts[i].text = diceMissions[i].title;
                break;
            case DiceMissonKind.LessthanNumMission:
                randomMissionValue1 = Random.Range(2, 7);
                diceMissions[i] = new LessthanNumMission(randomMissionValue1);
                missionTexts[i].text = diceMissions[i].title;
                break;
            case DiceMissonKind.MorethanNumMission:
                randomMissionValue1 = Random.Range(1, 5);
                diceMissions[i] = new MorethanNumMission(randomMissionValue1);
                missionTexts[i].text = diceMissions[i].title;
                break;
            case DiceMissonKind.IncludeNumMission:
                int[] a = PVPInGM.Instance.GetRandomInt(2, 1, 7);
                randomMissionValue1 = a[0];
                randomMissionValue2 = a[1];
                diceMissions[i] = new IncludeNumMission(randomMissionValue1, randomMissionValue2);
                missionTexts[i].text = diceMissions[i].title;
                break;
            case DiceMissonKind.ExceptionNumMission:
                int[] b = PVPInGM.Instance.GetRandomInt(2, 1, 7);
                randomMissionValue1 = b[0];
                randomMissionValue2 = b[1];
                diceMissions[i] = new ExceptionNumMission(randomMissionValue1, randomMissionValue2);
                missionTexts[i].text = diceMissions[i].title;
                break;
            case DiceMissonKind.AmountMission:
                amountValue = Random.Range(5, 31);
                diceMissions[i] = new AmountMission(amountValue);
                missionTexts[i].text = diceMissions[i].title;
                break;
            case DiceMissonKind.BelowTotalMission:
                amountValue = Random.Range(5, 31);
                diceMissions[i] = new BelowTotalMission(amountValue);
                missionTexts[i].text = diceMissions[i].title;
                break;
            case DiceMissonKind.MorethanTotalMission:
                amountValue = Random.Range(5, 31);
                diceMissions[i] = new MorethanTotalMission(amountValue);
                missionTexts[i].text = diceMissions[i].title;
                break;
        }
    }

    public void MissionClearCheck()
    {
        for (int i = 0; i < diceMissions.Length; i++)
        {
            if (diceMissions[i].ClearCheck() == true)
            {
                isDiceClear[i] = true;
                clearRewards[i] = diceMissions[i].spReward;
                MissionClearEvent(i);
            }
        }
    }
    //미션을 클리어 할 시 효과
    private void MissionClearEvent(int i)
    {
        successImgs[i].SetActive(true);
    }
    //미션 보상 적용
    public void MissonReward()
    {
        int rewardTotal = 0;
        //성공한 미션의 보상을 모아 한꺼번에 올려준다.
        for (int i = 0; i < diceMissions.Length; i++)
        {
            if (isDiceClear[i] == true)
            {
                rewardTotal += clearRewards[i];
            }
        }
        if (InGameInfoManager.Instance.isPVPMode) { PVPInGM.Instance.SP += rewardTotal; }
        else { InGM.Instance.SP += rewardTotal; }

    }
    
    
    //RollTime이 끝나면 미션을 초기화 해준다.
    public void DiceMissionInitialize()
    {
        for (int i = 0; i < successImgs.Length; i++)
        {
            successImgs[i].SetActive(false);
        }
    }
}
