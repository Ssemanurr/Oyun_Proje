using TMPro;
using UnityEngine;

namespace CryingSnow.CheckoutFrenzy
{
    public class StatsDisplayUpdater : MonoBehaviour
    {
        [SerializeField] private TMP_Text balanceText;
        [SerializeField] private TMP_Text levelText;

        private void Update()
        {
            var data = DataManager.Instance.Data;

            
            balanceText.text = $"${data.PlayerMoney:N2}";

           
            int level = data.TotalDays <= 3 ? 1 : 2;
            levelText.text = $"Level {level}";
        }
    }
}
