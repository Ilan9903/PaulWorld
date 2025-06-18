using UnityEngine;
using UnityEngine.SceneManagement; // Don't forget this namespace!

public class ChangeScene : MonoBehaviour
{
    [Tooltip("The name or index of the scene to load.")]
    public string sceneToLoad = "Scene 1"; // You can set this in the Inspector to "1" or "Scene1"

    [Tooltip("The tag of the player object that can trigger the scene change.")]
    public string playerTag = "Player";

    private bool canChangeScene = false; // Flag to track if the player is in range

    void Update()
    {
        // Check if the player is in range and the 'E' key is pressed
        if (canChangeScene && Input.GetKeyDown(KeyCode.E))
        {
            LoadNextScene();
        }
    }

    // Called when another collider enters this trigger
    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering collider is the player
        if (other.CompareTag("Player"))
        {
            canChangeScene = true;
            Debug.Log("Player entered trigger. Press E to change scene.");
        }
    }

    // Called when another collider exits this trigger
    private void OnTriggerExit(Collider other)
    {
        // Check if the exiting collider is the player
        if (other.CompareTag("Player"))
        {
            canChangeScene = false;
            Debug.Log("Player exited trigger. Cannot change scene.");
        }
    }

    void LoadNextScene()
    {
        // Ensure the scene to load exists in the Build Settings
        if (Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("Scene '" + sceneToLoad + "' cannot be loaded. Make sure it's added to File > Build Settings > Scenes In Build.");
        }
    }
}