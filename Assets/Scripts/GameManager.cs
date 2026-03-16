using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Events for UI and other systems
    public event Action<int> OnCoinsUpdated;
    public event Action<int> OnMovesUpdated;
    public event Action<int> OnLevelUpdated;
    public event Action OnGameWin;
    public event Action OnGameLose;
    public event Action OnNextLevel;

    // Events for powerups
    public event Action<int> OnBoomCountUpdated;
    public event Action<int> OnMoveCountUpdated;
    public event Action<bool> OnBoomModeChanged;

    [Header("Game State")]
    public int coins = 0;
    public int remainingMoves;

    [Header("Powerups")]
    public int boomCount = 1;
    public int moveBonusCount = 1;
    public bool isBoomModeActive = false;

    public bool IsPlaying { get; private set; }

    // Constants for PlayerPrefs
    private const string COINS_KEY = "PlayerCoins";
    private const string BOOM_KEY = "PlayerBoomCount";
    private const string MOVE_BONUS_KEY = "PlayerMoveBonusCount";

    // Current level reads from LevelManager
    public int currentLevel => LevelManager.Instance?.CurrentLevelData?.levelNumber ?? 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Load saved data
        coins = PlayerPrefs.GetInt(COINS_KEY, 0);
        boomCount = PlayerPrefs.GetInt(BOOM_KEY, 1);
        moveBonusCount = PlayerPrefs.GetInt(MOVE_BONUS_KEY, 1);
    }

    private void Start()
    {
        // StartGame() is called by LevelManager after level is loaded
        // to ensure LevelData is available
    }

    public void StartGame()
    {
        IsPlaying = true;
        isBoomModeActive = false;
        OnBoomModeChanged?.Invoke(isBoomModeActive);
        // Read remaining moves from current level data
        remainingMoves = LevelManager.Instance?.CurrentLevelData?.remainingMoves ?? 10;
        UpdateUI();
    }

    public void UseMove()
    {
        if (!IsPlaying) return;

        remainingMoves--;
        OnMovesUpdated?.Invoke(remainingMoves);

        // Defer loss check until the block finishes its movement.
        // LevelManager will check state and call CheckWinLoseCondition when stabilized
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        PlayerPrefs.SetInt(COINS_KEY, coins);
        PlayerPrefs.Save();
        OnCoinsUpdated?.Invoke(coins);
    }

    public void AddMoves(int amount)
    {
        remainingMoves += amount;
        OnMovesUpdated?.Invoke(remainingMoves);
    }

    public void BuyBoom()
    {
        if (coins >= 100)
        {
            AddCoins(-100);
            boomCount++;
            PlayerPrefs.SetInt(BOOM_KEY, boomCount);
            PlayerPrefs.Save();
            OnBoomCountUpdated?.Invoke(boomCount);
        }
    }

    public void UseBoom()
    {
        if (boomCount > 0)
        {
            boomCount--;
            PlayerPrefs.SetInt(BOOM_KEY, boomCount);
            PlayerPrefs.Save();
            OnBoomCountUpdated?.Invoke(boomCount);
            
            // Turn off boom mode after using
            ToggleBoomMode();
        }
    }

    public void BuyMoveBonus()
    {
        if (coins >= 100)
        {
            AddCoins(-100);
            moveBonusCount++;
            PlayerPrefs.SetInt(MOVE_BONUS_KEY, moveBonusCount);
            PlayerPrefs.Save();
            OnMoveCountUpdated?.Invoke(moveBonusCount);
        }
    }

    public void UseMoveBonus(int amountToAdd = 5)
    {
        if (moveBonusCount > 0)
        {
            moveBonusCount--;
            PlayerPrefs.SetInt(MOVE_BONUS_KEY, moveBonusCount);
            PlayerPrefs.Save();
            OnMoveCountUpdated?.Invoke(moveBonusCount);
            AddMoves(amountToAdd); // + Lượt di chuyển
        }
    }

    public void ToggleBoomMode()
    {
        isBoomModeActive = !isBoomModeActive;
        OnBoomModeChanged?.Invoke(isBoomModeActive);
    }

    public void CheckWinLoseCondition()
    {
        if (!IsPlaying) return;

        if (LevelManager.Instance != null && LevelManager.Instance.IsLevelCleared())
        {
            WinGame();
        }
        else if (remainingMoves <= 0 && LevelManager.Instance != null && !LevelManager.Instance.IsAnyBlockMoving())
        {
            LoseGame();
        }
    }

    public void CheckWinCondition()
    {
        CheckWinLoseCondition();
    }

    private void WinGame()
    {
        IsPlaying = false;
        AudioManager.Instance?.PlayWinSound();

        // Mở khóa level tiếp theo
        if (LevelManager.Instance != null)
        {
            LevelSelectionManager.Instance?.UnlockNextLevel(LevelManager.Instance.GetCurrentLevelIndex());
        }

        // Check if this is the last level
        if (LevelManager.Instance != null && LevelManager.Instance.IsLastLevel())
        {
            // Last level → show gameWinUI
            OnGameWin?.Invoke();
        }
        else
        {
            // Not the last level → show nextLevelUI
            OnNextLevel?.Invoke();
        }
    }

    private void LoseGame()
    {
        IsPlaying = false;
        AudioManager.Instance?.PlayLoseSound();
        OnGameLose?.Invoke();
    }

    public void RestartLevel()
    {
        // Reset moves from current level data
        remainingMoves = LevelManager.Instance?.CurrentLevelData?.remainingMoves ?? 10;
        IsPlaying = true;
        UpdateUI();
    }

    public void NextLevel()
    {
        AddCoins(10);
        LevelManager.Instance?.LoadNextLevel();
        // Read moves from new level data
        remainingMoves = LevelManager.Instance?.CurrentLevelData?.remainingMoves ?? 10;
        IsPlaying = true;
        UpdateUI();
    }

    public void AllLevelsCompleted()
    {
        AddCoins(10);
        LevelManager.Instance?.ReloadCurrentLevel();
        remainingMoves = LevelManager.Instance?.CurrentLevelData?.remainingMoves ?? 10;
        IsPlaying = true;
        UpdateUI();
    }

    public void OnAllLevelsCompleted()
    {
        // Xử lý khi đã hết tất cả level
        IsPlaying = false;
        Debug.Log("All levels completed!");
        OnGameWin?.Invoke();
    }

    private void UpdateUI()
    {
        OnLevelUpdated?.Invoke(currentLevel);
        OnCoinsUpdated?.Invoke(coins);
        OnMovesUpdated?.Invoke(remainingMoves);
        OnBoomCountUpdated?.Invoke(boomCount);
        OnMoveCountUpdated?.Invoke(moveBonusCount);
    }
}
