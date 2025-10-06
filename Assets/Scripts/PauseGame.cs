using UnityEngine;

public class PauseGame : MonoBehaviour
{

    public GameObject pauseMenu;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (!pauseMenu.activeSelf)
            {
               Pause();
            }
            else {
                Continue();
            }
        }
    }
    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
    }

    public void Continue()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

   public void Quit()

    {
        Time.timeScale = 1;
        Application.Quit();
    }
}
