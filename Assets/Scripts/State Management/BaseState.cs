using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseState : MonoBehaviour {
    private bool isEnabled;
    public FiniteStateMachine parentFSM;
    public EnemyVision vision;

    public virtual void OnStateEnter(BaseState previousState) { }
    public virtual void OnStateExit(BaseState nextState) { }
    
}
