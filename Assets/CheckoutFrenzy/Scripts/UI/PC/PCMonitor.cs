using System;
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
        [Header("Common")]
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
            // İlk doldurma
            PopulateProducts();
            PopulateFurnitures();

            // Dropdownları kur
            InitializeDropdown<Product.Category>(categoryDropdown, "All Categories", OnCategoryChanged);
            InitializeDropdown<Section>(sectionDropdown, "All Sections", OnSectionChanged);

            // Lisans ekranı
            foreach (var license in DataManager.Instance.LicenseDB)
            {
                var licUI = Instantiate(licenseListingPrefab, licenseListingParent);
                licUI.Initialize(license);
            }

            // Sepet işlemleri
            clearCartButton.onClick.AddListener(() => PC.Instance.ClearCart());
            checkoutButton.onClick.AddListener(() => PC.Instance.Checkout());
            PC.Instance.OnCartChanged += HandleCartChanged;
            totalPriceText.text = "Total: $0.00";

            // Tab’ler ve ekran pozisyonları
            float headerHalf = header.sizeDelta.y * 0.5f;
            for (int i = 0; i < screens.Count; i++)
            {
                var rect = screens[i].GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(0, -headerHalf);
                screens[i].SetActive(i == 0);
                tabs[i].interactable = i != 0;
                int idx = i;
                tabs[i].onClick.AddListener(() => ToggleActiveScreen(idx));
            }

            gameObject.SetActive(false);

            // Lisans satın alınca listelemeleri güncelle
            StoreManager.Instance.OnLicensePurchased += _ =>
            {
                PopulateProducts();
                PopulateFurnitures();
            };
        }

        /// <summary>
        /// PC ekranını açıp, kapanışta onClose callback'ini tetikler.
        /// </summary>
        public void Display(Action onClose)
        {
            gameObject.SetActive(true);
            UIManager.Instance.ToggleActionUI(
                ActionType.Return,
                true,
                () =>
                {
                    onClose?.Invoke();
                    gameObject.SetActive(false);
                    UIManager.Instance.ToggleActionUI(ActionType.Return, false, null);
                }
            );
        }

        #region Ürün Listeleme

        private void PopulateProducts()
        {
            // Önceki UI nesnelerini sil
            foreach (Transform t in productListingParent) Destroy(t.gameObject);
            productListings.Clear();

            // Sahip olunan lisanslardan açılan ürünleri ekle
            foreach (var product in DataManager.Instance.ProductDB)
            {
                bool unlocked = OwnedLicenses.Any(l => l.Products.Contains(product));
                if (unlocked)
                    CreateProductListing(product);
            }

            OnCategoryChanged(categoryDropdown.value);
        }

        private void CreateProductListing(Product product)
        {
            var ui = Instantiate(productListingPrefab, productListingParent);
            ui.Initialize(product);
            productListings.Add(ui);
        }

        private void OnCategoryChanged(int idx)
        {
            if (idx == 0)
            {
                productListings.ForEach(x => x.gameObject.SetActive(true));
                return;
            }

            var cat = (Product.Category)(idx - 1);
            productListings.ForEach(x => x.gameObject.SetActive(x.Category == cat));
        }

        #endregion

        #region Mobilya Listeleme

        private void PopulateFurnitures()
        {
            // Önceki UI nesnelerini sil
            foreach (Transform t in furnitureListingParent) Destroy(t.gameObject);
            furnitureListings.Clear();

            // Sahip olunan lisanslardan açılan mobilyaları ekle
            foreach (var furn in DataManager.Instance.FurnitureDB)
            {
                bool unlocked = OwnedLicenses.Any(l => l.Furnitures.Contains(furn));
                if (unlocked)
                {
                    var ui = Instantiate(furnitureListingPrefab, furnitureListingParent);
                    ui.Initialize(furn);
                    furnitureListings.Add(ui);
                }
            }

            OnSectionChanged(sectionDropdown.value);
        }

        private void OnSectionChanged(int idx)
        {
            if (idx == 0)
            {
                furnitureListings.ForEach(x => x.gameObject.SetActive(true));
                return;
            }

            var sec = (Section)(idx - 1);
            furnitureListings.ForEach(x => x.gameObject.SetActive(x.Section == sec));
        }

        #endregion

        #region Yardımcı Metotlar

        // Lisans listesi: hem başlangıçtan sahip olunanlar, hem satın alınanlar
        private IEnumerable<License> OwnedLicenses =>
            DataManager.Instance.LicenseDB
                .Where(l => l.IsOwnedByDefault || l.IsPurchased);

        private void ToggleActiveScreen(int activeIndex)
        {
            for (int i = 0; i < screens.Count; i++)
            {
                screens[i].SetActive(i == activeIndex);
                tabs[i].interactable = i != activeIndex;
            }
        }

        private void HandleCartChanged(Dictionary<IPurchasable, int> cart)
        {
            foreach (Transform c in cartItemsParent) Destroy(c.gameObject);

            decimal total = 0;
            int items = 0;

            foreach (var kvp in cart)
            {
                var ci = Instantiate(cartItemPrefab, cartItemsParent);
                ci.Initialize(kvp.Key, kvp.Value);

                int qty = (kvp.Key is Product p) ? p.GetBoxQuantity() : 1;
                total += kvp.Key.Price * qty * kvp.Value;
                items += kvp.Value;
            }

            totalPriceText.text = $"Total: ${total:N2}";
            cartLabel.text = items > 0 ? $"Cart<color=#FFB414> ({items})" : "Cart";
        }

        private void InitializeDropdown<TEnum>(TMP_Dropdown dd, string allLabel, UnityAction<int> onVal)
            where TEnum : Enum
        {
            dd.ClearOptions();
            var opts = new List<string> { allLabel };
            opts.AddRange(Enum.GetNames(typeof(TEnum)));
            dd.AddOptions(opts);
            dd.onValueChanged.AddListener(onVal);
        }

        #endregion
    }
}
