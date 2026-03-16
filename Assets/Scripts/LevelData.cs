using UnityEngine;

/// <summary>
/// Gắn vào root GameObject của mỗi Level Prefab.
/// Chứa thông tin cấu hình riêng cho từng level.
/// </summary>
public class LevelData : MonoBehaviour
{
    [Header("Level Config")]
    public int levelNumber = 1;
    public int remainingMoves = 10;
}
