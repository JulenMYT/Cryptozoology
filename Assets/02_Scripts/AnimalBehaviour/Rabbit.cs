using UnityEngine;

public class Rabbit : MonoBehaviour, IAnimal
{
    [SerializeField] private AnimalController controller;
    [SerializeField] private AnimalWander wanderBehaviour;

    public void Initialize()
    {
        controller.SetBehaviour(wanderBehaviour);
    }
}
