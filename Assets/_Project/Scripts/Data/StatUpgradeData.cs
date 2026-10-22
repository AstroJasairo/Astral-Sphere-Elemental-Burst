using UnityEngine;

// Một lựa chọn "tăng chỉ số" trong trận. Tạo bằng: Assets > Create > Astral Sphere > Stat Upgrade Data
[CreateAssetMenu(menuName = "Astral Sphere/Stat Upgrade Data")]
public class StatUpgradeData : ScriptableObject
{
    public StatType stat;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    [Tooltip("Hp/Def: cộng thẳng. Atk/Spd/Mag/Mp: dạng phần trăm (0.1 = 10%)")]
    public float amount;
}