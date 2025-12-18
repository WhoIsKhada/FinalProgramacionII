using UnityEngine;

public class Collectible : MonoBehaviour
{
    public enum Type { Provisions, Health, Ammo }
    public Type collectibleType;
    public int value = 1;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    void Collect()
    {
        if (GameManager.instance == null)
        {
            Debug.LogWarning("GameManager instance not found!");
            Destroy(gameObject); // Destroy anyway to feel responsive
            return;
        }

        switch (collectibleType)
        {
            case Type.Provisions:
                GameManager.instance.SumarPuntos(value); // Assuming Score = Provisions
                Debug.Log($"Collected Provisions: {value}");
                break;
            case Type.Health:
                GameManager.instance.CurarVida(value);
                break;
            case Type.Ammo:
                // We'd need to find the weapon component. 
                // For simplicity, let's assume we can access it via the player (GameManager doesn't track ammo yet)
                // You might want to add AddAmmo() to GameManager if Weapons are globally managed, 
                // or find the weapon on the player object.
                var weapon = FindObjectOfType<RaycastWeapon>();
                if(weapon != null)
                {
                    weapon.currentAmmo = Mathf.Min(weapon.currentAmmo + value * 5, weapon.maxAmmo); // Just an example logic
                }
                break;
        }

        Destroy(gameObject);
    }
}
