using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlResolution : MonoBehaviour
{
    public RectTransform[] rectControlResolution;

    // Start is called before the first frame update
    void Awake()
    {
        if (Screen.height < 2 * Screen.width) // 2보다 작으면, 대부분 = 1080x1920, 1440x2560
        {
            for (int i1 = 0; i1 < rectControlResolution.Length; i1++)
            {
                rectControlResolution[i1].localScale = new Vector3(0.9f, 0.9f, 1.0f);
                if (i1 == 2) // PanelDice인 경우
                {
                    rectControlResolution[i1].anchoredPosition += new Vector2(0.0f, 60.0f);
                }
            }
        }
    }
    
}
