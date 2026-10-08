using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerWeaponHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HotbarManager hotbar;
    [SerializeField] private Weapon swordController;
    [SerializeField] private BowController bowController;
    [SerializeField] private MagicController magicController;
    [SerializeField] private SummonController summonController;

    private void Start()
    {
        RelinkHotbar();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (hotbar != null)
            hotbar.OnSelectionChanged -= OnHotbarSelectionChanged;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(RelinkHotbarNextFrame());
    }

    private IEnumerator RelinkHotbarNextFrame()
    {
        yield return null; // let the new scene's HotbarManager finish initializing
        RelinkHotbar();
    }

    private void RelinkHotbar()
    {
        // Unsubscribe from whatever hotbar we were previously using
        // (harmless no-op if it was already destroyed)
        if (hotbar != null)
            hotbar.OnSelectionChanged -= OnHotbarSelectionChanged;

        hotbar = FindAnyObjectByType<HotbarManager>(FindObjectsInactive.Include);

        if (hotbar == null)
        {
            Debug.LogError("PlayerWeaponHandler: no HotbarManager found in this scene.", this);
            return;
        }

        if (swordController == null)
            Debug.LogError("PlayerWeaponHandler: swordController is not assigned.", this);
        if (bowController == null)
            Debug.LogError("PlayerWeaponHandler: bowController is not assigned.", this);
        if (magicController == null)
            Debug.LogError("PlayerWeaponHandler: magicController is not assigned.", this);
        if (summonController == null)
            Debug.LogError("PlayerWeaponHandler: summonController is not assigned.", this);

        hotbar.OnSelectionChanged += OnHotbarSelectionChanged;
        OnHotbarSelectionChanged(hotbar.GetSelectedItem());
    }

    private void OnHotbarSelectionChanged(ItemData item)
    {
        if (item == null || item.weaponData == null)
        {
            SetWeaponActive(false, false, false, false);
            return;
        }

        WeaponType type = item.weaponData.weaponType;
        bool isSword = type == WeaponType.Sword;
        bool isBow = type == WeaponType.Bow;
        bool isMagic = type == WeaponType.Magic;
        bool isSummon = type == WeaponType.Summon;

        SetWeaponActive(isSword, isBow, isMagic, isSummon);

        if (isSword && swordController != null)
            swordController.ApplyWeaponData(item.weaponData, item);
        else if (isBow && bowController != null)
            bowController.ApplyWeaponData(item.weaponData);
        else if (isMagic && magicController != null)
            magicController.ApplyWeaponData(item.weaponData);
        else if (isSummon && summonController != null)
            summonController.ApplyWeaponData(item.weaponData);
    }

    private void SetWeaponActive(bool sword, bool bow, bool magic, bool summon)
    {
        if (swordController != null) swordController.gameObject.SetActive(sword);
        if (bowController != null) bowController.gameObject.SetActive(bow);
        if (magicController != null) magicController.gameObject.SetActive(magic);
        if (summonController != null) summonController.gameObject.SetActive(summon);
    }
}