using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Animator anim;

    [Header("Configuración")]
    public float health = 100f;
    public float chaseRange = 10f;
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 2f; // Aumentado para dar tiempo a completar animaciones
    public float rotationSpeed = 10f; // Aumentado para rotación más rápida

    // Estados
    private enum State { Idle, Chase, PreAttack, Attack, Dead }
    private State currentState = State.Idle;

    // Componentes
    private NavMeshAgent agent;

    // Timers y control
    private float attackTimer = 0f;
    private float preAttackTimer = 0f;
    private float preAttackDuration = 0.3f; // Tiempo para rotar antes de atacar
    private bool hasDealtDamage = false; // Para evitar daño múltiple
    private int currentAttackIndex = 0;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (anim == null)
            anim = GetComponentInChildren<Animator>();

        if (agent != null)
        {
            agent.updateRotation = false; // Desactivamos rotación automática del NavMesh
            agent.updateUpAxis = false; // Evita que el agente rote en Y
        }
    }

    void Start()
    {
        // Buscar al jugador si no está asignado
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogError("❌ No se encontró al jugador con tag 'Player'");
        }
    }

    void Update()
    {
        if (currentState == State.Dead || player == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Actualizar timers
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        if (preAttackTimer > 0)
            preAttackTimer -= Time.deltaTime;

        // Máquina de estados
        switch (currentState)
        {
            case State.Idle:
                HandleIdleState(distanceToPlayer);
                break;

            case State.Chase:
                HandleChaseState(distanceToPlayer);
                break;

            case State.PreAttack:
                HandlePreAttackState(distanceToPlayer);
                break;

            case State.Attack:
                HandleAttackState(distanceToPlayer);
                break;
        }

        // Actualizar animación de velocidad SOLO si no está atacando
        if (anim != null && currentState != State.Attack && currentState != State.PreAttack)
        {
            float speed = agent.velocity.magnitude;
            anim.SetFloat("Speed", speed);
        }
        else if (anim == null)
        {
             if (Time.frameCount % 100 == 0) Debug.LogWarning($"❌ Enemy {gameObject.name} lost Animator reference!");
        }
    }

    void HandleIdleState(float distance)
    {
        if (distance < chaseRange)
        {
            currentState = State.Chase;
        }
    }

    void HandleChaseState(float distance)
    {
        // Perseguir al jugador
        if (agent.enabled)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

        // Rotar gradualmente mientras persigue
        RotateTowardsPlayer();

        // Si está en rango de ataque, pasar a PreAttack
        if (distance <= attackRange && attackTimer <= 0)
        {
            currentState = State.PreAttack;
            preAttackTimer = preAttackDuration;

            // Detener movimiento
            if (agent.enabled)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            // Detener animación de caminar
            if (anim != null)
                anim.SetFloat("Speed", 0f);
        }
    }

    void HandlePreAttackState(float distance)
    {
        // Fase de preparación: rotar hacia el jugador ANTES de atacar
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // Asegurar que Speed está en 0
        if (anim != null)
            anim.SetFloat("Speed", 0f);

        // Rotar rápidamente hacia el jugador
        RotateTowardsPlayer();

        // Verificar si el jugador se alejó
        if (distance > attackRange + 0.5f)
        {
            currentState = State.Chase;
            return;
        }

        // Cuando termine el tiempo de preparación, atacar
        if (preAttackTimer <= 0)
        {
            // Verificar que está mirando al jugador (dentro de un ángulo razonable)
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            dirToPlayer.y = 0;
            float angle = Vector3.Angle(transform.forward, dirToPlayer);

            if (angle < 15f) // Si está mirando al jugador (menos de 15 grados)
            {
                StartAttack();
            }
            else
            {
                // Si no está mirando bien, dar más tiempo para rotar
                preAttackTimer = 0.1f;
            }
        }
    }

    void HandleAttackState(float distance)
    {
        // Durante el ataque, mantener todo detenido
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (anim != null)
            anim.SetFloat("Speed", 0f);

        // Verificar si el jugador se alejó MUCHO (dar un poco de margen)
        if (distance > attackRange + 1f)
        {
            // Cancelar ataque
            if (anim != null)
            {
                anim.SetBool("IsAttacking", false);
                anim.ResetTrigger("Attack");
            }
            currentState = State.Chase;
            hasDealtDamage = false;
        }
    }

    void RotateTowardsPlayer()
    {
        if (player == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f; // Mantener rotación solo en el plano horizontal

        if (direction.sqrMagnitude < 0.01f)
            return;

        // Si el modelo está rotado 180°, invertir la dirección
        // Cambia 'direction' por '-direction' si el enemigo camina de espaldas
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    void StartAttack()
    {
        currentState = State.Attack;
        attackTimer = attackCooldown;
        hasDealtDamage = false;

        if (anim != null)
        {
            // Elegir ataque aleatorio
            currentAttackIndex = Random.Range(0, 2);

            Debug.Log($"🗡️ Iniciando ataque {currentAttackIndex}");

            // Configurar parámetros del animator
            anim.SetBool("IsAttacking", true);
            anim.SetInteger("AttackIndex", currentAttackIndex);
            anim.SetTrigger("Attack");
        }
    }

    // Este método debe ser llamado desde un Animation Event en la animación de ataque
    public void DealDamage()
    {
        // Evitar daño múltiple del mismo ataque
        if (hasDealtDamage)
        {
            Debug.Log("⚠️ Ya se hizo daño en este ataque");
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        // Verificar que el jugador sigue en rango
        if (distance <= attackRange + 0.5f) // Un poco de margen
        {
            hasDealtDamage = true;

            // Usar GameManager para restar vida
            if (GameManager.instance != null)
            {
                GameManager.instance.RestarVida();
                Debug.Log("💥 Enemigo atacó al jugador. Vidas restantes: " + GameManager.instance.vidas);
            }
            else
            {
                Debug.LogError("❌ GameManager.instance es null");
            }
        }
        else
        {
            Debug.Log($"⚠️ Jugador fuera de rango al momento del golpe (distancia: {distance})");
        }
    }

    // Este método debe ser llamado al final de la animación de ataque (Animation Event)
    public void OnAttackComplete()
    {
        Debug.Log("✅ Ataque completado");

        if (anim != null)
        {
            anim.SetBool("IsAttacking", false);
        }

        hasDealtDamage = false;

        // Verificar distancia para decidir siguiente estado
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= attackRange && attackTimer <= 0)
        {
            // Puede atacar de nuevo, ir a PreAttack
            currentState = State.PreAttack;
            preAttackTimer = preAttackDuration;
        }
        else
        {
            // Volver a perseguir
            currentState = State.Chase;
        }
    }

    public void TakeDamage(float damage)
    {
        if (currentState == State.Dead)
            return;

        health -= damage;
        Debug.Log("🩸 Enemigo recibió " + damage + " de daño. Salud: " + health + "/100");

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        currentState = State.Dead;

        Debug.Log("💀 Enemigo murió");

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (anim != null)
        {
            anim.SetBool("IsDead", true);
            anim.SetBool("IsAttacking", false);
        }

        // Desactivar colisiones
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        // Destruir después de 3 segundos
        // Check for LootDropper
        LootDropper looter = GetComponent<LootDropper>();
        if (looter != null)
        {
            looter.DropLoot();
        }

        Destroy(gameObject, 3f);
    }

    // Método para debug visual
    void OnDrawGizmosSelected()
    {
        // Rango de ataque (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Rango de persecución (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // Dirección hacia el jugador (verde)
        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}