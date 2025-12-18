using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    public float damage = 25f;
    public GameObject impactEffect; // Efecto de impacto opcional

    void Start()
    {
        Debug.Log("🔹 Bala creada: " + gameObject.name);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("💥 Bala colisionó con: " + collision.collider.gameObject.name + " (Tag: " + collision.collider.tag + ")");

        // Intentar obtener EnemyController del objeto golpeado
        EnemyController enemy = collision.collider.GetComponent<EnemyController>();

        // Si no está en el objeto directo, buscar en el padre
        if (enemy == null)
        {
            enemy = collision.collider.GetComponentInParent<EnemyController>();
            Debug.Log("🔍 Buscando en padre...");
        }

        // Si tampoco está en el padre, buscar en los hijos
        if (enemy == null)
        {
            enemy = collision.collider.GetComponentInChildren<EnemyController>();
            Debug.Log("🔍 Buscando en hijos...");
        }

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Debug.Log("✅ Daño aplicado a enemigo: " + damage);
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró EnemyController en: " + collision.collider.gameObject.name);

            // Debug adicional: mostrar todos los componentes del objeto
            Component[] components = collision.collider.GetComponents<Component>();
            Debug.Log("📋 Componentes en " + collision.collider.name + ":");
            foreach (Component comp in components)
            {
                Debug.Log("  - " + comp.GetType().Name);
            }
        }

        // Spawn efecto de impacto
        if (impactEffect != null)
        {
            GameObject effect = Instantiate(impactEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // Destruir la bala
        Destroy(gameObject);
    }
}