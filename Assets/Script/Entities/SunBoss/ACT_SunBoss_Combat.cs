using UnityEngine;
using System.Collections;

public class ACT_SunBoss_Combat : MonoBehaviour
{
    public BB_Sunboss_Master BB_Sunboss_Master;

    private Coroutine debugRoutine;
    private bool wasReachedLastFrame = false;


    private void Update()
    {
        bool reached = BB_Sunboss_Master.BB_SunbossCTX_Sense.ConeBox.ReachedTarget;

        // --- ENTER (false -> true) ---
        if (reached && !wasReachedLastFrame)
        {
            debugRoutine = StartCoroutine(DebugSpam());
        }

        // --- EXIT (true -> false) ---
        if (!reached && wasReachedLastFrame)
        {
            if (debugRoutine != null)
            {
                StopCoroutine(debugRoutine);
                debugRoutine = null;
            }
        }

        wasReachedLastFrame = reached;
    }

    private IEnumerator DebugSpam()
    {
        while (true)
        {
            CharacterStats targetCharacterStats = DATA_Player.Instance.CharacterStats;
            targetCharacterStats.HP -= BB_Sunboss_Master.CharacterStats.finalDamage;
            targetCharacterStats.HP = Mathf.Clamp(targetCharacterStats.HP, 0, targetCharacterStats.finalMaxHP);
            yield return new WaitForSeconds(0.123f);
        }
    }


}
