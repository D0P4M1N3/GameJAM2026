using UnityEngine;

public class Player_Footstep : MonoBehaviour
{

    public void FootStep()
    {
        AudioManager.Instance.Play("sfx_walk");
    }
}
