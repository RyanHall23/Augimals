using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vuforia;

public enum Emotions
{
    Angry,
    Happy,
    Sad,
    Scared,
    Surprised,
    MaxEmotions
}

/// <summary>
/// 
/// Thomas McKeown | Start: DATE | End: DATE
/// </summary>
public class GameManagerScript : MonoBehaviour
{
    // Game Object prefabs
    public GameObject imageTarget;
    public GameObject[] prefabEmotions;
    public GameObject[] prefabEmotionsText;
    public Text scoreText;

    // Externally defined variables
    public int gameModeNumber;
    public int maxRounds;
    public int columns;
    public int rows;
    public float xSize;
    public float ySize;

    // Private variables
    private int lastScoreCheck;
    private int currentRound;
    private int oddEmotionIndex;
    private bool scanned = false;
    private bool overlay = false;

    // Reference variables - used to avoid magic numbers
    private Vector3 screenPos = new Vector3(0, 2.340572f, 1977.315f);   // Can't directly access the camera background plane so the position must be locked and stored locally
    private Vector3 promptPos = new Vector3(-850, -450f, -2.5f);   // Position of the prompt image/text
    private Vector3 gridPos = new Vector3(-500, -450f, -2.5f);   // Position of the bottom left corner of the grid

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        // Initialise the game mode number and score data
        gameModeNumber = PlayerPrefs.GetInt("GameModeNumber");
        PlayerPrefs.SetInt("ScoreIncrement" + gameModeNumber, 1);
        PlayerPrefs.SetInt("ScoreMultiplier" + gameModeNumber, 1);
        PlayerPrefs.SetInt("CurrentScore" + gameModeNumber, 0);

        scoreText.text = "Score: 0";
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        // Check if the score has changed or not
        int currentScore = PlayerPrefs.GetInt("CurrentScore" + gameModeNumber, 0);
        if (currentScore != lastScoreCheck)
        {
            lastScoreCheck = currentScore;
            scoreText.text = "Score: " + currentScore.ToString();
        }

        // Create a state manager to track the active and inactive trackable objects
        StateManager sm = TrackerManager.Instance.GetStateManager();

        // Get an enumerable of active trackable objects
        IEnumerable<TrackableBehaviour> activeTrackables = sm.GetActiveTrackableBehaviours();

        // Loop through all active objects in the enumerable
        foreach (TrackableBehaviour obj in activeTrackables)
        {
            // If not previously scanned and still being tracked
            if (!scanned)
            {
                // Log the scan and store that the object has been scanned
                Debug.Log("Deck Found");
                scanned = true;

                // If no overlay is currently available create the overlay
                if (!overlay)
                {
                    // Log the creation of the overlay and store that the overlay has been made
                    Debug.Log("Creating overlay");
                    overlay = true;

                    // Create the overlay and initialise the current round counter
                    Debug.Log("Current round: " + currentRound);
                    currentRound = 0;
                    CreateOverlay();
                }
                else // Otherwise show the overlay
                {
                    // Log the showing of the overlay
                    Debug.Log("Showing overlay");

                    // Show the overlay
                    ShowOverlay();
                }
            }
        }

