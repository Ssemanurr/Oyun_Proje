using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CryingSnow.CheckoutFrenzy;


public class PaymentTimerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Image fillImage;

    private float totalTime = 40f;
    private float currentTime;
    private bool isRunning = false;
    private Customer currentCustomer;

    public void StartTimer(Customer customer)
    {
        currentCustomer = customer;
        currentTime = totalTime;
        isRunning = true;
        gameObject.SetActive(true);
    }

    public void StopTimer()
    {
        isRunning = false;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;
        timerText.text = Mathf.CeilToInt(currentTime).ToString();
        fillImage.fillAmount = currentTime / totalTime;

        if (currentTime <= 0)
        {
            isRunning = false;
            if (currentCustomer != null)
            {
                currentCustomer.DecreaseSatisfaction(10);
                currentCustomer.AskToLeave();
            }
            gameObject.SetActive(false);
        }
    }
}

