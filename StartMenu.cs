using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{

    public GameObject MalaStone;
    public string sceneNameToLoad;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MalaStone.transform.Rotate(0f, 0f, 0.03f);
    }

    public void RestartGame()
    {
        // Loads the currently active scene
        SceneManager.LoadScene(sceneNameToLoad);
    }

    public void ExitGame()
    {
        // Quits the application
        Application.Quit();

        // If running inside the Unity editor
    }
}
