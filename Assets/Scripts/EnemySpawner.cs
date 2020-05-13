using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Valve.VR.InteractionSystem;

public class EnemySpawner : MonoBehaviour {
    private const bool DEBUG = true;
    public bool isGameActive;
    public TextMeshProUGUI isGameActiveUI;
    public TextMeshProUGUI maxNumEnemiesUI;

    [Header("Enemy Management")]
    public GameObject enemyPrefab;
    public List<GameObject> activeEnemies;
    public int numberOfActiveEnemies, maxNumberOfActiveEnemies;
    private int numberOfAliveEnemies;
    public float destroyDeadEnemyDelay;

    [Header("Spawn Logic")]
    public int maxSpawnAttempts;
    public float minSpawnDelay;
    private float lastSpawnTime;
    public List<GameObject> spawnableAreas;
    public Vector3 verticalSpawnOffset;
    public float groundCheckRaycastDistance;
    public string spawnableGroundTag;

    public LayerMask groundLayerMask;

    // Start is called before the first frame update
    void Start() {
        lastSpawnTime = Time.time;
        maxNumEnemiesUI.text = "Max Num Enemies: " + maxNumberOfActiveEnemies;
    }

    // Update is called once per frame
    void Update() {
        if (isGameActive) {
            // If we still can spawn more enemies
            if (numberOfActiveEnemies < maxNumberOfActiveEnemies) {
                // Wait a min delay
                if (Time.time > lastSpawnTime + minSpawnDelay) {
                    SpawnEnemy();
                }
            }
        }

        HandleManuallySpawn();
        DrawSpawnDimensions(0);
    }

    void SpawnEnemy() {
        int spawnableAreaIndex;
        int spawnAttempts = 0;
        bool wasSpawnSuccessful = false;

        do {
            spawnAttempts++;

            // -- Find a position to spawn the enemy at
            // Pick a spawn area at random
            spawnableAreaIndex = Mathf.FloorToInt(Random.Range(0, spawnableAreas.Count));

            Vector3 spawnPositionOffset;
            spawnPositionOffset.x = Random.Range(-1 * GetSpawnDimension(0, spawnableAreaIndex), GetSpawnDimension(0, spawnableAreaIndex));
            spawnPositionOffset.y = Random.Range(-1 * GetSpawnDimension(1, spawnableAreaIndex), GetSpawnDimension(1, spawnableAreaIndex));
            spawnPositionOffset.z = Random.Range(-1 * GetSpawnDimension(2, spawnableAreaIndex), GetSpawnDimension(2, spawnableAreaIndex));

            // Raycast from our calculated position downwards, to find a spot to spawn
            RaycastHit rayHit;
            if (Physics.Raycast(spawnableAreas[spawnableAreaIndex].transform.position + spawnPositionOffset, Vector3.down, out rayHit, groundLayerMask)) {
                // If what we found is spawnable ground...
                if (rayHit.collider.gameObject.CompareTag(spawnableGroundTag)) {
                    // Spawn the enemy at the spawn position, just above the spawnable ground we just found
                    Vector3 spawnPosition = spawnableAreas[spawnableAreaIndex].transform.position + spawnPositionOffset;
                    spawnPosition.y = rayHit.point.y;

                    GameObject enemy = Instantiate(enemyPrefab,
                            spawnPosition + verticalSpawnOffset,
                            Quaternion.Euler(0, Random.Range(0f, 360f), 0f));
                    activeEnemies.Add(enemy);

                    Damagable damageable = enemy.GetComponent<Damagable>();
                    if (damageable) {
                        damageable.onHeathDepleted.AddListener(
                            delegate {
                                RegisterDeadEnemy(enemy);
                            }
                        );
                    }

                    lastSpawnTime = Time.time;
                    numberOfActiveEnemies++;
                    numberOfAliveEnemies++;
                    wasSpawnSuccessful = true;
                }
            }
        } while (!wasSpawnSuccessful && spawnAttempts < maxSpawnAttempts);
    }

