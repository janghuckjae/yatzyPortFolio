using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryEmoticon : MonoBehaviour
{
    public int EmoticonCnt = 5;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < GameDataManager.Instance.EmoticonDatas.Length;)
        {
            int value = Random.Range(0, GameDataManager.Instance.EmoticonDatas.Length);
            if (!InGameInfoManager.Instance.EmoticonDatas.Contains(GameDataManager.Instance.EmoticonDatas[value]))
            {
                InGameInfoManager.Instance.EmoticonDatas.Add(GameDataManager.Instance.EmoticonDatas[value]);
            }
            i++;
            if (InGameInfoManager.Instance.EmoticonDatas.Count == EmoticonCnt)
            {
                break;
            }
        }
    }
    
}
