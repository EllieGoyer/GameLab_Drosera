﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OreVein : InteractableBase
{
    [Header("Ore Vein")]
    [SerializeField]
    [Tooltip("Make sure these are children of this object, also the sizes are in decending order starting with an empty vein")]
    private GameObject[] _orePrefabSizes;

    //private Animator _animator; // Might be useless, keeping for now if it turns out it is needed

    private void Start()
    {
        //_animator = GetComponent<Animator>();
        //_animator.SetInteger("stage", _uses);
        foreach(GameObject obj in _orePrefabSizes)
        {
            obj.SetActive(false);
        }
        _orePrefabSizes[_uses].SetActive(true);
    }

    public override bool Interact(PlayerBase player)
    {
        if (!base.Interact(player)) return false;
        
        //_animator.SetInteger("stage", _uses);
        ChangeState();
        player.Ammo += player.AmmoPerOre;
        if (_uses <= 0)
        {
            GetComponent<Collider>().enabled = false;
        }

        return true;
    }

    private void ChangeState()
    {
        foreach (GameObject obj in _orePrefabSizes)
        {
            obj.SetActive(false);
        }
        _orePrefabSizes[_uses].SetActive(true);
    }
}