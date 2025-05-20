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
        [SerializeField, Tooltip("RectTransform of the PC monitor header (similar to menu bar).")]
        private RectTransform header;

        [SerializeField, Tooltip("List of GameObjects representing the different screens of the PC monitor.")]
        private List<GameObject> screens;

        [SerializeField, Tooltip("List of Buttons representing the tabs to switch between screens.")]
        private List<Button> tabs;

        [SerializeField, Tooltip("TextMeshPro text displaying the cart label and item count.")]
        private TMP_Text cartLabel;

        [Header("Product Screen")]
        [SerializeField, Tooltip("Transform of the parent object where product listings are instantiated.")]
        private Transform productListingParent;

        [SerializeField, Tooltip("Prefab of the ProductListing component to instantiate for each product.")]
        private ProductListing productListingPrefab;

        [SerializeField, Tooltip("Dropdown used to filter the displayed products by category.")]
        private TMP_Dropdown categoryDropdown;

        [Header("Furniture Screen")]
        [SerializeField, Tooltip("Transform of the parent object where furniture listings are instantiated.")]
        private Transform furnitureListingParent;

        [SerializeField, Tooltip("Prefab of the FurnitureListing component to instantiate for each furniture item.")]
        private FurnitureListing furnitureListingPrefab;

        [SerializeField, Tooltip("Dropdown used to filter the displayed furniture by store section.")]
        private TMP_Dropdown sectionDropdown;

        [Header("License Screen")]
        [SerializeField, Tooltip("Transform of the parent object where license listings are instantiated.")]
        private Transform licenseListingParent;

        [SerializeField, Tooltip("Prefab of the LicenseListing component to instantiate for each license.")]
        private LicenseListing licenseListingPrefab;

        [Header("Expansion Screen")]
        [SerializeField, Tooltip("Transform of the parent object where expansion listings are instantiated.")]
        private Transform expansionListingParent;

        [SerializeField, Tooltip("Prefab of the ExpansionListing component to instantiate for each expansion.")]
        private ExpansionListing expansionListingPrefab;

        [Header("Employee Screen")]
        [SerializeField, Tooltip("Transform of the parent object where employee listings are instantiated.")]
        private Transform employeeListingParent;

        [SerializeField, Tooltip("Prefab of the EmployeeListing component to instantiate for each employee.")]
        private EmployeeListing employeeListingPrefab;

        [Header("Cart Screen")]
        [SerializeField, Tooltip("Transform of the parent object where cart items are instantiated.")]
        private Transform cartItemsParent;

        [SerializeField, Tooltip("Prefab of the CartItem component to instantiate for each item in the cart.")]
        private CartItem cartItemPrefab;

        [SerializeField, Tooltip("TextMeshPro text displaying the total price of items in the cart.")]
        private TMP_Text totalPriceText;

        [SerializeField, Tooltip("Button to clear the shopping cart.")]
        private Button clearCartButton;

        [SerializeField, Tooltip("Button to proceed to checkout.")]
        private Button checkoutButton;

        private List<ProductListing> productListings = new List<ProductListing>();
        private List<FurnitureListing> furnitureListings = new List<FurnitureListing>();

        private void Start()
        {
            StoreManager.Instance.OnLicensePurchased += UpdateProductListing;

            // Initialize Product Screen
            foreach (var product in DataManager.Instance.ProductDB)
            {
                if (product.HasLicense)
                {
                    CreateProductListing(product);
                }
            }

            InitializeDropdown<Product.Category>(categoryDropdown, "All Categories", OnCategoryChanged);

            // Initialize Furniture Screen
            foreach (var furniture in DataManager.Instance.FurnitureDB)
            {
                var furnitureListing = Instantiate(furnitureListingPrefab, furnitureListingParent);
                furnitureListing.Initialize(furniture);
                furnitureListings.Add(furnitureListing);
            }

            InitializeDropdown<Section>(sectionDropdown, "All Sections", OnSectionChanged);

            // Initialize License Screen
            foreach (var license in DataManager.Instance.LicenseDB)
            {
                var licenseListing = Instantiate(licenseListingPrefab, licenseListingParent);
                licenseListing.Initialize(license);
            }

            // Initialize Expansion Screen
            foreach (var expansion in StoreManager.Instance.Expansions)
            {
                var expansionListing = Instantiate(expansionListingPrefab, expansionListingParent);
                expansionListing.Initialize(expansion);
            }

            // Initialize Employee Screen
            foreach (var employee in EmployeeManager.Instance.Employees)
            {
                var employeeListing = Instantiate(employeeListingPrefab, employeeListingParent);
                employeeListing.Initialize(employee);
            }

            // Initialize Cart Screen
            clearCartButton.onClick.AddListener(() => PC.Instance.ClearCart());
            checkoutButton.onClick.AddListener(() => PC.Instance.Checkout());
            PC.Instance.OnCartChanged += HandleCartChanged;
            totalPriceText.text = "Total: $0.00";

            // Initialize All Screens
            foreach (var screen in screens)
            {
                var screenRect = screen.GetComponent<RectTransform>();
                float headerHeight = header.sizeDelta.y / 2;
                screenRect.anchoredPosition = new Vector2(0f, -headerHeight);
                screen.SetActive(false);
            }

            // Initialize Tabs
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

        /// <summary>
        /// Displays the PC monitor UI and sets up the return button functionality.
        /// </summary>
        /// <param name="onClose">An action to be performed when the PC monitor is closed.</param>
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
                // Activate the screen at the given index and deactivate all others.
                screens[i].SetActive(i == activeScreenIndex);

                // Enable the tab for the inactive screens and disable the tab for the active screen.
                tabs[i].interactable = i != activeScreenIndex;
            }
        }

        /// <summary>
        /// Handles changes to the shopping cart, updating the cart UI.
        /// </summary>
        /// <param name="cart">
        /// A dictionary representing the current state of the shopping cart,
        /// where the key is the IPurchasable item and the value is the quantity.
        /// </param>
        private void HandleCartChanged(Dictionary<IPurchasable, int> cart)
        {
            // Clear existing cart items in the UI.
            foreach (Transform child in cartItemsParent)
            {
                Destroy(child.gameObject);
            }

            decimal totalPrice = 0m;
            int totalItems = 0;

            // Iterate through the cart items and update the UI.
            foreach (var item in cart)
            {
                // Instantiate a new cart item prefab for each item in the cart.
                CartItem newCartItem = Instantiate(cartItemPrefab, cartItemsParent);

                // Initialize the cart item with the item details and quantity.
                newCartItem.Initialize(item.Key, item.Value);

                // Calculate the total price and item count.
                int quantity = item.Key is Product product ? product.GetBoxQuantity() : 1; // Handle different purchasable types.
                totalPrice += item.Key.Price * quantity * item.Value;
                totalItems += item.Value;
            }

            // Update the total price and cart label text in the UI.
            totalPriceText.text = $"Total: ${totalPrice:N2}";
            cartLabel.text = "Cart";
            if (totalItems > 0) cartLabel.text += $"<color=#FFB414> ({totalItems})"; // Add item count to the cart label.
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
