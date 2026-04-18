using UnityEngine;

public class ACT_SunBoss_Brain : MonoBehaviour
{
    [Header("References")]
    public BB_SunbossCTX_Master BB_SunbossCTX_Master;

    [Header("Configs")]
    [SerializeField] SunBoss.SM_SunBoss_Brain SM_SunBoss_Brain;

    [Header("Runtime")]
    public SunBoss.SM_SunBoss_Brain SM_SunBoss_Brain_INST;



    private void Start()
    {
        SM_SunBoss_Brain_INST = ScriptableObject.CreateInstance<SunBoss.SM_SunBoss_Brain>();
        SM_SunBoss_Brain_INST.BB_SunbossCTX_Master = BB_SunbossCTX_Master;

        SM_SunBoss_Brain_INST.Begin();
    }



    void Update()
    {
        SM_SunBoss_Brain_INST.Tick();
    }

    private void LateUpdate()
    {
        SM_SunBoss_Brain_INST.TickLate();
    }




}
