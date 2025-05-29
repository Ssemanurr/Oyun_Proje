using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public GameObject playerGuidePrefab;
    public GameObject mainUI;
    public GameObject guideContinueButton;  // sahnedeki tek Continue butonu

    private GameObject guideInstance;
    private bool guideShown = false;

    public void ShowGuide()
    {
        if (guideShown) return;
        guideShown = true;

        mainUI.SetActive(false);
        Time.timeScale = 0f;

        // Rehberi göster
        var canvas = FindObjectOfType<Canvas>();
        guideInstance = Instantiate(playerGuidePrefab, canvas.transform, false);

        // sahnedeki tek Continue butonunu aç
        guideContinueButton.SetActive(true);
        // eski listener’larý temizle ve yeniye baðla
        var btn = guideContinueButton.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(CloseGuide);
    }

    private void CloseGuide()
    {
        // rehberi kapat
        Destroy(guideInstance);
        // Continue butonunu tekrar gizle
        guideContinueButton.SetActive(false);
        // zamaný devam ettir ve HUD’ý aç
        Time.timeScale = 1f;
        mainUI.SetActive(true);
        guideShown = false;
    }
}
