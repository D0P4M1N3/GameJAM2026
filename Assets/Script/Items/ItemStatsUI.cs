using TMPro;
using UnityEngine;

public class ItemStatsUI : MonoBehaviour
{
    private enum DataSourceType
    {
        Stash,
        Inventory,
    }

    [SerializeField] private DataSourceType dataSourceType = DataSourceType.Stash;
    [SerializeField] private StashData stashData;
    [SerializeField] private InventoryData inventoryData;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private string speedPrefix = "Speed: ";
    [SerializeField] private string healthPrefix = "Health: ";
    [SerializeField] private string attackPrefix = "Attack: ";
    [SerializeField] private string valuePrefix = "Value: ";

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    [ContextMenu("Refresh Stats UI")]
    public void Refresh()
    {
        ItemStats stats = GetStats();

        SetText(speedText, speedPrefix, stats.Speed);
        SetText(healthText, healthPrefix, stats.Health);
        SetText(attackText, attackPrefix, stats.Attack);
        SetText(valueText, valuePrefix, stats.Value);
    }

    private void Subscribe()
    {
        if (stashData != null)
        {
            stashData.Changed += Refresh;
        }

        if (inventoryData != null)
        {
            inventoryData.Changed += Refresh;
        }
    }

    private void Unsubscribe()
    {
        if (stashData != null)
        {
            stashData.Changed -= Refresh;
        }

        if (inventoryData != null)
        {
            inventoryData.Changed -= Refresh;
        }
    }

    private ItemStats GetStats()
    {
        return dataSourceType switch
        {
            DataSourceType.Inventory when inventoryData != null => inventoryData.TotalStats,
            DataSourceType.Stash when stashData != null => stashData.TotalStats,
            _ => ItemStats.Zero,
        };
    }

    private static void SetText(TMP_Text target, string prefix, float value)
    {
        if (target == null)
        {
            return;
        }

        target.text = prefix + value.ToString("0.##");
    }
}
