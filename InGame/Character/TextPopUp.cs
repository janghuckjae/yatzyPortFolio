using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class TextPopUp : MonoBehaviour
{
    [HideInInspector] public Animator anim;
    [HideInInspector] public TextMeshPro textMeshPro;
    //[HideInInspector] public SpriteRenderer spriteRenderer;


    // Start is called before the first frame update
    void Awake()
    {
        anim = transform.GetComponent<Animator>();
        textMeshPro = transform.GetComponent<TextMeshPro>();
      //  spriteRenderer = transform.GetComponentInChildren<SpriteRenderer>();
    }

    public void PopUpEventEnd()
    {
        InGameUIManager.Instance.textPopUpManager.InsertTextMesh(this);
    }
}
