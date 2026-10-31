using System.Collections.Generic;
using UnityEngine;

public class WeaponInventory : MonoBehaviour
{
    public const int MaxSlots = 6;   // 6 ô nguyên tố theo thiết kế

    //[SerializeField] private WeaponData[] startingWeapons;   // Vũ khí khởi đầu (sau này lấy từ nhân vật + số sao)
    private PlayerStats stats;

    private readonly List<WeaponBase> weapons = new List<WeaponBase>();

    public IReadOnlyList<WeaponBase> Weapons => weapons;
    public bool HasFreeSlot => weapons.Count < MaxSlots;

    [Header("CHỈ ĐỂ TEST: vũ khí thêm vào đầu trận")]
    [SerializeField] private WeaponData[] debugWeapons;
    [SerializeField, Range(1, 5)] private int debugWeaponLevel = 1;

    private void Start()
    {
        //foreach (WeaponData data in startingWeapons)
        //    AddWeapon(data);
        stats = GetComponent<PlayerStats>();

        // Vũ khí khởi đầu = các perk StartWeapon đang hoạt động (đã loại perk bị thay thế)
        foreach (StarPerk perk in stats.ActivePerks)
        {
            if (perk.type == PerkType.StartWeapon && perk.weapon != null)
                AddWeapon(perk.weapon, perk.weaponLevel);
        }
        //Debug code: thêm vũ khí test vào đầu trận
        foreach (WeaponData d in debugWeapons)
            if (d != null) AddWeapon(d, debugWeaponLevel);
    }

    public bool Has(WeaponData data)
    {
        foreach (WeaponBase w in weapons)
            if (w.Data == data) return true;
        return false;
    }

    public WeaponBase AddWeapon(WeaponData data, int startLevel = 1)
    {
        if (!HasFreeSlot || Has(data)) return null;

        // Tạo prefab vũ khí làm con của Player rồi khởi tạo dữ liệu
        GameObject go = Instantiate(data.weaponPrefab, transform);
        WeaponBase weapon = go.GetComponent<WeaponBase>();
        weapon.Initialize(data, startLevel);
        weapons.Add(weapon);
        return weapon;
    }
}