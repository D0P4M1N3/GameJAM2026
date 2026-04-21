using System.Collections.Generic;
using UnityEngine;

public class CameraObstructionFade : MonoBehaviour
{
    public Transform target; // player
    public LayerMask obstructionMask;

    public float fadeSpeed = 10f;
    public float transparentAlpha = 0.3f;

    private Dictionary<Renderer, Material[]> originalMats = new Dictionary<Renderer, Material[]>();

    private void Awake()
    {
        TryResolveTarget();
    }

    void Update()
    {
        if (target == null && !TryResolveTarget())
        {
            ClearDestroyedReferences();
            return;
        }

        FadeObjects();
    }

    private void OnDisable()
    {
        ClearDestroyedReferences();
        originalMats.Clear();
    }

    void FadeObjects()
    {
        ClearDestroyedReferences();

        if (target == null)
        {
            return;
        }

        Vector3 direction = target.position - transform.position;
        float distance = direction.magnitude;
        if (distance <= 0.001f)
        {
            return;
        }

        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, distance, obstructionMask);

        List<Renderer> newObstructions = new List<Renderer>();

        foreach (RaycastHit hit in hits)
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend == null) continue;

            newObstructions.Add(rend);

            if (!originalMats.ContainsKey(rend))
            {
                originalMats[rend] = rend.materials;
                MakeTransparent(rend);
            }
        }

        // Restore objects no longer blocking
        foreach (var rend in new List<Renderer>(originalMats.Keys))
        {
            if (rend == null)
            {
                originalMats.Remove(rend);
                continue;
            }

            if (!newObstructions.Contains(rend))
            {
                RestoreMaterial(rend);
                originalMats.Remove(rend);
            }
        }
    }

    void MakeTransparent(Renderer rend)
    {
        if (rend == null)
        {
            return;
        }

        foreach (var mat in rend.materials)
        {
            if (mat == null)
            {
                continue;
            }

            SetMaterialTransparent(mat);
            Color c = mat.color;
            c.a = transparentAlpha;
            mat.color = c;
        }
    }

    void RestoreMaterial(Renderer rend)
    {
        if (rend == null)
        {
            return;
        }

        var mats = rend.materials;
        foreach (var mat in mats)
        {
            if (mat == null)
            {
                continue;
            }

            Color c = mat.color;
            c.a = 1f;
            mat.color = c;
        }
    }

    void SetMaterialTransparent(Material mat)
    {
        mat.SetFloat("_Surface", 1); // Transparent
        mat.SetFloat("_Blend", 0);   // Alpha

        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);

        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = 3000;
    }

    private bool TryResolveTarget()
    {
        TopDownController playerController = FindAnyObjectByType<TopDownController>();
        if (playerController == null)
        {
            target = null;
            return false;
        }

        target = playerController.transform;
        return true;
    }

    private void ClearDestroyedReferences()
    {
        foreach (Renderer rend in new List<Renderer>(originalMats.Keys))
        {
            if (rend != null)
            {
                continue;
            }

            originalMats.Remove(rend);
        }
    }
}
