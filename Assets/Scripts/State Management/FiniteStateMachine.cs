using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiniteStateMachine : MonoBehaviour {
    [SerializeField]
    public BaseState enterState;
    private BaseState currentState;

    public Transform agentTransform;

    private Animator m_animator;
    public Animator animator { 
        get {
            return m_animator;
        } 
    }

    // Start is called before the first frame update
    void Start() {
        m_animator = GetComponentInParent<Animator>();

        currentState = enterState;
        enterState.enabled = true;
        enterState.parentFSM = this;

        enterState.OnStateEnter(null);
    }

    public void TransitionTo(BaseState newState) {
        newState.parentFSM = this;

        // Exit the current state
        currentState.OnStateExit(newState);
        currentState.enabled = false;

        // Enter the new state
        newState.enabled = true;
        newState.OnStateEnter(currentState);
        currentState = newState;
    }
}
