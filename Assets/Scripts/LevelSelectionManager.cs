using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Quản lý giao diện chọn level.
/// Gắn vào GameObject "Level" chứa Scroll View.
/// Tự động tạo các nút level từ prefab button dựa trên số lượng level trong LevelManager.
/// Hỗ trợ khóa/mở khóa level dựa trên tiến trình chơi.
/// </summary>
public class LevelSelectionManager : MonoBehaviour
{
    public static LevelSelectionManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("GameObject gốc của Level Selection panel (để bật/tắt)")]
    public GameObject levelSelectionPanel;

    [Tooltip("Transform Content trong Scroll View – nơi chứa các button level")]
    public Transform contentParent;

    [Tooltip("Prefab Button dùng để tạo các nút level (phải có Text/TMP con)")]
    public GameObject levelButtonPrefab;

    [Header("Button Colors")]
    [Tooltip("Màu button level đã mở khóa")]
    public Color unlockedColor = new Color(0.45f, 0.78f, 0.45f, 1f); // Xanh lá
    [Tooltip("Màu button level đang chọn / hiện tại")]
    public Color currentLevelColor = new Color(0.3f, 0.6f, 0.9f, 1f); // Xanh dương
    [Tooltip("Màu button level bị khóa")]
    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Xám


    // PlayerPrefs key prefix cho trạng thái mở khóa level
    private const string LEVEL_UNLOCK_KEY = "LevelUnlocked_";
    private const string MAX_UNLOCKED_KEY = "MaxUnlockedLevel";

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
        // Đảm bảo level 1 luôn được mở khóa
        if (GetMaxUnlockedLevel() < 1)
        {
            SetMaxUnlockedLevel(1);
        }

        // Ẩn panel khi bắt đầu
        if (levelSelectionPanel != null)
            levelSelectionPanel.SetActive(false);
    }

    public void ShowLevelSelection()
    {
        if (levelSelectionPanel == null) return;

        levelSelectionPanel.SetActive(true);

        // Animation mở panel
        levelSelectionPanel.transform.localScale = Vector3.zero;
        levelSelectionPanel.transform.DOScale(Vector3.one, 0.4f)
            .SetEase(Ease.OutBack);

        GenerateLevelButtons();
    }

    public void HideLevelSelection()
    {
        if (levelSelectionPanel == null) return;

        AudioManager.Instance?.PlayTapSound();

        levelSelectionPanel.transform.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() => levelSelectionPanel.SetActive(false));
    }

   
    public void GenerateLevelButtons()
    {
        if (contentParent == null || levelButtonPrefab == null)
        {
            Debug.LogWarning("LevelSelectionManager: Chưa gán contentParent hoặc levelButtonPrefab!");
            return;
        }

        // Xóa tất cả button cũ
        ClearButtons();

        int totalLevels = LevelManager.Instance != null ? LevelManager.Instance.GetTotalLevels() : 0;
        int maxUnlocked = GetMaxUnlockedLevel();
        int currentLevelIndex = LevelManager.Instance != null ? LevelManager.Instance.GetCurrentLevelIndex() : 0;

        for (int i = 0; i < totalLevels; i++)
        {
            int levelIndex = i; // Capture for closure
            int levelNumber = i + 1;
            bool isUnlocked = levelNumber <= maxUnlocked;
            bool isCurrent = levelIndex == currentLevelIndex;

            // Tạo button từ prefab
            GameObject buttonObj = Instantiate(levelButtonPrefab, contentParent);
            buttonObj.name = $"Level_{levelNumber}";

            // Tìm Text component (hỗ trợ cả TMP và Text cũ)
            TextMeshProUGUI tmpText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            Text legacyText = buttonObj.GetComponentInChildren<Text>();

            // Luôn hiển thị số level (phân biệt khóa/mở bằng màu và interactable)
            if (tmpText != null) tmpText.text = levelNumber.ToString();
            else if (legacyText != null) legacyText.text = levelNumber.ToString();

            // Thiết lập màu button
            Button btn = buttonObj.GetComponent<Button>();
            Image btnImage = buttonObj.GetComponent<Image>();

            if (btnImage != null)
            {
                if (isCurrent && isUnlocked)
                    btnImage.color = currentLevelColor;
                else if (isUnlocked)
                    btnImage.color = unlockedColor;
                else
                    btnImage.color = lockedColor;
            }

            // Thiết lập sự kiện click
            if (btn != null)
            {
                if (isUnlocked)
                {
                    btn.interactable = true;
                    btn.onClick.AddListener(() => OnLevelButtonClicked(levelIndex));
                }
                else
                {
                    btn.interactable = false;
                }
            }

            // Animation xuất hiện cho mỗi button
            buttonObj.transform.localScale = Vector3.zero;
            buttonObj.transform.DOScale(Vector3.one, 0.3f)
                .SetEase(Ease.OutBack)
                .SetDelay(i * 0.03f); // Stagger effect
        }
    }

    /// <summary>
    /// Xử lý khi người chơi nhấn chọn một level.
    /// </summary>
    private void OnLevelButtonClicked(int levelIndex)
    {
        AudioManager.Instance?.PlayTapSound();
        Debug.Log($"LevelSelectionManager: Selected Level {levelIndex + 1}");

        if (levelSelectionPanel == null) return;

        // Animation ẩn panel, sau khi xong mới load level
        UIManager.Instance?.settingUI.SetActive(false);
        levelSelectionPanel.transform.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                levelSelectionPanel.SetActive(false);

                // Load level được chọn
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.LoadLevel(levelIndex);
                }

                // Khởi động lại game state
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.StartGame();
                }

                // Cập nhật UI
                UIManager.Instance?.ResetUIState();
                UIManager.Instance?.RefreshUI();
            });
    }


    public void UnlockNextLevel(int completedLevelIndex)
    {
        int nextLevelNumber = completedLevelIndex + 2; // +1 cho index→number, +1 cho level tiếp theo
        int currentMax = GetMaxUnlockedLevel();

        if (nextLevelNumber > currentMax)
        {
            SetMaxUnlockedLevel(nextLevelNumber);
            Debug.Log($"LevelSelectionManager: Unlocked Level {nextLevelNumber}");
        }
    }

    /// <summary>
    /// Kiểm tra xem một level có được mở khóa không.
    /// </summary>
    public bool IsLevelUnlocked(int levelIndex)
    {
        int levelNumber = levelIndex + 1;
        return levelNumber <= GetMaxUnlockedLevel();
    }

    /// <summary>
    /// Lấy số level cao nhất đã mở khóa.
    /// </summary>
    public int GetMaxUnlockedLevel()
    {
        return PlayerPrefs.GetInt(MAX_UNLOCKED_KEY, 1);
    }

    /// <summary>
    /// Đặt số level cao nhất đã mở khóa.
    /// </summary>
    private void SetMaxUnlockedLevel(int level)
    {
        PlayerPrefs.SetInt(MAX_UNLOCKED_KEY, level);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Reset tất cả tiến trình mở khóa level (dùng cho debug/testing).
    /// </summary>
    public void ResetAllProgress()
    {
        SetMaxUnlockedLevel(1);
        Debug.Log("LevelSelectionManager: All progress reset. Only Level 1 is unlocked.");
    }

    /// <summary>
    /// Xóa tất cả button con trong Content.
    /// </summary>
    private void ClearButtons()
    {
        if (contentParent == null) return;

        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
    }
}
