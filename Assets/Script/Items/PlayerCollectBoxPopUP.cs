using UnityEngine;
using UnityEngine.Serialization;

public class PlayerCollectBoxPopUP : MonoBehaviour
{
    [FormerlySerializedAs("collectBoxPopUp")]
    [SerializeField] private GameObject collectBoxPopupUi;
    [SerializeField] private GameObject collectingPopup;
    [SerializeField] private ChangeCamProjection changeCamProjection;
    [SerializeField] private CameraController cameraController;

    private CollectingItemSpawner collectingItemSpawner;
    private CollectBoxDropZone collectBoxDropZone;

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
        EnsureReferences();

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
    }

    public void ClosePopUp()
    {
        EnsureReferences();
        collectingItemSpawner?.CancelPendingItem();

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
    }

    public void NotifyItemCollected()
    {
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

        if (collectBoxDropZone == null)
        {
            collectBoxDropZone = FindFirstObjectByType<CollectBoxDropZone>(FindObjectsInactive.Include);
        }

        if (collectBoxDropZone == null && collectingPopup != null)
        {
            Transform collectBoxTransform = collectingPopup.transform.Find("CollectBox");
            if (collectBoxTransform != null)
            {
                collectBoxDropZone = collectBoxTransform.GetComponent<CollectBoxDropZone>();
                if (collectBoxDropZone == null)
                {
                    collectBoxDropZone = collectBoxTransform.gameObject.AddComponent<CollectBoxDropZone>();
                }
            }
        }

        if (collectBoxDropZone != null && collectingItemSpawner != null)
        {
            collectBoxDropZone.SetSpawner(collectingItemSpawner);
        }

        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<CameraController>(FindObjectsInactive.Include);
        }
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
