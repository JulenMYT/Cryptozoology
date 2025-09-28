using UnityEngine;

public class AnimalController : MonoBehaviour
{
    private IAnimalBehaviour currentBehaviour;

    public void SetBehaviour(IAnimalBehaviour behaviour)
    {
        if (behaviour.IsActive())
            return;

        if (currentBehaviour != null)
            currentBehaviour.Deactivate();

        currentBehaviour = behaviour;

        if (currentBehaviour != null)
            currentBehaviour.Activate();
    }
}
