using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatButton : MonoBehaviour
{
    public int UpSP = 50;
    public void CheatButtonClick()
    {
        if (!InGameInfoManager.Instance.isPVPMode)
        {
            if (InGM.Instance.stageState != StageState.AssignedTime)
            {
                return;
            }
            InGM.Instance.SP += UpSP;
        }
        else
        {
            if (PVPInGM.Instance.pvpStageState != PVPStageState.AssignedTime)
            {
                return;
            }
            PVPInGM.Instance.SP += UpSP;
        }

    }
}
