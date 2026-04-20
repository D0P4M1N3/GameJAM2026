using TMPro;
using UnityEngine;

public class UI_PlayerStats : MonoBehaviour
{
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI storageText;
    public TextMeshProUGUI currencyText;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (DATA_Player.Instance != null)
        {
            CharacterStats stats = DATA_Player.Instance.CharacterStats;
            if (stats != null)
            {
                speedText.text = $"Speed: {stats.finalSpeed}";
                hpText.text = $"HP: {stats.finalMaxHP}";
                damageText.text = $"Damage: {stats.finalDamage}";
                storageText.text = $"Storage: {stats.finalStorage}";
                currencyText.text = $"Currency: {stats.Currency}";
            }
        }

    }

    void Update()
    {
        if (DATA_Player.Instance == null) return;

        CharacterStats stats = DATA_Player.Instance.CharacterStats;
        if (stats == null) return;

        speedText.text = $"Speed: {stats.finalSpeed}";
        hpText.text = $"HP: {stats.finalMaxHP}";
        damageText.text = $"Damage: {stats.finalDamage}";
        storageText.text = $"Storage: {stats.finalStorage}";
        currencyText.text = $"Currency: {stats.Currency}";
    }

}
