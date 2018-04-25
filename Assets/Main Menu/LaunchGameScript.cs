using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script to launch a specified game mode
/// Thomas McKeown | Start: DATE | End: DATE 
/// </summary>
public class LaunchGameScript : MonoBehaviour
{
    public int gameModeNumber; // ID of the game mode to be launched
    public string gameModeName; // Name of the game mode scene to be launched

    /// <summary>
    /// Launches the game mode scene specified externally and sets the game mode number in playerprefs
    /// Thomas McKeown | Start: DATE | End: DATE
    /// </summary>
    public void LaunchGameMode()
    {
        // Set the game mode number in playerprefs so each game mode can use it
        PlayerPrefs.SetInt("gameModeNumber", gameModeNumber);

        // Load the specified scene
        SceneManager.LoadScene(gameModeName);
    }
}
