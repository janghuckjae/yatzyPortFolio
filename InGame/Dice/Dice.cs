using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dice : MonoBehaviour
{

    //[HideInInspector] public bool onDiceChoose = false;
    public int buttonNum;
    [HideInInspector] public bool isClick = true;
    [HideInInspector] public Button myButton;
    [HideInInspector] public Image myImage;
    [HideInInspector] public GameObject readyText;
    private Animator myanim;
    WaitForSeconds delay_diceRollTime;
    void Awake()
    {
        myImage = GetComponent<Image>();
        myButton = GetComponent<Button>();
        myanim = GetComponent<Animator>();
        readyText = transform.GetChild(0).gameObject;
        delay_diceRollTime = new WaitForSeconds(DiceManager.Instance.diceRollTime);
    }

    public void RollSelf(int resultNum)
    {
        StartCoroutine(RollAnimStart(resultNum));
    }

    IEnumerator RollAnimStart(int resultNum)
    {
        DiceManager.Instance.isDiceRoll = false;
        DiceManager.Instance.isDiceUISwap = false;
        if (DiceManager.Instance.onDiceChoose[buttonNum] == false)
        {
            isClick = false;
            myanim.Play("roll");
            yield return delay_diceRollTime;
            myanim.Play("stop" + resultNum.ToString());
            yield return delay_diceRollTime;
            DiceManager.Instance.isDiceRoll = true;
            DiceManager.Instance.isDiceUISwap = true;
            isClick = true;
        }
    }

    public void ImageChange(int resultNum)
    {
        myanim.Play(string.Format("ImageChange{0}", resultNum));
    }
}
