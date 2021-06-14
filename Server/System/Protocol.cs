using BackEnd.Tcp;
using UnityEngine;
using System.Collections.Generic;

namespace Protocol
{
    // 이벤트 타입
    public enum Type : sbyte
    {
        SetTarget,              // 플레이어가 탐색 중 타겟이 기존의 타겟과 다를 때 신호를 보내준다.
        RetrunIdle,             // 유닛의 타겟이 null로 되어 캐릭터의 상태를 Idle로 바꿀 때 보내는 신호(타겟의 죽음,스턴 등)
        UnitChangeState,        // 유닛의 상태가 변할 때(애니메이션 변경) 보내주는 신호
        UnitAttack,             // 유닛 공격
        UnitWalk,               // 유닛 이동
        UnitDamaged,            // 유닛 데미지 받음
        UnitLevelUP,            // 유닛 레벨업시 
        UnitSkill,              // 유닛 터치스킬 발동시    //클라이언트가 서버로 보내는 패킷
        UnitSkill_Atk,          // 유닛 공격 스킬 사용
        UnitSkill_Spt,          // 유닛 보조 스킬 사용

        GatchaSkillInfo,        // 가챠 스킬 정보 전송
        GatchaSkillActive,      // 가챠 스킬 발동 시

        SetPrevGameInfo,        //사전 정보 보내기
        LoadRoomScene,          // 룸 씬으로 전환

        CountStart,             // 카운트(주사위 제한시간, 캐릭터 배치 시간)
        StartAssignedTime,      //양쪽 플레이어가 AssignedTime에 들어갔을 때 메세지
        BattleReadyCheck,       //양쪽 플레이어가 레디를 눌러 준비완료가 됬을 때 보내주는 메세지 
        RoundUP,                //호스트가 라운드에서 승리한 쪽을 보내주는 메세지

        SendDiceArr,            //주사위 롤을 한 뒤 나온 주사위 배열을 보내줌 
        DiceSelect,             //주사위를 선택 했을 때 보내주는 메세지
        SPAmount,               //SP를 사용하거나 얻었을 때 보내주는 메세지
        AssignedCharactor,      //서로가 캐릭터를 배치할 때 보내주는 리스트 값
        Emoticon,               //이모티콘 정보 값
        
        Max
    }

    public class Message
    {
        public Type type;

        public Message(Type type)
        {
            this.type = type;
        }
    }
    #region 게임 시작 전 프로토콜
    //게임 시작 전 사전에 필요한 정보(덱 정보,마법 정보, 이모티콘정보,이미지 정보 등)를 보내준다.
    public class SetPrevGameInfoMessage : Message
    {
        public SessionId playerSession;
        public string[] playerDecks = null;
        public string[] playerEmoticons = null;
        public SetPrevGameInfoMessage(SessionId session, string[] playerdeck, string[] playerEmoticons) : base(Type.SetPrevGameInfo)
        {
            this.playerSession = session;
            this.playerDecks = playerdeck;
            this.playerEmoticons = playerEmoticons;
        }
    }
    public class LoadRoomSceneMessage : Message
    {

        public LoadRoomSceneMessage() : base(Type.LoadRoomScene)
        {

        }
    }
    #endregion

    #region 인게임에서 사용되는 프로토콜
    #region GameFlow에 이용
    public class StartCountMessage : Message
    {
        public int time;
        public bool isRollTime;
        public StartCountMessage(int time, bool isRolltime) : base(Type.CountStart)
        {
            this.time = time;
            this.isRollTime = isRolltime;
        }
    }
        
    //i가 2일때(둘다 찬성 했을 때)RollTime을 시작한다.
    public class StartAssignedTimeMessage : Message
    {
        public bool agree;
        public int agreeCount = 0;
        public StartAssignedTimeMessage(bool agree) : base(Type.StartAssignedTime)
        {
            this.agree = agree;
            if (agree) { this.agreeCount++; }
        }
    }

    public class BattleReadyCheckMessage : Message
    {
        public bool agree;
        public int agreeCount = 0;

        public BattleReadyCheckMessage(bool agree) : base(Type.BattleReadyCheck)
        {
            this.agree = agree;
            if (agree) { this.agreeCount++; }
        }
    }
    public class RoundUPMessage : Message
    {
        public bool agree;
        public int agreeCount = 0;
        public RoundUPMessage(bool agree) : base(Type.RoundUP)
        {
            this.agree = agree;
            if (agree) { this.agreeCount++; }
        }
    }


    #endregion
    #region 인게임 기능 관련(주사위 정보, 캐릭터 소환정보, 공격,데미지 등)

    public class SendSPMessage : Message
    {
        public SessionId playerSession;
        public int sp;
        public SendSPMessage(SessionId session, int sp) : base(Type.SPAmount)
        {
            this.playerSession = session;
            this.sp = sp;
        }

    }
    public class SendDiceArrMessage : Message
    {
        public SessionId playerSession;
        public int[] diceArr;
        public SendDiceArrMessage(SessionId session, int[] diceArr) : base(Type.SendDiceArr)
        {
            this.playerSession = session;
            this.diceArr = diceArr;
        }
    }
    public class DiceSelectMessage : Message
    {
        //세션 아이디 
        public SessionId playerSession;
        //주사위 선택한 목록의 숫자 
        public int keepDiceCount;
        //현재 해당되는 족보 넘버 
        public int diceStateNum;
        
