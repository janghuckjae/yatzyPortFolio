using System.Collections;
using System.Collections.Generic;

public class CharactorBuff
{
    //코드만 가지고 사용
    //pvpCharactor에 정보만 가지고 있는다.

    public float duration;

    public float normalAttack;
    public float normalAttackSpeed;
    public float damageReduce;
    public float hp;
    public float speed;

    /*
     * 대미지 증가
     * 공격 속도 증가
     * 이동 속도 증가
     * 흡혈
     * 대미지 감소
     * 무적
     * 버서커
     * 변신
     */
}


public class CharactorAbnormalState
{
    public float duration;

    public int stateIndex;

    //상태이상이 동시에 여러개 걸렸을때의 처리 방법
    //일단 동시에 다 걸고 시간 카운트 -> 추가 들어오면 추가효과
    //동일 인덱스 있으면 효과 시간 연장?
    //세부 효과라던가 그런 부분에서 값이 다를수 있다.
    //각각 기록하자
    //출혈출혈출혈 같은것도 될수 있다


    /*
     * 넉백
     * 속박
     * 매혹
     * 침묵
     * 얼음
     * 혼란
     * 속감
     * 추방
     * 스턴
     * 출혈
     * 표식
     */
    
}