using UnityEngine;
namespace CryingSnow.CheckoutFrenzy
{
    public class PlayerUIBlocker
    {
        private PlayerController player;
        private PlayerController.State? previousState;

        public PlayerUIBlocker(PlayerController player)
        {
            this.player = player;
        }

        public void Block()
        {
            if (player == null) return;

            if (player.CurrentState != PlayerController.State.Busy)
                previousState = player.CurrentState;

            player.CurrentState = PlayerController.State.Busy;
            UIManager.Instance.ToggleCrosshair(false);
            UIManager.Instance.IsUIBlockingActions = true;
        }


        public void Unblock()
        {
            if (player == null)
            {
                Debug.LogWarning("UNBLOCK");
                return;
            }

            if (previousState == null)
            {
                player.CurrentState = PlayerController.State.Free;
            }
            else
            {
                player.CurrentState = previousState.Value;
            }

            bool wasBusy = previousState is PlayerController.State.Working or PlayerController.State.Busy;
            if (!wasBusy)
            {
                UIManager.Instance.ToggleCrosshair(true);
            }

            UIManager.Instance.IsUIBlockingActions = false;
            previousState = null;
        }






    }
}
