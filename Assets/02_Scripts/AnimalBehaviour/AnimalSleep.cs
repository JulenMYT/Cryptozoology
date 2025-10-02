using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

public class AnimalSleep : MonoBehaviour, IAnimalBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animal parent;
    [SerializeField] private float stoppingDistance = 1f;
    private Vector3 sleepScale = Vector3.zero;
    [SerializeField] private float shrinkDuration = 2f;

    private House house;
    private bool active;
    private bool reachedHouse;
    private bool isShrinking;
    private Vector3 originalScale = Vector3.one;

    public bool IsActive() => active;

    public event Action EnterHouse;
    public event Action ExitHouse;

    private void Awake()
    {
        parent = GetComponentInParent<Animal>();
    }

    public void SetHouse(GameObject houseObject)
    {
        house = houseObject.GetComponent<House>();
    }

    public void Activate()
    {
        active = true;
        reachedHouse = false;
        isShrinking = false;

        if (house != null)
            agent.SetDestination(house.transform.position);
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
        if (!active || reachedHouse) return;

        if (house)
            HandleHouseApproach();
        else
        {
            reachedHouse = true;
            EnterSleep();
        }
    }

    private void HandleHouseApproach()
    {
        float distance = Vector3.Distance(transform.position, house.transform.position);

        if (distance <= stoppingDistance && !isShrinking)
        {
            isShrinking = true;
            StartCoroutine(ShrinkOverTime(shrinkDuration));
        }

        if (!agent.pathPending && distance <= agent.stoppingDistance)
        {
            house.AddAnimal(this);
            reachedHouse = true;
            isShrinking = false;
            agent.ResetPath();
            EnterHouse?.Invoke();
        }
    }

    private IEnumerator ShrinkOverTime(float duration)
    {
        Vector3 startScale = originalScale;
        Vector3 endScale = sleepScale;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            parent.transform.localScale = Vector3.Lerp(startScale, endScale, timer / duration);
            yield return null;
        }

        parent.transform.localScale = endScale;
    }

    public void HouseDone()
    {
        ResetSize();
        ExitHouse?.Invoke();
    }

    private void ResetSize()
    {
        parent.transform.localScale = originalScale;
        isShrinking = false;
    }

    private void EnterSleep()
    {
        EnterHouse?.Invoke();
    }

    private void ExitSleep()
    {
        if (isShrinking)
            StopAllCoroutines();

        ResetSize();

        if (house != null)
        {
            house.RemoveAnimal(this);
            ExitHouse?.Invoke();
        }
    }
}
