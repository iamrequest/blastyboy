using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class VRPlayerLoader : MonoBehaviour {
    public GameObject playerPrefab;

    // Start is called before the first frame update
    void Awake() {
        InstantiatePlayer();
    }

    private void Start() {
        InstantiatePlayer();
    }

    private void InstantiatePlayer() {
        Player player = Player.instance;
        if (player == null) {
            GameObject playerGameObject = Instantiate(playerPrefab);
            DontDestroyOnLoad(playerGameObject);
        }

        if (Player.instance != null) {
            Player.instance.transform.position = transform.position;
            Player.instance.transform.rotation = transform.rotation;
        }
    }
}
