using UnityEngine;
using DG.Tweening;

namespace CryingSnow.CheckoutFrenzy
{
    [RequireComponent(typeof(SphereCollider))]
    public class TrashCanLid : MonoBehaviour
    {
        public Furniture Furniture { get; private set; }

        private void Awake()
        {
            Furniture = GetComponentInParent<Furniture>();
            GetComponent<SphereCollider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if the colliding object is a Box.
            if (other.TryGetComponent<Box>(out Box box))
            {
                // Ignore boxes that are not disposable (e.g., when held by the player or after delivery).
                if (!box.IsDisposable) return;

                // Disable the Box component, Rigidbody, and BoxCollider to prevent further interaction.
                box.enabled = false;
                box.GetComponent<Rigidbody>().isKinematic = true;
                box.GetComponent<BoxCollider>().enabled = false;

                Open(true);

                // Animate the box moving upwards and then scaling to zero before being destroyed.
                box.transform.DOMove(transform.position + Vector3.up * 0.8f, 0.5f);
                box.transform.DOScale(Vector3.zero, 0.25f).SetDelay(0.25f)
                    .OnComplete(() => Destroy(box.gameObject));
            }
        }

        public void Open(bool playAudio)
        {
            // Make the trash can lid jump slightly.
            transform.DOJump(transform.position, 0.5f, 1, 0.5f);

            // Play the trash can sound effect.
            if (playAudio)
                AudioManager.Instance.PlaySFX(AudioID.TrashCan);
        }
    }
}
