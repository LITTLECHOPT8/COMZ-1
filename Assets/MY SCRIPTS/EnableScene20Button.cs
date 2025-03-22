using System.Diagnostics;
using UnityEngine;

public class EnableScene20Button : MonoBehaviour
{
    // Reference to the scene20 button
    public GameObject scene20Button;

    void Start()
    {
        // Ensure the button is enabled when the scene starts
        EnableButton();
    }

    // Call this function to re-enable the button
    public void EnableButton()
    {
        if (scene20Button != null)
        {
            scene20Button.SetActive(true);  // Ensure the button is active
            scene20Button.GetComponent<UnityEngine.UI.Button>().interactable = true;  // Enable button interaction
        }
        else
        {
           // Debug.LogError("scene20Button reference is missing!");
        }
    }
}
