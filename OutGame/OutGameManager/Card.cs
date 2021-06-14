using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Card : MonoBehaviour
{
    public IconData cardData;

    public Image cardImage;
    public Text cardNameText;
    public Text cardGradeText;

    private Button myButton;
    [HideInInspector]public Animator anim;
    //데이터 값 받으면 넣어주기 
    public IconData CARDDATA
    {
        get { return cardData; }
        set {
            cardData = value;
            if (cardData != null)
            {
                cardImage.sprite = cardData.itemImage;
                cardImage.color = cardData.color;
                cardNameText.text = cardData.itemName;
                cardGradeText.text = cardData.itemGrade.ToString();
            }
        }
    }
    void Awake()
    {
        myButton = transform.GetComponent<Button>();
        anim = transform.GetComponent<Animator>();
        myButton.onClick.AddListener(CardClickEvent);
    }

    public void CardClickEvent()
    {
        //카드 회전
        anim.SetTrigger("Rotate");
        GatchaManager.Instance.rotatePoint++;
        //돌린횟수가 가챠돌리는 횟수와 다르다면 돌린 횟수 포인트를 증가시켜준다.
        if (GatchaManager.Instance.rotatePoint == GatchaManager.Instance.gatchaCnt)
        {
            //캔슬 버튼을 활성화시켜준다.
            GatchaManager.Instance.ONCANCEL = true;
        }
    }
}

