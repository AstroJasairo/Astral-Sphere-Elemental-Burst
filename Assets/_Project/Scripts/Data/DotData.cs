using UnityEngine;

// Một hiệu ứng sát thương theo thời gian (DOT). Tạo bằng: Assets > Create > Astral Sphere > Dot Data
// Cùng id (hoặc cùng asset) = CÙNG một hiệu ứng: cộng dồn stack và reset thời gian chung.
// Ignite và Flare đều là "bỏng" nhưng khác asset/id nên stack riêng.
[CreateAssetMenu(menuName = "Astral Sphere/Dot Data")]
public class DotData : ScriptableObject
{
    [Tooltip("Duy nhất. Trùng id = cùng một hiệu ứng. Vd: dot_bleeding")]
    public string id;
    public string displayName;
    public string shortLabel = "DOT";         // Chữ hiện thay icon khi chưa có hình
    public Sprite icon;
    public ElementType element = ElementType.Pure;

    [Header("Sát thương")]
    public float damagePerTick = 1f;          // Sát thương của MỖI stack mỗi nhịp
    public float tickInterval = 1f;           // Giây giữa hai nhịp
    public float duration = 5f;               // Giây hiệu lực (reset mỗi lần dính thêm)
    [Min(1)] public int maxStacks = 10;       // Mỗi loại DOT có thể có số stack tối đa khác nhau

    private void OnValidate()
    {
        tickInterval = Mathf.Max(0.05f, tickInterval);   // Chu kỳ quá nhỏ làm vòng lặp nhịp chạy dày
        duration = Mathf.Max(tickInterval, duration);
        maxStacks = Mathf.Max(1, maxStacks);
    }

    // Chuột phải tiêu đề asset để điền sẵn số theo mô tả (dùng ở 11D)
    [ContextMenu("Fill Preset: Bleeding (Warrior)")]
    private void FillBleeding() { Fill("dot_bleeding", "Chảy máu", "BLD", ElementType.Metal, 1f, 1f, 5f); }

    [ContextMenu("Fill Preset: Poison (Archer)")]
    private void FillPoison() { Fill("dot_poison", "Độc", "PSN", ElementType.Plant, 2f, 2f, 10f); }

    [ContextMenu("Fill Preset: Burn (Mage)")]
    private void FillBurn() { Fill("dot_burn", "Bỏng", "BRN", ElementType.Fire, 2f, 0.5f, 3f); }

    private void Fill(string newId, string display, string label, ElementType e, float damage, float interval, float time)
    {
        id = newId; displayName = display; shortLabel = label; element = e;
        damagePerTick = damage; tickInterval = interval; duration = time; maxStacks = 10;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}