using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollEnemy : MonoBehaviour {
    public bool isRagdollActiveOnStart;

    public List<Rigidbody> limbs;
    private bool m_isRagdollActive;

    public bool isRagdollActive {
        get { return m_isRagdollActive;  }
        set {
            foreach (Rigidbody limbRigidbody in limbs) {
                limbRigidbody.isKinematic = !value;
            }

            m_isRagdollActive = value;
        }
    }

    // Start is called before the first frame update
    void Start() {
        isRagdollActive = isRagdollActiveOnStart;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyUp(KeyCode.R)) {
            isRagdollActive = !isRagdollActive;
        }
    }
}
