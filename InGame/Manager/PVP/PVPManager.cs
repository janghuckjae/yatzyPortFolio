using System.Collections;
using UnityEngine;

using Protocol;
using BackEnd;
using BackEnd.Tcp;
using System;

public class PVPManager : MonoBehaviour
{
    private static PVPManager _instance;

    public static PVPManager Instance // 싱글톤 구현
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(PVPManager)) as PVPManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(PVPManager)) as PVPManager;
                }
            }
            return _instance;
        }
    }
    public GameObject[] onPVPUI;
    public GameObject[] offPVPUI;

    public int rollTime = 20;
    public int assignedTime = 10;
    //찬성할 때 더해줄 변수
    
    private int assignedTimeCount = 0;
    private int battleReadyCount = 0;   
    private int roundUPCnt = 0;   

    // Start is called before the first frame update
    void Start()
    {
        if (InGameInfoManager.Instance.isPVPMode)
        {
            //PVP에 사용할 UI 활성화
            for (int i = 0; i < onPVPUI.Length; i++) { onPVPUI[i].SetActive(true); }

            for (int j = 0; j < offPVPUI.Length; j++) { offPVPUI[j].SetActive(false); }

        }
        else
        {
            for (int i = 0; i < onPVPUI.Length; i++) { onPVPUI[i].SetActive(false); }

            for (int j = 0; j < offPVPUI.Length; j++) { offPVPUI[j].SetActive(false); }
        }
    }

    //서버가 보낸 통신을 받아서 이벤트 처리하는 메소드
    public void OnRecieve(MatchRelayEventArgs args)
    {
        if (args.BinaryUserData == null)
        {
            Debug.LogWarning(string.Format("빈 데이터가 브로드캐스팅 되었습니다.\n{0} - {1}", args.From, args.ErrInfo));
            // 데이터가 없으면 그냥 리턴
            return;
        }
        Message msg = DataParser.ReadJsonData<Message>(args.BinaryUserData);
        if (msg == null)
        {
            return;
        }
       
        switch (msg.type)
        {
            //RollTime, AssignedTime 시 호스트만 시간체크하고 그것을 서버에 올려 둘이 같은 시간을 보여주게한다.
            case Protocol.Type.CountStart:
                StartCountMessage startCount = DataParser.ReadJsonData<StartCountMessage>(args.BinaryUserData);
                PVPInGM.Instance.PVPTimeCheck(startCount.time, startCount.isRollTime);
                break;
                //양쪽이 모두 준비상태일 때 캐릭터 배치시간으로 넘어간다.
            case Protocol.Type.StartAssignedTime:
                StartAssignedTimeMessage assignedTimeMessage = DataParser.ReadJsonData<StartAssignedTimeMessage>(args.BinaryUserData);
                SetAssignedTimeStart(assignedTimeMessage);
                break;

            case Protocol.Type.BattleReadyCheck:
                BattleReadyCheckMessage battleReadyCheckMessage = DataParser.ReadJsonData<BattleReadyCheckMessage>(args.BinaryUserData);
                BattleReady(battleReadyCheckMessage);
                break;
                //호스트가 RoundUP할 때 보내는 메세지
            case Protocol.Type.RoundUP:
                RoundUPMessage roundUPMessage = DataParser.ReadJsonData<RoundUPMessage>(args.BinaryUserData);
                RoundUP(roundUPMessage);
                break;
                //캐릭터 배치시 동기화
            case Protocol.Type.AssignedCharactor:
                AssignedCharactorMessage assignedMessage = DataParser.ReadJsonData<AssignedCharactorMessage>(args.BinaryUserData);
                RivalManager.Instance.RivalCharSummon(assignedMessage.playerSession, assignedMessage.poolNum);
                break;
            //상대방의 SP가 소모, 증가 할 때 마다 신호를 받아 값을 적용 해준다.
            case Protocol.Type.SPAmount:
                SendSPMessage sendSPMessage = DataParser.ReadJsonData<SendSPMessage>(args.BinaryUserData);
                if (InGameInfoManager.Instance.mySessionID != sendSPMessage.playerSession)
                {
                    PVPInGM.Instance.RivalSPShow(sendSPMessage);
                }
                break;
            //상대방이 롤을 돌릴 때 전체 주사위 배열 값을 받아 동기화시켜준다.
            case Protocol.Type.SendDiceArr:
                SendDiceArrMessage sendDiceArrMessage = DataParser.ReadJsonData<SendDiceArrMessage>(args.BinaryUserData);
                DiceManager.Instance.SetRivalDiceArr(sendDiceArrMessage);
                break;
            //상대방 다이스 선택 목록 값을 받으면 내 화면에 동기화시켜준다.
            case Protocol.Type.DiceSelect:
                DiceSelectMessage scoreMessage = DataParser.ReadJsonData<DiceSelectMessage>(args.BinaryUserData);
                DiceManager.Instance.RivalSelectDiceInfo(scoreMessage);
                break;
            //상대가 가챠 스킬을 뽑았을 때 보내는 가챠스킬 정보
            case Protocol.Type.GatchaSkillInfo:
                GatchaSkillInfoMessage gatchaSkillInfo = DataParser.ReadJsonData<GatchaSkillInfoMessage>(args.BinaryUserData);
                if (gatchaSkillInfo.playerSession !=InGameInfoManager.Instance.mySessionID)
                {
                    SkillGatchaManager.Instance.RivalSelectGatchaSkill(gatchaSkillInfo.gatchaSkillNum);
                }
                break;
                //가챠 스킬이 발동 되었을 때 신호보내기
            case Protocol.Type.GatchaSkillActive:
                GatchaSkillActiveMessage gatchaSkillActive = DataParser.ReadJsonData<GatchaSkillActiveMessage>(args.BinaryUserData);
                if (gatchaSkillActive.playerSession != InGameInfoManager.Instance.mySessionID)
                {
                    Debug.Log("가챠스킬 발동 들어옴");
                    SkillGatchaManager.Instance.rivalGatchaSkill.RivalDoSkill(gatchaSkillActive.targets);
                }
                break;
                // 공격 도중 타겟이 NUll이 되면(타겟 죽음 등) 포지션을 잡아준다.
            case Protocol.Type.RetrunIdle:
                ReturnIdleMessage setPosMessage = DataParser.ReadJsonData<ReturnIdleMessage>(args.BinaryUserData);
                ReturnIdle(setPosMessage);
                break;
                //유닛이 이동을 시작할 때 메세지
            case Protocol.Type.UnitWalk:
                UnitWalkMessage unitWalkMessage = DataParser.ReadJsonData<UnitWalkMessage>(args.BinaryUserData);
                UnitMoveStart(unitWalkMessage);
                break;
                // 타겟이 바뀔 때 동기화해주는 메세지
            case Protocol.Type.SetTarget:
                SetTargetMessage setTargetMessage = DataParser.ReadJsonData<SetTargetMessage>(args.BinaryUserData);
                SetCharTarget(setTargetMessage);
                break;
                //캐릭터 공격 이벤트 동기화
            case Protocol.Type.UnitAttack:
                UnitAttackMessage attackMessage = DataParser.ReadJsonData<UnitAttackMessage>(args.BinaryUserData);
                SetUnitAttack(attackMessage);
                break;
                // 캐릭터 피격 이벤트 동기화
            case Protocol.Type.UnitDamaged:
                UnitDamegedMessage damegedMessage = DataParser.ReadJsonData<UnitDamegedMessage>(args.BinaryUserData);
                SetUnitDamage(damegedMessage);
                break;
            case Protocol.Type.UnitLevelUP:
                UnitLevelUPMessage unitLevelUPMessage = DataParser.ReadJsonData<UnitLevelUPMessage>(args.BinaryUserData);
                RivalManager.Instance.RivalUnitLevelUP(unitLevelUPMessage.playerSession, unitLevelUPMessage.poolNum);
                break;
            case Protocol.Type.Emoticon:
                EmoticonMessage emoticonMessage = DataParser.ReadJsonData<EmoticonMessage>(args.BinaryUserData);
                EmoticonManager.Instance.ReceiveEmoticon(emoticonMessage);
                break;
            case Protocol.Type.UnitSkill:
                //유닛 공격 스킬 발동시 실행
                UnitSkillActiveMessage unitSkillActiveMessage = DataParser.ReadJsonData<UnitSkillActiveMessage>(args.BinaryUserData);
                SetUnitSkillActive(unitSkillActiveMessage);
                break;
            case Protocol.Type.UnitSkill_Atk:
                //유닛 공격 스킬 발동시 실행
                UnitSkillAttackMessage usam = DataParser.ReadJsonData<UnitSkillAttackMessage>(args.BinaryUserData);
                SetUnitSkillAttack(usam);
                break;
            case Protocol.Type.UnitSkill_Spt:
                //유닛 보조 스킬 발동시 실행
                UnitSkillSupportMessage ussm = DataParser.ReadJsonData<UnitSkillSupportMessage>(args.BinaryUserData);
                SetUnitSkillSupport(ussm);
                break;
            default:
                Debug.Log("Unknown protocol type");
                return;
        }
    }
    //타겟이 null이 되었을 때 보내는 신호를 받아 동기화시켜준다.(타겟의 죽음, 스턴 등)
    private void ReturnIdle(ReturnIdleMessage msg)
    {
        if (!BackEndMatchManager.Instance.IsHost() && PVPInGM.Instance.pvpStageState == PVPStageState.BattleTime)
        {
            int unitNum = msg.activeNum;
            //Debug.Log("ReturnIDle : " + PVPInGM.Instance.activeUnits[unitNum].name);
            PVPInGM.Instance.activeUnits[unitNum].nearUnit = null;
            PVPInGM.Instance.activeUnits[unitNum].min = PVPInGM.Instance.activeUnits[unitNum].detectDistance;
            //만약 스턴이 끝날 때 
            if (PVPInGM.Instance.activeUnits[unitNum].pvpCharState == PVPCharState.stun)
            {
                PVPInGM.Instance.activeUnits[unitNum].StunOff();
                PVPInGM.Instance.activeUnits[unitNum].pvpCharState = PVPCharState.idle;
            }
            else
            {
                PVPInGM.Instance.activeUnits[unitNum].pvpCharState = PVPCharState.idle;
                PVPInGM.Instance.activeUnits[unitNum].anim.SetTrigger("idle") ;
            }
        }
    }
    private void UnitMoveStart(UnitWalkMessage msg)
    {
        if (!BackEndMatchManager.Instance.IsHost() && PVPInGM.Instance.pvpStageState == PVPStageState.BattleTime)
        {
            int unitNum = msg.activeNum;
            PVPInGM.Instance.activeUnits[unitNum].pvpCharState = PVPCharState.walk;
            PVPInGM.Instance.activeUnits[unitNum].anim.SetTrigger("walk");
            //Debug.Log("그냥 걸어가기!" + PVPInGM.Instance.activeUnits[unitNum].name + " : :" + PVPInGM.Instance.activeUnits[unitNum].isRival);
        }
    }
    //호스트가 아닌 사람의 아군 캐릭터, 상대 캐릭터의 타겟 정보를 동기화해준다.
    private void SetCharTarget(SetTargetMessage msg)
    {
        if (!BackEndMatchManager.Instance.IsHost() && PVPInGM.Instance.pvpStageState == PVPStageState.BattleTime)
        { 
            int unitNum = msg.activeUnitNum;
            int targetNum = msg.targetNum;
            PVPInGM.Instance.activeUnits[unitNum].nearUnit = PVPInGM.Instance.activeUnits[targetNum];
            PVPInGM.Instance.activeUnits[unitNum].pvpCharState = PVPCharState.walk;
            //Debug.Log("타겟 발견!" + PVPInGM.Instance.activeUnits[unitNum].name + " : :" + PVPInGM.Instance.activeUnits[unitNum].isRival);
        }
    }

    private void SetUnitAttack(UnitAttackMessage msg)
    {
        if (!BackEndMatchManager.Instance.IsHost() && PVPInGM.Instance.pvpStageState == PVPStageState.BattleTime)
        {
            //유닛 넘버를 찾아 그 유닛의 포지션을 보정해주고 공격하게한다.
            int unitNum = msg.activeUnitNum;
            PVPInGM.Instance.activeUnits[unitNum].transform.position = msg.myPos;
            //크리티컬과 공격가능여부를 활성화해준다.
            PVPInGM.Instance.activeUnits[unitNum].isCritical = msg.isCritical;
            //공격시킨다.
            PVPInGM.Instance.activeUnits[unitNum].pvpCharState = PVPCharState.attack;
            PVPInGM.Instance.activeUnits[unitNum].anim.SetTrigger("attack");
            //Debug.Log("공격해라!" + PVPInGM.Instance.activeUnits[unitNum].name + " : :" + PVPInGM.Instance.activeUnits[unitNum].isRival);
        }
    }
    private void SetUnitDamage(UnitDamegedMessage msg)
    {
        if (!BackEndMatchManager.Instance.IsHost() && PVPInGM.Instance.pvpStageState == PVPStageState.BattleTime)
        {
            int unitNum = msg.activeUnitNum;
            PVPInGM.Instance.activeUnits[unitNum].transform.position = msg.myPos;
            PVPInGM.Instance.activeUnits[unitNum].PVPOnDamageProcess(msg.damage, msg.isCritical);
        }
    }

    private void RoundUP(RoundUPMessage msg)
    {
        roundUPCnt += msg.agreeCount;
        if (roundUPCnt == 2)
        {
            //너무 바로 넘어가는 경향이 있어 딜레이를 준 후에 라운드를 넘긴다.
            //호스트가 아닌쪽은 아직 게임이 안끝났는데 라운드가 바뀌는 경우 방지
            //라운드가 끝났다는 신호가 양쪽에서 넘어와야 라운드 업을 해준다.
            PVPInGM.Instance.DelayRoundUp();
            roundUPCnt = 0;
        }
    }

    private void BattleReady(BattleReadyCheckMessage msg)
    {
        battleReadyCount += msg.agreeCount;
        //서로 찬성했을 경우 
        if(battleReadyCount ==2)
        {
            PVPInGM.Instance.GotoBattleTime();
            battleReadyCount = 0;
        }
    }
    //만약 둘다 RollTime에 찬성했다면 State를 RollTime으로 돌리고 서버에 시간을 세도록한다.
    private void SetAssignedTimeStart(StartAssignedTimeMessage assignedTimeMessage)
    {
        assignedTimeCount += assignedTimeMessage.agreeCount;
        //서로 찬성했을 경우
        if (assignedTimeCount == 2)
        {
            StartCoroutine(GoAssignedEvent());
            assignedTimeCount = 0;
        }
    }
    IEnumerator GoAssignedEvent()
    {
        yield return new WaitForSeconds(DiceManager.Instance.spGetDelayTime);

        //상대 다이스 정보 초기화
        DiceManager.Instance.DiceInfoClear();
        PVPInGM.Instance.pvpStageState = PVPStageState.AssignedTime;
        //소환 UI를 켜주고 DiceUI를 꺼준다.
        InGameUIManager.Instance.SwapPlayFunc(false);
        //호스트만 시간체크 메세지를 보낸다.
        if (BackEndMatchManager.Instance.IsHost())
        {
            StartCoroutine(CountStart(assignedTime, false));
        }
    }
    public void CountStartMethod(int time, bool isRolltime)
    {
        StartCoroutine(CountStart(time, isRolltime));
    }
    IEnumerator CountStart(int time, bool isRolltime)
    {
        //isRollTime = true 일때는  롤타임 ,false라면 배치시간이다.
        StartCountMessage msg = new StartCountMessage(time, isRolltime);
        //카운트
        for (int i = 0; i < time + 1; ++i)
        {
            //만약 주사위 롤 타임인데 캐릭터 배치 타임으로 넘어가면 코루틴 종료
            if (isRolltime==true && PVPInGM.Instance.pvpStageState == PVPStageState.AssignedTime)
            {
                yield break;
            }
            //만약 준비 완료 버튼을 눌렀다면 코루틴 종료
            if (isRolltime == false && PVPInGM.Instance.isBattleReady)
            {
                yield break;
            }
            msg.time = time - i;
            BackEndMatchManager.Instance.SendDataToInGame<StartCountMessage>(msg);
            yield return new WaitForSeconds(1);//1초 단위
        }
    }

    private void SetUnitSkillActive(UnitSkillActiveMessage msg)
    {
        if (BackEndMatchManager.Instance.IsHost())
        {
            //sessionID, UnitNum 확인해서 누가 행동한건지 확인해서 스킬 돌린다.

            PVPInGM.Instance.activeUnits[msg.UnitNum].OnCharSkill();
        }
    }

    private void SetUnitSkillAttack(UnitSkillAttackMessage msg)
    {
        if (!BackEndMatchManager.Instance.IsHost())
        {
            ////유닛 넘버를 찾아 그 유닛의 포지션을 보정해주고 공격하게한다.
            //int unitNum = msg.activeUnitNum;
            //PVPInGM.Instance.activeUnits[unitNum].transform.position = msg.myPos;
            ////크리티컬과 공격가능여부를 활성화해준다.
            //PVPInGM.Instance.activeUnits[unitNum].isCritical = msg.isCritical;
            //PVPInGM.Instance.activeUnits[unitNum].isAttack = true;
            ////공격시킨다.
            //PVPInGM.Instance.activeUnits[unitNum].Attack();

            foreach (int i in msg.targetUnitNums)
            {
                PVPInGM.Instance.activeUnits[i].PlaySkillAnimation();
            }
        }
    }

    private void SetUnitSkillSupport(UnitSkillSupportMessage msg)
    {
        if (!BackEndMatchManager.Instance.IsHost())
        {
            //효과 표시
            if(msg.skillType == 0f)
            {
                foreach (int i in msg.targetUnitNums)
                {
                    PVPInGM.Instance.activeUnits[i].CHARHP += msg.skillValue;
                }
            }


            foreach (int i in msg.targetUnitNums)
            {
                PVPInGM.Instance.activeUnits[i].PlaySkillAnimation();
            }
        }
    }
}
