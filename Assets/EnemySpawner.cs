using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnRadius = 20f;
    public float spawnInterval = 5f;
    public int maxEnemies = 10;

    public GameObject[] enemyPrefabs; // Change from single GameObject to Array

    private float timer;

    void Update()
    {
        // Simple limitation: count by tag could be expensive in large games, but fine for simple projects.
        // A better approach is to keep a list or static counter.
        if (GameObject.FindGameObjectsWithTag("Enemy").Length >= maxEnemies) 
            return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    
    void SpawnEnemy()
    {
        Vector3 randomPoint = GetRandomPointOnNavMesh();
        
        if (randomPoint != Vector3.zero)
        {
            // Pick a random prefab from the array if available
            if (enemyPrefabs != null && enemyPrefabs.Length > 0)
            {
                GameObject prefabToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                Instantiate(prefabToSpawn, randomPoint, Quaternion.identity);
                Debug.Log($"Spawned {prefabToSpawn.name} at {randomPoint}");
            }
            else if (enemyPrefab != null) // Fallback to single prefab
            {
                Instantiate(enemyPrefab, randomPoint, Quaternion.identity);
                Debug.Log($"Spawned Enemy (Single) at {randomPoint}");
            }
            else
            {
                Debug.LogWarning("EnemySpawner: No prefabs assigned!");
            }
        }
    }

    Vector3 GetRandomPointOnNavMesh()
    {
        Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
        randomDirection += transform.position;
        
        NavMeshHit hit;
        // SamplePosition ensures we snap to the NavMesh (valid walkable area)
        // 5f is the max distance to look for a NavMesh point from our random point
        if (NavMesh.SamplePosition(randomDirection, out hit, 5f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        return Vector3.zero; // Failed to find a spot
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
