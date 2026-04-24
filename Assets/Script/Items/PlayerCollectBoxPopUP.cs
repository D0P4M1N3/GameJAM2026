using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerCollectBoxPopUP : MonoBehaviour
{
    [FormerlySerializedAs("collectBoxPopUp")]
    [SerializeField] private GameObject collectBoxPopupUi;
    [SerializeField] private GameObject collectingPopup;
    [SerializeField] private ChangeCamProjection changeCamProjection;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Button acceptButton;

    private CollectingItemSpawner collectingItemSpawner;
    private ItemTriggerZone collectBoxTriggerZone;
    private PendingCollectTrashZone trashTriggerZone;
    private readonly HashSet<ItemWorldObject> popupItemsInTrash = new();
    private readonly HashSet<ItemWorldObject> popupItemsOutsideValidZones = new();
    private bool IsPopupOpen => collectBoxPopupUi != null && collectBoxPopupUi.activeSelf;

    private void Awake()
    {
        EnsureReferences();
    }

    private void OnValidate()
    {
        EnsureReferences();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Tab))
        {
            return;
        }

        EnsureReferences();

        if (IsPopupOpen)
        {
            ClosePopUp();
            return;
        }

        if (Pause3D.Instance != null && Pause3D.Instance.IsPaused)
        {
            return;
        }

        OpenPopUP();
    }

    public bool TryBeginCollecting(GameplayItemPickup pickup)
    {
        EnsureReferences();

        if (pickup == null || collectingItemSpawner == null)
        {
            return false;
        }

        if (!collectingItemSpawner.BeginCollecting(pickup, this))
        {
            return false;
        }

        OpenPopUP();
        return true;
    }

    public void OpenPopUP()
    {
        AudioManager.Instance?.Play("sfx_storageOpen_1");


        Pause3D.Instance.SetPause(true);
        EnsureReferences();
        SetAcceptButtonInteractable(false);

        if (collectBoxTriggerZone != null)
        {
            collectBoxTriggerZone.SetCollectBoxExitRemovalEnabled(true);
        }

        if (collectBoxPopupUi != null)
        {
            collectBoxPopupUi.SetActive(true);
        }

        if (collectingPopup != null)
        {
            collectingPopup.SetActive(true);
        }

        if (changeCamProjection != null)
        {
            changeCamProjection.SetProjectionType(ChangeCamProjection.ProjectionType.Orthographic);
        }

        if (cameraController != null)
        {
            cameraController.SetTransformToMode();
        }

        RebuildPopupItemState();
        RefreshAcceptButtonState();
    }

    public void ClosePopUp()
    {
        AudioManager.Instance?.Play("sfx_storageClose_1");

        EnsureReferences();
        RebuildPopupItemState();
        if (collectingItemSpawner != null &&
            collectingItemSpawner.HasPendingItem &&
            !collectingItemSpawner.CanAcceptPendingItem)
        {
            return;
        }

        if (popupItemsOutsideValidZones.Count > 0)
        {
            RefreshAcceptButtonState();
            return;
        }

        Pause3D.Instance.SetPause(false);
        if (collectBoxTriggerZone != null)
        {
            collectBoxTriggerZone.SetCollectBoxExitRemovalEnabled(false);
        }

        if (collectingPopup != null)
        {
            collectingPopup.SetActive(false);
        }

        if (collectBoxPopupUi != null)
        {
            collectBoxPopupUi.SetActive(false);
        }

        if (changeCamProjection != null)
        {
            changeCamProjection.SetProjectionType(ChangeCamProjection.ProjectionType.Perspective);
        }

        if (cameraController != null)
        {
            cameraController.SetTargetMode();
        }

        RefreshAcceptButtonState();
    }

    public void AcceptPendingItem()
    {
        EnsureReferences();
        RebuildPopupItemState();
        if (popupItemsOutsideValidZones.Count > 0)
        {
            RefreshAcceptButtonState();
            return;
        }

        DeleteTrashItems();
        RebuildPopupItemState();

        if (popupItemsInTrash.Count == 0)
        {
            ClosePopUp();
        }

        RefreshAcceptButtonState();
    }

    public void NotifyItemCollected()
    {
        RebuildPopupItemState();
        RefreshAcceptButtonState();
    }

    public void RespawnPendingItem()
    {
        EnsureReferences();
        RebuildPopupItemState();

        bool didRespawnAny = false;
        List<ItemWorldObject> itemsToRespawn = new(popupItemsOutsideValidZones);
        for (int i = 0; i < itemsToRespawn.Count; i++)
        {
            ItemWorldObject itemWorldObject = itemsToRespawn[i];
            if (itemWorldObject == null || collectingItemSpawner == null)
            {
                continue;
            }

            didRespawnAny |= collectingItemSpawner.RespawnPopupItem(itemWorldObject);
        }

        if (!didRespawnAny &&
            collectingItemSpawner != null &&
            collectingItemSpawner.RespawnPendingItem())
        {
            didRespawnAny = true;
        }

        if (!didRespawnAny)
        {
            RefreshAcceptButtonState();
            return;
        }

        RebuildPopupItemState();
        RefreshAcceptButtonState();
    }

    public void NotifyCollectBoxItemEntered(ItemWorldObject itemWorldObject)
    {
        if (itemWorldObject == null)
        {
            return;
        }

        popupItemsOutsideValidZones.Remove(itemWorldObject);
        popupItemsInTrash.Remove(itemWorldObject);
        RefreshAcceptButtonState();
    }

    public void NotifyCollectBoxItemExited(ItemWorldObject itemWorldObject)
    {
        if (itemWorldObject == null)
        {
            return;
        }

        if (!popupItemsInTrash.Contains(itemWorldObject))
        {
            popupItemsOutsideValidZones.Add(itemWorldObject);
        }

        RefreshAcceptButtonState();
    }

    public void RefreshAcceptButtonState()
    {
        RemoveDestroyedTrackedItems();
        bool canAccept = popupItemsOutsideValidZones.Count == 0;
        SetAcceptButtonInteractable(canAccept);
    }

    public bool TrySetCollectBoxItemTrashState(ItemWorldObject itemWorldObject, bool shouldBeInTrash)
    {
        EnsureReferences();

        if (itemWorldObject == null || itemWorldObject.ItemData == null)
        {
            return false;
        }

        if (collectingItemSpawner != null &&
            collectingItemSpawner.TrySetSpawnedItemTrashState(itemWorldObject, shouldBeInTrash))
        {
            TrackItemZoneState(itemWorldObject, shouldBeInTrash ? PopupZone.Trash : PopupZone.OutsideValidZones);
            RefreshAcceptButtonState();
            return true;
        }

        if (shouldBeInTrash)
        {
            TrackItemZoneState(itemWorldObject, PopupZone.Trash);
            RefreshAcceptButtonState();
            return true;
        }

        if (!popupItemsInTrash.Remove(itemWorldObject))
        {
            return false;
        }

        if (!itemWorldObject.IsInCollectBox)
        {
            popupItemsOutsideValidZones.Add(itemWorldObject);
        }

        RefreshAcceptButtonState();
        return true;
    }

    public bool TryDeletePopupItem(ItemWorldObject itemWorldObject)
    {
        EnsureReferences();

        if (itemWorldObject == null || itemWorldObject.ItemData == null)
        {
            return false;
        }

        if (collectingItemSpawner != null && collectingItemSpawner.DeletePendingItem(itemWorldObject))
        {
            popupItemsOutsideValidZones.Remove(itemWorldObject);
            popupItemsInTrash.Remove(itemWorldObject);
            RefreshAcceptButtonState();
            return true;
        }

        CollectBoxData collectBoxData = GameManager.Instance != null ? GameManager.Instance.CollectBoxData : null;
        collectBoxData?.RemoveItem(itemWorldObject.ItemData);
        popupItemsOutsideValidZones.Remove(itemWorldObject);
        popupItemsInTrash.Remove(itemWorldObject);
        itemWorldObject.SetCollectBoxState(false);
        Destroy(itemWorldObject.gameObject);
        RefreshAcceptButtonState();
        return true;
    }

    private void DeleteTrashItems()
    {
        CollectBoxData collectBoxData = GameManager.Instance != null ? GameManager.Instance.CollectBoxData : null;
        List<ItemWorldObject> itemsToDelete = new(popupItemsInTrash);

        for (int i = 0; i < itemsToDelete.Count; i++)
        {
            ItemWorldObject itemWorldObject = itemsToDelete[i];
            if (itemWorldObject == null || itemWorldObject.ItemData == null)
            {
                continue;
            }

            if (collectingItemSpawner != null && collectingItemSpawner.DeletePendingItem(itemWorldObject))
            {
                popupItemsOutsideValidZones.Remove(itemWorldObject);
                popupItemsInTrash.Remove(itemWorldObject);
                continue;
            }

            collectBoxData?.RemoveItem(itemWorldObject.ItemData);
            popupItemsOutsideValidZones.Remove(itemWorldObject);
            popupItemsInTrash.Remove(itemWorldObject);
            itemWorldObject.SetCollectBoxState(false);
            Destroy(itemWorldObject.gameObject);
        }
    }

    private void RemoveDestroyedTrackedItems()
    {
        popupItemsInTrash.RemoveWhere(item => item == null);
        popupItemsOutsideValidZones.RemoveWhere(item => item == null);
    }

    private void RebuildPopupItemState()
    {
        popupItemsInTrash.Clear();
        popupItemsOutsideValidZones.Clear();

        if (collectingPopup == null)
        {
            return;
        }

        ItemWorldObject[] popupItems = collectingPopup.GetComponentsInChildren<ItemWorldObject>(true);
        for (int i = 0; i < popupItems.Length; i++)
        {
            ItemWorldObject itemWorldObject = popupItems[i];
            if (itemWorldObject == null)
            {
                continue;
            }

            if (!IsPopupItem(itemWorldObject))
            {
                continue;
            }

            if (IsItemInsideTrashZone(itemWorldObject))
            {
                popupItemsInTrash.Add(itemWorldObject);
                continue;
            }

            if (!IsItemInsideCollectBoxZone(itemWorldObject))
            {
                popupItemsOutsideValidZones.Add(itemWorldObject);
            }
        }
    }

    private bool IsPopupItem(ItemWorldObject itemWorldObject)
    {
        return itemWorldObject != null &&
            collectingPopup != null &&
            itemWorldObject.transform.IsChildOf(collectingPopup.transform);
    }

    private bool IsItemInsideCollectBoxZone(ItemWorldObject itemWorldObject)
    {
        return IsItemInsideZone(itemWorldObject, collectBoxTriggerZone);
    }

    private bool IsItemInsideTrashZone(ItemWorldObject itemWorldObject)
    {
        return IsItemInsideZone(itemWorldObject, trashTriggerZone);
    }

    private static bool IsItemInsideZone(ItemWorldObject itemWorldObject, Component zoneComponent)
    {
        if (itemWorldObject == null || zoneComponent == null)
        {
            return false;
        }

        Collider2D zoneCollider = zoneComponent.GetComponent<Collider2D>();
        Collider2D itemCollider = itemWorldObject.GetComponentInChildren<Collider2D>(true);
        if (zoneCollider == null || itemCollider == null)
        {
            return false;
        }

        return zoneCollider.bounds.Intersects(itemCollider.bounds);
    }

    private void TrackItemZoneState(ItemWorldObject itemWorldObject, PopupZone zone)
    {
        if (itemWorldObject == null)
        {
            return;
        }

        popupItemsInTrash.Remove(itemWorldObject);
        popupItemsOutsideValidZones.Remove(itemWorldObject);

        if (zone == PopupZone.Trash)
        {
            popupItemsInTrash.Add(itemWorldObject);
        }
        else if (zone == PopupZone.OutsideValidZones)
        {
            popupItemsOutsideValidZones.Add(itemWorldObject);
        }
    }

    private enum PopupZone
    {
        CollectBox,
        Trash,
        OutsideValidZones,
    }

    private void EnsureReferences()
    {
        if (collectBoxPopupUi == null)
        {
            collectBoxPopupUi = FindPopupObject("ColllectBoxPopup");
        }

        if (collectingPopup == null)
        {
            collectingPopup = collectBoxPopupUi;
        }

        if (acceptButton == null && collectingPopup != null)
        {
            Transform acceptButtonTransform = collectingPopup.transform.Find("AcceptButton");
            if (acceptButtonTransform != null)
            {
                acceptButton = acceptButtonTransform.GetComponent<Button>();
            }
        }

        if (collectingItemSpawner == null)
        {
            collectingItemSpawner = FindFirstObjectByType<CollectingItemSpawner>(FindObjectsInactive.Include);
        }

        if (collectingItemSpawner == null && collectingPopup != null)
        {
            Transform itemSpawnerTransform = collectingPopup.transform.Find("ItemSpawner");
            GameObject spawnerTarget = itemSpawnerTransform != null ? itemSpawnerTransform.gameObject : collectingPopup;
            collectingItemSpawner = spawnerTarget.GetComponent<CollectingItemSpawner>();
            if (collectingItemSpawner == null)
            {
                collectingItemSpawner = spawnerTarget.AddComponent<CollectingItemSpawner>();
            }
        }

        if (collectBoxTriggerZone == null && collectingPopup != null)
        {
            Transform collectBoxTransform = collectingPopup.transform.Find("CollectBox");
            if (collectBoxTransform != null)
            {
                Transform detectionZoneTransform = collectBoxTransform.Find("DetectionZone");
                Transform triggerRoot = detectionZoneTransform != null ? detectionZoneTransform : collectBoxTransform;

                collectBoxTriggerZone = triggerRoot.GetComponent<ItemTriggerZone>();
                if (collectBoxTriggerZone == null)
                {
                    collectBoxTriggerZone = triggerRoot.gameObject.AddComponent<ItemTriggerZone>();
                }
            }
        }

        if (collectBoxTriggerZone != null && collectingItemSpawner != null)
        {
            collectBoxTriggerZone.SetCollectBoxSpawner(collectingItemSpawner, this);
        }

        if (trashTriggerZone == null && collectingPopup != null)
        {
            Transform trashTransform = collectingPopup.transform.Find("TrashZone");
            if (trashTransform != null)
            {
                trashTriggerZone = trashTransform.GetComponent<PendingCollectTrashZone>();
                if (trashTriggerZone == null)
                {
                    trashTriggerZone = trashTransform.gameObject.AddComponent<PendingCollectTrashZone>();
                }
            }
        }

        if (trashTriggerZone != null && collectingItemSpawner != null)
        {
            trashTriggerZone.SetCollectingItemSpawner(collectingItemSpawner);
        }

        if (trashTriggerZone != null)
        {
            trashTriggerZone.SetOwnerPopup(this);
        }

        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<CameraController>(FindObjectsInactive.Include);
        }
    }

    private void SetAcceptButtonInteractable(bool isInteractable)
    {
        if (acceptButton == null)
        {
            return;
        }

        acceptButton.gameObject.SetActive(isInteractable);
        acceptButton.interactable = isInteractable;
    }

    private static GameObject FindPopupObject(string popupName)
    {
        if (string.IsNullOrWhiteSpace(popupName))
        {
            return null;
        }

        GameObject[] popupCandidates = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < popupCandidates.Length; i++)
        {
            GameObject candidate = popupCandidates[i];
            if (candidate != null && candidate.name == popupName)
            {
                return candidate;
            }
        }

        return null;
    }
}
