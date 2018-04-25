using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script to launch the highscores scene
/// Thomas McKeown | Start: DATE | End: DATE 
/// </summary>
public class LaunchHighscores : MonoBehaviour
{
    public string sceneName; // Name of scene that will be launched

    /// <summary>
    /// Launches the highscores scene in single mode
    /// Thomas McKeown | Start: DATE | End: DATE
    /// </summary>
    public void LaunchScene()
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
