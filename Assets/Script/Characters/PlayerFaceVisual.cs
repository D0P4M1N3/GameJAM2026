using System.Collections.Generic;
using UnityEngine;

public class PlayerFaceVisual : MonoBehaviour
{
    private static readonly int FaceId = Shader.PropertyToID("_Face");
    private static readonly int EnumId = Shader.PropertyToID("_ENUM");
    private static readonly string[] EnumKeywords =
    {
        "_ENUM_A",
        "_ENUM_B",
        "_ENUM_C",
        "_ENUM_D",
        "_ENUM_E",
        "_ENUM_F",
        "_ENUM_G",
        "_ENUM_H"
    };

    [SerializeField] private Renderer[] targetRenderers;
    [SerializeField] private bool autoFindFaceRenderers = true;

    private readonly List<Material> runtimeMaterials = new();
    private bool isSubscribed;

    private void Awake()
    {
        CacheFaceRenderers();
        CreateRuntimeMaterials();
        ApplyCurrentFaceState();
    }

    private void OnEnable()
    {
        Subscribe();
        ApplyCurrentFaceState();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void LateUpdate()
    {
        if (!isSubscribed && DATA_Player.Instance != null)
        {
            Subscribe();
            ApplyCurrentFaceState();
        }
    }

    private void OnValidate()
    {
        if (!Application.isPlaying && autoFindFaceRenderers)
        {
            CacheFaceRenderers();
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();

        for (int i = 0; i < runtimeMaterials.Count; i++)
        {
            if (runtimeMaterials[i] != null)
            {
                Destroy(runtimeMaterials[i]);
            }
        }
    }

    [ContextMenu("Refresh Face Renderers")]
    public void RefreshFaceRenderers()
    {
        CacheFaceRenderers();
    }

    private void Subscribe()
    {
        if (DATA_Player.Instance == null)
        {
            return;
        }

        DATA_Player.Instance.FaceChanged -= HandleFaceChanged;
        DATA_Player.Instance.FaceChanged += HandleFaceChanged;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (DATA_Player.Instance != null)
        {
            DATA_Player.Instance.FaceChanged -= HandleFaceChanged;
        }

        isSubscribed = false;
    }

    private void HandleFaceChanged(PlayerFaceState faceState)
    {
        ApplyFaceState(faceState);
    }

    private void ApplyCurrentFaceState()
    {
        if (DATA_Player.Instance == null)
        {
            return;
        }

        ApplyFaceState(DATA_Player.Instance.CurrentFaceState);
    }

    private void CacheFaceRenderers()
    {
        if (!autoFindFaceRenderers && targetRenderers != null && targetRenderers.Length > 0)
        {
            return;
        }

        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        List<Renderer> matches = new();

        for (int i = 0; i < allRenderers.Length; i++)
        {
            Renderer rendererToCheck = allRenderers[i];
            if (rendererToCheck == null)
            {
                continue;
            }

            Material[] sharedMaterials = rendererToCheck.sharedMaterials;
            for (int materialIndex = 0; materialIndex < sharedMaterials.Length; materialIndex++)
            {
                Material sharedMaterial = sharedMaterials[materialIndex];
                if (sharedMaterial == null)
                {
                    continue;
                }

                if (sharedMaterial.HasProperty(FaceId) || sharedMaterial.HasProperty(EnumId))
                {
                    matches.Add(rendererToCheck);
                    break;
                }
            }
        }

        targetRenderers = matches.ToArray();
    }

    private void CreateRuntimeMaterials()
    {
        if (runtimeMaterials.Count > 0)
        {
            return;
        }

        runtimeMaterials.Clear();

        if (targetRenderers == null)
        {
            return;
        }

        for (int rendererIndex = 0; rendererIndex < targetRenderers.Length; rendererIndex++)
        {
            Renderer targetRenderer = targetRenderers[rendererIndex];
            if (targetRenderer == null)
            {
                continue;
            }

            Material[] sharedMaterials = targetRenderer.sharedMaterials;
            Material[] runtimeSlots = targetRenderer.materials;
            bool changed = false;

            for (int materialIndex = 0; materialIndex < runtimeSlots.Length; materialIndex++)
            {
                Material sharedMaterial = materialIndex < sharedMaterials.Length ? sharedMaterials[materialIndex] : null;
                if (sharedMaterial == null || (!sharedMaterial.HasProperty(FaceId) && !sharedMaterial.HasProperty(EnumId)))
                {
                    continue;
                }

                Material runtimeMaterial = new(sharedMaterial);
                runtimeSlots[materialIndex] = runtimeMaterial;
                runtimeMaterials.Add(runtimeMaterial);
                changed = true;
            }

            if (changed)
            {
                targetRenderer.materials = runtimeSlots;
            }
        }
    }

    private void ApplyFaceState(PlayerFaceState faceState)
    {
        if (runtimeMaterials.Count == 0)
        {
            CreateRuntimeMaterials();
        }

        int variantIndex = Mathf.Clamp((int)faceState.Variant, 0, EnumKeywords.Length - 1);

        for (int i = 0; i < runtimeMaterials.Count; i++)
        {
            Material runtimeMaterial = runtimeMaterials[i];
            if (runtimeMaterial == null)
            {
                continue;
            }

            if (runtimeMaterial.HasProperty(FaceId))
            {
                runtimeMaterial.SetFloat(FaceId, variantIndex);
            }

            if (runtimeMaterial.HasProperty(EnumId))
            {
                runtimeMaterial.SetFloat(EnumId, variantIndex);
            }

            for (int keywordIndex = 0; keywordIndex < EnumKeywords.Length; keywordIndex++)
            {
                if (keywordIndex == variantIndex)
                {
                    runtimeMaterial.EnableKeyword(EnumKeywords[keywordIndex]);
                    continue;
                }

                runtimeMaterial.DisableKeyword(EnumKeywords[keywordIndex]);
            }
        }
    }
}
