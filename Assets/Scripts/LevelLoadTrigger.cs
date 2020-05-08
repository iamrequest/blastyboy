using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class LevelLoadTrigger : MonoBehaviour {
    public string levelName;
    private void OnTriggerEnter(Collider other) {
        SteamVR_LoadLevel.Begin(levelName);
    }
}
