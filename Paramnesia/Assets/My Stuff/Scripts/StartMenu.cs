using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
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
        SceneManager.LoadScene(SceneManager.GetSceneAt(0).name);
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
