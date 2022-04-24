using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoad : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    public bool gameHasEnded = false;
    public Animator fade;
    public float transitionTime = 1f;
    public float restartDelay = 2f;

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadNextLevel()
    {
        StartCoroutine(LoadLevelIndex(SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void LoadLevelInd(int index)
    {
        StartCoroutine(LoadLevelIndex(index));
    }

    public void LoadLevelStr(string name)
    {
        StartCoroutine(LoadLevelString(name));
    }


    public IEnumerator LoadLevelIndex(int index)
    {
        fade.SetTrigger("Start");
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(transitionTime);
        AsyncOperation operation = SceneManager.LoadSceneAsync(index);
    }

    public IEnumerator LoadLevelString(string LevelName)
    {
        fade.SetTrigger("Start");
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(transitionTime);
        AsyncOperation operation = SceneManager.LoadSceneAsync(LevelName);
    }

    public void EndGame()
    {
        if (gameHasEnded == false)
        {
            gameHasEnded = true;
            fade.SetTrigger("Start");
            Invoke("Restart", restartDelay);
        }
        
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }    
}
