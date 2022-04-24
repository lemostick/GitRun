using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class TimerControl : MonoBehaviour
{
    [Header("Timer control")]
    [SerializeField] private Slider slider;
    [SerializeField] private float maxTimeValue;
    [SerializeField] private float currentTime;
    [SerializeField] private float playerTime;
    [SerializeField] private TMP_Text timerText;

    [Header("Player movement script for death")]
    [SerializeField] private DeathControl deathControl;
    [SerializeField] private bool isDead;

    public UnityEvent onTimerOut;

    // Start is called before the first frame update
    void Awake()
    {
        slider.maxValue = maxTimeValue;
        currentTime = maxTimeValue;
        slider.value = currentTime;
    }

    // Update is called once per frame
    void Update()
    {
        timerText.text = playerTime.ToString("0.0");

        if (deathControl.hasStarted && !deathControl.isDead)
        {
            playerTime += Time.deltaTime;

            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                slider.value = currentTime;
            }
            else
            {
                onTimerOut?.Invoke();
            }
        }
    }
}
