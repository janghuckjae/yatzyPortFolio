using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public enum UIEffectKind
{ 
    Victory,
    Lose,
    Draw,
    RoundUp
}
public class UIEffect : MonoBehaviour
{
    [Header("승패 관련")]
    [SerializeField] private GameObject roundWinUI;
    [SerializeField] private GameObject roundLoseUI;
    [SerializeField] private GameObject roundDrawUI;

    [Header("라운드 업 관련")]
    [SerializeField] private RectTransform roundUpUI;
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI roundNumText;
    [SerializeField] private float roundUpUiMoveTime;
    [SerializeField] private float bgMoveTime;
    [SerializeField] private Ease roundUpUiEase;
    [SerializeField] private Ease diceUIUpEase;

    //배경 RectTransForm
    RectTransform bgRect;
    //PlayFuncArea RectTransForm
    RectTransform funcAreaRect;
    private float originBGY;
    private float originFuncAreaY;


    Sequence roundUpSequence;
    Sequence goDiceTimeSequence;

    [SerializeField]private GameObject currentResultObj;
   
    private void Start()
    {
        DOTween.defaultAutoPlay = AutoPlay.None;
        //배경, 다이스UI 데이터 캐싱
        bgRect = InGameUIManager.Instance.pvpBackGround.rectTransform;
        funcAreaRect = InGameUIManager.Instance.funcAreaRect;

        originBGY = bgRect.anchoredPosition.y;
        originFuncAreaY = funcAreaRect.anchoredPosition.y;

        roundUpSequence = DOTween.Sequence()
            .SetAutoKill(false)
            .Append(roundUpUI.DOAnchorPosX(0, roundUpUiMoveTime).SetEase(roundUpUiEase))
            .Append(roundUpUI.DOAnchorPosX(-1600, roundUpUiMoveTime).SetEase(roundUpUiEase))
            .Join(background.DOFade(0, roundUpUiMoveTime ).SetEase(roundUpUiEase))
            .OnComplete(() =>
            {
                background.gameObject.SetActive(false);
                roundUpUI.gameObject.SetActive(false);
                PVPCharManager.Instance.SetFriend();
                RivalManager.Instance.SetRival();

            });

        //한 라운드가 끝나고 주사위 시간으로 갈때 배경,주사위 UI가 아래에서 위 방향으로 이동
        goDiceTimeSequence = DOTween.Sequence()
            .SetAutoKill(false)
            .Append(funcAreaRect.DOAnchorPosY(originFuncAreaY, bgMoveTime).SetEase(diceUIUpEase))
            .Join(bgRect.DOAnchorPosY(originBGY,bgMoveTime).SetEase(diceUIUpEase));
    }
    public void OnUIEffect(UIEffectKind uIEffect)
    {
        roundUpSequence.Rewind();
        background.gameObject.SetActive(true);
        switch (uIEffect)
        {
            case UIEffectKind.Victory:
                RoundResultEffect(roundWinUI);
                break;
            case UIEffectKind.Lose:
                RoundResultEffect(roundLoseUI);
                break;
            case UIEffectKind.Draw:
                RoundResultEffect(roundDrawUI);
                break;
            case UIEffectKind.RoundUp:
                RoundEffect();
                break;
        }
    }
    public void GoDiceTimeEffect()
    {
        bgRect.anchoredPosition = Vector2.zero;
        funcAreaRect.anchoredPosition = new Vector2(funcAreaRect.anchoredPosition.x, funcAreaRect.anchoredPosition.y - 1500);
        funcAreaRect.gameObject.SetActive(true);
        goDiceTimeSequence.Rewind();
        goDiceTimeSequence.Restart();
    }


    private void RoundEffect()
    {
        roundUpUI.gameObject.SetActive(true);
        if (currentResultObj != null)
        {
            currentResultObj.SetActive(false);
        }
        roundNumText.text = string.Format("{0}", PVPInGM.Instance.currentRound);
        roundUpSequence.Restart();
    }
    private void RoundResultEffect(GameObject resultObj)
    {
        resultObj.SetActive(true);
        currentResultObj = resultObj;
    }
  
}

