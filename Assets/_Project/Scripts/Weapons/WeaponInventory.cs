using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponInventory : MonoBehaviour
{
    public const int MaxSlots = 6;   // 6 ô nguyên tố theo thiết kế

    // Một passive đang gắn trên một vũ khí
    private class PassiveEntry
    {
        public WeaponBase weapon;
        public PassiveSkillData data;
        public PassiveSkillEffect effect;
        public bool running;          // Đang Enable hay không (Silence/Choáng có thể tắt)
    }

    private PlayerStats stats;
    private readonly List<WeaponBase> weapons = new List<WeaponBase>();
    private readonly List<PassiveEntry> passives = new List<PassiveEntry>();

    public IReadOnlyList<WeaponBase> Weapons => weapons;
    public bool HasFreeSlot => weapons.Count < MaxSlots;

    // Phát khi thêm vũ khí hoặc một vũ khí lên cấp (Hộ giá, Lưỡi gươm... nghe để tính lại)
    public event Action OnWeaponsChanged;

    // Tổng cấp của mọi vũ khí đang mang (kể cả vũ khí đặc trưng)
    public int TotalWeaponLevels
    {
        get
        {
            int sum = 0;
            for (int i = 0; i < weapons.Count; i++) sum += weapons[i].Level;
            return sum;
        }
    }

    [Header("CHỈ ĐỂ TEST: vũ khí thêm vào đầu trận")]
    [SerializeField] private WeaponData[] debugWeapons;
    [SerializeField, Range(1, 5)] private int debugWeaponLevel = 1;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        // Vũ khí khởi đầu = các perk StartWeapon đang hoạt động (đã loại perk bị thay thế)
        foreach (StarPerk perk in stats.ActivePerks)
        {
            if (perk.type == PerkType.StartWeapon && perk.weapon != null)
                AddWeapon(perk.weapon, perk.weaponLevel);
        }

        // Debug: thêm vũ khí test vào đầu trận
        if (Debug.isDebugBuild)
        {
            foreach (WeaponData d in debugWeapons)
                if (d != null) AddWeapon(d, debugWeaponLevel);
        }
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
        weapons.Add(weapon);                         // Thêm vào danh sách TRƯỚC khi đăng ký passive (Hộ giá cần đếm vũ khí này)

        weapon.Leveled += HandleWeaponLeveled;
        RegisterPassives(weapon);

        OnWeaponsChanged?.Invoke();
        return weapon;
    }

    // ---------------- Passive đính kèm vũ khí ----------------

    private void RegisterPassives(WeaponBase weapon)
    {
        List<PassiveSkillData> list = weapon.Data.passiveSkills;
        if (list == null) return;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null) continue;

            var entry = new PassiveEntry
            {
                weapon = weapon,
                data = list[i],
                effect = list[i].CreateEffect(stats)
            };

            entry.effect.SetLevel(weapon.Level);               // Cấp passive = cấp vũ khí
            passives.Add(entry);

            if (ShouldRun(entry))
            {
                entry.running = true;
                entry.effect.Enable();
            }
        }
    }

    // Vũ khí lên cấp: cập nhật cấp passive rồi báo cho Hộ giá, Lưỡi gươm...
    private void HandleWeaponLeveled(WeaponBase weapon)
    {
        for (int i = 0; i < passives.Count; i++)
        {
            PassiveEntry p = passives[i];
            if (p.weapon != weapon) continue;

            p.effect.SetLevel(weapon.Level);
            if (p.running) p.effect.Enable();       // Đăng ký lại với giá trị mới (Enable của mọi effect gọi nhiều lần vẫn an toàn)
        }

        OnWeaponsChanged?.Invoke();
    }

    // Silence hoặc Choáng tắt passive vũ khí, trừ khi skill có cờ miễn nhiễm tương ứng
    private bool ShouldRun(PassiveEntry p)
    {
        if (stats.IsSilenced && !p.data.ignoresSilence) return false;
        if (stats.IsStunned && !p.data.ignoresStun) return false;
        return true;
    }

    private void Update()
    {
        if (passives.Count == 0) return;
        float dt = Time.deltaTime;

        for (int i = 0; i < passives.Count; i++)
        {
            PassiveEntry p = passives[i];

            // Chỉ gọi Enable/Disable khi trạng thái ĐỔI
            bool run = ShouldRun(p);
            if (run != p.running)
            {
                p.running = run;
                if (run) p.effect.Enable();
                else p.effect.Disable();
            }

            if (p.running) p.effect.Tick(dt);
        }
    }
}