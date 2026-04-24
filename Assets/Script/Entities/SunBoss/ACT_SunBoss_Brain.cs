using UnityEngine;

public class ACT_SunBoss_Brain : MonoBehaviour
{
    [Header("References")]
    public BB_Sunboss_Master BB_Sunboss_Master;

    [Header("Configs")]
    [SerializeField] SM_SunBoss__BASE BrainStateMachine;

    [Header("Runtime")]
    public SM_SunBoss__BASE SM_SunBoss_Brain_INST;
    public InterruptionRegistry intrREGIS;



    private void Start()
    {
        SM_SunBoss_Brain_INST = Instantiate(BrainStateMachine);
        SM_SunBoss_Brain_INST.BB_Sunboss_Master = BB_Sunboss_Master;

        SM_SunBoss_Brain_INST.Begin();
    }



    void Update()
    {
        if (intrREGIS.isInterrupted || Pause3D.Instance.IsPaused ) { return; }
        SM_SunBoss_Brain_INST.Tick();
    }

    private void LateUpdate()
    {
        if (intrREGIS.isInterrupted || Pause3D.Instance.IsPaused) { return;  }
        SM_SunBoss_Brain_INST.TickLate();
    }




}
