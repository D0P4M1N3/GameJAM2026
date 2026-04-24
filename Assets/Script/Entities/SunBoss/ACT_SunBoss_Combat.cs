using UnityEngine;

public class ACT_SunBoss_Combat : MonoBehaviour
{
    public BB_Sunboss_Master BB_Sunboss_Master;

    private bool wasReachedLastFrame = false;
    private bool isDamaging = false;

    private float damagePerSecond;

    private void Start()
    {
        damagePerSecond = BB_Sunboss_Master.CharacterStats.finalDamage / 0.5f;
    }

    private void Update()
    {
        bool reached = BB_Sunboss_Master.BB_SunbossCTX_Sense.ConeBox.ReachedTarget;

        // --- ENTER ---
        if (reached && !wasReachedLastFrame)
        {
            isDamaging = true;
        }

        // --- EXIT ---
        if (!reached && wasReachedLastFrame)
        {
            isDamaging = false;
        }

        // --- DAMAGE LOOP ---
        if (isDamaging)
        {
            ApplyDamageOverTime();
        }

        wasReachedLastFrame = reached;
    }

    private void ApplyDamageOverTime()
    {
        if (Pause3D.Instance.IsPaused) { return; }

        if (DATA_Player.Instance == null || DATA_Player.Instance.CharacterStats == null)
        {
            return;
        }

        CharacterStats targetCharacterStats = DATA_Player.Instance.CharacterStats;
        float previousHp = targetCharacterStats.HP;

        float damageThisFrame = damagePerSecond * Time.deltaTime;

        targetCharacterStats.HP -= damageThisFrame;
        targetCharacterStats.HP = Mathf.Clamp(targetCharacterStats.HP, 0, targetCharacterStats.finalMaxHP);

        if (targetCharacterStats.HP < previousHp)
        {
            DATA_Player.Instance.PlayDamageFaceSwap();
        }
    }
}
