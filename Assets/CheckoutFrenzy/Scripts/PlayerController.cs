using UnityEngine;
using SimpleInputNamespace;

namespace CryingSnow.CheckoutFrenzy
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField, Tooltip("Movement speed of the player.")]
        private float movingSpeed = 7.5f;

        [SerializeField, Tooltip("Gravity applied to the player.")]
        private float gravity = -9.81f;

        [SerializeField, Tooltip("Rotation speed of the player's view.")]
        private float lookSpeed = 2.0f;

        [SerializeField, Tooltip("Maximum angle the player can look up or down.")]
        private float lookXLimit = 45.0f;

        [SerializeField, Tooltip("Maximum distance for interaction.")]
        private float interactDistance = 3.5f;

        [SerializeField, Tooltip("Time in seconds the interact button must be held to trigger an interaction.")]
        private float interactHoldThreshold = 1.0f;

        [SerializeField, Tooltip("Transform representing the player's holding point (hands).")]
        private Transform holdPoint;

        [SerializeField, Tooltip("Transform representing the player's camera.")]
        private Transform playerCamera;

        [Header("Sway Settings")]
        [SerializeField, Tooltip("Amount of sway applied to the camera.")]
        private float swayAmount = 0.05f;

        [SerializeField, Tooltip("Speed of the camera sway.")]
        private float swaySpeed = 5.0f;

        [SerializeField, Tooltip("Maximum amount of sway applied to the camera.")]
        private float maxSwayAmount = 0.2f;

        [Header("Bobbing Settings")]
        [SerializeField, Tooltip("Frequency of the camera bobbing effect.")]
        private float bobFrequency = 10.0f;

        [SerializeField, Tooltip("Horizontal amplitude of the camera bobbing effect.")]
        private float bobHorizontalAmplitude = 0.04f;

        [SerializeField, Tooltip("Vertical amplitude of the camera bobbing effect.")]
        private float bobVerticalAmplitude = 0.04f;

        [SerializeField, Tooltip("Smoothing applied to the camera bobbing effect.")]
        private float bobSmoothing = 8f;

        [Header("Sound Effects")]
        [SerializeField, Tooltip("Array of audio clips used for footstep sounds.")]
        private AudioClip[] footstepClips;

        [SerializeField, Tooltip("Distance traveled before playing a footstep sound.")]
        private float stepDistance = 2.0f;

        public Transform HoldPoint => holdPoint;

        public enum State { Free, Holding, Moving, Working, Busy }
        public State CurrentState { get; set; }

        private bool canMove => CurrentState != State.Working && CurrentState != State.Busy;

        private Camera mainCamera;

        private CharacterController controller;
        private Vector3 movement;
        private Vector3 playerVelocity = Vector3.zero;
        private float rotationX = 0;
        private string xLookAxis;
        private string yLookAxis;
        private bool isMobileControl;

        private AudioSource audioSource;

        private IInteractable lastInteractable;
        private Shelf lastShelf;
        private Rack lastRack;

        private Vector3 holdPointOrigin;

        private float bobTimer = 0.0f;
        private float interactHoldDuration = 0f;

        private float distanceTraveled;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            audioSource = GetComponent<AudioSource>();
            mainCamera = Camera.main;
            holdPointOrigin = holdPoint.localPosition;

            isMobileControl = GameConfig.Instance.ControlMode == ControlMode.Mobile;
            xLookAxis = isMobileControl ? "Look X" : "Mouse X";
            yLookAxis = isMobileControl ? "Look Y" : "Mouse Y";

#if UNITY_EDITOR
            var touchpad = FindObjectOfType<Touchpad>();
            touchpad.sensitivity = 1f;
