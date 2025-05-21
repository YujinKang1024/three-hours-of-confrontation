using UnityEngine;
using UnityEngine.UI;

public class ModalManager : MonoBehaviour
{
    public GameObject logModal;
    public GameObject thinkingModal;
    public ScrollRect logModalscrollRect;

    public void OpenLogModal()
    {
        logModal.SetActive(true);
        thinkingModal.SetActive(false);
        logModalscrollRect.verticalNormalizedPosition = 0f;
    }

    public void OpenThinkingModal()
    {
        logModal.SetActive(false);
        thinkingModal.SetActive(true);
    }

    public void CloseAllModals()
    {
        logModal.SetActive(false);
        thinkingModal.SetActive(false);
    }
}
