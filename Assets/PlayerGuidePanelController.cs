using UnityEngine;
using UnityEngine.UI;

public class PlayerGuidePanelController : MonoBehaviour
{
    // Inspector’dan atanacak, panelin kökü (Image v.s. root objesi)
    public GameObject panelRoot;

    // Burasý GameManager’ýn CloseGuide’ýysa orayý da çaðýrabiliriz
    public void OnContinueClicked()
    {
        // panelRoot’u kapat
        panelRoot.SetActive(false);
        // Oyunu devam ettir
        Time.timeScale = 1f;
    }
}
