using UnityEngine;
using DG.Tweening;

namespace CryingSnow.CheckoutFrenzy
{
    [RequireComponent(typeof(Rigidbody))]
    public class BroomCleaner : MonoBehaviour, IInteractable
    {
        private Rigidbody body;
        private PlayerController player;

        // Broom'un oyuncu tarafýndan elde tutulup tutulmadýðýný belirten bayrak
        public bool IsHeld { get; private set; } = false;

        private void Awake()
        {
            gameObject.layer = GameConfig.Instance.InteractableLayer.ToSingleLayer();
            body = GetComponent<Rigidbody>();
            SetActivePhysics(false);
        }

        public void Interact(PlayerController player)
        {
            this.player = player;

            // Broom elde alýndý
            SetHeld(true);

            foreach (Transform child in transform)
                child.gameObject.layer = GameConfig.Instance.HeldObjectLayer.ToSingleLayer();

            UIManager.Instance.ToggleActionUI(ActionType.Throw, true, Throw);

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
            transform.SetParent(null);
            SetActivePhysics(true);
            body.AddForce(transform.forward * 3.5f, ForceMode.Impulse);

            foreach (Transform child in transform)
                child.gameObject.layer = LayerMask.NameToLayer("Default");

            UIManager.Instance.ToggleActionUI(ActionType.Throw, false, null);

            // Broom elden býrakýldý
            SetHeld(false);

            player.CurrentState = PlayerController.State.Free;
            player = null;
        }

        private void SetActivePhysics(bool value)
        {
            body.isKinematic = !value;
        }

        // Broom'un elde olup olmadýðýný dýþarýdan ayarlamak için method
        public void SetHeld(bool value)
        {
            IsHeld = value;
        }
    }
}
