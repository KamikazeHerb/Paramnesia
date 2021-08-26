using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject StartButton;
    public AudioSource MenuMusic;
    // Start is called before the first frame update
    void Start()
    {
        Button btn = StartButton.GetComponent<Button>();
        btn.onClick.AddListener(StartGame);
    }

    void StartGame()
    {
        SceneManager.LoadScene("Level1", LoadSceneMode.Single);
        SceneManager.SetActiveScene(SceneManager.GetSceneByPath("Assets/Scenes/Level1.unity"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
