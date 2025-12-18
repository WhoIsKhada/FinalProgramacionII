using UnityEngine;
using System.Collections.Generic;

public class LootDropper : MonoBehaviour
{
    [System.Serializable]
    public class DropItem
    {
        public GameObject prefab;
        [Range(0, 100)] public float dropChance;
    }

    public List<DropItem> possibleDrops;

    public void DropLoot()
    {
        foreach (var item in possibleDrops)
        {
            float roll = Random.Range(0f, 100f);
            if (roll <= item.dropChance)
            {
                if (item.prefab != null)
                {
                    Instantiate(item.prefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
                    Debug.Log($"Dropped {item.prefab.name}");
                }
            }
        }
    }
}
