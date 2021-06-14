using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Icon : MonoBehaviour
{
    //배정받을 데이터 변수
    public IconData myData;
    //해금 여부를 받아줄 변수 
    [HideInInspector] public bool isRelease;
    public int charSlotNum;

    public int charDeckNum;
    //나의 스프라이트 랜더러 
    private Image myRenderer;
    //나의 버튼 컴포넌트
    private Button myBtn;

    public bool isClick = false;

    [HideInInspector] public bool isDeckIn = false;
    
    void Awake()
    {
        myRenderer = transform.GetComponent<Image>();
        myBtn = transform.GetComponent<Button>();
        myBtn.onClick.AddListener(MyBtnClick);

    }
    //나를 클릭할 시(해금이 된 상태만 클릭 가능)
    //내가 가지고 있는 이미지를 SelectImg에 적용해준다.
    //리소스가 딱히 없으므로 임시로 Color값도 같이 넘겨준다.
    //내가 가지고있는 버튼 클릭 이벤트를 각 버튼의 Onclick.AddListener에 할당해준다.
    void MyBtnClick()
    {
        
        if (!isClick && !LobbyUI.Instance.errorObject.activeSelf)
        {
            if (InventoryManager.Instance.recentIcon != null)
            {
                InventoryManager.Instance.recentIcon.isClick = false;
                InventoryManager.Instance.recentIcon = this;
            }
            else { InventoryManager.Instance.recentIcon = this; }

            isClick = true;
            //선택 UI 켜주기 
            InventoryManager.Instance.selectWindow.SetActive(true);
            //덱 이미지 적용
            InventoryManager.Instance.selectImg.sprite = myData.itemImage;
            //해금이 됬을 때 
            if (isRelease)
            {
                //버튼이벤트 초기화
                InventoryManager.Instance.UseBtn.onClick.RemoveAllListeners();
                InventoryManager.Instance.RemoveBtn.onClick.RemoveAllListeners();
                InventoryManager.Instance.infoBtn.onClick.RemoveAllListeners();
                //각각의 버튼에 버튼 이벤트 할당
                InventoryManager.Instance.UseBtn.onClick.AddListener(UseBtnClick);
                InventoryManager.Instance.RemoveBtn.onClick.AddListener(RemoveBtnClick);
                InventoryManager.Instance.infoBtn.onClick.AddListener(InfoBtnClick);


                //임시 : 색 적용
                InventoryManager.Instance.selectImg.color = myData.color;
                //만약 덱 안에 들어가있으면 제거버튼 활성화,사용버튼 비활성화 해주고 인벤토리 안에있다면 반대로해준다.
                //인벤토리 안에 있을 때 
                if (!isDeckIn)
                {
                    //현재 데이터 양과 덱의 총 양을 비교하여 USE,Remove버튼의 활성화 여부를 고려한다.
                    DeckFullTest(DeckManager.Instance.myDeck);
                }
                else
                {
                    InventoryManager.Instance.UseBtn.gameObject.SetActive(false);
                    InventoryManager.Instance.RemoveBtn.gameObject.SetActive(true);
                }
            }
            //해금이 안됬을 때 
            else
            {
                //임시 : 색 적용
                InventoryManager.Instance.selectImg.color = myData.lockColor;
                //Use 버튼 Remove버튼 꺼주기
                InventoryManager.Instance.UseBtn.gameObject.SetActive(false);
                InventoryManager.Instance.RemoveBtn.gameObject.SetActive(false);
            }
            //위치 적용
            InventoryManager.Instance.selectWindow.transform.position = new Vector2(transform.position.x, transform.position.y + InventoryManager.Instance.selectImgY);
        }
        else
        {
            isClick = false;
            InventoryManager.Instance.selectWindow.SetActive(false);
        }

        
    }
    //use버튼을 클릭할시
    //덱에다가 넣어준다.
    public void UseBtnClick()
    {
        
        isClick = false;
        //덱에서 넣을 떄 내 정보를 인벤토리 매니저에있는 리스트에서 제외해준다.
        InventoryManager.Instance.DeckIn(this);
        InventoryManager.Instance.selectWindow.SetActive(false);
    }
    //제거 버튼을 클릭할시 
    void RemoveBtnClick()
    {
        isClick = false;
        //덱에서 뺼때 내정보를 인벤토리 매니저에 있는 리스트에 추가해준다.
        InventoryManager.Instance.DeckOut(this);
        InventoryManager.Instance.selectWindow.SetActive(false);
    }
    //Info 버튼을 클릭할 시
    void InfoBtnClick()
    {
        InventoryManager.Instance.selectWindow.SetActive(false);
        isClick = false;
    }
    public void DataSetEvent()
    {
        myRenderer.sprite = myData.itemImage;
        if (isRelease)
        {
            myRenderer.color = myData.color;
        }
        else
        {
            myRenderer.color = myData.lockColor;
        }
    }
    //현재 데이터 양과 덱의 총 양을 비교하여 USE,Remove버튼의 활성화 여부를 고려한다.
    void DeckFullTest(IconData[] datas)
    {
        if (!datas.Contains(null))
        {
            InventoryManager.Instance.UseBtn.gameObject.SetActive(false);
            InventoryManager.Instance.RemoveBtn.gameObject.SetActive(false);
        }
        else
        {
            InventoryManager.Instance.UseBtn.gameObject.SetActive(true);
            InventoryManager.Instance.RemoveBtn.gameObject.SetActive(false);
        }
    }
}
