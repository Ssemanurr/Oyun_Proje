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

        // Rehberi g�ster
        var canvas = FindObjectOfType<Canvas>();
        guideInstance = Instantiate(playerGuidePrefab, canvas.transform, false);

        // sahnedeki tek Continue butonunu a�
        guideContinueButton.SetActive(true);
        // eski listener�lar� temizle ve yeniye ba�la
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
        // zaman� devam ettir ve HUD�� a�
        Time.timeScale = 1f;
        mainUI.SetActive(true);
        guideShown = false;
    }
}
