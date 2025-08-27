using System.Collections;
using UnityEngine;

public class ReloadWeapon : MonoBehaviour
{
    [Header("Refs")]
    public Animator animator;
    public WeaponAnimationEvents animationEvents;
    public Transform leftHand;

    [Header("State")]
    public bool isReloading;
    public float dropForce = 1.5f;

    [Header("Debug")]
    public bool debugLogs = false;   

    GameObject magazineHand;
    ActiveWeapon activeWeapon;
    AmmoWidget ammoWidget;

    void Awake()
    {
        ammoWidget = FindObjectOfType<AmmoWidget>();
        activeWeapon = GetComponent<ActiveWeapon>();
    }

    void Start()
    {
        if (animationEvents)
            animationEvents.WeaponAnimationEvent.AddListener(OnWeaponAnimationEvent);
    }

    void Update()
    {
        var weapon = activeWeapon ? activeWeapon.GetActiveWeapon() : null;
        if (weapon && !activeWeapon.isChangingWeapon)
        {
            if ((Input.GetKeyDown(KeyCode.R) || weapon.ShouldReload()) && !isReloading)
            {
                isReloading = true;
                if (animator) animator.SetTrigger("reload_weapon");
            }

            if (weapon.isFiring && ammoWidget)
                ammoWidget.Refresh(weapon.ammoCount, weapon.clipCount);
        }
    }

    void OnWeaponAnimationEvent(string eventName)
    {
        if (debugLogs) Debug.Log("OnAnimationEvent:" + eventName);

        if (!isReloading) return;

        switch (eventName)
        {
            case "detach_magazine": DetachMagazine(); break;
            case "drop_magazine": DropMagazine(); break;
            case "refill_magazine": RefillMagazine(); break;
            case "attach_magazine": AttachMagazine(); break;
        }
    }

    void DetachMagazine()
    {
        var weapon = activeWeapon ? activeWeapon.GetActiveWeapon() : null;
        if (!weapon || !leftHand || !weapon.magazine) return;

        if (magazineHand) Destroy(magazineHand);
        magazineHand = Instantiate(weapon.magazine, leftHand, true);
        if (magazineHand) magazineHand.SetActive(true);

        weapon.magazine.SetActive(false);
    }

    void DropMagazine()
    {
        if (!magazineHand) return;

        var dropped = Instantiate(magazineHand, magazineHand.transform.position, magazineHand.transform.rotation);
        dropped.SetActive(true);

        var body = dropped.AddComponent<Rigidbody>();
        Vector3 dropDirection = (-transform.right + Vector3.down).normalized;
        body.AddForce(dropDirection * dropForce, ForceMode.Impulse);
        dropped.AddComponent<BoxCollider>();

        magazineHand.SetActive(false);
    }

    void RefillMagazine()
    {
        if (magazineHand) magazineHand.SetActive(true);
    }

    void AttachMagazine()
    {
        var weapon = activeWeapon ? activeWeapon.GetActiveWeapon() : null;

        if (weapon && weapon.magazine) weapon.magazine.SetActive(true);
        CleanupHandMagazine();

        if (weapon)
        {
            weapon.RefillAmmo();
            if (animator) animator.ResetTrigger("reload_weapon");
            if (ammoWidget) ammoWidget.Refresh(weapon.ammoCount, weapon.clipCount);
        }

        isReloading = false;
    }

    void CleanupHandMagazine()
    {
        if (magazineHand) Destroy(magazineHand);
        magazineHand = null;
    }
}