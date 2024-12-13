using UnityEngine;
using TMPro;

public class NumberDisplayManager : MonoBehaviour
{
    public TextMeshProUGUI numberText; // Drag the TextMeshPro object from the canvas here

    // Call this method to update the number dynamically
    public void UpdateGraphic(float shotPercent, float attempts)
    {
        if (numberText != null)
        {
            numberText.text = $"Ratio: \n Missed - {100 - shotPercent:F1}% \n Scored - {shotPercent:F1}% \n Attempted: \n {attempts:F0} "; // Update the text
        }
        else
        {
            Debug.LogError("Number Text is not assigned!");
        }
    }
}
