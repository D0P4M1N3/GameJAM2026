using UnityEngine;

public class LiquidUpdater : MonoBehaviour
{
    private Color color => DATA_Player.Instance.CharacterStats.CharacterColor;
    private float HP => DATA_Player.Instance.CharacterStats.HP;

    private Material liquidMaterial;

    private void Awake()
    {
        // Create a unique instance for this renderer
        liquidMaterial = GetComponent<Renderer>().material;
    }

    private void Update()
    {
        UpdateLiquid();
    }

    private void UpdateLiquid()
    {
        if (liquidMaterial == null) return;

        liquidMaterial.SetColor("_Color", color);

        float fill = Mathf.Clamp01(HP / DATA_Player.Instance.CharacterStats.finalMaxHP);
        liquidMaterial.SetFloat("_fill", fill); // make sure name matches shader!
    }
}