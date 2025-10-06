using UnityEngine;
using UnityEngine.SceneManagement;

public class Control : MonoBehaviour
{

    public string MainScene;

    private void Start()
    {
        NextScene();
    }
    public void NextScene()
    {
        SceneManager.LoadScene("MainScene");
    }
}