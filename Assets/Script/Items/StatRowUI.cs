using TMPro;
using UnityEngine;

public class StatRowUI : MonoBehaviour
{
    private const string ValueStatName = "Value";

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
            statValueText.text = FormatStatValue(statName, statValue);
        }
    }

    private static string FormatStatValue(string statName, float value)
    {
        bool includePercentSuffix = !string.Equals(statName, ValueStatName, System.StringComparison.OrdinalIgnoreCase);
        string format = includePercentSuffix ? "+0.##;-0.##;0" : "0.##;-0.##;0";
        return value.ToString(format) + (includePercentSuffix ? "%" : string.Empty);
    }
}
