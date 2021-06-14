using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum PopUpType
{
    Damage,
    SheildDamage,
    Critical,
    Heal,

    //기타 상태
}
public class TextPopUpManager : MonoBehaviour
{
    //텍스트 팝업들을 보관할 풀
    private GameObject textPopUpPool;
    //텍스트 메쉬 오브젝트
    [SerializeField] private GameObject textMeshObj;
    //텍스트팝업을 미리 생산할 양
    public int textMeshAmount;

    //pooling에서 체크할 것 : 이미지 , TextMeshPro
    private Queue<TextPopUp> textPopUps;


    private GameObject popUpObj;
    private TextPopUp popUp;

    [SerializeField]private float plusY;

    void Start()
    {
        
        textPopUps = new Queue<TextPopUp>();
        textPopUpPool = new GameObject("textPopUpPool");
        for (int i = 0; i < textMeshAmount; i++)
        {
            popUpObj = Instantiate(textMeshObj, textPopUpPool.transform);
            textPopUps.Enqueue(popUpObj.transform.GetComponentInChildren<TextPopUp>());
            popUpObj.SetActive(false);
        }
    }

    public void GetTextMesh(Vector2 textMeshPos,string text, PopUpType popUpType)
    {
        popUp = textPopUps.Dequeue();
        popUp.transform.parent.gameObject.SetActive(true);
        popUp.transform.parent.position = new Vector2(textMeshPos.x, textMeshPos.y + plusY);
        popUp.textMeshPro.text = text;
        popUp.anim.Play(string.Format("TextPopUp_{0}", popUpType));
    }

    public void InsertTextMesh(TextPopUp popUp)
    {
        popUp.transform.parent.position = Vector2.zero;
        popUp.transform.parent.gameObject.SetActive(false);
        textPopUps.Enqueue(popUp);
    }
}
