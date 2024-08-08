using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private inGameUI inGameUI;

    [Header("Level Managment")]
    [SerializeField] private float levelTimer;
    [SerializeField] private int currentLevelIndex;
    private int nextLevelIndex;
    
    [Header("Fruit Management")]
    public bool fruitsAreRandom;
    public int fruitsCollected;
    public int fruitsTotal;
    public Transform fruitsParent;

    [Header("Checkpoints")]
    public bool canReactivate;

    [Header("Managers")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private SkinManager skinManager;
    [SerializeField] private DifficultyManager difficultyManager;
    [SerializeField] private ObjectCreator objectCreator;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        nextLevelIndex = currentLevelIndex + 1;
        inGameUI = inGameUI.instance;
        
        CollectFruitsInfo();
        CreateManagersIfNeeded();
    }

    private void Update()
    {
        levelTimer = Time.time;

        inGameUI.UpdateTimerUI(levelTimer);
    }

    private void CreateManagersIfNeeded()
    {
        if (AudioManager.instance == null)
            Instantiate(audioManager);

        if (PlayerManager.instance == null)
            Instantiate(playerManager);

        if (SkinManager.instance == null)
            Instantiate(skinManager);

        if (DifficultyManager.instance == null)
            Instantiate(difficultyManager);

        if (ObjectCreator.instance == null)
            Instantiate(objectCreator);
    }

    private void CollectFruitsInfo()
    {
        Fruit[] allFruits = FindObjectsByType<Fruit>(FindObjectsSortMode.None);
        fruitsTotal = allFruits.Length;

        inGameUI.instance.UpdateFruitUI(fruitsCollected, fruitsTotal);

        PlayerPrefs.SetInt("Level" + currentLevelIndex + "TotalFruits", fruitsTotal);
    }

    [ContextMenu("Parent All Fruits")]
    private void ParentAllTheFruits()
    {
        if (fruitsParent == null)
            return;

        Fruit[] allFruits = FindObjectsByType<Fruit>(FindObjectsSortMode.None);

        foreach (Fruit fruit in allFruits)
        {
            fruit.transform.parent = fruitsParent;
        }
    }

    
    public void AddFruit()
    {
        fruitsCollected++;
        inGameUI.UpdateFruitUI(fruitsCollected, fruitsTotal);
    }

    public void RemoveFruit()
    {
        fruitsCollected--;
        inGameUI.UpdateFruitUI(fruitsCollected, fruitsTotal);
    }

    public int FruitsCollected() => fruitsCollected;
    public bool FruitsHaveRandomLook() => fruitsAreRandom;

    public void LevelFinished()
    {
        SaveLevelProgression();
        SaveBestTime();
        SaveFruitInfo();

        LoadNextScene();
    }

    private void SaveFruitInfo()
    {
        int fruitsCollectedBefore = PlayerPrefs.GetInt("Level" + currentLevelIndex + "FruitCollected");

        if (fruitsCollectedBefore < fruitsCollected)
            PlayerPrefs.SetInt("Level" + currentLevelIndex + "FruitsCollected", fruitsCollected);

        int totalFruitsBank = PlayerPrefs.GetInt("TotalFruitsAmount");
        PlayerPrefs.SetInt("TotalFruitsAmount", totalFruitsBank + fruitsCollected);
    }

    private void SaveBestTime()
    {
        PlayerPrefs.SetFloat("Level" + currentLevelIndex + "BestTime", levelTimer);

    }

    private void SaveLevelProgression()
    {
        PlayerPrefs.SetInt("Level" + nextLevelIndex + "Unlocked", 1);

        if (NoMoreLevels() == false)
        {
            PlayerPrefs.SetInt("ContinueLevelNumber", nextLevelIndex);

            SkinManager skinManager = SkinManager.instance;

            if (skinManager != null)
                PlayerPrefs.SetInt("LastUsedSkin", SkinManager.instance.GetSkinId());
        }
    }

    public void RestartLevel()
    {
        inGameUI.instance.fadeEffect.ScreenFade(1, .75f, LoadCurrentScene);
    }

    private void LoadCurrentScene() => SceneManager.LoadScene("Level_" + currentLevelIndex);
    private void LoadTheEndScene() => SceneManager.LoadScene("TheEnd");
    private void LoadNextLevel()
    {
        SceneManager.LoadScene("Level_" + nextLevelIndex);
    }

    private void LoadNextScene()
    {
        UI_FadeEffect fadeEffect = inGameUI.instance.fadeEffect;

        if (NoMoreLevels())
            fadeEffect.ScreenFade(1, 1.5f, LoadTheEndScene);
        else
            fadeEffect.ScreenFade(1, 1.5f, LoadNextLevel);
    }
    private bool NoMoreLevels()
    {
        int lastLevelIndex = SceneManager.sceneCountInBuildSettings - 2;
        bool noMoreLevels = currentLevelIndex == lastLevelIndex;

        return noMoreLevels;
    }
}
