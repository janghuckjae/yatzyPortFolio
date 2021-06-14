using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuLoadingSceneManager : MonoBehaviour
{
    public static string nextScene;

    [SerializeField] private Image lodingBar;
    [SerializeField] private Image lodingImage;
    [SerializeField] private Text tipText;
    [Header("뿌려줄 정보")]
    //랜덤으로 뿌려줄 이미지
    [SerializeField] private Sprite[] wallPaper;
    [SerializeField] private string[] tips;


    // Start is called before the first frame update
    void Start()
    {
        ChangeLoadingInfo();
        StartCoroutine(LoadNextScene());
    }
  
    public static void LoadingtoNextScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }

    IEnumerator LoadNextScene()
    {
        yield return null;
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);

        op.allowSceneActivation = false;

        float timer = 0.0f;
        while (!op.isDone)
        {
            yield return null;
            timer += Time.deltaTime;
            if (op.progress < 0.9f)
            {
                lodingBar.fillAmount = Mathf.Lerp(lodingBar.fillAmount, op.progress, timer);
                if (lodingBar.fillAmount >= op.progress)
                {
                    timer = 0f;
                }
            }
            else
            {
                lodingBar.fillAmount = Mathf.Lerp(lodingBar.fillAmount, 1f, timer);
                if (lodingBar.fillAmount == 1.0f)
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }
    // 로딩씬이 시작 할 때 이미지와 설명 창의 내용을 바꾸어준다..
    void ChangeLoadingInfo()
    {
        //
        int randomWallPaperNum = Random.Range(0, wallPaper.Length);
        //int randomTipNum = Random.Range(0, tips.Length + 1);

        //tipText.text = tips[randomTipNum];
        lodingImage.sprite = wallPaper[randomWallPaperNum];

    }
}
