﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GunnerDOTGrenade : Ability
{
    public UnityEvent OnFire;

    [SerializeField] GameObject _projectile;

    Transform _grenadeSpawn;
    Gunner _gunner;


    private void Awake()
    {   
        _gunner = GetComponent<Gunner>();
        _grenadeSpawn = _gunner.granadeSpawn;
    }

    protected override void ActivateAbility()
    {
        _gunner.Animator.SetBool("grenadeAni", true);
        StartCoroutine(CooldownTimer());
        Instantiate(_projectile, _grenadeSpawn.position, _grenadeSpawn.rotation);
        _gunner.Ammo -= _ammoCost;
        OnFire?.Invoke();
    }
}
