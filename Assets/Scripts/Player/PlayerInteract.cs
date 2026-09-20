using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    private bool canInteract = true; // interruptor maestro

    public void SetInteractEnabled(bool state)
    {
        canInteract = state;
    }

    void OnInteract()
    {
        if (!canInteract) return;
        Debug.Log("Interact: ¡Buscando civil cercano para rescatar o estatua para rezar!");
    }
}