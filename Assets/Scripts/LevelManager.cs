using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Prefabs")]
    public GameObject[] levelPrefabs;   // Kéo thả các Level Prefab vào đây theo thứ tự

    [Header("Level Spawn")]
    public Transform levelContainer;    // GameObject rỗng làm cha cho level được spawn

    private GameObject currentLevelInstance;
    private LevelData currentLevelData;
    private List<Block> activeBlocks = new List<Block>();
    private int currentLevelIndex = 0;

    private const string LAST_PLAYED_LEVEL_KEY = "LastPlayedLevel";

    [HideInInspector] public bool isTransitioning = false;

    public LevelData CurrentLevelData => currentLevelData;

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
    }

    private void Start()
    {
        // Load level gần nhất đã chơi
        currentLevelIndex = PlayerPrefs.GetInt(LAST_PLAYED_LEVEL_KEY, 0);
        if (currentLevelIndex < 0 || currentLevelIndex >= levelPrefabs.Length)
            currentLevelIndex = 0;

        LoadLevel(currentLevelIndex);
        // Start game after level is loaded so GameManager can read LevelData
        GameManager.Instance?.StartGame();
    }

    // Load level theo index, spawn prefab và tự động tìm tất cả Block trong đó
    public void LoadLevel(int index)
    {
        // Xoá level cũ nếu có
        if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
        }

        activeBlocks.Clear();

        if (levelPrefabs == null || levelPrefabs.Length == 0)
        {
            Debug.LogWarning("LevelManager: Chưa gán Level Prefab nào!");
            return;
        }

        if (index < 0 || index >= levelPrefabs.Length)
        {
            Debug.Log("LevelManager: Đã hết level!");
            GameManager.Instance?.OnAllLevelsCompleted();
            return;
        }

        // Spawn level prefab
        Transform parent = levelContainer != null ? levelContainer : transform;
        currentLevelInstance = Instantiate(levelPrefabs[index], parent);
        currentLevelIndex = index;

        // Lưu level gần nhất
        PlayerPrefs.SetInt(LAST_PLAYED_LEVEL_KEY, index);
        PlayerPrefs.Save();

        // Đọc LevelData từ prefab
        currentLevelData = currentLevelInstance.GetComponent<LevelData>();
        if (currentLevelData == null)
            Debug.LogWarning($"LevelManager: Level prefab {index} thiếu LevelData component!");

        // Tự động tìm tất cả Block con trong level vừa spawn
        Block[] blocks = currentLevelInstance.GetComponentsInChildren<Block>();
        foreach (Block b in blocks)
        {
            activeBlocks.Add(b);
        }

        // Hiệu ứng scale in cho tất cả object con trong Grid (Block, Saw, v.v.)
        Transform grid = currentLevelInstance.transform.Find("Grid");
        if (grid != null && grid.childCount > 0)
        {
            isTransitioning = true;

            int lastIndex = grid.childCount - 1;
            for (int i = 0; i < grid.childCount; i++)
            {
                Transform child = grid.GetChild(i);
                child.localScale = Vector3.zero;
                var tween = child.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(i * 0.05f);

                if (i == lastIndex)
                {
                    tween.OnComplete(() => isTransitioning = false);
                }
            }
        }

        Debug.Log($"LevelManager: Loaded Level {index + 1} with {activeBlocks.Count} blocks.");
    }

    public void LoadNextLevel()
    {
        LoadLevel(currentLevelIndex + 1);
    }

    public void ReloadCurrentLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    public void OnBlockRemoved(Block block)
    {
        activeBlocks.Remove(block);

        Debug.Log($"LevelManager: Block removed. Remaining: {activeBlocks.Count}");
        
        CheckGameState();
    }

    public bool IsAnyBlockMoving()
    {
        foreach (var b in activeBlocks)
        {
            if (b != null && b.IsBusy) return true;
        }
        return false;
    }

    public void CheckGameState()
    {
        // Cleanup nulls just in case
        activeBlocks.RemoveAll(b => b == null);
        
        if (activeBlocks.Count == 0)
        {
            GameManager.Instance?.CheckWinLoseCondition();
        }
        else if (!IsAnyBlockMoving())
        {
            GameManager.Instance?.CheckWinLoseCondition();
        }
    }

    public bool IsLevelCleared()
    {
        activeBlocks.RemoveAll(b => b == null);
        return activeBlocks.Count == 0;
    }

    public int GetCurrentLevelIndex() => currentLevelIndex;
    public int GetTotalLevels() => levelPrefabs != null ? levelPrefabs.Length : 0;

    public bool IsLastLevel()
    {
        return currentLevelIndex >= levelPrefabs.Length - 1;
    }
}
