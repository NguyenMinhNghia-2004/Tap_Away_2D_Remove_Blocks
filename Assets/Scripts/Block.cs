using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Block : MonoBehaviour
{
    public enum Direction { Up, Down, Left, Right }
    
    [Header("Block Settings")]
    public Direction moveDirection;
    public float moveDistance = 10f;
    public float moveDuration = 0.5f;
    public float blockSpacing = 1.65f;

    [Header("Effects")]
    public GameObject removeEffect;
    
    private bool isRemoved = false;
    private bool isMoving = false;
    private bool isBouncing = false;    // Guard chống tap liên tục
    public bool isRotating = false;
    public bool IsBusy => isMoving || isBouncing || isRotating;
    private Vector2 startPosition;
    public Vector2 truePosition { get; private set; } // The stable position for logic
    private Tween moveTween;
    private Collider2D col2D;
    private Rigidbody2D rb2D;
    private SpriteRenderer sr;
    private Color originalColor;

    private static float lastTapTime = 0f;
    private const float tapDelay = 0.25f;    // Thời gian trễ giữa mỗi lần tap (tính bằng giây)

    private void Start()
    {
        col2D = GetComponent<Collider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        
        if (sr != null) originalColor = sr.color;

        if (rb2D != null)
        {
            rb2D.bodyType = RigidbodyType2D.Kinematic;
            rb2D.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
        truePosition = transform.position; // Khởi tạo truePosition
        // LevelManager tự scan block từ prefab — không cần tự register
    }

    private void OnMouseDown()
    {
        // Nhấn đè hoặc chặn tap xuyên UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
        if (LevelManager.Instance != null && LevelManager.Instance.isTransitioning) return;
        if (isRemoved || isMoving || isBouncing || isRotating) return;
        if (TurnObject.IsAnyRotating) return;

        // Tránh việc nhấp quá nhanh giữa các block hoặc spam nhấp
        if (Time.time - lastTapTime < tapDelay) return;
        lastTapTime = Time.time;

        if (GameManager.Instance != null && GameManager.Instance.isBoomModeActive)
        {
            ApplyBomb();
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.remainingMoves <= 0) return; // Không cho tap tiếp khi hết lượt di chuyển

        TryMove();
    }

    private void TryMove()
    {
        AudioManager.Instance?.PlayTapSound();
        GameManager.Instance?.UseMove();

        Vector2 dirVectors = GetDirectionVector(moveDirection);
        
        // Save start position for bouncing back if blocked
        startPosition = transform.position;
        isMoving = true;

        // Always start moving — collision detection happens via OnTriggerEnter2D
        Vector2 targetPos = startPosition + dirVectors * moveDistance;
        moveTween = transform.DOMove(targetPos, moveDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                // Block flew out without hitting anything — remove it
                RemoveBlock(false, false);
            });
    }

    public void RemoveBlock(bool playSound = true, bool playVisual = true)
    {
        if (isRemoved) return;
        isRemoved = true;
        isMoving = false;
        
        if (col2D != null) col2D.enabled = false;

        if (playSound)
        {
            AudioManager.Instance?.PlayRemoveSound();
        }

        if (playVisual && removeEffect != null)
        {
            GameObject effect = Instantiate(removeEffect, transform.position, Quaternion.identity);
            if (!playSound) 
            {
                effect.transform.localScale = Vector3.one * 1.5f; // Bomb scale
            }
            Destroy(effect, 0.5f);
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnBlockRemoved(this);
        }
        
        Destroy(gameObject);
    }

    private void ApplyBomb()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UseBoom();
        }

        // Tạo hiệu ứng nổ lớn hơn một chút
        if (removeEffect != null)
        {
            GameObject effect = Instantiate(removeEffect, transform.position, Quaternion.identity);
            effect.transform.localScale = Vector3.one * 1.5f;
            Destroy(effect, 0.5f);
        }
        AudioManager.Instance?.PlayRemoveSound();

        // 1 ô radius: bao gồm các khối liền kề và đường chéo (sqrt(2) * blockSpacing ~ 1.41 * 1.65 = 2.33)
        float explosionRadius = blockSpacing * 1.45f;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        bool soundPlayed = false;
        foreach (Collider2D coll in colliders)
        {
            Block b = coll.GetComponent<Block>();
            if (b != null && !b.isRemoved)
            {
                b.RemoveBlock(!soundPlayed, true);
                soundPlayed = true;
            }
        }
    }

    private void BounceBack(Vector2 collisionPoint)
    {
        // Guard flag to prevent another collision from overtaking
        isBouncing = true;
        isMoving = false;

        // Kill the current move tween
        if (moveTween != null && moveTween.IsActive())
        {
            moveTween.Kill();
        }

        Vector2 dir = GetDirectionVector(moveDirection);

        // To ensure the block stops exactly ON THE GRID (avoiding floating precision errors):
        // We know it traveled along 'dir' from 'startPosition'. We calculate how far it went:
        float traveledDistance = Vector2.Distance(startPosition, transform.position);
        
        // Find the nearest valid grid slot (number of steps). 
        // We subtract a little bit before rounding/flooring because we want the block to stop BEFORE the obstacle, not inside it.
        // Usually, the block's front hits the obstacle when it has traveled (step + 0.5) to (step + 1) of blockSpacing.
        int stepsTaken = Mathf.FloorToInt((traveledDistance) / blockSpacing);
        
        // Final stop position is exactly integer steps from startPosition
        Vector2 stopPos = startPosition + dir * (stepsTaken * blockSpacing);
        
        // Update its new absolute stable state
        truePosition = stopPos;

        // Animate bounce and shake
        transform.DOMove(stopPos, 0.15f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // Shake slightly to indicate blocked
                transform.DOShakePosition(0.15f, 
                    strength: new Vector3(dir.x, dir.y, 0) * 0.15f, 
                    vibrato: 10, randomness: 0, snapping: false, fadeOut: true)
                    .OnComplete(() => {
                        transform.position = truePosition; // Về đúng điểm grid
                        isBouncing = false; 
                        if (LevelManager.Instance != null)
                        {
                            LevelManager.Instance.CheckGameState();
                        }
                    }); 
            });
    }

    public void FlashRed()
    {
        if (sr != null)
        {
            sr.DOBlendableColor(Color.red, 0.1f).SetLoops(2, LoopType.Yoyo);
        }
    }

    private Vector2 GetDirectionVector(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return Vector2.up;
            case Direction.Down: return Vector2.down;
            case Direction.Left: return Vector2.left;
            case Direction.Right: return Vector2.right;
            default: return Vector2.zero;
        }
    }

    public void RotateToPosition(Vector2 targetPos, float duration, System.Action onComplete = null)
    {
        isMoving = true;
        isRotating = true;
        truePosition = targetPos;
        transform.DOKill(); // Dừng các animation cũ nếu có để tránh conflict state
        transform.DOJump(targetPos, 0.5f, 1, duration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                isMoving = false;
                isRotating = false;
                // Update position properly just in case
                transform.position = truePosition;
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.CheckGameState();
                }
                onComplete?.Invoke();
            });
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isMoving || isBouncing || isRotating) return;

        if (collision.CompareTag("Saw"))
        {
            if (moveTween != null && moveTween.IsActive())
            {
                moveTween.Kill();
            }
            RemoveBlock(true, true);
        }
        else if (collision.CompareTag("Block") || collision.CompareTag("Stone"))
        {
            // Nháy đỏ block bị đụng trúng
            Block hitBlock = collision.GetComponent<Block>();
            if (hitBlock != null)
            {
                hitBlock.FlashRed();
            }

            // Truyền vị trí va chạm, nhưng logic BounceBack sẽ tự tính dựa trên khoảng cách di chuyển từ Grid!
            BounceBack(collision.transform.position); 
        }
    }
}
