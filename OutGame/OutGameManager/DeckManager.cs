using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    private static DeckManager _instance;
    public static DeckManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(DeckManager)) as DeckManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(DeckManager)) as DeckManager;
                }
            }
            return _instance;
        }
    }


    [Header("덱 창관련")]
    //덱
    public Transform[] charDeckArea;
    //인벤토리
    public GameObject charSlotParent;

    public IconData[] myDeck;
    public bool[] isCharDeck;


    private void Start()
    {
        isCharDeck = new bool[charDeckArea.Length];
        myDeck = new IconData[charDeckArea.Length];
        isCharDeck.Initialize();
    }
    //덱 
    public void RegistDeckData()
    {
        InGameInfoManager.Instance.charactorDatas = myDeck.ToList();
    }
}
