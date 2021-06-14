using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    //타겟
    public Transform target;
    public int bulletPoolNum;

    [HideInInspector] public Transform enemyTowerPos;
    [HideInInspector]public bool onBullet = false;
    //크리티컬 발동여부
    [HideInInspector]public bool criticalActive;
    //크리티컬 배수 
    [HideInInspector] public float criticalRatio =1f;
    [HideInInspector]public float bulletPow;
    [SerializeField]private float bulletSpeed;
    [SerializeField] private float attackDistance;



    //임시 저장할 Enemy스크립트 
    private Enemy enemyStat;
    private Charactor friendStat;
    private void Awake()
    {
        //PVP모드가 일 때 꺼준다.   
        if (InGameInfoManager.Instance.isPVPMode)
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
                if (target == null)
                {
                    target = null;
                    onBullet = false;
                    BulletPoolingManager.Instance.InsertPool(gameObject, bulletPoolNum);
                    yield break;
                }
                //적방향으로 바라보게 함
                LookTarget();

                //적에게 이동
                transform.position = Vector2.MoveTowards(transform.position, target.position, bulletSpeed * Time.deltaTime);

                
                #region 만약 타겟의 Tag가 Enemy일 때 
                if (target.CompareTag("Enemy"))
                {
                    float distance = (target.position - transform.position).sqrMagnitude;
                    //만약 타겟과의 거리가 공격거리 안쪽으로 들어왔을 때  공격한다.
                    if (distance <= attackDistance)
                    {
                        enemyStat = target.GetComponent<Enemy>();

                        if (enemyStat.enemyState != EnemyState.death)
                        {
                            if (criticalActive) { enemyStat.OnDamageProcess(bulletPow * criticalRatio, true); }
                            else { enemyStat.OnDamageProcess(bulletPow, false); }

                            target = null;
                            onBullet = false;
                            BulletPoolingManager.Instance.InsertPool(gameObject, bulletPoolNum);
                        }
                        else
                        {
                            target = null;
                            onBullet = false;
                            BulletPoolingManager.Instance.InsertPool(gameObject, bulletPoolNum);
                        }
                    }
                }
                #endregion
                #region 만약 타겟의 Tag가 Player일 때 
                else if (target.CompareTag("Player"))
                {
                    float distance = (target.position - transform.position).sqrMagnitude;
                    //만약 타겟과의 거리가 공격거리 안쪽으로 들어왔을 때  공격한다.
                    if (distance <= attackDistance)
                    {
                        friendStat = target.GetComponent<Charactor>();

                        if (friendStat.charState != CharState.death)
                        {
                            if (criticalActive) { friendStat.OnDamageProcess(bulletPow * criticalRatio, criticalActive); }
                            else { friendStat.OnDamageProcess(bulletPow, criticalActive); }

                            target = null;
                            onBullet = false;
                            BulletPoolingManager.Instance.InsertPool(gameObject, bulletPoolNum);
                        }
                        else
                        {
                            target = null;
                            onBullet = false;
                            BulletPoolingManager.Instance.InsertPool(gameObject, bulletPoolNum);
                        }
                    }
                }
                #endregion
                #region 만약 타겟이 적 타워라면 
                else if (target == enemyTowerPos)
                {
                    float distance = (target.position - transform.position).sqrMagnitude;
                    //만약 타겟과의 거리가 공격거리 안쪽으로 들어왔을 때  공격한다.
                    if (distance <= attackDistance)
                    {
                        // 크리티컬 일 때 
                        if (criticalActive)
                        {
                            //상대 타워가 에너미 타워 일때 
                            if (enemyTowerPos.CompareTag("EnemyTower"))
                            {
                                InGM.Instance.towerEnemy.TowerDamagedProcess(bulletPow * criticalRatio);
                            }
                            //에너미 타워가 아군 타워 일때 
                            else { InGM.Instance.towerPlayer.TowerDamagedProcess(bulletPow * criticalRatio); }
                        }
                        else
                        {
                            if (enemyTowerPos.CompareTag("EnemyTower"))
                            {
                                InGM.Instance.towerEnemy.TowerDamagedProcess(bulletPow);
                            }
                            else { InGM.Instance.towerPlayer.TowerDamagedProcess(bulletPow); }
                        }
                        target = null;
                        onBullet = false;
                        BulletPoolingManager.Instance.InsertPool(gameObject, bulletPoolNum);
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
