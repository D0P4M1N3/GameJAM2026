using UnityEngine;

public class ACT_SunBoss_Brain : MonoBehaviour
{
    [Header("References")]
    public BB_SunbossCTX_Master BB_SunbossCTX_Master;

    [Header("Configs")]
    [SerializeField] SM_SunBoss__BASE BrainStateMachine;

    [Header("Runtime")]
    public SM_SunBoss__BASE SM_SunBoss_Brain_INST;



    private void Start()
    {
        SM_SunBoss_Brain_INST = Instantiate(BrainStateMachine);
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
