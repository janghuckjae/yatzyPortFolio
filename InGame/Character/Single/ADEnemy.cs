using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ADEnemy : Enemy
{
    public GameObject bullet;
    private Bullet bulletInfo;
    //발사 위치 
    [SerializeField]
    private Transform firePos;

    private int bulletPoolNum;

    private GameObject obj;
    protected override void OnEnable()
    {
        base.OnEnable();
        FindBulletInfo();
    }
    //공격 타이밍

    protected override void OnAttackProcess()
    {
        //총알을 풀에서 가져온 후 발사지점에 위치 시키기  
        obj = BulletPoolingManager.Instance.GetPool(bulletPoolNum);
        obj.transform.position = firePos.position;
        bulletInfo = obj.transform.GetComponent<Bullet>();
        bulletInfo.target = nearUnit;
        bulletInfo.enemyTowerPos = playerTowerPos;
        bulletInfo.onBullet = true;
        //크리티컬 여부 
        float randomValue = Random.Range(0f, 100f);

        if (randomValue <= criticalPercent)
        {
            bulletInfo.criticalActive = true;
        }
        else { bulletInfo.criticalActive = false; }

    }

    //오브젝트 풀의 총알 배열중 어떤 총알인지 넘버링을 찾아주는 함수
    void FindBulletInfo()
    {
        bulletInfo = bullet.transform.GetComponent<Bullet>();
        bulletPoolNum = bulletInfo.bulletPoolNum;
        bulletInfo.bulletPow = myDamage;
        bulletInfo.criticalRatio = criticalRatio;
    }
}
