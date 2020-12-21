﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // Set in inspector
    public GameObject redEnemyPrefab;
    public GameObject blueEnemyPrefab;
    public float checkpointRange;

    // Set at Start()
    public GameObject enemies;
    Wave currentWave;
    public string currentWaveName;
    float timer;

    public bool isWaveSpawned;
    public bool isWaveCleared;
    public int numLeft;
    public int numSpawned;
    
    // Start is called before the first frame update
    void Start()
    {
        enemies = new GameObject("enemies");

        currentWave = CreateWaves();
        currentWaveName = currentWave.Name;
        timer = currentWave.SpawnDelay;     // Allows the first enemy to spawn immediately
    }

    // Update is called once per frame
    void Update()
    {
        if(gameObject.GetComponent<StateManager>().currentMenuState == MenuState.game) {
            if(Input.GetKeyDown(KeyCode.Space)
            && !currentWave.HasCleared
            && !currentWave.HasSpawned) {
                currentWave.StartSpawn();
            }

            // The current wave has been cleared
            if(currentWave.HasCleared) {
                // If there is a next wave, that wave is the new current wave
                if(currentWave.NextWave != null) {
                    currentWave = currentWave.NextWave;
                    currentWaveName = currentWave.Name;
                    timer = currentWave.SpawnDelay;
                }
                // No Waves left, the game is over
                else
                    gameObject.GetComponent<StateManager>().ChangeMenuState(MenuState.gameOver);
            }
			else {
                CheckEnemies();
            }

            if(currentWave != null) {
                isWaveSpawned = currentWave.HasSpawned;
                isWaveCleared = currentWave.HasCleared;
                numLeft = currentWave.EnemiesLeft;
                numSpawned = currentWave.EnemiesSpawned;
            }
        }
    }

	void FixedUpdate()
	{
        if(currentWave.HasSpawned
            && !currentWave.HasCleared) {
            // Increments timer
            timer += Time.deltaTime;

            if(timer >= currentWave.SpawnDelay
                && currentWave.EnemiesSpawned < currentWave.WaveCount) {
                // Resets timer and spawns an enemy
                timer = 0.0f;
                SpawnEnemy(currentWave.EnemyPrefab);
            }
        }
    }

    /// <summary>
    /// A method to hold all lines of code dealing with creating waves
    /// </summary>
    /// <returns>The first wave</returns>
    Wave CreateWaves()
	{
		Wave wave3 = new Wave("Wave 3", blueEnemyPrefab, 3, 1.5f);
		Wave wave2 = new Wave("Wave 2", redEnemyPrefab, 2, 1.0f, wave3);
        Wave wave1 = new Wave("Wave 1", blueEnemyPrefab, 4, 0.5f, wave2);

        return wave1;
    }

    /// <summary>
    /// Creates an enemy of the given prefab and places it in the scene
    /// </summary>
    /// <param name="enemy">The prefab of the to-be created enemy</param>
    void SpawnEnemy(GameObject enemy)
    {
		// Calculate the position of the entrance checkpoint, zero-ing out its y-value
		GameObject spawnPoint = gameObject.GetComponent<LevelManager>().checkpoints.transform.Find("entrance").gameObject;
        Vector3 position = spawnPoint.transform.position;
        position.y = 0.0f;
        // Creates an enemy and adds it to the parent GO
        GameObject newEnemy = Instantiate(enemy, position, Quaternion.identity, enemies.transform);
        newEnemy.name = "enemy" + currentWave.EnemiesSpawned;
        newEnemy.GetComponent<Enemy>().currentCheckpoint = spawnPoint;

        // Updates the Wave object that an enemy was spawned from it
        currentWave.EnemySpawned();
    }

    void CheckEnemies()
	{
        List<GameObject> enemiesToDestroy = new List<GameObject>();
        Vector3 exitPos = gameObject.GetComponent<LevelManager>().checkpoints.transform.Find("exit").position;

		// Loops through each enemy child, checking if they need to be removed
        for(int child = 0; child < enemies.transform.childCount; child++) {
			GameObject enemy = enemies.transform.GetChild(child).gameObject;
			// If the enemy has no health
			if(enemy.GetComponent<Enemy>().health <= 0.0f) {
                enemiesToDestroy.Add(enemy);
				RemoveEnemy(enemy);
			}
			// If the enemy has reached the exit
			else if(Vector3.Distance(
						enemy.transform.position,
						exitPos) <= checkpointRange) {
				enemiesToDestroy.Add(enemy);
				RemoveEnemy(enemy);
				gameObject.GetComponent<Player>().TakeDamage(enemy.GetComponent<Enemy>().damage);
			}
		}

        // Destroys every GO in the created list
        foreach(GameObject enemy in enemiesToDestroy) {
            Destroy(enemy);
		}
	}

	void RemoveEnemy(GameObject enemy)
	{
		enemy.SetActive(false);
		currentWave.EnemyRemoved();
	}
}
