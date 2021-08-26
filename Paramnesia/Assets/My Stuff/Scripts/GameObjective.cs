using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjective : MonoBehaviour
{
    public GameObject WinUI;

    public void Start()
    {
        WinUI.SetActive(false);
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(("Player")))
        {
            WinUI.SetActive(true);
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
