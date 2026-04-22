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
    private ItemWorldObject pendingCollectBoxTrashItem;

    private void Awake()
    {
        EnsureReferences();
    }

    private void OnValidate()
    {
        EnsureReferences();
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

        RefreshAcceptButtonState();
    }

    public void ClosePopUp()
    {
        EnsureReferences();
        if (collectingItemSpawner != null &&
            collectingItemSpawner.HasPendingItem &&
            !collectingItemSpawner.CanAcceptPendingItem)
        {
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

        if (collectingItemSpawner != null && collectingItemSpawner.AcceptPendingItem())
        {
            RefreshAcceptButtonState();
            return;
        }

        if (pendingCollectBoxTrashItem == null)
        {
            RefreshAcceptButtonState();
            return;
        }

        CollectBoxData collectBoxData = GameManager.Instance != null ? GameManager.Instance.CollectBoxData : null;
        collectBoxData?.RemoveItem(pendingCollectBoxTrashItem.ItemData);
        pendingCollectBoxTrashItem.SetCollectBoxState(false);
        Destroy(pendingCollectBoxTrashItem.gameObject);
        pendingCollectBoxTrashItem = null;
        RefreshAcceptButtonState();
    }

    public void NotifyItemCollected()
    {
        RefreshAcceptButtonState();
    }

    public void RefreshAcceptButtonState()
    {
        bool canAccept = (collectingItemSpawner != null && collectingItemSpawner.CanAcceptPendingItem) ||
            pendingCollectBoxTrashItem != null;
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
            RefreshAcceptButtonState();
            return true;
        }

        if (shouldBeInTrash)
        {
            pendingCollectBoxTrashItem = itemWorldObject;
            RefreshAcceptButtonState();
            return true;
        }

        if (pendingCollectBoxTrashItem != itemWorldObject)
        {
            return false;
        }

        pendingCollectBoxTrashItem = null;
        RefreshAcceptButtonState();
        return true;
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
            collectBoxTriggerZone.SetCollectBoxSpawner(collectingItemSpawner);
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
