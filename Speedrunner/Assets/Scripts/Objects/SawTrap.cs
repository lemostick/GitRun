using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawTrap : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float speed;
    [SerializeField] private List<Transform> Waypoints;
    private int nextID;
    int idChangeValue = 1;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        nextID = 0;
    }

    // Update is called once per frame
    void Update()
    {
        doPatrol();
    }

    private void doPatrol()
    {
        if (Vector3.Distance(transform.position, Waypoints[nextID].position) > 0.1f)
        {
            MoveTo(Waypoints[nextID].position);
        }
        else
        {
            Reached();
        }
    }

    private void MoveTo(Vector3 position)
    {
        //anim.SetBool("isRunning", true);
        var dir = (position - transform.position).normalized;
        rb.MovePosition(transform.position + dir * speed * Time.fixedDeltaTime);
    }

    void Reached()
    {

        if (nextID == Waypoints.Count - 1)
        {
            idChangeValue = -1;
        }
        if (nextID == 0)
        {
            idChangeValue = +1;
        }
        nextID += idChangeValue;
    }
}