        public DiceSelectMessage(SessionId session,int keepDiceCount, int diceStateNum) : base(Type.DiceSelect)
        {
            this.playerSession = session;
            this.keepDiceCount = keepDiceCount;
            this.diceStateNum = diceStateNum;
        }
    }
    
    public class GatchaSkillInfoMessage : Message
    {
        public SessionId playerSession;
        public int gatchaSkillNum;

        public GatchaSkillInfoMessage(SessionId session, int gatchaSkillNum) : base(Type.GatchaSkillInfo)
        {
            this.playerSession = session;
            this.gatchaSkillNum = gatchaSkillNum;
        }
    }
    public class GatchaSkillActiveMessage : Message
    {
        public SessionId playerSession;
        public int[] targets;
        public GatchaSkillActiveMessage(SessionId sessionId ,int[] targets) : base(Type.GatchaSkillActive)
        {
            this.playerSession = sessionId;
            this.targets = targets;
        }
    }

    public class AssignedCharactorMessage : Message
    {
        public SessionId playerSession;
        public int poolNum;
        public AssignedCharactorMessage(SessionId session,int poolNum) : base(Type.AssignedCharactor)
        {
            this.playerSession = session;
            this.poolNum = poolNum;
        }
    }

    public class ReturnIdleMessage : Message
    {
        public int activeNum;
        public ReturnIdleMessage(int activeNum) : base(Type.RetrunIdle)
        {
            this.activeNum = activeNum;
        }
    }
    public class UnitWalkMessage :Message
    {
        public int activeNum;
        public UnitWalkMessage(int activeNum) : base(Type.UnitWalk)
        {
            this.activeNum = activeNum;
        }

    }
    //호스트가 보내는 캐릭터 타겟 동기화 메세지
    //받는거는 호스트가 아닌 사람만 받는다.

    public class SetTargetMessage : Message
    {
        public int activeUnitNum;
        public int targetNum;
        
        public SetTargetMessage(int activeUnitNum,int targetNum) : base(Type.SetTarget)
        {
            this.activeUnitNum = activeUnitNum;
            this.targetNum = targetNum;
        }
    }


    public class UnitAttackMessage : Message
    {
        public int activeUnitNum;
        public Vector2 myPos;
        public bool isCritical;
        public UnitAttackMessage(int activeUnitNum,Vector2 myPos,bool isCritical) : base(Type.UnitAttack)
        {
            this.activeUnitNum = activeUnitNum;
            this.myPos = myPos;
            this.isCritical = isCritical;
        }
    }

    public class UnitDamegedMessage : Message
    {
        public int activeUnitNum;
        public Vector2 myPos;
        public float damage;
        public bool isCritical;
      
        public UnitDamegedMessage(int activeUnitNum, Vector2 myPos, float damage, bool isCritical) : base(Type.UnitDamaged)
        {
            this.activeUnitNum = activeUnitNum;
            this.myPos = myPos;
            this.damage = damage;
            this.isCritical = isCritical;
        }
    }
    public class UnitLevelUPMessage : Message
    {
        public SessionId playerSession;
        public int poolNum;
        public UnitLevelUPMessage(SessionId session, int poolNum) : base(Type.UnitLevelUP)
        {
            this.playerSession = session;
            this.poolNum = poolNum;
        }
    }
        
    public class EmoticonMessage : Message
    {
        public SessionId SessionId;
        public int EmoticonNum;
        public EmoticonMessage(SessionId sessionId,int emoticonNum) : base(Type.Emoticon)
        {
            this.SessionId = sessionId;
            this.EmoticonNum = emoticonNum;
        }
    }
    
    public class UnitSkillActiveMessage : Message
    {
        public SessionId SessionId;
        public int UnitNum;
        public UnitSkillActiveMessage(SessionId sessionId, int unitNum) : base(Type.UnitSkill)
        {
            this.SessionId = sessionId;
            this.UnitNum = unitNum;
        }
    }

    public class UnitSkillAttackMessage : Message
    {
        public int[] targetUnitNums;
        public Vector2 myPos;
        public bool isCritical;
        public UnitSkillAttackMessage(int[] targetUnitNums, Vector2 myPos, bool isCritical) : base(Type.UnitSkill_Atk)
        {
            this.targetUnitNums = targetUnitNums;
            this.myPos = myPos;
            this.isCritical = isCritical;
        }
    }

    public class UnitSkillSupportMessage : Message
    {
        public int[] targetUnitNums;
        public Vector2 myPos;
        public float skillType;
        public float skillValue;
        public UnitSkillSupportMessage(int[] targetUnitNums,float T,float V, Vector2 myPos) : base(Type.UnitSkill_Spt)
        {
            this.targetUnitNums = targetUnitNums;
            this.myPos = myPos;

            skillType = T;
            skillValue = V;
        }
    }
    #endregion
    #endregion


}
