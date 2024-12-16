using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeadScene : MonoBehaviour
{
    public GameObject Restart;
    public GameObject Exit;
    public PlayerMovement playerMovement;
    public ManagePlayer playerManage;
    private bool isdead;

    // Start is called before the first frame update
    void Start()
    {
        isdead = false;
        Restart.SetActive(false);
        
        Exit.SetActive(false);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)&& !isdead)
        {
            if (Restart.activeSelf==true)
            {
                playerMovement.enabled = true;
                playerManage.enabled = true;
                Restart.SetActive(false);
                Exit.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the window
                Cursor.visible = false; // Hide the cursor
            }
            else
            {
                dead();
            }
        }
    }
    public void deadSceneActivate()
    {
        isdead = true;
        Invoke(nameof(dead), 1f);
    }

    private void dead()
    {
        playerMovement.enabled = false;
        playerManage.enabled = false;
        Restart.SetActive(true);
        Exit.SetActive(true);
        Cursor.lockState = CursorLockMode.None; // Free the cursor
        Cursor.visible = true; // Make the cursor visible
    }
    public void RestartGame()
    {
        // Loads the currently active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        // Quits the application
        Application.Quit();

    }
}
