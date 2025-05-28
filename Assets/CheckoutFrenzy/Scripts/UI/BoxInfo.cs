using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CryingSnow.CheckoutFrenzy
{
    public class BoxInfo : MonoBehaviour
    {
        [SerializeField, Tooltip("Image to display the icon of the product in the box.")]
        private Image iconImage;

        [SerializeField, Tooltip("Text to display information about the box (name, quantity, size).")]
        private TMP_Text infoText;

        private Sprite emptyIcon;

        private void Awake()
        {
            emptyIcon = iconImage.sprite; // Store the default (empty box) icon.
        }

        /// <summary>
        /// Updates the displayed information based on the provided box.
        /// </summary>
        /// <param name="box">The Box object to display information for.</param>
        public void UpdateInfo(Box box)
        {
            var boxSizeInCm = box.Size * 100; // Convert box size to centimeters.

            string name = $": <color=red>Empty</color>"; // Default name if the box is empty.
            string qty = $": <color=red>0/{box.Capacity}</color>"; // Default quantity if the box is empty.
            string size = $": <color=green>{boxSizeInCm.x}x{boxSizeInCm.z}x{boxSizeInCm.y}</color>"; // Display the box size.

            if (box.Product != null) // If the box contains a product:
            {
                iconImage.sprite = box.Product.Icon; // Display the product icon.

                name = $": <color=green>{box.Product.Name}</color>"; // Display the product name.
                qty = $": <color=green>{box.Quantity}/{box.Capacity}</color>"; // Display the quantity.
            }
            else // If the box is empty:
            {
                iconImage.sprite = emptyIcon; // Display the default (empty) icon.
            }

            infoText.text = $"{name}\n{qty}\n{size}"; // Update the info text.
        }
    }
}
