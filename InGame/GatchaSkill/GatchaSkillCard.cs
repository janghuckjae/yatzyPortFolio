using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GatchaSkillCard : MonoBehaviour
{
    public Image gatchaSkillImg;
    public Text gatchaSkillInfoText;
    public Text gatchaSkillNameText;
    [SerializeField] private GameObject backImgObj;
    [SerializeField]private GameObject selectEffect;
    public bool isClicked;

    private GatchaSkill currentSkill;
    //가챠스킬 프로퍼티
    public GatchaSkill GATCHASKILL 
    {
        get { return currentSkill; }
        set{
            currentSkill = value;
            gatchaSkillImg.sprite = currentSkill.gatchaSkillInfo.skillImg;
            gatchaSkillInfoText.text = currentSkill.gatchaSkillInfo.skillInfo;
            gatchaSkillNameText.text = currentSkill.gatchaSkillInfo.skillName;
            selectEffect.SetActive(false);
            isClicked = false;
        }
    }

    private Button gatchaBtn;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = transform.GetComponent<Animator>();
        gatchaBtn = transform.GetComponent<Button>();
        gatchaBtn.onClick.AddListener(() => SelectGatchaSkill());
    }
    public void RotateCard()
    {
        anim.Play("SkillCardRotate", -1,0f);
    }

    public void SelectGatchaSkill()
    {
        if (SkillGatchaManager.Instance.gatchaTryPoint == 1)
        {
            return;
        }
        //만약 해당 버튼을 누르면 
        for (int i = 0; i < SkillGatchaManager.Instance.gatchaSkillCards.Length; i++)
        {
            //임시로 선택 효과 제거 
            SkillGatchaManager.Instance.gatchaSkillCards[i].selectEffect.SetActive(false);
            isClicked = false;
        }
        //선택
        isClicked = true;
        //선택 효과
        selectEffect.SetActive(true);

        //임시 선택 마법에 값 할당
        SkillGatchaManager.Instance.ApplyGatchaSkill(currentSkill);
    }

    public void CardInitialized()
    {
        isClicked = false;
        selectEffect.SetActive(false);
        backImgObj.SetActive(true);
        currentSkill = null;
    }
}
