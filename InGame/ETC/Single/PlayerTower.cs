using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TowerState
{ 
    idle,
    walk,
    attack,
    death
}
public class PlayerTower : MonoBehaviour
{

    [Header("타워 체력 관련")]
    //타워의 HP 프로퍼티 
    //타워의 처음 체력 
    public float firstTowerHp = 20;
    //타워의 현재 체력 
    private float currentTowerHp;

    //타워의 hp 바
    [SerializeField] private Slider towerHpBar;
    [SerializeField] private Slider PlayerUIHpBar;

    
    [Header("타워 FSM")]
    public float towerSpeed = 0.1f;
    public TowerState towerState;
    private Transform parent;
    private float minX;

    public float towerPower;
    public float attackDelayTime;
    public Transform firePos;
    public GameObject myBulletObj;
    public float towerCriticalPercent;
    public float towerCriticalRaitio;

    private Bullet myBullet;
    private int myBulletPoolNum;
    private WaitForSeconds delayTime_attackDelayTime;
    public float TOWERHP
    {
        get { return currentTowerHp; }
        set {
            currentTowerHp = value;
            towerHpBar.value = currentTowerHp;
            PlayerUIHpBar.value = currentTowerHp;
            //타워의 체력이 달으면 호스트가 체력정보를 보내준다.
            //타워의 HP가 다달으면
            if (currentTowerHp <= 0)
            {
                InGM.Instance.GameEndEvent();
                InGM.Instance.stageState = StageState.Lose;
                InGameUIManager.Instance.loseUI.SetActive(true);
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
            PlayerUIHpBar.maxValue = firstTowerHp;
            PlayerUIHpBar.value = firstTowerHp;

            towerState = TowerState.idle;
            parent = transform.parent;
        }
    }

    private void Start()
    {
        minX = InGM.Instance.playAreaMinX;
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
                   new Vector2(minX, transform.position.y), towerSpeed * Time.deltaTime);
            }
            //타워 공격 시간에만 공격 가능
            else if (towerState == TowerState.attack && InGM.Instance.stageState== StageState.TowerAttackTime)
            {
                TowerAttackProcess();
                yield return delayTime_attackDelayTime;
                
            }
            yield return null;
        }
    }
    public void TowerDamagedProcess(float damage)
    {
        TOWERHP -= damage;
    }

    public void TowerAttackProcess()
    {
        //적이 이겼을 때 (아군 타워가 공격)
        if (InGM.Instance.activeFriend.Count == 0)
        {
            if (InGM.Instance.activeEnemy.Count != 0)
            {
                GameObject obj = BulletPoolingManager.Instance.GetPool(myBulletPoolNum);
                obj.transform.position = firePos.position;
                myBullet = obj.transform.GetComponent<Bullet>();
                myBullet.target = InGM.Instance.activeEnemy[0];
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
    
    void SetMyBullet()
    {
        delayTime_attackDelayTime = new WaitForSeconds(attackDelayTime);
        myBullet = myBulletObj.transform.GetComponent<Bullet>();
        myBulletPoolNum = myBullet.bulletPoolNum;
        
    }
   
}