        // If the image target has been scanned previously then check if it has been lost
        if (scanned)
        {
            // Get an enumerable containing all trackable objects, in this case only the deck target
            IEnumerable<TrackableBehaviour> trackables = sm.GetTrackableBehaviours();

            // Loop through all objects in the enumerable
            foreach (TrackableBehaviour obj in trackables)
            {
                // Check if the object has been lost and is currently "NOT FOUND"
                if (obj.CurrentStatus == TrackableBehaviour.Status.NOT_FOUND)
                {
                    // Log the hiding of the overlay and store that the object needs to be scanned again
                    Debug.Log("Hiding overlay");
                    scanned = false;

                    // Hide the overlay
                    HideOverlay();
                }
            }
        }
    }

    public void NextRound()
    {
        if (currentRound < maxRounds)
        {
            Debug.Log("Starting new round");

            // Wipe the current overlay before creating a new one
            ClearOverlay();

            // Create a new overlay
            CreateOverlay();

            // Increment the round index
            ++currentRound;
        }
        else
        {
            // End of the game mode
            Debug.Log("End of game mode");

            // Wipe the overlay since the game mode is over
            ClearOverlay();

            // Display the Game Over text
            GameObject.Find("GameOverText").GetComponent<Text>().enabled = true;

            // Set the final score in the playerprefs
            int currentScore = PlayerPrefs.GetInt("CurrentScore" + gameModeNumber, 0);
            PlayerPrefs.SetInt("FinalScore" + gameModeNumber, currentScore);

            // Swap scenes here
            SceneManager.LoadScene("Highscores");
        }
    }

    /// <summary>
    /// Getter for the odd emotion that is to be selected by the user
    /// </summary>
    /// <returns>The index for the odd emotion</returns>
    public int GetEmotionIndex()
    {
        return oddEmotionIndex;
    }

    /// <summary>
    /// Creates a grid of emotions with a prompt (image or text).
    /// The user will try to select the correct emotion in order to get points.
    /// </summary>
    private void CreateOverlay()
    {
        var rand = new System.Random();

        // Choose a random prefab
        int oddCombinationIndex = rand.Next() % prefabEmotions.Length;

        // Calculate the animal and emotion that was selected
        int numAnimals = prefabEmotions.Length / (int)Emotions.MaxEmotions;
        oddEmotionIndex = oddCombinationIndex % (int)Emotions.MaxEmotions;

        // Random chance for image or text prompts
        if (rand.Next() % 2 == 0)
        {
            // Select a random animal with the emotion to be the image prompt
            int promptAnimal = rand.Next() % numAnimals;

            CreateClone(prefabEmotions[(promptAnimal * (int)Emotions.MaxEmotions) + oddEmotionIndex], promptPos, "Prompt");
        }
        else
        {
            // Otherwise use the emotion text as the prompt
            CreateClone(prefabEmotionsText[oddEmotionIndex], promptPos, "Prompt");
        }

        // Random position for the odd emotion is selected
        int oddEmotionX = (int)(Random.value * columns);
        int oddEmotionY = (int)(Random.value * rows);

        // Grid is made using the remaining emotions and one of the odd emotions at the random position
        for (int x = 0; x < columns; ++x)
        {
            for (int y = 0; y < rows; ++y)
            {
                Vector3 movement = new Vector3(x * xSize, y * ySize); // Stores the vector of movement from the top left corner of the grid
                Vector3 position = gridPos + movement; // Stores the position on the grid that the clone will be created

                // Check if the next clone is in the selected position
                if (x == oddEmotionX && y == oddEmotionY)
                {
                    // If the clone is at the selected position then the odd emotion is cloned
                    CreateClone(prefabEmotions[oddCombinationIndex], position, "Clone");
                }
                else
                {
                    // If the clone is not at the selected position then other animals and emotions are cloned
                    int emotion;

                    // Loop until the emotion selected for the grid is not the odd emotion
                    // This forces only one of the odd emotions to appear in each grid
                    do
                    {
                        emotion = rand.Next() % (int)Emotions.MaxEmotions;
                    }
                    while (emotion == oddEmotionIndex);

                    // Get a random animal
                    int animal = rand.Next() % numAnimals;

                    // Get animal and emotion combination index
                    GameObject gridEmotion = prefabEmotions[(animal * (int)Emotions.MaxEmotions) + emotion];

                    // Create the grid clone
                    CreateClone(gridEmotion, position, "Clone");
                }
            }
        }
    }

    /// <summary>
    /// Creates a clone of the specified GameObject prefab, with the specified tag, at the position relative to the background plane
    /// </summary>
    /// <param name="prefab">The GameObject prefab to create a clone of</param>
    /// <param name="position">The position relative to the background plane to create the clone at</param>
    /// <param name="tag">The tag to add to the clone</param>
    private void CreateClone(GameObject prefab, Vector3 position, string tag)
    {
        // Find the position of the object relative to the camera's background plane
        Vector3 truePosition = screenPos + position;

        // Create an instance of the prefab and store it as a clone
        GameObject clone = Instantiate(prefab, truePosition, Quaternion.identity);

        // Set the tag of the clone in order to be found later during clean up
        clone.tag = tag;
    }

    /// <summary>
    /// Loops through all clones and prompts and hides and disables the collider in each
    /// </summary>
    private void HideOverlay()
    {
        // Get an enumerable of all game objects that are tagged with "Prompt"
        IEnumerable<GameObject> prompts = GameObject.FindGameObjectsWithTag("Prompt");

        // Loop through each clone and show them by disabling the renderer and collider in each
        foreach (GameObject prompt in prompts)
        {
            prompt.GetComponent<Renderer>().enabled = false;
        }

        // Get an enumerable of all game objects that are tagged with "Clone"
        IEnumerable<GameObject> clones = GameObject.FindGameObjectsWithTag("Clone");

        // Loop through each clone and hide them by disabling the renderer and collider in each
        foreach (GameObject clone in clones)
        {
            clone.GetComponent<Renderer>().enabled = false;
            clone.GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    /// <summary>
    /// Loops through all clones and prompts and shows and enables the collider in each
    /// </summary>
    private void ShowOverlay()
    {
        // Get an enumerable of all game objects that are tagged with "Prompt"
        IEnumerable<GameObject> prompts = GameObject.FindGameObjectsWithTag("Prompt");

        // Loop through each clone and show them by enabling the renderer and collider in each
        foreach (GameObject prompt in prompts)
        {
            prompt.GetComponent<Renderer>().enabled = true;
        }

        // Get an enumerable of all game objects that are tagged with "Clone"
        IEnumerable<GameObject> clones = GameObject.FindGameObjectsWithTag("Clone");

        // Loop through each clone and show them by enabling the renderer and collider in each
        foreach (GameObject clone in clones)
        {
            clone.GetComponent<Renderer>().enabled = true;
            clone.GetComponent<BoxCollider2D>().enabled = true;
        }
    }

    /// <summary>
    /// Loops through all of the clones and prompts and deletes them
    /// </summary>
    private void ClearOverlay()
    {
        // Get an enumerable of all game objects that are tagged with "Prompt"
        IEnumerable<GameObject> prompts = GameObject.FindGameObjectsWithTag("Prompt");

        // Loop through each prompt and destroy them
        foreach (GameObject prompt in prompts)
        {
            Destroy(prompt);
        }

        // Get an enumerable of all game objects that are tagged with "Clone"
        IEnumerable<GameObject> clones = GameObject.FindGameObjectsWithTag("Clone");

        // Loop through each clone and destroy them
        foreach (GameObject clone in clones)
        {
            Destroy(clone);
        }
    }
}
