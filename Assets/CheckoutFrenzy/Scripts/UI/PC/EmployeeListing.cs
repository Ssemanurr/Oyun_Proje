using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CryingSnow.CheckoutFrenzy
{
    public class EmployeeListing : MonoBehaviour
    {
        [SerializeField] private Image avatar;
        [SerializeField] private TMP_Text typeLabel;
        [SerializeField] private TMP_Text costLabel;
        [SerializeField] private TMP_Text description;
        [SerializeField] private Button hireButton;
        [SerializeField] private TMP_Text hireLabel;
        [SerializeField] private Color hireColor = Color.green;
        [SerializeField] private Color fireColor = Color.red;

        private EmployeeType employeeType;

        public void Initialize(Employee employee)
        {
            employeeType = employee.Type;
            avatar.sprite = employee.Avatar;
            typeLabel.text = employee.Type.ToString();
            costLabel.text = $"Cost: ${employee.Cost:N2} / day";
            description.text = employee.Description;

            EmployeeManager.Instance.OnUnpaidEmployeeFired += UpdateHireButton;
            UpdateHireButton();
        }

        private void UpdateHireButton()
        {
            bool hired = DataManager.Instance.Data.HiredEmployees.Contains(employeeType);

            hireButton.image.color = hired ? fireColor : hireColor;
            hireLabel.text = hired ? "Fire" : "Hire";

            hireButton.onClick.RemoveAllListeners();
            hireButton.onClick.AddListener(() =>
            {
                if (hired) EmployeeManager.Instance.FireEmployee(employeeType);
                else EmployeeManager.Instance.HireEmployee(employeeType);

                UpdateHireButton();
            });
        }
    }
}
