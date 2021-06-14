using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillPoolingManager : MonoBehaviour
{
    private static SkillPoolingManager _instance;

    public static SkillPoolingManager Instance // 싱글톤 구현
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(SkillPoolingManager)) as SkillPoolingManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(SkillPoolingManager)) as SkillPoolingManager;
                }
            }
            return _instance;
        }
    }
    [SerializeField]int poolNum = 0;
    Dictionary<int, Queue<GameObject>> gatchaSkillObjPool = new Dictionary<int, Queue<GameObject>>(); 
    Dictionary<int, Queue<GameObject>> gatchaSkillEffectPool= new Dictionary<int, Queue<GameObject>>();
    // 임시 오브젝트
    private GameObject s_obj;
    private GameObject e_obj;
    Quaternion rotation = Quaternion.Euler(0, 180, 0);
    [SerializeField] private GameObject skillPool;
    [SerializeField] private GameObject effectPool;

    //가챠 스킬 스크립트에서 스킬오브젝트와 스킬 이펙트을 받아 오브젝트 풀 화 시킨다.
    //isRival의 여부로 생성시 Rotation을 바꿔준다.
    public int CreateObjPool(GameObject skillObj, GameObject EffectObj,int Amount,bool isRival)
    {
        //추후에 가챠로 뽑는 스킬의 양이 더 늘어날 수 있기 때문에 (무한의 탑 등)이렇게 자료구조 구성
        poolNum++;
        if (isRival == true)
        {
            //Pool 딕셔너리 안에 PoolNum과 
            if (skillObj != null) { gatchaSkillObjPool.Add(poolNum, new Queue<GameObject>()); }
            if (EffectObj != null) { gatchaSkillEffectPool.Add(poolNum, new Queue<GameObject>()); }

            for (int i = 0; i < Amount; i++)
            {
                if (skillObj != null)
                {
                    s_obj = Instantiate(skillObj,skillPool.transform.position,rotation,skillPool.transform);
                    gatchaSkillObjPool[poolNum].Enqueue(s_obj);
                    s_obj.SetActive(false);
                }

                if (EffectObj != null)
                {
                    e_obj = Instantiate(EffectObj,effectPool.transform.position,rotation,effectPool.transform);
                    gatchaSkillEffectPool[poolNum].Enqueue(e_obj);
                    e_obj.SetActive(false);
                }
            }
            //리턴 값으로 풀링 넘버를 반환한다.
            return poolNum;
        }
        else
        {
            //Pool 딕셔너리 안에 PoolNum과 
            if (skillObj != null) { gatchaSkillObjPool.Add(poolNum, new Queue<GameObject>()); }
            if (EffectObj != null) { gatchaSkillEffectPool.Add(poolNum, new Queue<GameObject>()); }

            for (int i = 0; i < Amount; i++)
            {
                if (skillObj != null)
                {
                    s_obj = Instantiate(skillObj,skillPool.transform);
                    gatchaSkillObjPool[poolNum].Enqueue(s_obj);
                    s_obj.SetActive(false);
                }

                if (EffectObj != null)
                {
                    e_obj = Instantiate(EffectObj,effectPool.transform);
                    gatchaSkillEffectPool[poolNum].Enqueue(e_obj);
                    e_obj.SetActive(false);
                }
            }
            //리턴 값으로 풀링 넘버를 반환한다.
            return poolNum;
        }
    }



    public GameObject GetSkillObj(int poolNum)
    {
        s_obj = gatchaSkillObjPool[poolNum].Dequeue();
        //스킬 오브젝트는 활성화 하자마자 이동하기 때문에 SetActive는 Gatcha스킬 스크립트에서 설정
        return s_obj;
    }
    public GameObject GetSkillEffect(int poolNum)
    {
        e_obj = gatchaSkillEffectPool[poolNum].Dequeue();
        //스킬 이펙트도 
        return e_obj;
    }
    public void InsertSkillObj(GameObject skillObj, int poolNum)
    {
        if (gatchaSkillObjPool.Count == 0)
        {
            Destroy(skillObj);
            return;
        }
        gatchaSkillObjPool[poolNum].Enqueue(skillObj);
        skillObj.SetActive(false);
    }
    public void InsertSkillEffect(GameObject effectObj, int poolNum)
    {
        if (gatchaSkillEffectPool.Count ==0)
        {
            Destroy(effectObj);
            return;
        }
        gatchaSkillEffectPool[poolNum].Enqueue(effectObj);
        effectObj.SetActive(false);
    }
    // 스킬 오브젝트 파괴
    public void SkillInitialized()
    {
        if (poolNum != 0)
        {
            for (int i = 1; i < poolNum + 1; i++)
            {
                foreach (GameObject skillobj in gatchaSkillObjPool[i])
                {
                    Destroy(skillobj);
                }
                foreach (GameObject effectobj in gatchaSkillEffectPool[i])
                {
                    Destroy(effectobj);
                }
            }
            gatchaSkillObjPool.Clear();
            gatchaSkillEffectPool.Clear();
            poolNum = 0;
        }
    }
}
