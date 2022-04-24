using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathControl : MonoBehaviour
{
    [Header("Death control")]
    [SerializeField] public bool isDead;
    [SerializeField] public bool hasStarted;
    [SerializeField] private bool needStartUi;
    [SerializeField] private Look cameraScript;
    [SerializeField] private Movement movementScript;
    [SerializeField] private GameObject deathMenu;
    [SerializeField] private GameObject startMenu;
    [SerializeField] private GameObject pauseMenu;


    // Start is called before the first frame update
    void Start()
    {
        isDead = false;
        if (needStartUi)
        {
            hasStarted = false;
            movementScript.enabled = false;
            startMenu.SetActive(true);
        }
        cameraScript.enabled = true;
        deathMenu.SetActive(false);
        pauseMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        isDead = movementScript.hasDied;

        if (isDead)
        {
            cameraScript.enabled = false;
            movementScript.enabled = false;
            deathMenu.SetActive(true);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        if (needStartUi)
        {
            if (!hasStarted)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    movementScript.enabled = true;
                    startMenu.SetActive(false);
                    hasStarted = true;
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseMenu();
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PauseMenu();
            }
        }

        if (!isDead)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        
    }

    public void PauseMenu()
    {
        if (!pauseMenu.activeInHierarchy)
        {
            pauseMenu.SetActive(true);
            cameraScript.CursorUnlock();
            cameraScript.enabled = false;
            if (!needStartUi)
            {
                movementScript.enabled = false;
            }
        }
        else
        {
            pauseMenu.SetActive(false);
            cameraScript.enabled = true;
            cameraScript.CursorLock();
            if (!needStartUi)
            {
                movementScript.enabled = true;
            }
        }
    }
}
