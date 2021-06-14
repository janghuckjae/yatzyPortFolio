using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DiceMissionProtocol
{
    public class DiceMission
    {
        public DiceMissonKind diceMisson;
        public int spReward;     //보상 
        public string title;     //제목
        public int missionValue1; //미션 값1
        public int missionValue2; //미션 값2
        public DiceMission(DiceMissonKind dicemission)
        {
            this.diceMisson = dicemission;
        }
        public virtual bool ClearCheck()
        {
            return true;
        }
        
    }
    public class StateMission : DiceMission
    {
        public StateMission(int stateNum) : base(DiceMissonKind.StateMission)
        {
            this.spReward = 100;
            this.missionValue1 = stateNum;
            this.title = string.Format("미션 : {0}를 맞추세요.", (HandRank)stateNum);
        }
        public override bool ClearCheck()
        {
            if (DiceManager.Instance.currentRankList.Contains((HandRank)this.missionValue1))
            {
                return true;
            }
            else { return false; }

        }

    }
    //연속 된 숫자 체크 
    public class ContiuonsMission : DiceMission
    {
        public ContiuonsMission(int missionValue) : base(DiceMissonKind.ContinuousMission)
        {
            this.spReward = 100;
            this.missionValue1 = missionValue;
            this.title = string.Format("미션 : 연속된 숫자가 {0}개 이상 되게 하세요.", missionValue);
        }
        public override bool ClearCheck()
        {
            int straight_C = DiceManager.Instance.straight_C;
            if (straight_C >= this.missionValue1) { return true; }
            else { return false; }
        }
    }
    
    public class OddNumMission : DiceMission
    {
        public OddNumMission() : base(DiceMissonKind.OddNumMission)
        {
            this.spReward = 100;
            this.title = "미션 : 주사위 값이 홀수로만 이루어지게 하세요";
        }
        public override bool ClearCheck() 
        {
            List<int> keepList = DiceManager.Instance.keepDiceList;
            //만약 전부 선택 하지 않았다면 false반환
            if (keepList.Count != 5)
            {
                return false;
            }
            int count = 0;
            foreach (int num in keepList)
            {
                if (num % 2 != 0)
                {
                    count++;
                }
            }
            if (count == keepList.Count) { return true; }
            else { return false; }
        }
    }
    public class EvenNumMission : DiceMission
    {
        public EvenNumMission() : base(DiceMissonKind.EvenNumMission)
        {
            this.spReward = 100;
            this.title = "미션 : 주사위 값이 짝수로만 이루어지게 하세요.";
        }
        public override bool ClearCheck() 
        {
            List<int> keepList = DiceManager.Instance.keepDiceList;
            //만약 전부 선택 하지 않았다면 false반환
            if (keepList.Count != 5)
            {
                return false;
            }
            int count = 0;
            foreach (int num in keepList)
            {
                if (num % 2 == 0)
                {
                    count++;
                }
            }
            if (count == keepList.Count) { return true; }
            else { return false; }
        }
    }
    public class LessthanNumMission : DiceMission
    {
        //주사위의 값 모두가 MissionValue 보다 낮을 때 
        public LessthanNumMission(int missionValue) : base(DiceMissonKind.LessthanNumMission)
        {
            this.spReward = 100;
            this.missionValue1 = missionValue;
            this.title = string.Format("미션 : 선택한 주사위가 모두 {0} 이하가 되도록 만드세요", missionValue);
        }
        public override bool ClearCheck()
        {
            List<int> keepList = DiceManager.Instance.keepDiceList;
            //만약 전부 선택 하지 않았다면 false반환
            if (keepList.Count != 5)
            {
                return false;
            }
            int count = 0;
            foreach (int num in keepList)
            {
                if (num <= this.missionValue1)
                {
                    count++;
                }
            }
            if (count == keepList.Count) { return true; }
            else { return false; }
        }
    }
    public class MorethanNumMission : DiceMission
    {
        //주사위의 값 모두가 MissionValue 보다 낮을 때         
        public MorethanNumMission(int missionValue) : base(DiceMissonKind.LessthanNumMission)
        {
            this.spReward = 100;
            this.missionValue1 = missionValue;
            this.title = string.Format("미션 : 선택한 주사위가 모두 {0} 이상이 되도록 만드세요", missionValue);

        }
        public override bool ClearCheck()
        {
            List<int> keepList = DiceManager.Instance.keepDiceList;
            //만약 전부 선택 하지 않았다면 false반환
            if (keepList.Count != 5)
            {
                return false;
            }
            int count = 0;
            foreach (int num in keepList)
            {
                if (num >= this.missionValue1)
                {
                    count++;
                }
            }
            if (count == keepList.Count) { return true; }
            else { return false; }
        }
    }
    public class IncludeNumMission : DiceMission
    {
        public IncludeNumMission(int missionValue1, int missionValue2) : base(DiceMissonKind.IncludeNumMission)
        {
            this.missionValue1 = missionValue1;
            this.missionValue2 = missionValue2;
            this.spReward = 100;
            this.title = string.Format("미션 : 선택한 주사위 리스트에 {0}과 {1}이 포함되게 만드세요", missionValue1, missionValue2);
        }
        public override bool ClearCheck()
        {
            List<int> keepList = DiceManager.Instance.keepDiceList;
            //만약 전부 선택 하지 않았다면 false반환
            //if (keepList.Count != 5)
            //{
            //    return false;
            //}
            if (keepList.Contains(missionValue1) && keepList.Contains(missionValue2))
            {
                return true;
            }
            else { return false; }
        }
    }
    public class ExceptionNumMission : DiceMission
    {
        public ExceptionNumMission(int missionValue1, int missionValue2) : base(DiceMissonKind.ExceptionNumMission)
        {
            this.missionValue1 = missionValue1;
            this.missionValue2 = missionValue2;
            this.spReward = 100;
            this.title = string.Format("미션 : 선택한 주사위 리스트에 {0}과 {1}이 제외되게 만드세요", missionValue1, missionValue2);
        }
        public override bool ClearCheck()
        {
            List<int> keepList = DiceManager.Instance.keepDiceList;
            //만약 전부 선택 하지 않았다면 false반환
            if (keepList.Count != 5)
            {
                return false;
            }

            if (!keepList.Contains(missionValue1) && !keepList.Contains(missionValue2))
            {
                return true;
            }
            else { return false; }
        }
    }
    //합계 체크
    public class AmountMission : DiceMission
    {

        public AmountMission(int missionValue) : base(DiceMissonKind.AmountMission)
        {

            this.spReward = 100;
            this.missionValue1 = missionValue;
            this.title = string.Format("미션 : 주사위 값의 합계가 {0}이 되도록 만드세요.", missionValue);
        }
        public override bool ClearCheck()
        {
            List<int> keepList = DiceManager.Instance.keepDiceList;
            //만약 전부 선택 하지 않았다면 false반환
            if (keepList.Count != 5)
            {
                return false;
            }
            //만약 리스트 안의 값이 mission값과 같을때 true
            if (keepList.Sum() == this.missionValue1)
            {
                return true;
            }
            else { return false; }
        }
    }
    public class BelowTotalMission : DiceMission
    {
        public BelowTotalMission(int missionValue) : base(DiceMissonKind.BelowTotalMission)
        {
            this.missionValue1 = missionValue;
            this.spReward = 100;
            this.title = string.Format("미션 : 선택한 주사위 합이 {0}보다 적게 만드세요.", missionValue);
        }
        public override bool ClearCheck()
        {
            List<int> keepList = DiceManager.Instance.keepDiceList;
            //만약 전부 선택 하지 않았다면 false반환
            if (keepList.Count != 5)
            {
                return false;
            }

            if (keepList.Sum() <= this.missionValue1)
            {
                return true;
            }
            else { return false; }
        }
    }
    public class MorethanTotalMission : DiceMission
    {
        public MorethanTotalMission(int missionValue) : base(DiceMissonKind.MorethanTotalMission)
        {
            this.missionValue1 = missionValue;
            this.spReward = 100;
            this.title = string.Format("미션 : 선택한 주사위 합이 {0}보다 많게 만드세요.", missionValue);
        }
        public override bool ClearCheck()
        {
            List<int> keepList = DiceManager.Instance.keepDiceList;
            if (keepList.Sum() >= this.missionValue1)
            {
                return true;
            }
            else { return false; }
        }
    }
}




