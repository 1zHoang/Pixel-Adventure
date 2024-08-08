using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ui_Credits : MonoBehaviour
{
    private UI_FadeEffect fadeEffect;
    [SerializeField] private RectTransform rectT;
    [SerializeField] private float scroolSpeed = 200;
    [SerializeField] private float offScreenPosition = 1800;

    [SerializeField] private string mainMenuSceneName = "MainMenu";
    private bool creditsSkipped;

    private void Awake()
    {
        fadeEffect = GetComponentInChildren<UI_FadeEffect>();
        fadeEffect.ScreenFade(0, 2);
    }
    private void Update()
    {
        rectT.anchoredPosition += Vector2.up * scroolSpeed * Time.deltaTime;

        if (rectT.anchoredPosition.y > offScreenPosition)
            GoToMainMenu();
    }

    public void SkipCredits()
    {
        if(creditsSkipped == false)
        {
            scroolSpeed *= 10;
            creditsSkipped = true;
        }
        else
        {
            GoToMainMenu();
        }
    }

    private void GoToMainMenu() => fadeEffect.ScreenFade(1, 1, GoToMainMenu);

    private void SwitchToMenuScene()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
