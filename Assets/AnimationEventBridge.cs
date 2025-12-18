using UnityEngine;

// Este script va en el GameObject que tiene el Animator (el hijo)
// Sirve para redirigir los Animation Events al EnemyController del padre

public class AnimationEventBridge : MonoBehaviour
{
    private EnemyController controller;

    void Awake()
    {
        // Buscar el EnemyController en el padre
        controller = GetComponentInParent<EnemyController>();

        if (controller == null)
        {
            Debug.LogError("❌ AnimationEventBridge: No se encontró EnemyController en el padre");
        }
    }

    // Estos métodos son llamados por los Animation Events
    public void DealDamage()
    {
        if (controller != null)
        {
            controller.DealDamage();
        }
        else
        {
            Debug.LogError("❌ AnimationEventBridge: Controller es null en DealDamage");
        }
    }

    public void OnAttackComplete()
    {
        if (controller != null)
        {
            controller.OnAttackComplete();
        }
        else
        {
            Debug.LogError("❌ AnimationEventBridge: Controller es null en OnAttackComplete");
        }
    }
}