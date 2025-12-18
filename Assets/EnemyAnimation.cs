using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimation : MonoBehaviour
{
    Animator anim;
    NavMeshAgent agent;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    public void UpdateMovement()
    {
        if (anim == null || agent == null) return;

        anim.SetFloat("Speed", agent.velocity.magnitude);
    }

    public void TriggerAttack()
    {
        if (anim == null) return;

        anim.SetTrigger("Attack");
    }

    public void SetDead()
    {
        if (anim == null) return;

        anim.SetBool("IsDead", true);
    }
}
