using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using BackEnd.Tcp;
using Protocol;
using Battlehub.Dispatcher;

[System.Serializable]
public struct StageImgStruct
{
    //스테이지 정보에서 보여줄 샘플 이미지(한꺼번에 합쳐져있는 이미지)
    public Sprite sampleImg;
    public Sprite bg_FrontObj;
    public Sprite bg_GameField;
    public Sprite bg_BackObj;
    public Sprite bg_Sky;
}
public class GameDataManager : MonoBehaviour
{
    private static GameDataManager _instance;
    public static GameDataManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(GameDataManager)) as GameDataManager;
                if (!_instance)
                {
                    GameObject _container = new GameObject();
                    _container.name = "Container";
                    _instance = _container.AddComponent(typeof(GameDataManager)) as GameDataManager;
                }
            }
            return _instance;
        }
    }
    [Header("캐릭터 아이콘 데이터")]
    public IconData[] charIconDatas;
    [Header("이모티콘 데이터")]
    public EmoticonData[] EmoticonDatas;
    [Header("가챠 스킬")]
    public List<GatchaSkill> gatchaSkills;

    //public PieceIconData[] pieceIconDatas;
    //public RuneIconData[] runeIconDatas;
    [System.Serializable]
    public struct StageStruct
    {
        //스테이지 별 정보 
        public StageData[] stageDatas;
    }
    [Header("PVP시 MaxRound")]
    public int pvpMaxRound;
    [Header("챕터-스테이지별 데이터배열")]
    public StageStruct[] arrStageStruct;
    public GameObject[] enemyPrefabs;
    public StageImgStruct[] inGameBGImgs;
    
    public Sprite[] pvpGameBGImgs;
   

    private EnemyData[] enemyDatas;
    //캐릭터 데이터 테이블 이름
    private const string charFileId = "15765";





    //임시 Chardata를 담을 변수
    CharData chardata;
    //캐릭터 데이터의 유니크 넘버를 분해하여 값을 읽는다.
    private int[] crackValue;
    //개발 단계에서는 Asset에서 XML파일을 긁어오고 출시하고 나서는 뒤끝(차트 관리)에 등록하고 사용한다.
    public void GetCharctorChartContents()
    {
        List<Dictionary<string, object>> data = CSVReader.Read("CharDataSheet");

        for (int i = 0; i < data.Count; i++)
        {
            for (int j = 0; j < charIconDatas.Length; j++)
            {
                if (data[i]["uniqueNumber"].ToString() == charIconDatas[j].uniqueNumber)
                {
                    SetCharactorInfo(data[i], j);
                }
            }
        }
    }
    //데이터 세팅
    private void SetCharactorInfo(Dictionary<string, object> data,int j)
    {
        crackValue = new int[5];
        chardata = charIconDatas[j].deckPrefab.transform.GetComponent<PVPCharactor>().charData;

        for (int i = 0; i < crackValue.Length; i++)
        {
            var value = data["uniqueNumber"].ToString()[i];
            crackValue[i] = (int)char.GetNumericValue(value);
            switch (i)
            {
               
                case 2:
                    charIconDatas[j].charIconType = (CharIconType)(crackValue[i]);
                    break;
                case 3:
                    //charIconDatas[j].itemGrade = (ItemGrade)crackValue[i];
                    break;
            }
        }

        //코스트 할당
        charIconDatas[j].itemSP = 5 * ((int)charIconDatas[j].itemGrade + 1);
        charIconDatas[j].itemName = data["charName"].ToString();
        charIconDatas[j].itemInfo = data["charInfo"].ToString();
        charIconDatas[j].gatchaWeight = int.Parse(data["gatchaWeight"].ToString());

        chardata.hp = float.Parse(data["hp"].ToString());
        chardata.power = float.Parse(data["power"].ToString()); 
        chardata.speed = float.Parse(data["speed"].ToString()); 
        chardata.attackDistance = float.Parse(data["attackDistance"].ToString()); 
        chardata.attackSpeed = float.Parse(data["attackSpeed"].ToString()); 
        chardata.criPercentage = float.Parse(data["criticalPercentage"].ToString());
        chardata.criRatio = float.Parse(data["criticalMultiple"].ToString());

        
        
    }

    public void GetEnemyChartContents()
    {
        List<Dictionary<string, object>> data = CSVReader.Read("EnemyDataSheet");
        enemyDatas = new EnemyData[enemyPrefabs.Length];
        //에너미 데이터 값 캐싱 
        for (int k = 0; k < enemyPrefabs.Length; k++)
        {
            enemyDatas[k] = enemyPrefabs[k].transform.GetComponent<Enemy>().enemyData;
        }
        for (int i = 0; i < data.Count; i++)
        {
            for (int j = 0; j < enemyDatas.Length; j++)
            {
                //고유 넘버가 같은 데이터에 값할당 
                if (data[i]["uniqueNumber"].ToString() == enemyDatas[j].uniqueNumber)
                {
                    SetEnemyInfo(data[i], j);
                }
            }
        }
    }
    private void SetEnemyInfo(Dictionary<string, object> data, int j)
    {
        //중간의 직업 값을 가져온다.
        var value = data["uniqueNumber"].ToString()[2];
        enemyDatas[j].enemyType = (CharIconType)((int)char.GetNumericValue(value)+1);
        enemyDatas[j].enemyName = data["devilName"].ToString();
        enemyDatas[j].hp = float.Parse(data["hp"].ToString());
        enemyDatas[j].power = float.Parse(data["power"].ToString());
        enemyDatas[j].speed = float.Parse(data["speed"].ToString());
        enemyDatas[j].attackDistance = float.Parse(data["attackDistance"].ToString());
        enemyDatas[j].attackSpeed = float.Parse(data["attackSpeed"].ToString());
        enemyDatas[j].criticalPercent = float.Parse(data["criticalPercentage"].ToString());
        enemyDatas[j].criticalMultiple = float.Parse(data["criticalMultiple"].ToString());
    }

    public void GetEmoticonChartContents()
    {
        List<Dictionary<string, object>> data = CSVReader.Read("EmoticonDataSheet");
        for (int i = 0; i < data.Count; i++)
        {
            for (int j = 0; j < EmoticonDatas.Length; j++)
            {
                if (data[i]["uniqueNumber"].ToString() == EmoticonDatas[j].uniqueNumber)
                {
                    EmoticonDatas[j].EmoticonName = data[i]["EmoticonName"].ToString();
                }
            }
        }
    }
    





        //뒤끝 용 테이블 가져오는 함수
        //public void GetCharctorChartContents1()
        //{
        //    Backend.Chart.GetChartContents(charFileId, callback =>
        //    {
        //        if (!callback.IsSuccess())
        //        {
        //            Debug.LogError("캐릭터 테이블 ID가 다릅니다.");
        //            return;
        //        }
        //        LitJson.JsonData rows = callback.GetReturnValuetoJSON()["rows"];

        //        //테이블 
        //        for (int i = 0; i < rows.Count; i++)
        //        {
        //            for (int j = 0; j < charIconDatas.Length; j++)
        //            {
        //                //CharInfo info = new CharInfo();
        //                //만약 고유번호가 캐릭터 데이터의 유니크 넘버와 같다면?
        //                if (rows[i]["uniqueNumber"][0].ToString() == charIconDatas[j].uniqueNumber)
        //                {

        //                }
        //            }
        //        }
        //    });
        //}
    }


