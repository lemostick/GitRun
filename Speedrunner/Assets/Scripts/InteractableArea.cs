using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class InteractableArea : MonoBehaviour
{
    [SerializeField] private GameObject marker;
    [SerializeField] private Collider trigger = default;

    //[SerializeField] private bool _once = default;

    public UnityEvent OnEnter;
    public UnityEvent OnExit;

    public UnityEvent OnInteractOnce;
    public UnityEvent OnInteract;

    private bool canInteract = false;
    private bool onceDone = false;

    private void Start()
    {
        if (marker != null)
            marker.SetActive(false);
    }

    private void OnValidate()
    {
        if(trigger == null)
            trigger = GetComponent<Collider>();
    }

    private void Update()
    {
        if (canInteract == false)
            return;

        if (Input.GetKeyUp(KeyCode.E))
        {
            Interact();
        }
    }

    public void Interact()
    {
        OnInteract?.Invoke();
        if (!onceDone)
        {
            OnInteractOnce?.Invoke();
            onceDone = true;
        }

        //if (_once)
        //{
        //    Activate(false);
        //}
    }

    public void Activate(bool enable)
    {
         trigger.enabled = enable;
    }

    //public void Deactivate()
    //{
    //    _trigger.enabled = true;
    //}


    private void OnTriggerEnter(Collider collision)
    {
        var hero = collision.GetComponent<CharacterController>();
        if (hero != null)
        {
            canInteract = true;
            if (marker != null)
                marker.SetActive(true);

            OnEnter?.Invoke();
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        var hero = collision.GetComponent<CharacterController>();
        if (hero != null)
        {
            canInteract = false;
            if (marker != null)
                marker.SetActive(false);

            OnExit?.Invoke();
        }
    }
}
