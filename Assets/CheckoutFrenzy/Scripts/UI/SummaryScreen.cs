using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using CryingSnow.CheckoutFrenzy.sara_code;


namespace CryingSnow.CheckoutFrenzy
{
    public class SummaryScreen : MonoBehaviour
    {
        public static SummaryScreen Instance { get; private set; }

        [SerializeField] private RectTransform mainPanel;
        [SerializeField] private TMP_Text valuesText;
        //[SerializeField] private Toggle skipToggle;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button repeatButton;

        bool isLevel1Summary;


        private PlayerUIBlocker playerUIBlocker;

        private void Awake()
        {
            Instance = this;
            if (continueButton == null || repeatButton == null)
                Debug.LogError("SummaryScreen: Button references not assigned!");
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

        public void Show(SummaryData data, System.Action onComplete, bool isLevel1)
        {
            gameObject.SetActive(true);
            TimeManager.Instance.IsPausedBySummary = true;

            float avgSatisfaction = CustomerSatisfactionTracker.GetAverageSatisfaction();
            string values = $"{data.PreviousBalance:N2}" +
                            $"\n<color=green>+${data.TotalRevenues:N2}" +
                            $"\n<color=red>-${data.TotalSpending:N2}" +
                            $"\n<color=white>${DataManager.Instance.PlayerMoney:N2}" +
                            $"\n<color=yellow>Satisfaction: {avgSatisfaction:F1}%</color>";

            valuesText.text = values;

            playerUIBlocker?.Block();

            continueButton.onClick.RemoveAllListeners();
            repeatButton.onClick.RemoveAllListeners();

            continueButton.gameObject.SetActive(true);
            repeatButton.gameObject.SetActive(true);

            if (isLevel1)
            {
                // Level 1 rules
                continueButton.interactable = avgSatisfaction >= 60f;

                continueButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance.PlaySFX(AudioID.Click);
                    playerUIBlocker?.Unblock();
                    gameObject.SetActive(false);
                    TimeManager.Instance.IsPausedBySummary = false;
                    StoreManager.Instance.AdvanceLevel();
                    TimeManager.Instance.AllowTimeUpdate = true;
                    //StoreManager.Instance.AdvanceLevel(); // goes to day 4
                });

                repeatButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance.PlaySFX(AudioID.Click);
                    playerUIBlocker?.Unblock();
                    gameObject.SetActive(false);
                    TimeManager.Instance.IsPausedBySummary = false;
                    StoreManager.Instance.RepeatLevel();
                    TimeManager.Instance.AllowTimeUpdate = true;
                    //StoreManager.Instance.RepeatLevel(); // goes to day 1
                });
            }
            else
            {
                // Level 2: End of game
                continueButton.interactable = true;

                continueButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance.PlaySFX(AudioID.Click);
                    playerUIBlocker?.Unblock();
                    gameObject.SetActive(false);
                    TimeManager.Instance.IsPausedBySummary = false;
                    SceneManager.LoadScene("MainMenu");
                    TimeManager.Instance.AllowTimeUpdate = true;
                    //SceneManager.LoadScene("MainMenu");
                });

                repeatButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance.PlaySFX(AudioID.Click);
                    playerUIBlocker?.Unblock();
                    gameObject.SetActive(false);
                    TimeManager.Instance.IsPausedBySummary = false;
                    StoreManager.Instance.RepeatLevel();
                    TimeManager.Instance.AllowTimeUpdate = true;
                    //StoreManager.Instance.RepeatLevel(); // goes to day 4
                });
            }

            // Finally
            onComplete?.Invoke();
        }


        public void Hide()
        {
            mainPanel.gameObject.SetActive(false);
        }


    }
}

