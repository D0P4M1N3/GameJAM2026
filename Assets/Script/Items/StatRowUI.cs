using TMPro;
using UnityEngine;

public class StatRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text statNameText;
    [SerializeField] private TMP_Text statValueText;

    public void Bind(string statName, float statValue)
    {
        if (statNameText != null)
        {
            statNameText.text = statName;
        }

        if (statValueText != null)
        {
            statValueText.text = statValue.ToString("0.##");
        }
    }
}