#endif
        }

        private void Update()
        {
            HandleMovement();
            HandleSway();
            HandleBobbing();
            HandleFootsteps();

            switch (CurrentState)
            {
                case State.Free:
                    DetectInteractable();
                    DetectShelfToCustomize();
                    DetectRack();
                    break;

                case State.Holding:
                    DetectShelfToRestock();
                    DetectRack();
                    break;

                case State.Working:
                    Work();
                    break;

                case State.Moving:
                case State.Busy:
                default:
                    break;
            }
        }

        private void HandleMovement()
        {
            // Gravity handling
            if (controller.isGrounded && playerVelocity.y < 0)
            {
                playerVelocity.y = 0f;  // Reset vertical velocity when grounded
            }

            // Handle horizontal movement (X and Z axis)
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);

            float curSpeedX = canMove ? movingSpeed * SimpleInput.GetAxis("Vertical") : 0f;
            float curSpeedY = canMove ? movingSpeed * SimpleInput.GetAxis("Horizontal") : 0f;

            movement = (forward * curSpeedX) + (right * curSpeedY);

            // Move character based on horizontal input
            controller.Move(movement * Time.deltaTime);

            // Apply gravity over time (fall faster if in the air)
            playerVelocity.y += gravity * Time.deltaTime;

            // Move the character based on gravity (Y-axis)
            controller.Move(playerVelocity * Time.deltaTime);

            // Handle player rotation (if applicable)
            if (canMove)
            {
                rotationX += -SimpleInput.GetAxis(yLookAxis) * lookSpeed;
                rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
                playerCamera.localRotation = Quaternion.Euler(rotationX, 0, 0);
                transform.rotation *= Quaternion.Euler(0, SimpleInput.GetAxis(xLookAxis) * lookSpeed, 0);
            }
        }

        private void HandleSway()
        {
            // Get the look input values (horizontal and vertical).
            float lookX = SimpleInput.GetAxis(xLookAxis);
            float lookY = SimpleInput.GetAxis(yLookAxis);

            // Calculate the target position for the hold point based on look input.
            Vector3 targetPosition = new Vector3(-lookX, -lookY, 0) * swayAmount;

            // Clamp the sway amount to prevent excessive movement.
            targetPosition.x = Mathf.Clamp(targetPosition.x, -maxSwayAmount, maxSwayAmount);
            targetPosition.y = Mathf.Clamp(targetPosition.y, -maxSwayAmount, maxSwayAmount);

            // Smoothly interpolate the hold point's position towards the target position.
            holdPoint.localPosition = Vector3.Lerp(holdPoint.localPosition, holdPointOrigin + targetPosition, Time.deltaTime * swaySpeed);
        }

        private void HandleBobbing()
        {
            // Check if the player is moving.
            if (movement.magnitude > 0.1f)
            {
                // Increment the bob timer based on movement speed and frequency.
                bobTimer += Time.deltaTime * bobFrequency;

                // Calculate the horizontal and vertical offsets for the bobbing effect.
                float horizontalOffset = Mathf.Sin(bobTimer) * bobHorizontalAmplitude;
                float verticalOffset = Mathf.Cos(bobTimer * 2) * bobVerticalAmplitude;

                // Combine the offsets into a bobbing position vector.
                Vector3 bobPosition = new Vector3(horizontalOffset, verticalOffset, 0);

                // Smoothly interpolate the hold point's position with the bobbing effect.
                holdPoint.localPosition = Vector3.Lerp(holdPoint.localPosition, holdPointOrigin + bobPosition, Time.deltaTime * bobSmoothing);
            }
            else
            {
                // Smoothly return the hold point to its origin when the player is not moving.
                holdPoint.localPosition = Vector3.Lerp(holdPoint.localPosition, holdPointOrigin, Time.deltaTime * bobSmoothing);
            }
        }

        private void HandleFootsteps()
        {
            // If the player is not moving, don't play footsteps.
            if (movement.magnitude < 0.1f) return;

            // Increase the distance traveled based on movement magnitude and time.
            distanceTraveled += movement.magnitude * Time.deltaTime;

            // Check if the traveled distance exceeds the step distance threshold.
            if (distanceTraveled >= stepDistance)
            {
                PlayFootstepSound();
                distanceTraveled = 0f;
            }
        }

        private void PlayFootstepSound()
        {
            if (footstepClips.Length == 0) return;

            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            audioSource.PlayOneShot(clip);
        }

        public void SetInteractable(IInteractable interactable)
        {
            lastInteractable = interactable;
            InteractWithCurrent();
        }

        private void DetectInteractable()
        {
            // Create a ray from the center of the viewport.
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            // Perform a raycast to detect an interactable within the interact distance.
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, GameConfig.Instance.InteractableLayer))
            {
                IInteractable interactable = hit.transform.GetComponent<IInteractable>();
                if (interactable != lastInteractable)
                {
                    // Defocus the previous interactable (if any)
                    lastInteractable?.OnDefocused();

                    // Focus on the new interactable
                    UIManager.Instance.ToggleInteractButton(true);
                    lastInteractable = interactable;
                    lastInteractable.OnFocused();

                    // Reset hold duration and UI
                    interactHoldDuration = 0f;
                    UIManager.Instance.UpdateHoldProgress(0f);
                }
            }
            else if (lastInteractable != null)
            {
                // Defocus the last interactable when no interactable is hit
                UIManager.Instance.ToggleInteractButton(false);
                lastInteractable.OnDefocused();
                lastInteractable = null;

                // Reset hold duration and UI
                interactHoldDuration = 0f;
                UIManager.Instance.UpdateHoldProgress(0f);
            }

            if (lastInteractable != null)
            {
                if (lastInteractable is Furniture)
                {
                    // Hold interaction for Furniture
                    if (isMobileControl ? SimpleInput.GetButton("Interact") : Input.GetMouseButton(0))
                    {
                        interactHoldDuration += Time.deltaTime;

                        // Update the radial fill UI based on hold progress
                        UIManager.Instance.UpdateHoldProgress(interactHoldDuration / interactHoldThreshold);

                        if (interactHoldDuration >= interactHoldThreshold)
                        {
                            InteractWithCurrent();
                            interactHoldDuration = 0f;
                            UIManager.Instance.UpdateHoldProgress(0f);
                        }
                    }
                    else
                    {
                        interactHoldDuration = 0f;
                        UIManager.Instance.UpdateHoldProgress(0f);
                    }
                }
                else if (isMobileControl ? SimpleInput.GetButtonDown("Interact") : Input.GetMouseButtonDown(0))
                {
                    // Immediate interaction for other interactables
                    InteractWithCurrent();
                }
            }
        }

        private void InteractWithCurrent()
        {
            lastInteractable?.Interact(this);
            UIManager.Instance.ToggleInteractButton(false);
        }

        private void DetectShelfToCustomize()
        {
            // Create a ray from the center of the viewport.
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            // Perform a raycast to detect a shelf within the interact distance.
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, GameConfig.Instance.ShelfLayer))
            {
                Shelf detectedShelf = hit.transform.GetComponent<Shelf>();

                // Check if the detected shelf is different from the last detected shelf.
                if (detectedShelf != lastShelf)
                {
                    UIManager.Instance.ToggleActionUI(ActionType.Price, false, null);
                    UIManager.Instance.ToggleActionUI(ActionType.Label, false, null);

                    // Check if the detected shelf has a product.
                    if (detectedShelf?.Product != null)
                    {
                        // Enable the set price button and set its click action.
                        UIManager.Instance.ToggleActionUI(ActionType.Price, true, () =>
                        {
                            UIManager.Instance.ToggleActionUI(ActionType.Price, false, null);

                            CurrentState = State.Busy;
                            UIManager.Instance.ToggleCrosshair(false);

                            var priceCustomizer = UIManager.Instance.PriceCustomizer;

                            // Remove any existing listeners and add a new listener for the price customizer's close event.
                            priceCustomizer.OnClose.RemoveAllListeners();
                            priceCustomizer.OnClose.AddListener(() =>
                            {
                                CurrentState = State.Free;
                                UIManager.Instance.ToggleCrosshair(true);
                            });

                            priceCustomizer.Open(detectedShelf.Product);

                            // Open the shelving unit if it's not already open.
                            if (!detectedShelf.ShelvingUnit.IsOpen)
                            {
                                detectedShelf.ShelvingUnit.Open(true, true);
                            }

                            detectedShelf.ShelvingUnit.OnDefocused();
                        });
                    }
                    else
                    {
                        UIManager.Instance.ToggleActionUI(ActionType.Label, true, () =>
                        {
                            UIManager.Instance.ToggleActionUI(ActionType.Label, false, null);

                            CurrentState = State.Busy;
                            UIManager.Instance.ToggleCrosshair(false);

                            var labelCustomizer = UIManager.Instance.LabelCustomizer;

                            // Remove any existing listeners and add a new listener for the label customizer's close event.
                            labelCustomizer.OnClose.RemoveAllListeners();
                            labelCustomizer.OnClose.AddListener(() =>
                            {
                                CurrentState = State.Free;
                                UIManager.Instance.ToggleCrosshair(true);
                            });

                            labelCustomizer.Open(detectedShelf);
                            detectedShelf.ShelvingUnit.OnDefocused();
                        });
                    }

                    lastShelf = detectedShelf;
                }
            }
            else if (lastShelf != null)
            {
                // Disable the set price and set label action UIs if no shelf is detected.
                UIManager.Instance.ToggleActionUI(ActionType.Price, false, null);
                UIManager.Instance.ToggleActionUI(ActionType.Label, false, null);

                // Reset the last detected shelf.
                lastShelf = null;
            }
        }

        private void DetectShelfToRestock()
        {
            // Create a ray from the center of the viewport.
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            // Get the currently held box.
            Box box = lastInteractable as Box;

            // Check if the box is open and perform a raycast to detect a shelf.
            if (box.IsOpen && Physics.Raycast(ray, out RaycastHit hit, interactDistance, GameConfig.Instance.ShelfLayer))
            {
                Shelf detectedShelf = hit.transform.GetComponent<Shelf>();

                // Check if the detected shelf is different from the last detected shelf.
                if (detectedShelf != lastShelf)
                {
                    // Check if the box has items to place.
                    if (box.Quantity > 0)
                        // Enable the place button and set its click action.
                        UIManager.Instance.ToggleActionUI(ActionType.Place, true, () =>
                        {
                            // Attempt to place the box's contents on the shelf.
                            bool placed = box.Place(detectedShelf);

                            // Open the shelving unit if placement was successful and it's not already open.
                            if (placed && !detectedShelf.ShelvingUnit.IsOpen)
                                detectedShelf.ShelvingUnit.Open(true, true);
                        });

                    // Check if the shelf has items to take.
                    if (detectedShelf.Quantity > 0)
                        // Enable the take button and set its click action.
                        UIManager.Instance.ToggleActionUI(ActionType.Take, true, () =>
                        {
                            // Attempt to take items from the shelf and put them in the box.
                            bool taken = box.Take(detectedShelf);

                            // Open the shelving unit if taking was successful and it's not already open.
                            if (taken && !detectedShelf.ShelvingUnit.IsOpen)
                                detectedShelf.ShelvingUnit.Open(true, true);
                        });
                    else
                        // Disable the take button if the shelf is empty.
                        UIManager.Instance.ToggleActionUI(ActionType.Take, false, null);

                    // Update the last detected shelf.
                    lastShelf = detectedShelf;
                }
            }
            else if (lastShelf != null)
            {
                // Disable the place and take buttons if no shelf is detected.
                UIManager.Instance.ToggleActionUI(ActionType.Place, false, null);
                UIManager.Instance.ToggleActionUI(ActionType.Take, false, null);

                // Reset the last detected shelf.
                lastShelf = null;
            }
        }

        private void DetectRack()
        {
            // Create a ray from the center of the viewport.
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, GameConfig.Instance.RackLayer))
            {
                Rack detectedRack = hit.transform.GetComponent<Rack>();

                // Check if the detected rack is different from the last detected rack.
                if (detectedRack != lastRack)
                {
                    // Get the currently held box.
                    Box box = lastInteractable as Box;

                    // Check if player held a box and it is not empty.
                    if (box != null && box.Quantity > 0)
                    {
                        // Enable the place button and set its click action.
                        UIManager.Instance.ToggleActionUI(ActionType.Place, true, () =>
                        {
                            // Attempt to store the box on the rack.
                            box.Store(detectedRack, true);
                        });
                    }
                    // Check if the player is NOT holding a box OR is holding an empty box.
                    else if (box == null && detectedRack.BoxQuantity > 0)
                    {
                        // Enable the take button and set its click action.
                        UIManager.Instance.ToggleActionUI(ActionType.Take, true, () =>
                        {
                            // Attempt to retrieve boxes from the rack.
                            detectedRack.RetrieveBox(this);
                            UIManager.Instance.ToggleActionUI(ActionType.Take, false, null);
                        });
                    }
                    else
                    {
                        // Disable the take button if the shelf is empty or the player is holding an empty box.
                        UIManager.Instance.ToggleActionUI(ActionType.Take, false, null);
                    }

                    // Update the last detected shelf.
                    lastRack = detectedRack;
                }
            }
            else if (lastRack != null)
            {
                // Disable the place and take buttons if no rack is detected.
                if (CurrentState != State.Moving)
                    UIManager.Instance.ToggleActionUI(ActionType.Place, false, null);

                UIManager.Instance.ToggleActionUI(ActionType.Take, false, null);

                // Reset the last detected rack.
                lastRack = null;
            }
        }

        private void Work()
        {
            // Check for mouse click and if the last interactable is a CheckoutCounter.
            if (Input.GetMouseButtonDown(0) && lastInteractable is CheckoutCounter counter)
            {
                // If the counter is in the placing state (customer is still placing items), don't scan items.
                if (counter.CurrentState == CheckoutCounter.State.Placing) return;

                // Create a ray from the mouse position.
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                // Perform a raycast to detect a checkout item within the interact distance.
                if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, GameConfig.Instance.CheckoutItemLayer))
                {
                    if (hit.transform.TryGetComponent<CheckoutItem>(out CheckoutItem item))
                    {
                        item.Scan();
                    }
                }
            }
        }

        /// <summary>
        /// Calculates a position in front of the player, taking into account the camera's pitch.
        /// </summary>
        /// <returns>A Vector3 representing the calculated front position.</returns>
        public Vector3 GetFrontPosition()
        {
            // Get the camera's pitch angle.
            float pitch = mainCamera.transform.localEulerAngles.x;

            // Adjust the pitch angle if it's greater than 180 degrees.
            if (pitch > 180) pitch -= 360;

            // Normalize the pitch angle to a 0-1 range.
            float normalizedPitch = Mathf.InverseLerp(lookXLimit, 0f, pitch);

            // Define the minimum and maximum distances for the front position.
            float minDistance = 1.5f;
            float maxDistance = 3f;

            // Interpolate between the minimum and maximum distances based on the normalized pitch.
            float offset = Mathf.Lerp(minDistance, maxDistance, normalizedPitch);

            // Calculate the front position based on the player's transform and the calculated offset.
            Vector3 front = transform.TransformPoint(Vector3.forward * offset);

            // Return the front position, zeroing out the Y-component and flooring the X and Z components to the nearest tenth.
            return new Vector3(front.x, 0f, front.z).FloorToTenth();
        }
    }
}
