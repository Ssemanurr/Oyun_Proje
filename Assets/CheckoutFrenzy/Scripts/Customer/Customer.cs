using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

namespace CryingSnow.CheckoutFrenzy
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class Customer : MonoBehaviour
    {
        [SerializeField] private HandAttachments handAttachments;
        [SerializeField] private GameObject Timer;

        public List<Product> Inventory => inventory;

        private int satisfaction = 100;
        public int Satisfaction => satisfaction;

        private Animator animator;
        private NavMeshAgent agent;

        private ShelvingUnit shelvingUnit;
        private List<Product> inventory = new List<Product>();
        private int queueNumber = int.MaxValue;

        private bool isPicking;

        private ChatBubble chatBubble;
        private Dialogue notFoundDialogue => GameConfig.Instance.NotFoundDialogue;
        private Dialogue overpricedDialogue => GameConfig.Instance.OverpricedDialogue;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();

            agent.speed = 1.5f;
            agent.angularSpeed = 3600f;
            agent.acceleration = 100f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            agent.SetAreaCost(3, 50f);
        }

        private void Start()
        {
            StartCoroutine(CheckEnteringStore());
            StartCoroutine(Shopping());
        }

        private void Update()
        {
            CheckStoreDoors();
        }

        private void CheckStoreDoors()
        {
            Ray ray = new Ray(transform.position + Vector3.up, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 1f, GameConfig.Instance.InteractableLayer))
            {
                if (hit.transform.TryGetComponent<EntranceDoor>(out EntranceDoor door))
                {
                    door.OpenIfClosed();
                }
            }
        }

        private IEnumerator CheckEnteringStore()
        {
            while (!StoreManager.Instance.IsWithinStore(transform.position))
            {
                yield return new WaitForSeconds(0.1f);
            }

            AudioManager.Instance.PlaySFX(AudioID.Bell);
        }

        private IEnumerator Shopping()
        {
            bool continueShopping = true;

            while (continueShopping)
            {
                yield return FindShelvingUnit();
                yield return PickProduct();

                continueShopping = Random.value < 0.5f;
            }

            if (shelvingUnit != null && shelvingUnit.IsOpen)
            {
                shelvingUnit.Close(true, false);
            }

            if (inventory.Count > 0)
            {
                yield return UpdateQueue();
                yield return StoreManager.Instance.Checkout(this);
            }
            else
            {
                UpdateChatBubble(notFoundDialogue.GetRandomLine());
                yield return StoreManager.Instance.CustomerLeave(this);
            }
        }

        private IEnumerator FindShelvingUnit()
        {
            var newShelvingUnit = StoreManager.Instance.GetShelvingUnit();

            if (shelvingUnit != null && shelvingUnit != newShelvingUnit && shelvingUnit.IsOpen)
            {
                shelvingUnit.Close(true, false);
            }

            shelvingUnit = newShelvingUnit;

            if (shelvingUnit == null) yield break;

            StoreManager.Instance.UnregisterShelvingUnit(shelvingUnit);

            agent.SetDestination(shelvingUnit.Front);

            while (!HasArrived())
            {
                if (shelvingUnit.IsMoving)
                {
                    agent.SetDestination(transform.position);
                    shelvingUnit = null;
                    yield break;
                }

                yield return null;
            }

            yield return LookAt(shelvingUnit.transform);
        }

        private IEnumerator PickProduct()
        {
            if (shelvingUnit == null) yield break;

            var shelf = shelvingUnit.GetShelf();

            if (shelf == null || shelvingUnit.IsMoving)
            {
                StoreManager.Instance.RegisterShelvingUnit(shelvingUnit);
                yield break;
            }

            var product = shelf.Product;

            if (IsWillingToBuy(product))
            {
                inventory.Add(product);

                var productObj = shelf.TakeProductModel();

                if (!shelf.ShelvingUnit.IsOpen) shelf.ShelvingUnit.Open(true, false);

                float height = shelf.transform.position.y;
                string pickTrigger = "PickMedium";
                if (height < 0.5f) pickTrigger = "PickLow";
                else if (height > 1.5f) pickTrigger = "PickHigh";

                animator.SetTrigger(pickTrigger);

                yield return new WaitUntil(() => isPicking);

                Transform grip = handAttachments.Grip;

                productObj.transform.SetParent(grip);

                isPicking = false;

                productObj.transform.DOLocalRotate(Vector3.zero, 0.25f);
                productObj.transform.DOLocalMove(Vector3.zero, 0.25f);

                bool isIdle = false;
                while (!isIdle)
                {
                    isIdle = animator.GetCurrentAnimatorStateInfo(0).IsName("Idle");
                    yield return null;
                }

                Destroy(productObj);

                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                string chat = overpricedDialogue.GetRandomLine();
                chat = chat.Replace("{product}", product.Name);
                UpdateChatBubble(chat);
            }

            StoreManager.Instance.RegisterShelvingUnit(shelvingUnit);
        }

        private bool IsWillingToBuy(Product product)
        {
            float priceToleranceFactor = 1f + Mathf.Pow(Random.value, 2f);
            decimal maxAcceptablePrice = product.MarketPrice * (decimal)priceToleranceFactor;
            decimal customPrice = DataManager.Instance.GetCustomProductPrice(product);
            return customPrice <= maxAcceptablePrice;
        }

        private IEnumerator UpdateQueue()
        {
            while (queueNumber > 0)
            {
                var newQueue = StoreManager.Instance.GetQueueNumber(this);

                if (newQueue.queueNumber < queueNumber)
                {
                    queueNumber = newQueue.queueNumber;
                    yield return MoveTo(newQueue.queuePosition);
                    yield return LookAt(newQueue.lookDirection);
                }
                else
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        public IEnumerator HandsPayment(bool isUsingCash, Cashier cashier)
        {
            bool isPaying = true;

            animator.SetBool("IsPaying", isPaying);

            handAttachments.ActivatePaymentObject(isUsingCash);

            Camera mainCamera = Camera.main;

            while (isPaying)
            {
                if (cashier != null)
                {
                    yield return new WaitForSeconds(0.3f);
                    cashier.TakePayment();
                    yield return new WaitForSeconds(0.7f);
                    isPaying = false;
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, 10f, GameConfig.Instance.PaymentLayer))
                    {
                        isPaying = false;
                    }
                }

                yield return null;
            }

            animator.SetBool("IsPaying", isPaying);
            handAttachments.DeactivatePaymentObjects();
        }

        public IEnumerator MoveTo(Vector3 position)
        {
            agent.SetDestination(position);
            yield return new WaitUntil(() => HasArrived());
            yield return new WaitForEndOfFrame();
        }

        public void AskToLeave()
        {
            if (inventory.Count > 0) return;
            StopAllCoroutines();
            if (shelvingUnit != null) StoreManager.Instance.RegisterShelvingUnit(shelvingUnit);
            StartCoroutine(StoreManager.Instance.CustomerLeave(this));
        }

        public void IncreaseSatisfaction(int amount)
        {
            satisfaction = Mathf.Min(satisfaction + amount, 100);
        }

        public void DecreaseSatisfaction(int amount)
        {
            satisfaction = Mathf.Max(satisfaction - amount, 0);
        }

        private IEnumerator LookAt(Transform target)
        {
            var lookDirection = (target.position - transform.position).Flatten();
            var lookRotation = Quaternion.LookRotation(lookDirection);
            yield return transform.DORotateQuaternion(lookRotation, 0.5f).WaitForCompletion();
        }

        private IEnumerator LookAt(Vector3 lookDirection)
        {
            var lookRotation = Quaternion.LookRotation(lookDirection.Flatten());
            yield return transform.DORotateQuaternion(lookRotation, 0.5f).WaitForCompletion();
        }

        private bool HasArrived()
        {
            if (!agent.pathPending)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    {
                        animator.SetBool("IsMoving", false);
                        return true;
                    }
                }
            }

            animator.SetBool("IsMoving", true);
            return false;
        }

        private void UpdateChatBubble(string chat)
        {
            if (chatBubble != null) return;
            chatBubble = UIManager.Instance.ShowChatBubble(chat, transform);
        }

        public void OnPick(AnimationEvent animationEvent)
        {
            isPicking = true;
        }
    }
}
