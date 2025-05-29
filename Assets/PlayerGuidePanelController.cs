using UnityEngine;
using UnityEngine.UI;

public class PlayerGuidePanelController : MonoBehaviour
{
    // Inspector�dan atanacak, panelin k�k� (Image v.s. root objesi)
    public GameObject panelRoot;

    // Buras� GameManager��n CloseGuide��ysa oray� da �a��rabiliriz
    public void OnContinueClicked()
    {
        // panelRoot�u kapat
        panelRoot.SetActive(false);
        // Oyunu devam ettir
        Time.timeScale = 1f;
    }
}
