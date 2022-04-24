using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingingAxe : MonoBehaviour
{
    //[SerializeField] private Transform connectionPoint;
    [SerializeField] private float maxAngle;
    [SerializeField] private float speed;
    [SerializeField] private float startTime;

    Quaternion start;
    Quaternion end;



    // Start is called before the first frame update
    void Start()
    {
        start = PendulumRotation(maxAngle);
        end = PendulumRotation(-maxAngle);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        startTime += Time.deltaTime;
        transform.rotation = Quaternion.Lerp(start, end, (Mathf.Sin(startTime * speed + Mathf.PI / 2) + 1f) / 2);
    }

    void resetTimer()
    {
        startTime = 0f;
    }

    Quaternion PendulumRotation(float angle)
    {
        var pendulumRotation = transform.rotation;
        var angleZ = pendulumRotation.eulerAngles.z + angle;

        if (angleZ > 180)
        {
            angleZ -= 360;
        }
        else if (angleZ < -180)
        {
            angleZ += 360;
        }

        pendulumRotation.eulerAngles = new Vector3(pendulumRotation.eulerAngles.x, pendulumRotation.eulerAngles.y, angleZ);
        return pendulumRotation;
    }
}
