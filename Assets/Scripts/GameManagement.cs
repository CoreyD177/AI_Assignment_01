using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagement : MonoBehaviour
{
    //Restart the game from restart menu
    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }
    //Restarts the time scale at beginning of game (Attached to start button) in case it was paused elsewhere
    public void StartTime()
    {
        Time.timeScale = 1f;
    }
    //Exits the game from any of the menus
    public void ExitGame()
    {
#if UNITY_EDITOR
        //If game is being played in Unity Editor when we activate this function, then get out of play mode
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
