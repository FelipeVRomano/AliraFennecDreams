using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.InputSystem;

public class FDInteractEvent : MonoBehaviour
{
    public event Action InteractEvent;
    public UnityEvent InteractionEvent;

    public void Interacting(InputAction.CallbackContext valueInput)
    {
        if (valueInput.performed)
        {
            InteractEvent?.Invoke();
            InteractionEvent?.Invoke();
        }
    }
}
