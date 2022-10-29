using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject m_pauseMenu;

    static bool s_isPaused = false;
    static string s_xmlFile = "";
    public static int final_score;

    // Start is called before the first frame update
    void Start()
    {
        SetPause(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {   // this doubles as the option key in the android navigation bar
            SetPause(!s_isPaused);
        }
    }

    public static bool IsPaused()
    {
        return s_isPaused;
    }

    public void SetPause(bool setPause)
    {
        if (setPause)
        {
            Time.timeScale = 0.0f;
        }
        else
        {
            Time.timeScale = 1.0f;
        }
        m_pauseMenu.SetActive(setPause);
        s_isPaused = setPause;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Start");
    }

    public void ChooseXMLFile(string file)
    {
        s_xmlFile = file;
        SceneManager.LoadScene("Game");
    }


    public static void updateScore(int score){
        final_score = score;
    }

    public static string GetXMLFile()
    {
        return s_xmlFile;
    }
}
