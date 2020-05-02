using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Damagable : MonoBehaviour {
    [Header("Health")]
    public UnityEvent onDamaged;
    public UnityEvent onHeathDepleted;

    public int maxHealth, currentHealth;

    [Header("UI")]
    public TextMeshProUGUI healthText;

    // Start is called before the first frame update
    void Start() {
        currentHealth = maxHealth;
        UpdateUI();
    }

    virtual public void receiveDamage(int damage) {
        currentHealth -= damage;
        onDamaged.Invoke();

        UpdateUI();

        // Test if we're out of health
        if (currentHealth <= 0) {
            onHeathDepleted.Invoke();
        }
    }

    public void UpdateUI() {
        // Update the UI
        if (healthText != null) {
            healthText.text = currentHealth.ToString();

            if (currentHealth <= 0) {
                healthText.enabled = false;
            }
        }
    }
}
