// Mười nguyên tố của game (theo tài liệu vũ khí)
public enum ElementType
{
    Metal, Fire, Lightning, Wind, Soul, Light, Earth, Water, Wood, Dark
}

// Năm loại kẻ địch thường. THỨ TỰ phải khớp với format (Cận chiến/Xạ thủ/Đỡ đòn/Pháp sư/Sát thủ)
public enum EnemyType
{
    Melee, Ranged, Tank, Mage, Assassin
}

// Các loại vật phẩm rơi (theo tài liệu kẻ địch). Giai đoạn này chỉ dùng Gold và Crystal.
public enum PickupType
{
    Gold, Crystal, HealthPotion, SpeedPotion, RagePotion,
    Magnet, ManaPotion, Shield, ScoreBoost
}

// 6 chỉ số cơ bản của nhân vật
public enum StatType { Hp, Mp, Atk, Def, Spd, Mag }