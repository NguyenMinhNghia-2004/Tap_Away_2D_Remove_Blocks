using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections.Generic;

public class TurnObject : MonoBehaviour
{
    [Header("UI & Input")]
    [Tooltip("Kéo list thả các object mũi tên xoay (ico_Turn...) vào mảng này")]
    public List<Transform> turnIcons = new List<Transform>();

    [Header("Settings")]
    public float interactionRadius = 2.4f; // Covers adjacent and diagonal blocks (spacing ~ 1.65)
    public float rotateDuration = 0.3f;
    public float tapDelay = 0.5f;
    public float blockSpacing = 1.65f;

    private float lastTapTime = 0f;
    private List<Transform> animatingIcons = new List<Transform>();
    public static bool IsAnyRotating = false;
    private Dictionary<Transform, Vector3> iconStartPos = new Dictionary<Transform, Vector3>();

    private void Start()
    {
        foreach (Transform icon in turnIcons)
        {
            if (icon != null)
            {
                iconStartPos[icon] = icon.position;
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Prevent tapping through UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null)
            {
                // Kiểm tra xem tia chạm trúng vào Script holder (Rota) hay trực tiếp vào 1 icon con
                bool isValidHit = (hit.gameObject == gameObject);
                
                if (!isValidHit)
                {
                    foreach (Transform icon in turnIcons)
                    {
                        if (icon != null && (hit.transform == icon || hit.transform.IsChildOf(icon)))
                        {
                            isValidHit = true;
                            break;
                        }
                    }
                }

                // Nếu chạm vào vùng cho phép, tìm Icon gần nhất với điểm bấm để thi triển vòng xoay
                if (isValidHit)
                {
                    Transform closestIcon = null;
                    float minDist = float.MaxValue;

                    foreach (Transform icon in turnIcons)
                    {
                        if (icon == null) continue;
                        float dist = Vector2.Distance(mousePos, icon.position);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            closestIcon = icon;
                        }
                    }

                    if (closestIcon != null)
                    {
                        ProcessTap(closestIcon);
                    }
                }
            }
        }
    }

    private void ProcessTap(Transform icon)
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
        if (LevelManager.Instance != null && LevelManager.Instance.isTransitioning) return;
        if (GameManager.Instance != null && GameManager.Instance.remainingMoves <= 0) return; // Prevent turn if 0 moves
        if (animatingIcons.Contains(icon) || IsAnyRotating) return;

        if (Time.time - lastTapTime < tapDelay) return;
        lastTapTime = Time.time;

        AudioManager.Instance?.PlayTapSound();

        IsAnyRotating = true;

        // Bounce animation like a button
        animatingIcons.Add(icon);
        
        if (!iconStartPos.ContainsKey(icon)) iconStartPos[icon] = icon.position;
        Vector3 origPos = iconStartPos[icon];
        
        // Kill existing tweens to prevent offset bugs if clicked unexpectedly
        icon.DOKill();
        icon.position = origPos;
        
        icon.DOMoveY(origPos.y + 0.2f, 0.1f).SetLoops(2, LoopType.Yoyo).OnComplete(() =>
        {
            animatingIcons.Remove(icon);
        });

        RotateSurroundingBlocks(origPos);
    }

    private void RotateSurroundingBlocks(Vector2 iconCenter)
    {
        // Mảng 8 vị trí xung quanh TurnObject theo đúng chiều kim đồng hồ bắt đầu từ phía trên (Top)
        Vector2[] ringOffsets = new Vector2[8] {
            new Vector2(0, 1),   // Top
            new Vector2(1, 1),   // Top-Right
            new Vector2(1, 0),   // Right
            new Vector2(1, -1),  // Bottom-Right
            new Vector2(0, -1),  // Bottom
            new Vector2(-1, -1), // Bottom-Left
            new Vector2(-1, 0),  // Left
            new Vector2(-1, 1)   // Top-Left
        };

        Collider2D[] colliders = Physics2D.OverlapCircleAll(iconCenter, interactionRadius);
        List<Block> initialBlocks = new List<Block>();
        
        foreach (Collider2D coll in colliders)
        {
            Block b = coll.GetComponent<Block>();
            if (b != null)
            {
                initialBlocks.Add(b);
            }
        }

        if (initialBlocks.Count == 0)
        {
            IsAnyRotating = false;
            return;
        }

        // Tự động neo đúng tâm của lưới cờ dựa theo block đầu tiên tìm được
        // Điều này bù trừ việc các Icon có pivot/center không khớp với tâm Grid (bị lệch trục Y)
        Vector2 gridCenter = iconCenter;
        int firstIdx = GetClosestRingIndex(initialBlocks[0].truePosition, iconCenter, ringOffsets);
        if (firstIdx != -1)
        {
            gridCenter = initialBlocks[0].truePosition - ringOffsets[firstIdx] * blockSpacing;
        }

        // Quét lại vị trí 8 ô quanh tâm thực sự lưới để đảm bảo không bỏ sót 
        // bất kỳ block nào (nếu offset icon lớn) và loại bỏ trùng lặp collider
        List<Block> blocksToMove = new List<Block>();
        for (int i = 0; i < 8; i++)
        {
            Vector2 p = gridCenter + ringOffsets[i] * blockSpacing;
            Collider2D[] exactHits = Physics2D.OverlapCircleAll(p, 0.5f);
            foreach (var h in exactHits)
            {
                Block b = h.GetComponent<Block>();
                if (b != null && !blocksToMove.Contains(b))
                {
                    if (b.IsBusy) 
                    {
                        IsAnyRotating = false;
                        return; // Prevent rotation if any block is currently moving/rotating
                    }
                    blocksToMove.Add(b);
                }
            }
        }

        if (blocksToMove.Count == 0)
        {
            IsAnyRotating = false;
            return;
        }

        // Quét để xác định vị trí nào đang bị chặn bởi Saw hoặc Stone
        bool[] isValid = new bool[8];
        for (int i = 0; i < 8; i++)
        {
            isValid[i] = true;
            Vector2 p = gridCenter + ringOffsets[i] * blockSpacing;
            Collider2D[] hits = Physics2D.OverlapCircleAll(p, 0.5f);
            foreach (var h in hits)
            {
                if (h.CompareTag("Saw") || h.CompareTag("Stone"))
                {
                    isValid[i] = false;
                    break;
                }
            }
        }



        int totalToMove = 0;
        int completedCount = 0;

        foreach (Block block in blocksToMove)
        {
            int currentIndex = GetClosestRingIndex(block.truePosition, gridCenter, ringOffsets);
            if (currentIndex != -1)
            {
                int nextIndex = (currentIndex + 1) % 8;
                int attempts = 0;
                
                // Tìm vị trí tiếp theo trên chu kì không bị can thiệp bởi Saw/Stone
                while (!isValid[nextIndex] && attempts < 8)
                {
                    nextIndex = (nextIndex + 1) % 8;
                    attempts++;
                }

                if (attempts < 8 && isValid[nextIndex])
                {
                    totalToMove++;
                }
            }
        }

        if (totalToMove == 0)
        {
            IsAnyRotating = false;
            return;
        }

        // Đã có block di chuyển được, tính là 1 lượt chơi
        GameManager.Instance?.UseMove();

        foreach (Block block in blocksToMove)
        {
            int currentIndex = GetClosestRingIndex(block.truePosition, gridCenter, ringOffsets);
            if (currentIndex != -1)
            {
                int nextIndex = (currentIndex + 1) % 8;
                int attempts = 0;
                
                while (!isValid[nextIndex] && attempts < 8)
                {
                    nextIndex = (nextIndex + 1) % 8;
                    attempts++;
                }

                if (attempts < 8 && isValid[nextIndex])
                {
                    Vector2 targetPos = gridCenter + ringOffsets[nextIndex] * blockSpacing;
                    block.RotateToPosition(targetPos, rotateDuration, () => {
                        completedCount++;
                        if (completedCount >= totalToMove)
                        {
                            IsAnyRotating = false;
                        }
                    });
                }
            }
        }
    }

    private int GetClosestRingIndex(Vector2 blockPos, Vector2 center, Vector2[] offsets)
    {
        float minDist = float.MaxValue;
        int bestIndex = -1;
        for (int i = 0; i < 8; i++)
        {
            float d = Vector2.Distance(blockPos, center + offsets[i] * blockSpacing);
            if (d < minDist)
            {
                minDist = d;
                bestIndex = i;
            }
        }
        
        // Cho sai số tìm kiếm để đảm bảo block thuộc vùng chỉ định
        if (minDist < 1.2f) return bestIndex;
        return -1;
    }
}
