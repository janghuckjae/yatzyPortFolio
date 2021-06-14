using System.Collections.Generic;
using UnityEngine;



public abstract class GatchaSkill : MonoBehaviour
{
    [System.Serializable]
    public struct GatchaSkillInfo
    {
        public string uniqueNumber;
        public string skillName;
        [TextArea]
        public string skillInfo;
        //스킬타입 
        public GatchaSkillType skillType;
        public Sprite skillImg;
        //스킬의 가중치
        public int gatchaWeight;
    }
    
    [Header("스킬 정보")]
    public GatchaSkillInfo gatchaSkillInfo;
    public List<int> pvpTargetNums = new List<int>();
    //public List<int> pvpRivalTargetNums = new List<int>();
    [HideInInspector] public List<Charactor> targets = new List<Charactor>();
    [HideInInspector] public List<Enemy> enemyTarget = new List<Enemy>();
    [Header("스킬 생성 및 풀링 관련")]
    public GameObject gatchaSkillObj;       //생성할 스킬
    public GameObject gatchaSkillEffectObj; //생성할 스킬
    public int skillObjAmount;              //스킬 풀링 시 생성할 오브젝트 양 
    public int gatchaSkillPoolNum;                     //스킬 풀링 시 얻는 풀링 넘버

    [Header("스킬 발동 관련 ")]
    public bool isRivalSkill;               //상대 스킬인지 내스킬인지 체크하는 bool 함수 true면 상대 스킬이다.
    

    //스킬 발동
    public virtual void DoSkill() { }

    //호스트가 아닌 사람이 스킬을 사용 할 때 
    public virtual void RivalDoSkill(int[] targetNums) { }
 
    //스킬 관련 자료 초기화
    public virtual void Initialize() { }

    //게임 오브젝트및 이펙트 오브젝트 풀링
   
    
}


