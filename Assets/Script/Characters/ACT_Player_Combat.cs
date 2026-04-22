using UnityEngine;
using VInspector.Libs;

public class ACT_Player_Combat : MonoBehaviour
{
    [Header("Reference")]
    public BB_Player_Master BB_Player_Master;

    [Header("Configs")]
    public GameObject ProjectilePrefab;
    public float projectileSpeed => BB_Player_Master.BB_PlayerCTX_Combat.projectileSpeed;
    public float turnSpeed => BB_Player_Master.BB_PlayerCTX_Combat.turnSpeed;
    public float Lifetime => BB_Player_Master.BB_PlayerCTX_Combat.BulletLifetime;

    public Transform firePoint;

    public void Shoot(Transform target)
    {
        GameObject proj = Instantiate(
            ProjectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        HomingProjectile hp = proj.GetComponent<HomingProjectile>();

        if (hp == null)
        {
            Debug.LogError("ProjectilePrefab missing HomingProjectile script");
            return;
        }

        hp.Initialize(target, projectileSpeed, turnSpeed, BB_Player_Master.CharacterStats.finalDamage, Lifetime);
    }

    private Transform GetClosestTarget()
    {
        BB_Sunboss_Master[] BSMs = FindObjectsOfType<BB_Sunboss_Master>();
        Transform ClosestT = null;
        float ClosestDist = float.PositiveInfinity;
        Vector3 CurrentPos = BB_Player_Master.BB_PlayerCTX_Body.WholeBody.transform.position;

        foreach (var BSM in BSMs)
        {
            if (BSM == null) continue;
            if (BSM.BB_SunbossCTX_Body?.WholeBody == null) continue;

            float dist = Vector3.Distance(CurrentPos, BSM.BB_SunbossCTX_Body.WholeBody.position);

            if (dist < ClosestDist)
            {
                ClosestDist = dist;
                ClosestT = BSM.BB_SunbossCTX_Body?.WholeBody;
            }
        }

        return ClosestT;
    }


    private void Start()
    {
        BB_Player_Master.ProjectileShooterStats.ProjectileCount_Current = BB_Player_Master.ProjectileShooterStats.ProjectileCount_Max;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Transform target = GetClosestTarget();

            if (target != null)
            {
                if (BB_Player_Master.ProjectileShooterStats.ProjectileCount_Current >0)
                {
                    Shoot(target);
                    if (DATA_Player.Instance != null)
                    {
                        DATA_Player.Instance.SetFaceForDuration(PlayerFaceVariant.C, 0.5f);
                    }
                    BB_Player_Master.ProjectileShooterStats.ProjectileCount_Current--;
                }
            }
        }
    }
}
