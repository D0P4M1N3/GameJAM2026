using System.Collections;
using UnityEngine;

public class ACT_SunBoss_HitBox : MonoBehaviour
{
    [Header("References")]
    public BB_Sunboss_Master BB_Sunboss_Master;

    private void OnTriggerEnter(Collider other)
    {
        HomingProjectile getHomingProjectile = other.GetComponent<HomingProjectile>();
        if (getHomingProjectile){
            StartCoroutine(HitRoutine(other, getHomingProjectile));
        }
    }

    private IEnumerator HitRoutine(Collider other, HomingProjectile getHomingProjectile)
    {
        string OtherID = other.GetInstanceID().ToString();
        float StuntTime = getHomingProjectile.Damage;
        Destroy(getHomingProjectile.gameObject);

        BB_Sunboss_Master.BB_SunbossCTX_Move.ACT_SunBoss_Navagent.intrREGIS.Add(OtherID);
        BB_Sunboss_Master.BB_SunbossCTX_Brain.ACT_SunBoss_Brain.intrREGIS.Add(OtherID);
        BB_Sunboss_Master.BB_SunbossCTX_Sense.ConeBox.IntrREGIS.Add(OtherID);
        BB_Sunboss_Master.VisionConeRenderer.IntrREGIS.Add(OtherID);

        yield return new WaitForSeconds(StuntTime);

        BB_Sunboss_Master.BB_SunbossCTX_Move.ACT_SunBoss_Navagent.intrREGIS.Remove(OtherID);
        BB_Sunboss_Master.BB_SunbossCTX_Brain.ACT_SunBoss_Brain.intrREGIS.Remove(OtherID);
        BB_Sunboss_Master.BB_SunbossCTX_Sense.ConeBox.IntrREGIS.Remove(OtherID);
        BB_Sunboss_Master.VisionConeRenderer.IntrREGIS.Remove(OtherID);
    }


}
