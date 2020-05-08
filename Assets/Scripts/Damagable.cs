using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.SceneManagement;
using Valve.VR;

public class Damagable : MonoBehaviour {
    [Header("Health")]
    public UnityEvent onDamaged;
    public UnityEvent onHeathDepleted;

    public int maxHealth, currentHealth;
    public AudioSource audioSource;
    public AudioClip onDamagedAudio;
    public AudioClip onDeadAudio;

    // Since we have multiple damageable hitboxes for an enemy's limbs, we need to have invincibility frames
    // This is mainly for the case where a projectile hits multiple limbs at the same time
    private bool m_isInvincible;
    public virtual bool isInvincible {
        get { return m_isInvincible;  }
    }
    public float invincibilityDuration;

    [Header("UI")]
    public TextMeshProUGUI healthText;
    public bool disableUIOnDeath;

    [Header("Enemy AI")]
    public FiniteStateMachine fsm;
    public ShootingState shootState;


    // Start is called before the first frame update
    void Start() {
        currentHealth = maxHealth;
        m_isInvincible = false;
        UpdateUI();
    }

    virtual public void receiveDamage(int damage) {
        if (isInvincible) return;
        StartCoroutine(ApplyInvincibilityFrames());

        currentHealth -= damage;
        onDamaged.Invoke();

        UpdateUI();

        // Test if we're out of health
        if (currentHealth <= 0) {
            onHeathDepleted.Invoke();
            if (audioSource != null && onDeadAudio != null) {
                audioSource.PlayOneShot(onDeadAudio);
            }

            if (fsm != null) fsm.TransitionTo(fsm.ragdollState);
        } else {
            if (audioSource != null && onDamagedAudio != null) {
                audioSource.PlayOneShot(onDamagedAudio);
            }

            if (fsm != null) {
                if (fsm.currentState != shootState && fsm.currentState != fsm.ragdollState) {
                    fsm.TransitionTo(shootState);
                }

                // If we haven't already spotted the enemy for the first time, record their current position
                if (!shootState.vision.IsInSight(shootState.target.transform.position, "Player")) {
                    shootState.SpotTarget(shootState.target.transform.position);
                }
            }
        }
    }

    public void UpdateUI() {
        // Update the UI
        if (healthText != null) {
            healthText.text = currentHealth.ToString();

            if (currentHealth <= 0) {
                if (disableUIOnDeath) {
                    healthText.enabled = false;
                }
            }
        }
    }

    private IEnumerator ApplyInvincibilityFrames() {
        this.m_isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        this.m_isInvincible = false;
    }

    public void RestartLevel() {
        SteamVR_LoadLevel.Begin(SceneManager.GetActiveScene().name);
        currentHealth = maxHealth;
    }
    public void LoadLevel(string levelName) {
        SteamVR_LoadLevel.Begin(levelName);
        currentHealth = maxHealth;
    }
}
