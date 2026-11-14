using System;
using UnityEngine;

// Chọn nhiều nguyên tố bằng một ô dropdown trong Inspector (Nothing / Everything / từng nguyên tố).
// Mỗi bit khớp (int)ElementType. KHÔNG có Pure: Pure không bao giờ bị miễn nhiễm.
[Flags]
public enum ElementMask
{
    None = 0,
    Metal = 1 << 0,
    Fire = 1 << 1,
    Lightning = 1 << 2,
    Wind = 1 << 3,
    Soul = 1 << 4,
    Light = 1 << 5,
    Earth = 1 << 6,
    Water = 1 << 7,
    Plant = 1 << 8,
    Dark = 1 << 9
}

// Kháng nguyên tố cơ bản: 0.2 = kháng 20%, âm = yếu hệ (nhận thêm sát thương), tối đa 0.9.
// Viết từng trường có tên (thay vì mảng) để Inspector hiện đúng tên nguyên tố.
[Serializable]
public class ElementResistances
{
    public const float MinResist = -1f;
    public const float MaxResist = 0.9f;

    [Header("0.2 = kháng 20%. Âm = yếu hệ. Pure không có kháng")]
    public float metal;
    public float fire;
    public float lightning;
    public float wind;
    public float soul;
    public float light;
    public float earth;
    public float water;
    public float plant;
    public float dark;

    public float Get(ElementType element)
    {
        switch (element)
        {
            case ElementType.Metal: return metal;
            case ElementType.Fire: return fire;
            case ElementType.Lightning: return lightning;
            case ElementType.Wind: return wind;
            case ElementType.Soul: return soul;
            case ElementType.Light: return light;
            case ElementType.Earth: return earth;
            case ElementType.Water: return water;
            case ElementType.Plant: return plant;
            case ElementType.Dark: return dark;
            default: return 0f;                       // Pure
        }
    }

    // Giá trị đã kẹp trong [-1, 0.9]
    public float GetClamped(ElementType element)
    {
        return Mathf.Clamp(Get(element), MinResist, MaxResist);
    }
}

public static class ElementMaskUtil
{
    // 10 nguyên tố thường (không gồm Pure)
    public static readonly ElementType[] Basic =
    {
        ElementType.Metal, ElementType.Fire, ElementType.Lightning, ElementType.Wind, ElementType.Soul,
        ElementType.Light, ElementType.Earth, ElementType.Water, ElementType.Plant, ElementType.Dark
    };

    public static bool Contains(ElementMask mask, ElementType element)
    {
        if (element == ElementType.Pure) return false;
        return ((int)mask & (1 << (int)element)) != 0;
    }
}