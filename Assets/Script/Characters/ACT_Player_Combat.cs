using UnityEngine;

public class ACT_Player_Combat : MonoBehaviour
{
    [Header("Reference")]
    public BB_Player_Master BB_Player_Master;

    [Header("Configs")]
    public GameObject ProjectilePrefab;
    public float projectileSpeed = 10f;
    public float turnSpeed = 5f;

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

        hp.Initialize(target, projectileSpeed, turnSpeed);
    }

    private Transform GetClosestTarget()
    {
        BB_Sunboss_Master[] bosses = FindObjectsOfType<BB_Sunboss_Master>();

        Transform closest = null;
        float closestDistSqr = Mathf.Infinity;

        Vector3 currentPos = transform.position;

        foreach (var boss in bosses)
        {
            if (boss == null) continue;

            Transform body = boss.BB_SunbossCTX_Body?.WholeBody;

            if (body == null) continue;

            float distSqr = (body.position - currentPos).sqrMagnitude;

            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closest = body;
            }
        }

        return closest;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Transform target = GetClosestTarget();

            if (target != null)
            {
                Shoot(target);
            }
        }
    }
}