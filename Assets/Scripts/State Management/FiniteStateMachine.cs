using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

// TODO: Set target properly once I start using scenes
public class FiniteStateMachine : MonoBehaviour {
    [SerializeField]
    public BaseState enterState;
    public RagdollState ragdollState;
    private BaseState m_currentState;
    public BaseState currentState { 
        get {
            return m_currentState;
        } 
    }

    public GameObject target{
        get {
            return Player.instance.hmdTransform.gameObject;
        }
    }

    public Transform agentTransform;
    public Damagable damageable;


    private Animator m_animator;
    public Animator animator { 
        get {
            return m_animator;
        } 
    }

    // Start is called before the first frame update
    void Start() {
        m_animator = GetComponentInParent<Animator>();

        m_currentState = enterState;
        enterState.enabled = true;
        enterState.parentFSM = this;

        enterState.OnStateEnter(null);
    }

    public void TransitionTo(BaseState newState) {
        // Don't do anything if we're already dead
        if (damageable.currentHealth <= 0 && currentState == ragdollState) return;

        newState.parentFSM = this;

        // Exit the current state
        currentState.OnStateExit(newState);
        currentState.enabled = false;

        // Enter the new state
        newState.enabled = true;
        newState.OnStateEnter(currentState);
        m_currentState = newState;
    }
}
