using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PVPBullet : MonoBehaviour
{
    //타겟
    public PVPCharactor target;
    public int bulletPoolNum;

    [HideInInspector] public bool onBullet = false;
    [HideInInspector] public bool isRival = false;
    //크리티컬 발동여부
    [HideInInspector] public bool criticalActive;
    //크리티컬 배수 
    [HideInInspector] public float criticalRatio = 1f;
    public float bulletPow;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float attackDistance;

    
    private void Awake()
    {
        //PVP모드가 아닐때 꺼준다.
        if (!InGameInfoManager.Instance.isPVPMode)
        {
            this.enabled = false;
        }
    }
    private void OnEnable()
    {
        StartCoroutine(BulletUpdateCoroutine());
    }
    IEnumerator BulletUpdateCoroutine()
    {
        while (true)
        {
            yield return null;
            if (onBullet)
            {
                if (PVPInGM.Instance.isWinCheck == false)
                {
                    target = null;
                    onBullet = false;
                    PVPBulletPoolingManager.Instance.InsertPool(gameObject, bulletPoolNum);
                    yield break;
                }
                //적방향으로 바라보게 함
                LookTarget();

                //적에게 이동
                transform.position = Vector2.MoveTowards(transform.position, target.transform.position, bulletSpeed * Time.deltaTime);

                #region 만약 타겟의 Tag가 Player일 때 
                if (target.CompareTag("Player"))
                {
                    float distance = (target.transform.position - transform.position).sqrMagnitude;
                    //만약 타겟과의 거리가 공격거리 안쪽으로 들어왔을 때  공격한다.
                    if (distance <= attackDistance)
                    {
                        if (target.pvpCharState != PVPCharState.death)
                        {
                            //타워가 게임이 끝난 후 남아있던 총알 때문에 오류가 생겨 방어 코드 작성
                            if (PVPInGM.Instance.pvpStageState != PVPStageState.BattleTime)
                            {
                                target = null;
                                onBullet = false;
                                PVPBulletPoolingManager.Instance.InsertPool(gameObject, bulletPoolNum);
                                yield break;
                            }
                            if (BackEndMatchManager.Instance.IsHost())
                            {
                                if (criticalActive) { target.PVPOnDamageProcess(bulletPow * criticalRatio, criticalActive); }
                                else { target.PVPOnDamageProcess(bulletPow, criticalActive); }
                            }
                            target = null;
                            onBullet = false;
                            PVPBulletPoolingManager.Instance.InsertPool(gameObject, bulletPoolNum);
                        }
                        else
                        {
                            target = null;
                            onBullet = false;
                            PVPBulletPoolingManager.Instance.InsertPool(gameObject, bulletPoolNum);
                        }
                    }
                }
                #endregion
            }
        }
    }

    // 총알이 타겟이 위치한 방향으로 기울어서 이동
    void LookTarget()
    {
        Vector2 dir = target.transform.position - transform.position;

        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
