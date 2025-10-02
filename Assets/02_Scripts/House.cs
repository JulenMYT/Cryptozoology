using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    private List<AnimalSleep> animals = new();

    public void AddAnimal(AnimalSleep animal)
    {
        if (!animals.Contains(animal))
        {
            animals.Add(animal);
        }
    }

    public void RemoveAnimal(AnimalSleep animal)
    {
        if (animals.Contains(animal))
        {
            animals.Remove(animal);
        }
    }

    private void OnDestroy()
    {
        foreach (var animal in animals)
        {
            if (animal != null && animal.IsActive())
            {
                animal.HouseDone();
            }
        }
    }
}
