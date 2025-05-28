using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CryingSnow.CheckoutFrenzy
{
    public class ProductListing : MonoBehaviour
    {
        [SerializeField, Tooltip("Image displaying the product icon.")]
        private Image iconImage;

        [SerializeField, Tooltip("Text displaying the product name.")]
        private TMP_Text nameText;

        [SerializeField, Tooltip("Text displaying the product category.")]
        private TMP_Text categoryText;

        [SerializeField, Tooltip("Text displaying the quantity per box/pack.")]
        private TMP_Text quantityText;

        [SerializeField, Tooltip("Text displaying the product section.")]
        private TMP_Text sectionText;

        [SerializeField, Tooltip("Text displaying the price per box/pack.")]
        private TMP_Text priceText;

        [SerializeField, Tooltip("Text displaying the selected amount of boxes/packs.")]
        private TMP_Text amountText;

        [SerializeField, Tooltip("Text displaying the total price of the selected amount.")]
        private TMP_Text totalText;

        [SerializeField, Tooltip("Button to decrease the selected amount.")]
        private Button decreaseButton;

        [SerializeField, Tooltip("Button to increase the selected amount.")]
        private Button increaseButton;

        [SerializeField, Tooltip("Button to add the selected product amount to the cart.")]
        private Button addToCartButton;

        public Product.Category Category { get; private set; }

        private int amount;
        private int boxQuantity;
        private decimal singlePrice;
        private Product currentProduct;
        private bool isAlreadyInCart = false;

        public void Initialize(Product product)
        {
            currentProduct = product;
            Category = product.ProductCategory;

            iconImage.sprite = product.Icon;
            nameText.text = product.Name;

            var categoryName = product.ProductCategory.ToString();
            var formattedName = Regex.Replace(categoryName, @"([a-z])([A-Z])", "$1 $2");
            formattedName = Regex.Replace(formattedName, @"\bAnd\b", "&");
            categoryText.text = formattedName;

            boxQuantity = product.GetBoxQuantity();
            quantityText.text = $"<sprite=12> <size=30>{boxQuantity} pcs/pack";

            sectionText.text = $"Section: {product.Section}";

            singlePrice = product.Price * boxQuantity;
            priceText.text = $"Price: ${singlePrice:N2}";

            UpdateAmount(1, false);

            decreaseButton.onClick.AddListener(() => UpdateAmount(-1));
            increaseButton.onClick.AddListener(() => UpdateAmount(1));

            addToCartButton.onClick.AddListener(() =>
            {
                if (isAlreadyInCart)
                {
                    Debug.Log("Bu ürün zaten sepete eklendi.");
                    return;
                }

                PC.Instance.AddToCart(currentProduct, amount);
                isAlreadyInCart = true;
            });
        }

        private void UpdateAmount(int value, bool playSFX = true)
        {
            amount += value;
            amount = Mathf.Clamp(amount, 1, 10);
            amountText.text = $"Amount: {amount}";

            decimal totalPrice = singlePrice * amount;
            totalText.text = $"Total: ${totalPrice:N2}";

            if (playSFX) AudioManager.Instance.PlaySFX(AudioID.Click);
        }
    }
}

