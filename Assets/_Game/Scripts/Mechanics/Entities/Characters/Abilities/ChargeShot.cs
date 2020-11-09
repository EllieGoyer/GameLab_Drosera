﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChargeShot : MonoBehaviour
{
    public UnityEvent OnHit;
    public UnityEvent OnCriticalHit;

    GunnerAltFire _altFire;
    Hitbox _hitbox;
    Rigidbody _rb;

    //ParticleSystem _vfx;
    ElectricRoundExpandFire _vfxController;

    bool _isCharging = true;
    bool _crit = false;
    float _charge;
    Vector3 _startScale;

    public float Charge { get { return _charge; } }

    [Header("Shot Properties")]
    [SerializeField] float _moveSpeed;
    [SerializeField] float _damageMultiplier = 1f;
    [SerializeField] float _lifespan = 5f;
    [SerializeField] GameObject _AOEEffect;

    [Header("VFX")]
    [SerializeField]
    protected GameObject effect;
    [SerializeField]
    protected float effectDuration;

    private void Awake()
    {
        _altFire = FindObjectOfType<GunnerAltFire>();
        _hitbox = GetComponent<Hitbox>();
        _rb = GetComponentInChildren<Rigidbody>();
        _vfxController = GetComponentInChildren<ElectricRoundExpandFire>();
        //_vfx = GetComponentInChildren<ParticleSystem>();
    }

    private void Start()
    {
        _startScale = transform.localScale;
        _hitbox.baseDamage = 5f;
    }

    private void OnEnable()
    {
        _altFire.OnFire.AddListener(Fire);
    }

    private void OnDisable()
    {
        _altFire.OnFire.RemoveListener(Fire);
    }

    private void Update()
    {
        if (!_isCharging)
        {
            GetComponent<SphereCollider>().enabled = true;
            if (_charge < .2)
            {
                //_rb.MovePosition(transform.position + transform.forward * 0.2f * Time.deltaTime * _moveSpeed);
                _vfxController.Fire(_moveSpeed * 0.2f);
            }
            else
            {
                //_rb.MovePosition(transform.position + transform.forward * _charge * Time.deltaTime * _moveSpeed);
                _vfxController.Fire(_moveSpeed * _charge);
            }
        }
        else
        {
            _charge = _altFire.Charge;

            transform.position = _altFire.GunEnd.position;
            transform.rotation = _altFire.GunEnd.rotation;
            //transform.localScale = _startScale + Vector3.one * _charge * _scaleMultiplier;
            
            _hitbox.baseDamage = 5 + _charge * _damageMultiplier;

            _vfxController.Charge((1/_altFire.ChargeRate));

            /*
            var shape = _vfx.shape;
            shape.radius = 0.6f + (_charge);

            var emission = _vfx.emission;
            emission.rateOverTime = 140f + (_charge) * 100f;

            var trail = _vfx.GetComponentInChildren<TrailRenderer>();
            trail.widthMultiplier = .2f + (_charge);
            */
        }
    }

    public void Fire()
    {
        Destroy(gameObject, _lifespan);
        _isCharging = false;
        int num = Random.Range(0, 3);
        if (num == 0)
        {
            Debug.Log("Crit!");
            _crit = true;
            _hitbox.baseDamage *= 2;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        int layer = other.gameObject.layer;
        if (!(layer == 11 || layer == 13 || layer == 15)) //hit anything but player, other hitboxes, and invisible walls
        {
            Vector3 pos = transform.position + transform.forward;
            ChargeShotAOE aoe = Instantiate(_AOEEffect, pos, Quaternion.identity).GetComponent<ChargeShotAOE>();
            aoe.IgnoreTarget(other.gameObject);
            if (_crit)
            {
                OnCriticalHit?.Invoke();
                aoe.crit = true;
            }
            else
            {
                OnHit?.Invoke();
            }
            Destroy(gameObject);
        }
    }

    public void SpawnHitVFX()
    {
        if (effect != null && VFXSpawner.vfx != null)
        {
            GameObject vfx = VFXSpawner.vfx.SpawnVFX(effect, effectDuration, transform.position, transform.rotation);
        }
    }
}
