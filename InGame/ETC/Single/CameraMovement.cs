using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
//, IBeginDragHandler, IDragHandler, IEndDragHandler
public class CameraMovement : MonoBehaviour
{
    //카메라의 반너비, 반높이 값을 지닐 변수 
    private float halfWidth;
    private float halfHeight;

    //카메라 변수 
    private RectTransform moblieScreen;
    public Transform camTarget;

    private Camera cam;

    private float minCamPosX;
    private float maxCamPosX;
    private float moblieScreenY;

    //카메라 이동이 가능한 상태 
    [HideInInspector] public bool isMoveCam;

    private Vector3 firstScreenPos;

    //같이 움직일 배경들 
    [SerializeField]private RectTransform bg_Sky;
    [SerializeField]private RectTransform bg_BackObj;
    [SerializeField]private RectTransform bg_FrontObj;

         
    
    private readonly float moveSpeed = 1f;
    private void Awake()
    {
        //PVP모드에서는 작동안되게 함
        if (InGameInfoManager.Instance.isPVPMode)
        {
            this.enabled = false;
        }
    }

    void Start()
    {
        cam = InGM.Instance.mainCam;
        moblieScreen = transform.GetComponent<RectTransform>();

        //화면의 절반 높이
        halfHeight = cam.orthographicSize;
        //화면의 절반 폭
        halfWidth = halfHeight * cam.aspect;

        //카메라 포지션의 x값
        //halfWidth를 더하거나 빼주지 않으면 화면의 절반크기만큼 어긋나기때문에 아래처럼 적용해준다.
        minCamPosX = -InGM.Instance.towerPosX + halfWidth;
        maxCamPosX = InGM.Instance.towerPosX - halfWidth;

        isMoveCam = false;
        firstScreenPos = moblieScreen.position;
        moblieScreenY = moblieScreen.position.y;
    }


    //이긴 유닛이 상대 타워쪽으로 이동할 때 카메라를 그 뱡향으로 움직여 준다.(마지막 라운드 제외)
    private void LateUpdate()
    {
        if (camTarget != null && InGM.Instance.currentRound < InGM.Instance.maxRound && InGM.Instance.stageState == StageState.BattleTime)
        {
            moblieScreen.position = Vector3.Lerp(moblieScreen.position, new Vector2(camTarget.position.x, moblieScreenY), moveSpeed * Time.deltaTime);

            //배경이 모바일스크린을 따라가게끔 작업
            //뒤따라오는것
            bg_Sky.position = Vector3.Lerp(bg_Sky.position, new Vector2(camTarget.position.x, bg_Sky.position.y), moveSpeed * 0.01f * Time.deltaTime);
            bg_BackObj.position = Vector3.Lerp(bg_BackObj.position, new Vector2(camTarget.position.x, bg_BackObj.position.y), moveSpeed * 0.04f * Time.deltaTime);

            //카메라보다 빨리가야하는것 
            //bg_FrontObj.position = Vector3.Lerp(bg_FrontObj.position,new Vector2(camTarget.position.x, bg_FrontObj.position.y), moveSpeed * 0.04f * Time.deltaTime);
            //캠의 위치가 최대 사거리,최소 사거리에 왔다면 타겟을 제거 해주고 움직임을 멈춘다.
            if (moblieScreen.position.x > maxCamPosX)
            {
                moblieScreen.position = new Vector2(maxCamPosX, moblieScreenY);
                camTarget = null;
            }
            if (moblieScreen.position.x < minCamPosX)
            {
                moblieScreen.position = new Vector2(minCamPosX, moblieScreenY);
                camTarget = null;
            }
        }
    }

    public void ComBackCamPos()
    {
        camTarget = null;
        moblieScreen.anchoredPosition = firstScreenPos;
    }
}