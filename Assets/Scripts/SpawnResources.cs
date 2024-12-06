using UnityEngine;
using Unity.Netcode;

public class SpawnResources : NetworkBehaviour
{
    public GameObject woodPrefab;
    public GameObject rockPrefab;
    public int resourceCount = 10; 
    public Vector3 spawnArea = new Vector3(-800f, 1f, 80f); 

    void Start()
    {
        if (IsServer)
        {
            Spawn();
        }
    }
    public void TriggerSpawnResources() //button click spawn resource
    {
        if (IsServer)
        {
            Spawn();
        }
    }

    void Spawn()
    {
        for (int i = 0; i < resourceCount; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(-spawnArea.x / 2, spawnArea.x / 2),
                spawnArea.y,
                Random.Range(-spawnArea.z / 2, spawnArea.z / 2)
            );
            //randomPosition = new Vector3(-60,1,10);

            GameObject woodInstance = Instantiate(woodPrefab, randomPosition, Quaternion.identity);
            GameObject rockInstance = Instantiate(rockPrefab, randomPosition, Quaternion.identity);
            woodInstance.GetComponent<NetworkObject>().Spawn();
            rockInstance.GetComponent<NetworkObject>().Spawn();
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualize the spawn area in the editor
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.zero, spawnArea);
    }
}

