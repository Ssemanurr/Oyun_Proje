using UnityEngine;
using UnityEngine.UI;

public class ContinueButtonController : MonoBehaviour
{
    [Tooltip("Sahnedeki rehber panel objesi")]
    public GameObject guidePanel;

    private Button btn;
    private CanvasGroup cg;

    void Awake()
    {
        btn = GetComponent<Button>();
        if (btn == null) Debug.LogError("ContinueButtonController: Button bulunamad�!");

        // CanvasGroup ekleyin veya Inspector�dan ekleyin
        cg = GetComponent<CanvasGroup>();
        if (cg == null)
            cg = gameObject.AddComponent<CanvasGroup>();

        // T�klan�nca paneli kapat
        btn.onClick.AddListener(OnClickHandler);
    }

    void Update()
    {
        bool visible = guidePanel != null && guidePanel.activeInHierarchy;

        // G�r�n�rl��� ve interactable'� ayarla:
        cg.alpha = visible ? 1f : 0f;
        cg.blocksRaycasts = visible;
        btn.interactable = visible;
    }

    void OnClickHandler()
    {
        // Paneli kapat
        guidePanel.SetActive(false);
        // Oyunu devam ettir
        Time.timeScale = 1f;
    }
}
