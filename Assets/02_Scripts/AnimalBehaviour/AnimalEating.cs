using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

public class AnimalEating : MonoBehaviour, IAnimalBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    private IEdible targetFood;
    private bool active;

    public event Action<string> Eating;
    public event Action DoneEating;

    public bool IsActive() => active;

    public void SetTarget(IEdible food)
    {
        if (food == null || !food.CanBeEaten()) return;
        targetFood = food;
    }

    public void Activate()
    {
        if (targetFood == null) return;
        active = true;
        MoveToTarget();
    }

    public void Deactivate()
    {
        active = false;
        agent.ResetPath();
    }

    private void Update()
    {
        if (!active) return;

        if (targetFood == null || !targetFood.CanBeEaten())
        {
            FinishEating();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(EatRoutine());
        }
    }

    private void MoveToTarget()
    {
        if (targetFood != null && agent.isOnNavMesh)
            agent.SetDestination(((MonoBehaviour)targetFood).transform.position);
    }

    private IEnumerator EatRoutine()
    {
        active = false;
        agent.ResetPath();

        string id = targetFood.GetId();

        Eating?.Invoke(id);

        if (animator != null)
            animator.SetTrigger("Eat");

        targetFood.Eat();
        yield return new WaitForSeconds(2f);

        if (!targetFood.CanBeEaten())
            FinishEating();
        else
            active = true;
    }

    private void FinishEating()
    {
        DoneEating?.Invoke();

        targetFood = null;
    }
}
