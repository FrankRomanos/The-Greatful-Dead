using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;


public class UnitWorldUI : MonoBehaviour
{
    [SerializeField] private Image healthBarImage;
    [SerializeField] private HealthSystem healthSystem;

    private void Start()
    {
        healthSystem.OnDamaged += HealthSystem_OnDamaged;
        UpdateHealthBar();
    }

    private void HealthSystem_OnDamaged(object sender, EventArgs e)
    {
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        healthBarImage.fillAmount = healthSystem.GetHealthNormalized();
    }
}
