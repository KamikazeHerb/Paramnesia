using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetSceneByName("Level2").IsValid() == true)
        {
            GameObject.FindGameObjectWithTag("NextLevelButton").SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1;
        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ToggleMusic()
    {
        if (GameObject.FindGameObjectWithTag("GameController").GetComponent<GameMusic>().musicEnabled)
        {
            GameObject.FindGameObjectWithTag("GameController").GetComponent<GameMusic>().musicEnabled = false;
        }
        else
        {
            GameObject.FindGameObjectWithTag("GameController").GetComponent<GameMusic>().musicEnabled = true;
        }
    }
}
