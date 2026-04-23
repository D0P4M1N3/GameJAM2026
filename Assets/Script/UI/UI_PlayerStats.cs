using TMPro;
using UnityEngine;

public class UI_PlayerStats : MonoBehaviour
{
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI storageText;
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI bullet;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (DATA_Player.Instance != null)
        {
            CharacterStats stats = DATA_Player.Instance.CharacterStats;
            if (stats != null)
            {
                speedText.text = $"Speed: {stats.finalSpeed:0}";
                hpText.text = $"HP: {stats.finalMaxHP:0}";
                damageText.text = $"Damage: {stats.finalDamage:0}";
                storageText.text = $"Storage: {stats.finalStorage:0}";
                currencyText.text = $"Currency: {stats.Currency:0}";

                bullet.text = "Bullets: " + DATA_Player.Instance.ProjectileShooterStats.ProjectileCount_Current + "/" + DATA_Player.Instance.ProjectileShooterStats.ProjectileCount_Max;
            }
        }

    }

    void Update()
    {
        if (DATA_Player.Instance == null) return;

        CharacterStats stats = DATA_Player.Instance.CharacterStats;
        if (stats == null) return;

        speedText.text = $"{stats.finalSpeed:0}";
        hpText.text = $"{stats.finalMaxHP:0}";
        damageText.text = $"{stats.finalDamage:0}";
        storageText.text = $"{stats.finalStorage:0}";
        currencyText.text = $"{stats.Currency:0}";

        bullet.text = DATA_Player.Instance.ProjectileShooterStats.ProjectileCount_Current + "/" + DATA_Player.Instance.ProjectileShooterStats.ProjectileCount_Max;

    }

}
