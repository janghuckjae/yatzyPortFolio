using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyTower : MonoBehaviour
{
    [Header("타워 체력 관련")]
    //타워의 HP 프로퍼티 
    //타워의 처음 체력 
    public float firstTowerHp = 20;
    //타워의 현재 체력 
    private float currentTowerHp;

    //타워의 hp 바
    [SerializeField] private Slider towerHpBar;
    [SerializeField] private Slider rivalUIHpBar;
    

    [Header("타워 FSM")]
    public float towerSpeed = 0.1f;
    public TowerState towerState;
    private Transform parent;
    private float maxX;

    public float towerPower;
    public float attackDelayTime;
    public float towerCriticalRaitio;
    public float towerCriticalPercent;
    public Transform firePos;
    public GameObject myBulletObj;

    private Bullet myBullet;
    private int myBulletPoolNum;
    private WaitForSeconds delayTime_attackDelayTime;
    private bool winCheck;
    public float TOWERHP
    {
        get { return currentTowerHp; }
        set
        {
            currentTowerHp = value;
            towerHpBar.value = currentTowerHp;
            rivalUIHpBar.value = currentTowerHp;
            //타워의 HP가 다달으면
            if (currentTowerHp <= 0)
            {
                InGM.Instance.GameEndEvent();
                if (InGameInfoManager.Instance.isPVPMode)
                {
                    //승리
                    if (winCheck)
                    {
                        if (BackEndMatchManager.Instance.IsHost())
                        { 
                            BackEndMatchManager.Instance.MatchGameOver(InGameInfoManager.Instance.mySessionID, InGameInfoManager.Instance.rivalSessionID, false);
                            winCheck = false;
                        }
                    }
                }
                 
                InGM.Instance.stageState = StageState.Victory;
                InGameUIManager.Instance.winUI.SetActive(true);
            }
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        //PVP모드에서는 작동안되게 함
        if (InGameInfoManager.Instance.isPVPMode)
        {
            this.enabled = false;
        }
        else
        {
            TOWERHP = firstTowerHp;
            towerHpBar.maxValue = firstTowerHp;
            towerHpBar.value = firstTowerHp;
            rivalUIHpBar.maxValue = firstTowerHp;
            rivalUIHpBar.value = firstTowerHp;
            towerState = TowerState.idle;
            parent = transform.parent;
            winCheck = true;
        }
        
    }

    private void Start()
    {
        maxX = InGM.Instance.playAreaMaxX;
        
        SetMyBullet();
        StartCoroutine(TowerFSM());
    }
    IEnumerator TowerFSM()
    {
        while (true)
        {
            if (towerState == TowerState.walk)
            {

                parent.position = Vector2.MoveTowards(parent.position,
                    new Vector2(maxX, transform.position.y), towerSpeed * Time.deltaTime);
            }
            //타워 공격 시간에만 공격 가능
            else if (towerState == TowerState.attack && InGM.Instance.stageState == StageState.TowerAttackTime)
            {
                TowerAttackProcess();
                yield return delayTime_attackDelayTime;
                
            }
            yield return null;
        }
    }

    public void TowerAttackProcess()
    {
        //아군이 이겼을 때(적 타워가 공격)
        if (InGM.Instance.activeEnemy.Count == 0)
        {
            if (InGM.Instance.activeFriend.Count != 0)
            {
                GameObject obj = BulletPoolingManager.Instance.GetPool(myBulletPoolNum);
                obj.transform.position = firePos.position;

                myBullet = obj.transform.GetComponent<Bullet>();
                myBullet.target = InGM.Instance.activeFriend[0];
                myBullet.onBullet = true;
                myBullet.bulletPow = towerPower;

                float randomValue = Random.Range(0f, 100f);
                if (randomValue <= towerCriticalPercent)
                {
                    myBullet.criticalActive = true;
                }
                else { myBullet.criticalActive = false; }
                myBullet.criticalRatio = towerCriticalRaitio;
            }
        }
    }
    public void TowerDamagedProcess(float damage)
    {
        TOWERHP -= damage;
    }
    void SetMyBullet()
    {
        delayTime_attackDelayTime = new WaitForSeconds(attackDelayTime);
        myBullet = myBulletObj.transform.GetComponent<Bullet>();
        myBulletPoolNum = myBullet.bulletPoolNum;
        
    }
    
}
