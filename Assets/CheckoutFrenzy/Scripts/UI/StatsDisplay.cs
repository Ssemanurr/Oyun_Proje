using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CryingSnow.CheckoutFrenzy
{
    public class StatsDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text moneyDisplay;
        [SerializeField] private TMP_Text levelDisplay;
        [SerializeField] private Image levelFill;

        private void Start()
        {
            DataManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;

            UpdateMoneyDisplay(DataManager.Instance.PlayerMoney);
            UpdateLevelDisplay();
        }

        private void UpdateMoneyDisplay(decimal amount)
        {
            moneyDisplay.text = $"$ {amount:N2}";
        }

        private void UpdateLevelDisplay()
        {
            int totalDays = DataManager.Instance.Data.TotalDays;
            int level = totalDays <= 3 ? 1 : 2;
            levelDisplay.text = $"Level {level}";

            // Level fill göstergesini gizle veya sıfırla çünkü XP sistemi kullanılmıyor
            levelFill.fillAmount = 1f;
        }

        private void Update()
        {
            UpdateLevelDisplay();
        }
    }
}

