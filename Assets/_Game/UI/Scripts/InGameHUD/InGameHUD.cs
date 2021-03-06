﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class InGameHUD : MonoBehaviour
{
    [Header("Top Left UI")]
    [SerializeField] Image objectiveImage;
    //[SerializeField] Image objectiveImageText;
    [SerializeField] Sprite hyperSeedSprite;
    [SerializeField] Sprite dropshipSprite;
    [SerializeField] Sprite[] objectiveSprites;
    // [SerializeField] TextMeshProUGUI objectiveText;
    [SerializeField] TextMeshProUGUI biomeText;

    [Header("Bottom Right UI")]
    [Header("Ammo")]
    [SerializeField] Image lowAmmoImage;
    [SerializeField] TextMeshProUGUI currentAmmoText;
    [SerializeField] TextMeshProUGUI maxAmmoText;
    bool showingAmmoFlash;

    [Header("Ability")]
    [SerializeField] Image selectedAbilityImage;
    [SerializeField] Image fillUpAbilityCDImage1;
    [SerializeField] Image otherAbilityImage;
    [SerializeField] Image fillUpAbilityCDImage2;
    [SerializeField] Sprite[] abilitySprites;
    [SerializeField] float ability1Timer = 0;
    [SerializeField] TextMeshProUGUI ability1CooldownText;
    [SerializeField] float ability2Timer = 0;
    [SerializeField] TextMeshProUGUI ability2CooldownText;
    bool firstSelected = true;
    bool ability1OnCD;
    bool ability2OnCD;

    [Header("Alt Fire")]
    [SerializeField] Image secondaryAttackImage;
    public bool secondaryAttackOnCooldown = false;
    public float secondaryAttackCooldown = 6;
    public float secondaryAttackTimer = 0;

    [Header("Dodge")]
    [SerializeField] Image dodgeImage;
    public bool dodgeOnCooldown = false;
    public float dodgeCooldown = 2;
    public float dodgeTimer = 0;

    [Header("Reload")]
    [SerializeField] Image reloadImage;
    public bool reloadOnCooldown = false;
    public float reloadCooldown = 2;
    public float reloadTimer = 0;

    // references
    PlayerBase playerHookup;

    // UnityEvent hookups
    HyperSeed hyperSeedHookup;
    OreVein[] oreVeinHookups;

    HealthBar healthBarUI;

    Gunner gunnerHookup;
    GunnerPrimaryFire gunnerPrimaryFireHookup;
    GunnerDOTGrenade gunnerDOTGrenadeHookup;
    GunnerGrenade gunnerGrenadeHookup;
    GunnerAltFire gunnerSecondaryFireHookup;

    private void Awake()
    {
        playerHookup = FindObjectOfType<PlayerBase>();

        healthBarUI = GetComponent<HealthBar>();

        gunnerHookup = FindObjectOfType<Gunner>();

        gunnerPrimaryFireHookup = gunnerHookup.GetComponent<GunnerPrimaryFire>();
        gunnerDOTGrenadeHookup = gunnerHookup.GetComponent<GunnerDOTGrenade>();
        gunnerGrenadeHookup = gunnerHookup.GetComponent<GunnerGrenade>();
        gunnerSecondaryFireHookup = gunnerHookup.GetComponent<GunnerAltFire>();

    }

    private void Start()
    {
        gunnerHookup.OnTakeDamage.AddListener(healthBarUI.TakeDamage);
        gunnerHookup.OnHeal.AddListener(healthBarUI.Heal);

        gunnerPrimaryFireHookup.OnFire.AddListener(UpdateAmmoText);
        gunnerSecondaryFireHookup.OnFire.AddListener(DisplaySecondaryAttackCooldown);

        ShowPhaseOneObjectiveText();
        UpdateAmmoText();

        dodgeCooldown = playerHookup.DodgeCooldownTime;

        SetupAbilityImage();

        GameManager.Instance.OnStateChange += ChangePhaseText;
    }

    void ChangePhaseText()
    {
        if(GameManager.Instance.GameState == DroseraGlobalEnums.GameState.MainOne)
        {
            StartCoroutine(ShowPhaseOneObjectiveText());
        }
        else if (GameManager.Instance.GameState == DroseraGlobalEnums.GameState.MainTwo)
        {
            StartCoroutine(ShowPhaseTwoObjectiveText());
        }
    }

    private void Update()
    {
        // key Q/Y
        if (playerHookup.SwapAbilityButton)
        {
            UpdateAbilityImage();
        }

        // key R/X
        /*
        if (playerHookup.ReloadButton)
        {
            UpdateAmmoText();
        }
        */
        
        // key Space/LT
        if (playerHookup.DodgeButtonKey)
        {
            DisplayDodgeCooldown();
        }

        // key Shift
        if (playerHookup.AbilityButton && !gunnerGrenadeHookup.OnCooldown)
        {
            StartAbilityCooldown1();
        }
        if (playerHookup.AbilityButton && !gunnerDOTGrenadeHookup.OnCooldown)
        {
            StartAbilityCooldown2();
        }

        AbilityCooldown();
       
        if (secondaryAttackOnCooldown)
        {
            if (secondaryAttackTimer < secondaryAttackCooldown)
            {
                secondaryAttackTimer += Time.deltaTime;

                secondaryAttackImage.fillAmount = secondaryAttackTimer / secondaryAttackCooldown;
            }
            else
            {
                secondaryAttackOnCooldown = false;

                secondaryAttackImage.fillAmount = 1;
            }
        }

        if (dodgeOnCooldown)
        {
            if (dodgeTimer < dodgeCooldown)
            {
                dodgeTimer += Time.deltaTime;

                dodgeImage.fillAmount = dodgeTimer / dodgeCooldown;

                if (dodgeImage.fillAmount > 0.825f)
                    dodgeImage.fillAmount = 0.825f;
            }
            else
            {
                dodgeOnCooldown = false;

                dodgeImage.fillAmount = 0.825f;

                dodgeImage.DOFade(0, 0.5f);
            }
        }

        if (reloadOnCooldown)
        {
            if (reloadTimer < reloadCooldown)
            {
                reloadTimer += Time.deltaTime;

                reloadImage.fillAmount = reloadTimer / reloadCooldown;
            }
            else
            {
                reloadOnCooldown = false;

                reloadImage.fillAmount = 1;

                reloadImage.DOFade(0, 0.5f);
            }
        }
    }

    // top-left stuff
    // call at Start()
    IEnumerator ShowPhaseOneObjectiveText()
    {
        if (objectiveImage != null)
        {
            objectiveImage.sprite = hyperSeedSprite;
            // objectiveImage.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
        }

        // objectiveImageText.sprite = objectiveSprites[0];

        objectiveImage.DOFade(1, 1);
        // objectiveImageText.DOFade(1, 1);
        // biomeText.DOFade(1, 1);

        yield return new WaitForSeconds(10);

        objectiveImage.DOFade(0, 2);
        // objectiveImageText.DOFade(0, 2);
        // biomeText.DOFade(0, 2);
    }

    // call when player acquires the Hyperseed
    IEnumerator ShowPhaseTwoObjectiveText()
    {
        if (objectiveImage != null)
        {
            objectiveImage.sprite = dropshipSprite;
            // objectiveImage.transform.localScale = new Vector3(1, 1, 1);
        }

        // objectiveImageText.sprite = objectiveSprites[1];

        objectiveImage.DOFade(1, 0);
        // objectiveImageText.DOFade(1, 0);
        // biomeText.DOFade(1, 0);

        yield return new WaitForSecondsRealtime(10);

        objectiveImage.DOFade(0, 2);
        // objectiveImageText.DOFade(0, 2);
        // biomeText.DOFade(0, 2);
    }

    // bottom-right stuff
    // call for each shot (M1/RT)
    public void UpdateAmmoText()
    {
        currentAmmoText.text = playerHookup.Ammo.ToString();
        maxAmmoText.text = playerHookup.HeldAmmo.ToString();

        if (playerHookup.Ammo + playerHookup.HeldAmmo < 10)
        {
            if (!showingAmmoFlash)
            {
                showingAmmoFlash = true;
                lowAmmoImage.gameObject.SetActive(true);
                lowAmmoImage.DOFade(0.65f, 0f);

                StartCoroutine(LowAmmoFlash());
            }
        }
        else
        {
            showingAmmoFlash = false;
            StopCoroutine(LowAmmoFlash());

            lowAmmoImage.gameObject.SetActive(false);
        }
    }

    // call when dodge is used (Space/LT)
    public void DisplayReloadCooldown()
    {
        if (!reloadOnCooldown)
        {
            reloadTimer = 0;
            reloadCooldown = 1;

            reloadImage.fillAmount = 0;

            reloadImage.DOComplete();
            reloadImage.DOFade(1, 0);

            reloadOnCooldown = true;
        }
    }

    IEnumerator LowAmmoFlash()
    {
        while (showingAmmoFlash)
        {
            lowAmmoImage.DOFade(0.15f, 0.5f);

            yield return new WaitForSeconds(0.5f);

            lowAmmoImage.DOFade(0.65f, 0.5f);

            yield return new WaitForSeconds(0.5f);
        }
    }

    void SetupAbilityImage()
    {
        selectedAbilityImage.sprite = abilitySprites[0];
        selectedAbilityImage.SetNativeSize();

        otherAbilityImage.sprite = abilitySprites[3];
        otherAbilityImage.SetNativeSize();
    }

    void StartAbilityCooldown1()
    {
        if (firstSelected)
        {
            ability1Timer = gunnerGrenadeHookup.Cooldown;
            ability1CooldownText.text = ability1Timer.ToString();
            ability1OnCD = true;

            Image image = selectedAbilityImage.GetComponent<Image>();
            var tmpColor = image.color;
            tmpColor.a = 0.25f;
            image.color = tmpColor;

            for (int i = 0; i < selectedAbilityImage.transform.childCount; i++)
            {
                selectedAbilityImage.transform.GetChild(i).gameObject.SetActive(true);
            }
        }
    }

    void StartAbilityCooldown2()
    {
        if (!firstSelected)
        {
            ability2Timer = gunnerDOTGrenadeHookup.Cooldown;
            ability2CooldownText.text = ability2Timer.ToString();
            ability2OnCD = true;

            Image image = otherAbilityImage.GetComponent<Image>();
            var tmpColor = image.color;
            tmpColor.a = 0.25f;
            image.color = tmpColor;

            for (int i = 0; i < otherAbilityImage.transform.childCount; i++)
            {
                otherAbilityImage.transform.GetChild(i).gameObject.SetActive(true);
            }
        }
    }

    void AbilityCooldown()
    {
        if (ability1Timer > 0)
        {
            ability1Timer -= Time.deltaTime;
            ability1CooldownText.text = Mathf.RoundToInt(ability1Timer).ToString();
            fillUpAbilityCDImage1.fillAmount = (gunnerGrenadeHookup.Cooldown - ability1Timer) / gunnerGrenadeHookup.Cooldown;
        }
        else
        {
            ability1Timer = 0;
            ability1OnCD = false;

            Image image = selectedAbilityImage.GetComponent<Image>();
            var tmpColor = image.color;
            tmpColor.a = 1f;
            image.color = tmpColor;

            for (int i = 0; i < selectedAbilityImage.transform.childCount; i++)
            {
                selectedAbilityImage.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        if (ability2Timer > 0)
        {
            ability2Timer -= Time.deltaTime;
            ability2CooldownText.text = Mathf.RoundToInt(ability2Timer).ToString();
            fillUpAbilityCDImage2.fillAmount = (gunnerDOTGrenadeHookup.Cooldown - ability2Timer) / gunnerDOTGrenadeHookup.Cooldown;
        }
        else
        {
            ability2Timer = 0;
            ability2OnCD = false;

            Image image = otherAbilityImage.GetComponent<Image>();
            var tmpColor = image.color;
            tmpColor.a = 1f;
            image.color = tmpColor;

            for (int i = 0; i < otherAbilityImage.transform.childCount; i++)
            {
                otherAbilityImage.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    // call when ability is toggled (Q/Y)
    public void UpdateAbilityImage()
    {
        firstSelected = !firstSelected;

        // incediary grenade
        if (firstSelected && ability1Timer == 0)
        {
            selectedAbilityImage.sprite = abilitySprites[0];
            selectedAbilityImage.SetNativeSize();

            otherAbilityImage.sprite = abilitySprites[3];
            otherAbilityImage.SetNativeSize();
        }
        // pesticide grenade- DOT 
        else if (!firstSelected && ability2Timer == 0)
        {
            selectedAbilityImage.sprite = abilitySprites[1];
            selectedAbilityImage.SetNativeSize();

            otherAbilityImage.sprite = abilitySprites[2];
            otherAbilityImage.SetNativeSize();
        }
    }

    // call when skill is used (M2/RB)
    public void DisplaySecondaryAttackCooldown()
    {
        if (!secondaryAttackOnCooldown)
        {
            secondaryAttackTimer = 0;
            secondaryAttackCooldown = gunnerSecondaryFireHookup.Cooldown;

            secondaryAttackImage.fillAmount = 0;

            secondaryAttackOnCooldown = true;
        }
    }

    // call when dodge is used (Space/LT)
    public void DisplayDodgeCooldown()
    {
        if (!dodgeOnCooldown)
        {
            dodgeTimer = 0;
            dodgeCooldown = playerHookup.DodgeCooldownTime + playerHookup.DodgeTime;

            dodgeImage.fillAmount = 0;

            dodgeImage.DOComplete();
            dodgeImage.DOFade(1, 0);

            dodgeOnCooldown = true;
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnStateChange -= ChangePhaseText;
    }
}
