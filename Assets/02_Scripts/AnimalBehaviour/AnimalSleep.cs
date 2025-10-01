using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

public class AnimalSleep : MonoBehaviour, IAnimalBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform parentTransform;
    [SerializeField] private float stoppingDistance = 1f;

    private Transform house;
    private bool active = false;
    private bool reachedHouse = false;

    private Vector3 originalScale;
    [SerializeField] private Vector3 sleepScale = new Vector3(0.5f, 0.5f, 0.5f);
    [SerializeField] private float shrinkDuration = 2f;

    private bool isShrinking = false;

    public bool IsActive() => active;

    public void SetHouse(Transform houseTransform)
    {
        house = houseTransform;
    }

    public void Activate()
    {
        active = true;
        reachedHouse = false;
        isShrinking = false;
        originalScale = parentTransform.localScale;

        if (house != null)
        {
            agent.SetDestination(house.position);
        }
        else
        {
            reachedHouse = true;
            EnterSleep();
        }
    }

    public void Deactivate()
    {
        active = false;
        isShrinking = false;
        agent.ResetPath();
        ExitSleep();
    }

    private void Update()
    {
        if (!active) return;

        if (house != null && !reachedHouse)
        {
            float distance = Vector3.Distance(transform.position, house.position);

            if (distance <= stoppingDistance && !isShrinking)
            {
                isShrinking = true;
                StartCoroutine(ShrinkOverTime(shrinkDuration));
            }

            if (!agent.pathPending && distance <= agent.stoppingDistance)
            {
                reachedHouse = true;
                isShrinking = false;
                agent.ResetPath();
            }
        }
    }

    private IEnumerator ShrinkOverTime(float duration)
    {
        Vector3 startScale = parentTransform.localScale;
        Vector3 endScale = sleepScale;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            parentTransform.localScale = Vector3.Lerp(startScale, endScale, timer / duration);
            yield return null;
        }

        parentTransform.localScale = endScale;
    }


    private void EnterSleep()
    {
        if (house != null)
        {
            parentTransform.localScale = sleepScale;
        }
    }

    private void ExitSleep()
    {
        parentTransform.localScale = originalScale;
    }
}
