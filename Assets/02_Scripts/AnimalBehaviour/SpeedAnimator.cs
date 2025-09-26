using UnityEngine;
using UnityEngine.AI;

public class SpeedAnimator : MonoBehaviour
{
    public Animator animator;
    public NavMeshAgent agent;

    void Update()
    {
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
    }
}
