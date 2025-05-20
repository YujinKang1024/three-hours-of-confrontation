using UnityEngine;

public class ModalManager : MonoBehaviour
{
    public GameObject logModal;
    public GameObject thinkingModal;

    public void OpenLogModal()
    {
        logModal.SetActive(true);
        thinkingModal.SetActive(false);
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
