using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


namespace CryingSnow.CheckoutFrenzy
{
    public class SummaryScreen : MonoBehaviour
    {
        public static SummaryScreen Instance { get; private set; }

        [SerializeField] private RectTransform mainPanel;
        [SerializeField] private TMP_Text valuesText;
       //[SerializeField] private Toggle skipToggle;
        [SerializeField] private Button continueButton;

        private PlayerUIBlocker playerUIBlocker;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            mainPanel.anchoredPosition = Vector2.zero;

            if (TryGetComponent<Image>(out Image image))
            {
                image.color = new Color(0f, 0f, 0f, 0.4f);
            }

            /*skipToggle.onValueChanged.AddListener(isOn =>
            {
                AudioManager.Instance.PlaySFX(AudioID.Click);
            });*/

            var player = FindFirstObjectByType<PlayerController>();

            if (player != null)
                playerUIBlocker = new PlayerUIBlocker(player);

            gameObject.SetActive(false);
        }

        public void Show(SummaryData data, System.Action<bool> onContinue)
        {
            gameObject.SetActive(true);
            TimeManager.Instance.IsPausedBySummary = true; // zamanı durdur
            string values = "";
            values += $"{data.PreviousBalance:N2}";
            values += $"\n<color=green>+${data.TotalRevenues:N2}";
            values += $"\n<color=red>-${data.TotalSpending:N2}";
            values += $"\n<color=white>${DataManager.Instance.PlayerMoney:N2}";
            values += "\n%";




            valuesText.text = values;

            playerUIBlocker?.Block();

            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySFX(AudioID.Click);

                playerUIBlocker?.Unblock();
                TimeManager.Instance.IsPausedBySummary = false;
                TimeManager.Instance.AllowTimeUpdate = true; // 

                gameObject.SetActive(false);

                if (DataManager.Instance.Data.TotalDays >= 8)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                }
            });


        }
    }
}