    private void DrawSpawnDimensions(int spawnableAreaIndex) {
        if (DEBUG) {
            Debug.DrawRay(spawnableAreas[spawnableAreaIndex].transform.position, new Vector3(GetSpawnDimension(0, spawnableAreaIndex), 0, 0), Color.red);
            Debug.DrawRay(spawnableAreas[spawnableAreaIndex].transform.position, new Vector3(-1 * GetSpawnDimension(0, spawnableAreaIndex), 0, 0), Color.red);

            Debug.DrawRay(spawnableAreas[spawnableAreaIndex].transform.position, new Vector3(0, GetSpawnDimension(1, spawnableAreaIndex), 0), Color.green);
            Debug.DrawRay(spawnableAreas[spawnableAreaIndex].transform.position, new Vector3(0, -1 * GetSpawnDimension(1, spawnableAreaIndex), 0), Color.green);

            Debug.DrawRay(spawnableAreas[spawnableAreaIndex].transform.position, new Vector3(0, 0, GetSpawnDimension(2, spawnableAreaIndex)), Color.blue);
            Debug.DrawRay(spawnableAreas[spawnableAreaIndex].transform.position, new Vector3(0, 0, -1 * GetSpawnDimension(2, spawnableAreaIndex)), Color.blue);
        }
    }

    private float GetSpawnDimension(int dim, int index) {
        Vector3 scale = spawnableAreas[index].transform.localScale;
        switch (dim) {
            case 0: return scale.x / 2;
            case 1: return scale.y / 2;
            default: return scale.z / 2;
        }
    }

    public void RegisterDeadEnemy(GameObject enemy) {
        numberOfAliveEnemies--;
        StartCoroutine(DestroyAfterDelay(enemy));
    }

    public IEnumerator DestroyAfterDelay(GameObject target) {
        yield return new WaitForSeconds(destroyDeadEnemyDelay);
        lastSpawnTime = Time.time;

        // TODO: Does this break if we're force grabbing the target?
        if (target != null) {
            numberOfActiveEnemies--;
            activeEnemies.Remove(target);
            Destroy(target);
        }
    }

    public void SetGameStatus() {
        SetGameStatus(!isGameActive);
    }
    public void SetGameStatus(bool isGameActive) {
        if (!isGameActive) {
            foreach (GameObject enemy in activeEnemies) {
                activeEnemies.Remove(enemy);
                Destroy(enemy);
            }

            numberOfActiveEnemies = 0;
            numberOfAliveEnemies = 0;
            isGameActiveUI.text = "Game stopped";
        } else {
            lastSpawnTime = Time.time;
            isGameActiveUI.text = "Game is active";
        }

        // Attempt to heal the player
        Damagable d = Player.instance.GetComponent<Damagable>();
        if (d != null) {
            d.currentHealth = d.maxHealth;
            d.UpdateUI();
        }


        this.isGameActive = isGameActive;
    }


    private void HandleManuallySpawn() {
        if (Input.GetKeyUp(KeyCode.S)) {
            if (numberOfActiveEnemies < maxNumberOfActiveEnemies) {
                SpawnEnemy();
            }
        }

        if (Input.GetKeyUp(KeyCode.D)) {
            if (numberOfActiveEnemies > 0) {
                RegisterDeadEnemy(activeEnemies[numberOfActiveEnemies - 1]);
            }
        }
    }

    public void AddMaxEnemyCount() {
        maxNumberOfActiveEnemies++;
        maxNumEnemiesUI.text = "Max Num Enemies: " + maxNumberOfActiveEnemies;
    }
    public void SubMaxEnemyCount() {
        if (maxNumberOfActiveEnemies > 0) {
            maxNumberOfActiveEnemies--;
        }
        maxNumEnemiesUI.text = "Max Num Enemies: " + maxNumberOfActiveEnemies;
    }
}
