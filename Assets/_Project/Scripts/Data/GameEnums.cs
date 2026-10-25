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

// Loại đặc quyền theo số sao của nhân vật
public enum PerkType
{
    StartWeapon,                // Khởi đầu trận với một vũ khí ở cấp nhất định
    ElementDamageBonus,         // Tăng sát thương một nguyên tố
    AllOutAssaultDamageBonus    // Tăng sát thương một nguyên tố khi đang trong All Out Assault
}

// Các buff tạm thời có thời hạn (All Out Assault có kênh riêng nên không nằm ở đây)
public enum BuffType { Speed, Rage, Magnet, ScoreBoost }