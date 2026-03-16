using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
     public static UIManager Instance { get; private set; }

    [Header("UI Groups")]
    public GameObject inGameUI;
    public GameObject settingUI;
    public GameObject gameOverUI;
    public GameObject gameWinUI;
    public GameObject nextLevelUI;
    public GameObject levelSelectionUI;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI bonusMoveText;

    [Header("Settings Display Objects")]
    [SerializeField] private GameObject musicOnObj;
    [SerializeField] private GameObject musicOffObj;
    [SerializeField] private GameObject soundOnObj;
    [SerializeField] private GameObject soundOffObj;
    [SerializeField] private GameObject shakeOnObj;
    [SerializeField] private GameObject shakeOffObj;

    [Header("Powerups UI")]
    [SerializeField] private UnityEngine.UI.Image boomButtonImage;
    [SerializeField] private GameObject boomImageObj;
    [SerializeField] private GameObject boomCoinObj;
    [SerializeField] private TextMeshProUGUI boomCountText;
    [SerializeField] private GameObject moveImageObj;
    [SerializeField] private GameObject moveCoinObj;
    [SerializeField] private TextMeshProUGUI moveCountText;

    private bool isPaused = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Time.timeScale = 1f; 
        ResetUIState();
        SubscribeEvents();
        RefreshUI();
    }

    public void ResetUIState()
    {
        if (inGameUI != null)       inGameUI.SetActive(true);
        if (settingUI != null)      settingUI.SetActive(false);
        if (gameOverUI != null)     gameOverUI.SetActive(false);
        if (gameWinUI != null)      gameWinUI.SetActive(false);
        if (nextLevelUI != null)    nextLevelUI.SetActive(false);
        if (levelSelectionUI != null) levelSelectionUI.SetActive(false);
        if (bonusMoveText != null)  bonusMoveText.gameObject.SetActive(false);
    }

    private void SubscribeEvents()
    {
        // Unsubscribe first to prevent duplicate subscriptions
        UnsubscribeEvents();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelUpdated += UpdateLevel;
            GameManager.Instance.OnMovesUpdated += UpdateMoves;
            GameManager.Instance.OnCoinsUpdated += UpdateCoins;
            GameManager.Instance.OnBoomCountUpdated += UpdateBoom;
            GameManager.Instance.OnMoveCountUpdated += UpdateMoveBonus;
            GameManager.Instance.OnBoomModeChanged += UpdateBoomMode;
            GameManager.Instance.OnGameWin += WinGame;
            GameManager.Instance.OnGameLose += LoseGame;
            GameManager.Instance.OnNextLevel += ShowNextLevelUI;
        }
    }

    private void UnsubscribeEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelUpdated -= UpdateLevel;
            GameManager.Instance.OnMovesUpdated -= UpdateMoves;
            GameManager.Instance.OnCoinsUpdated -= UpdateCoins;
            GameManager.Instance.OnBoomCountUpdated -= UpdateBoom;
            GameManager.Instance.OnMoveCountUpdated -= UpdateMoveBonus;
            GameManager.Instance.OnBoomModeChanged -= UpdateBoomMode;
            GameManager.Instance.OnGameWin -= WinGame;
            GameManager.Instance.OnGameLose -= LoseGame;
            GameManager.Instance.OnNextLevel -= ShowNextLevelUI;
        }
    }

    /// <summary>
    /// Manually refreshes all UI text from GameManager's current state.
    /// Call this whenever values change outside of events (e.g., on Start, Retry, NextLevel).
    /// </summary>
    public void RefreshUI()
    {
        if (GameManager.Instance == null) return;

        UpdateLevel(GameManager.Instance.currentLevel);
        UpdateMoves(GameManager.Instance.remainingMoves);
        UpdateCoins(GameManager.Instance.coins);
        UpdateBoom(GameManager.Instance.boomCount);
        UpdateMoveBonus(GameManager.Instance.moveBonusCount);
        UpdateBoomMode(GameManager.Instance.isBoomModeActive);
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    // --- UI Update Callbacks ---
    private void UpdateLevel(int level)
    {
        if (levelText != null) levelText.text = $"Level {level}";
    }

    private void UpdateMoves(int moves)
    {
        if (movesText != null) movesText.text = $"{moves} moves";
    }

    private void UpdateCoins(int coins)
    {
        if (coinText != null) coinText.text = coins.ToString();
    }

    private void UpdateBoom(int count)
    {
        if (count > 0)
        {
            if (boomImageObj != null) boomImageObj.SetActive(true);
            if (boomCoinObj != null) boomCoinObj.SetActive(false);
            if (boomCountText != null) boomCountText.text = count.ToString();
        }
        else
        {
            if (boomImageObj != null) boomImageObj.SetActive(false);
            if (boomCoinObj != null) boomCoinObj.SetActive(true);
            
            // Disable boom mode if count runs out while active
            // Let's ensure GameManager knows too
            if (GameManager.Instance != null && GameManager.Instance.isBoomModeActive)
            {
                GameManager.Instance.ToggleBoomMode();
            }
        }
    }

    private void UpdateMoveBonus(int count)
    {
        if (count > 0)
        {
            if (moveImageObj != null) moveImageObj.SetActive(true);
            if (moveCoinObj != null) moveCoinObj.SetActive(false);
            if (moveCountText != null) moveCountText.text = count.ToString();
        }
        else
        {
            if (moveImageObj != null) moveImageObj.SetActive(false);
            if (moveCoinObj != null) moveCoinObj.SetActive(true);
        }
    }

    private void UpdateBoomMode(bool isActive)
    {
        if (boomButtonImage != null)
        {
            boomButtonImage.color = isActive ? Color.red : Color.white;
        }
    }

    // --- Game States ---
    public void WinGame()
    {
        Debug.Log("All levels completed! You Win!");
        if (gameWinUI != null)
        {
            gameWinUI.SetActive(true);
            gameWinUI.transform.localScale = Vector3.zero;
            gameWinUI.transform.DOScale(Vector3.one, 0.5f)
                .SetEase(Ease.OutBack)
                .SetDelay(0.2f);  
        }
    }

    public void ShowNextLevelUI()
    {
        Debug.Log("Level cleared! Next level available.");
        if (nextLevelUI != null)
        {
            nextLevelUI.SetActive(true);
            nextLevelUI.transform.localScale = Vector3.zero;
            nextLevelUI.transform.DOScale(Vector3.one, 0.5f)
                .SetEase(Ease.OutBack)
                .SetDelay(0.2f);  
        }
    }

    public void LoseGame()
    {
        Debug.Log("You Lose!");
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            gameOverUI.transform.localScale = Vector3.zero;
            gameOverUI.transform.DOScale(Vector3.one, 0.5f)
                .SetEase(Ease.OutBack)
                .SetDelay(0.2f);  
        }
    }

    // --- Buttons ---
    public void OnBoomButtonClicked()
    {
        AudioManager.Instance?.PlayTapSound();
        if (GameManager.Instance == null) return;
        
        if (GameManager.Instance.boomCount > 0)
        {
            GameManager.Instance.ToggleBoomMode();
        }
        else
        {
            GameManager.Instance.BuyBoom();
        }
    }

    public void OnMoveButtonClicked()
    {
        AudioManager.Instance?.PlayTapSound();
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.moveBonusCount > 0)
        {
            // If boom mode is active, deactivate it as per requirement: "hoặc tap sang Move bt"
            if (GameManager.Instance.isBoomModeActive)
            {
                GameManager.Instance.ToggleBoomMode();
            }

            // Gọi logic trừ vật phẩm và + thêm 5 lượt
            GameManager.Instance.UseMoveBonus(5);
            
            ShowBonusMoveText(5);
        }
        else
        {
            GameManager.Instance.BuyMoveBonus();
        }
    }

    private void ShowBonusMoveText(int amount)
    {
        if (bonusMoveText != null)
        {
            bonusMoveText.gameObject.SetActive(true);
            bonusMoveText.text = $"+{amount} Moves";
            bonusMoveText.alpha = 1f;

            // Simple animation
            bonusMoveText.transform.localScale = Vector3.one * 0.5f;
            Sequence seq = DOTween.Sequence();
            seq.Append(bonusMoveText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
            seq.AppendInterval(0.5f);
            seq.Append(bonusMoveText.DOFade(0f, 0.5f));
            seq.OnComplete(() => bonusMoveText.gameObject.SetActive(false));
        }
    }

    public void Setting()
    {
        AudioManager.Instance.PlayTapSound();
        if (settingUI != null)
        {
            UpdateSettingsUI();
            settingUI.SetActive(true);
            settingUI.transform.localScale = Vector3.zero;
            settingUI.transform.DOScale(Vector3.one, 0.5f)
                .SetEase(Ease.OutBack)
                .SetDelay(0.2f);  
        }
    }

    public void Setting_Close()
    {
        AudioManager.Instance.PlayTapSound();
        if (settingUI != null)
        {
            settingUI.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => settingUI.SetActive(false));
        }
    }

    public void RetryLevel()
    {
        AudioManager.Instance.PlayTapSound();
        Time.timeScale = 1;

        // Reset UI panels
        ResetUIState();

        // Reload current level without reloading the scene
        LevelManager.Instance?.ReloadCurrentLevel();

        // Restart game state in GameManager
        GameManager.Instance?.RestartLevel();

        // Re-subscribe events (in case GameManager was recreated) and refresh UI texts
        SubscribeEvents();
        RefreshUI();
    }

    public void NextLevel()
    {
        AudioManager.Instance.PlayTapSound();
        GameManager.Instance?.NextLevel();
        if (nextLevelUI != null)    
        {
            nextLevelUI.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => nextLevelUI.SetActive(false));

        }
    }
    public void AllLevelsCompleted()
    {
        AudioManager.Instance.PlayTapSound();
        GameManager.Instance?.AllLevelsCompleted();
        if (gameWinUI != null)    
        {
            gameWinUI.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => gameWinUI.SetActive(false));

        }
    }

    public void ShowLevelSelection()
    {
        AudioManager.Instance?.PlayTapSound();
        LevelSelectionManager.Instance?.ShowLevelSelection();
    }

    public void HideLevelSelection()
    {
        LevelSelectionManager.Instance?.HideLevelSelection();
        
    }

    // --- Settings Logic ---
    public void UpdateSettingsUI()
    {
        if (AudioManager.Instance == null) return;
        
        if (musicOnObj != null) musicOnObj.SetActive(AudioManager.Instance.isMusicOn);
        if (musicOffObj != null) musicOffObj.SetActive(!AudioManager.Instance.isMusicOn);

        if (soundOnObj != null) soundOnObj.SetActive(AudioManager.Instance.isSoundOn);
        if (soundOffObj != null) soundOffObj.SetActive(!AudioManager.Instance.isSoundOn);

        if (shakeOnObj != null) shakeOnObj.SetActive(AudioManager.Instance.isShakeOn);
        if (shakeOffObj != null) shakeOffObj.SetActive(!AudioManager.Instance.isShakeOn);
    }

    public void OnMusicToggleClick()
    {
        AudioManager.Instance.PlayTapSound();
        AudioManager.Instance.ToggleMusic();
        UpdateSettingsUI();
    }

    public void OnSoundToggleClick()
    {
        AudioManager.Instance.PlayTapSound();
        AudioManager.Instance.ToggleSound();
        UpdateSettingsUI();
    }

    public void OnShakeToggleClick()
    {
        AudioManager.Instance.PlayTapSound();
        AudioManager.Instance.ToggleShake();
        AudioManager.Instance.Vibrate(); // test vibrate
        UpdateSettingsUI();
    }
    public void AddCoins()
    {
        GameManager.Instance?.AddCoins(100);
    }
    
}
