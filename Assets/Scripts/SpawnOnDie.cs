using UnityEngine;

public class SpawnOnDie : MonoBehaviour
{
    public GameObject[] prefabSpawneable;


    private void OnDisable()
    {
        Spawn();
    }

    public void Spawn()
    {
        foreach (var prefab in prefabSpawneable)
        {
            Instantiate(prefab, transform.position, Quaternion.identity);
        }
    }
}
