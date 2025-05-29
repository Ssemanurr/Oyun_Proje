using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace CryingSnow.CheckoutFrenzy
{
    public class PCMonitor : MonoBehaviour
    {
        [SerializeField] private RectTransform header;
        [SerializeField] private List<GameObject> screens;
        [SerializeField] private List<Button> tabs;
        [SerializeField] private TMP_Text cartLabel;

        [Header("Product Screen")]
        [SerializeField] private Transform productListingParent;
        [SerializeField] private ProductListing productListingPrefab;
        [SerializeField] private TMP_Dropdown categoryDropdown;

        [Header("Furniture Screen")]
        [SerializeField] private Transform furnitureListingParent;
        [SerializeField] private FurnitureListing furnitureListingPrefab;
        [SerializeField] private TMP_Dropdown sectionDropdown;

        [Header("License Screen")]
        [SerializeField] private Transform licenseListingParent;
        [SerializeField] private LicenseListing licenseListingPrefab;

        [Header("Cart Screen")]
        [SerializeField] private Transform cartItemsParent;
        [SerializeField] private CartItem cartItemPrefab;
        [SerializeField] private TMP_Text totalPriceText;
        [SerializeField] private Button clearCartButton;
        [SerializeField] private Button checkoutButton;

        private List<ProductListing> productListings = new List<ProductListing>();
        private List<FurnitureListing> furnitureListings = new List<FurnitureListing>();

        private void Start()
        {
            StoreManager.Instance.OnLicensePurchased += UpdateProductListing;

            foreach (var product in DataManager.Instance.ProductDB)
            {
                if (product.HasLicense)
                    CreateProductListing(product);
            }

            InitializeDropdown<Product.Category>(categoryDropdown, "All Categories", OnCategoryChanged);

            foreach (var furniture in DataManager.Instance.FurnitureDB)
            {
                var furnitureListing = Instantiate(furnitureListingPrefab, furnitureListingParent);
                furnitureListing.Initialize(furniture);
                furnitureListings.Add(furnitureListing);
            }

            InitializeDropdown<Section>(sectionDropdown, "All Sections", OnSectionChanged);

            foreach (var license in DataManager.Instance.LicenseDB)
            {
                var licenseListing = Instantiate(licenseListingPrefab, licenseListingParent);
                licenseListing.Initialize(license);
            }

            // Cart
            clearCartButton.onClick.AddListener(() => PC.Instance.ClearCart());
            checkoutButton.onClick.AddListener(() => PC.Instance.Checkout());
            PC.Instance.OnCartChanged += HandleCartChanged;
            totalPriceText.text = "Total: $0.00";

            foreach (var screen in screens)
            {
                var screenRect = screen.GetComponent<RectTransform>();
                float headerHeight = header.sizeDelta.y / 2;
                screenRect.anchoredPosition = new Vector2(0f, -headerHeight);
                screen.SetActive(false);
            }

            for (int i = 0; i < tabs.Count; i++)
            {
                int index = i;
                tabs[i].onClick.AddListener(() =>
                {
                    ToggleActiveScreen(index);
                    AudioManager.Instance.PlaySFX(AudioID.Click);
                });
            }

            ToggleActiveScreen(0);
            gameObject.SetActive(false);
        }

        public void Display(System.Action onClose)
        {
            gameObject.SetActive(true);

            UIManager.Instance.ToggleActionUI(ActionType.Return, true, () =>
            {
                onClose?.Invoke();
                gameObject.SetActive(false);
                UIManager.Instance.ToggleActionUI(ActionType.Return, false, null);
            });
        }

        private void ToggleActiveScreen(int activeScreenIndex)
        {
            for (int i = 0; i < screens.Count; i++)
            {
                screens[i].SetActive(i == activeScreenIndex);
                tabs[i].interactable = i != activeScreenIndex;
            }
        }

        private void HandleCartChanged(Dictionary<IPurchasable, int> cart)
        {
            foreach (Transform child in cartItemsParent)
            {
                Destroy(child.gameObject);
            }

            decimal totalPrice = 0m;
            int totalItems = 0;

            foreach (var item in cart)
            {
                CartItem newCartItem = Instantiate(cartItemPrefab, cartItemsParent);
                newCartItem.Initialize(item.Key, item.Value);

                int quantity = item.Key is Product product ? product.GetBoxQuantity() : 1;
                totalPrice += item.Key.Price * quantity * item.Value;
                totalItems += item.Value;
            }

            totalPriceText.text = $"Total: ${totalPrice:N2}";
            cartLabel.text = "Cart";
            if (totalItems > 0) cartLabel.text += $"<color=#FFB414> ({totalItems})";
        }

        private void CreateProductListing(Product product)
        {
            var productListing = Instantiate(productListingPrefab, productListingParent);
            productListing.Initialize(product);
            productListings.Add(productListing);
        }

        private void UpdateProductListing(License license)
        {
            foreach (var product in license.Products)
            {
                CreateProductListing(product);
            }

            OnCategoryChanged(categoryDropdown.value);
        }

        private void InitializeDropdown<TEnum>(TMP_Dropdown dropdown, string allLabel, UnityAction<int> onValueChanged) where TEnum : System.Enum
        {
            dropdown.ClearOptions();
            var options = new List<string> { allLabel };
            options.AddRange(System.Enum.GetNames(typeof(TEnum)).Select(name => name.ToTitleCase()));
            dropdown.AddOptions(options);
            dropdown.onValueChanged.AddListener(onValueChanged);
        }

        private void OnCategoryChanged(int index)
        {
            if (index == 0)
            {
                productListings.ForEach(listing => listing.gameObject.SetActive(true));
                return;
            }

            var selectedCategory = (Product.Category)(index - 1);

            foreach (var listing in productListings)
            {
                bool shouldShow = listing.Category == selectedCategory;
                listing.gameObject.SetActive(shouldShow);
            }
        }

        private void OnSectionChanged(int index)
        {
            if (index == 0)
            {
                furnitureListings.ForEach(listing => listing.gameObject.SetActive(true));
                return;
            }

            var selectedSection = (Section)(index - 1);

            foreach (var listing in furnitureListings)
            {
                bool shouldShow = listing.Section == selectedSection;
                listing.gameObject.SetActive(shouldShow);
            }
        }
    }
}
