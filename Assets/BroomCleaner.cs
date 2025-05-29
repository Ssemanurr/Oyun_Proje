
using UnityEngine;
using DG.Tweening;

namespace CryingSnow.CheckoutFrenzy
{
    [RequireComponent(typeof(Rigidbody))]
    public class BroomCleaner : MonoBehaviour, IInteractable
    {
        private Rigidbody body;
        private PlayerController player;

        // Broom'un oyuncu tarafından elde tutulup tutulmadığını belirten bayrak
        public bool IsHeld { get; private set; } = false;

        private void Awake()
        {
            gameObject.layer = GameConfig.Instance.InteractableLayer.ToSingleLayer();
            body = GetComponent<Rigidbody>();

            // X ve Z eksenlerindeki rotasyonu kilitle, Y eksenini serbest bırak
            body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            // Başlangıçta fizik devre dışı
            SetActivePhysics(false);
        }

        public void Interact(PlayerController player)
        {
            this.player = player;

            // Broom elde alındı
            SetHeld(true);

            // Tüm çocuk objeleri "held object" layer'ına geçir
            foreach (Transform child in transform)
                child.gameObject.layer = GameConfig.Instance.HeldObjectLayer.ToSingleLayer();

            // Throw butonunu göster
            UIManager.Instance.ToggleActionUI(ActionType.Throw, true, Throw);

            // Broom'u oyuncunun el noktasına taşı
            transform.SetParent(player.HoldPoint);
            transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutQuint);
            transform.DOLocalRotate(Vector3.zero, 0.5f).SetEase(Ease.OutQuint);

            AudioManager.Instance.PlaySFX(AudioID.Pick);

            player.CurrentState = PlayerController.State.Holding;
            UIManager.Instance.InteractMessage.Hide();
        }

        public void OnFocused()
        {
            UIManager.Instance.InteractMessage.Display("Tap to pick up broom");
        }

        public void OnDefocused()
        {
            UIManager.Instance.InteractMessage.Hide();
        }

        private void Throw()
        {
            // Broom'u serbest bırak
            transform.SetParent(null);

            // Fizikleri aktif et
            SetActivePhysics(true);

            // İleri doğru kuvvet uygula
            body.AddForce(transform.forward * 3.5f, ForceMode.Impulse);

            // X/Z eksenindeki rotasyon kilitleri sayesinde dik kalır,
            // yine de pozisyonu sıfırlayarak tam dik yapalım
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);

            // Layer'ı varsayılan yap
            foreach (Transform child in transform)
                child.gameObject.layer = LayerMask.NameToLayer("Default");

            // Throw butonunu gizle
            UIManager.Instance.ToggleActionUI(ActionType.Throw, false, null);

            // Broom elden bırakıldı
            SetHeld(false);

            player.CurrentState = PlayerController.State.Free;
            player = null;
        }

        private void SetActivePhysics(bool value)
        {
            body.isKinematic = !value;
        }

        // Broom'un elde olup olmadığını dışarıdan ayarlamak için method
        public void SetHeld(bool value)
        {
            IsHeld = value;
        }
    }
}
