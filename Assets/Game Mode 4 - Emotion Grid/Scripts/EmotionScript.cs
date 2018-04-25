using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 
/// Thomas McKeown | Start: DATE | End: DATE
/// </summary>
public class EmotionScript : MonoBehaviour
{
    public Emotions index;

    /// <summary>
    /// Check for correct emotion and add to score
    /// </summary>
    private void OnMouseDown()
    {
        // Only allow clones to be clickable (not prompts or the original objects)
        if (this.tag == "Clone")
        {
            // Log that the object was selected
            Debug.Log(this.name + " selected");

            // Get the camera object in order to access the script
            GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");

            // Get the script contained by the camera
            var script = camera.GetComponent<GameManagerScript>();

            // Get the game mode number from the playerprefs
            int gameModeNumber = PlayerPrefs.GetInt("GameModeNumber"); // The index of the gamemode

            // Check if the this object is the same type of emotion as the odd emotion from the main script
            if (script.GetEmotionIndex() == (int)index)
            {
                // If the emotions are the same type then the correct emotion was selected and points are added
                // Log that the correct emotion was selected
                Debug.Log("Correct emotion selected");

                // Display that the correct emotion was selected
                GameObject.Find("CorrectText").GetComponent<Text>().text = "Correct";

                // Get score data from playerprefs
                int scoreIncrement = PlayerPrefs.GetInt("ScoreIncrement" + gameModeNumber, 1); // The number of score increase per correct answer
                int scoreMultiplier = PlayerPrefs.GetInt("ScoreMultiplier" + gameModeNumber, 1); // The score multiplier based on consecutive correct answers
                int currentScore = PlayerPrefs.GetInt("CurrentScore" + gameModeNumber, 0); // The current score in this game
                int scoreToAdd = scoreIncrement * scoreMultiplier;

                PlayerPrefs.SetInt("CurrentScore" + gameModeNumber, currentScore + scoreToAdd); // Set the new current score
                PlayerPrefs.SetInt("ScoreMultiplier" + gameModeNumber, scoreMultiplier + 1); // Increase the score multiplier by one
            }
            else
            {
                // Log that an incorrect emotion was selected
                Debug.Log("Incorrect emotion selected");

                // Display that the incorrect emotion was selected
                GameObject.Find("CorrectText").GetComponent<Text>().text = "Incorrect";

                // Reset the score multiplier on incorrect answers
                PlayerPrefs.SetInt("ScoreMultiplier" + gameModeNumber, 1);
            }

            // Trigger the next round
            script.NextRound();
        }
    }
}
