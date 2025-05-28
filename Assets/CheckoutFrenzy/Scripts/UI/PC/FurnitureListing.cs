using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CryingSnow.CheckoutFrenzy
{
    public class FurnitureListing : MonoBehaviour
    {
        [SerializeField, Tooltip("Image displaying the furniture icon.")]
        private Image iconImage;

        [SerializeField, Tooltip("Text displaying the furniture name.")]
        private TMP_Text nameText;

        [SerializeField, Tooltip("Text displaying the furniture section.")]
        private TMP_Text sectionText;

        [SerializeField, Tooltip("Text displaying the furniture price.")]
        private TMP_Text priceText;

        [SerializeField, Tooltip("Text displaying the selected amount of furniture.")]
        private TMP_Text amountText;

        [SerializeField, Tooltip("Text displaying the total price of the selected furniture amount.")]
        private TMP_Text totalText;

        [SerializeField, Tooltip("Button to decrease the selected amount.")]
        private Button decreaseButton;

        [SerializeField, Tooltip("Button to increase the selected amount.")]
        private Button increaseButton;

        [SerializeField, Tooltip("Button to add the selected furniture to the cart.")]
        private Button addToCartButton;

        public Section Section { get; private set; }

        private int amount;
        private decimal price;

        private Furniture currentFurniture;
        private bool isAlreadyInCart = false;

        public void Initialize(Furniture furniture)
        {
            currentFurniture = furniture;
            this.Section = furniture.Section;

            iconImage.sprite = furniture.Icon;
            nameText.text = furniture.Name;

            if (furniture.Section == Section.General)
                sectionText.gameObject.SetActive(false);
            else
                sectionText.text = $"Section: {furniture.Section}";

            price = furniture.Price;
            priceText.text = $"Price: ${price:N2}";

            UpdateAmount(1, false);

            decreaseButton.onClick.AddListener(() => UpdateAmount(-1));
            increaseButton.onClick.AddListener(() => UpdateAmount(1));

            addToCartButton.onClick.AddListener(() =>
            {
                if (isAlreadyInCart)
                {
                    Debug.Log("Bu mobilya zaten sepete eklendi.");
                    return;
                }

                PC.Instance.AddToCart(currentFurniture, amount);
                isAlreadyInCart = true;
            });
        }

        private void UpdateAmount(int value, bool playSFX = true)
        {
            amount += value;
            amount = Mathf.Clamp(amount, 1, 10);
            amountText.text = $"Amount: {amount}";

            decimal totalPrice = price * amount;
            totalText.text = $"Total: ${totalPrice:N2}";

            if (playSFX) AudioManager.Instance.PlaySFX(AudioID.Click);
        }
    }
}
