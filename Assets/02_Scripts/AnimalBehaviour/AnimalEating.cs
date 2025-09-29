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
    private bool eating = false;

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
        targetFood = null;
        eating = false;
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

        if (eating)
            return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            eating = true;
            StartCoroutine(EatRoutine());
        }
    }

    private void MoveToTarget()
    {
        if (targetFood == null || !agent.isOnNavMesh) return;

        Vector3 direction = ((MonoBehaviour)targetFood).transform.position - transform.position;
        direction.y = 0;
        direction.Normalize();

        float stopDistance = agent.stoppingDistance;
        Vector3 targetPos = ((MonoBehaviour)targetFood).transform.position - direction * stopDistance;

        agent.SetDestination(targetPos);
    }

    private IEnumerator EatRoutine()
    {
        if (targetFood == null) yield break;

        string id = targetFood.GetId();
        
        targetFood.Eat();

        //if (animator != null)
        //    animator.Play("Eat");

        yield return new WaitForSeconds(2f);

        Eating?.Invoke(id);

        if (targetFood != null && targetFood.CanBeEaten())
        {
            eating = false;
        }
        else
            FinishEating();
    }

    private void FinishEating()
    {
        DoneEating?.Invoke();

        targetFood = null;
        eating = false;
    }
}
