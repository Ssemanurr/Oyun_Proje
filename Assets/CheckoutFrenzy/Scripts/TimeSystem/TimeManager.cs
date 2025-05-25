using System.Globalization;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace CryingSnow.CheckoutFrenzy
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }
        public bool IsPausedBySummary { get; set; } = false;

        [SerializeField] private GameObject skipDialog;
        [SerializeField] private Button skipButton;

        [SerializeField, Range(0f, 1439f)] private float totalMinutes = 480f;
        [SerializeField, Range(1, 60)] private float timeScale = 1.0f;
        [SerializeField] private float sunMidnightOffset = -90f;
        [SerializeField] private float sunDirectionOffset = -90f;
        [SerializeField] private TimeRange nightTime;
        [SerializeField] private Material[] emissiveMaterials;

        [Header("Fog Colors")]
        [SerializeField] private Color nightFogColor = Color.grey;
        [SerializeField] private Color dayFogColor = Color.blue;

        private Light sun;
        private bool wasNightTime;
        private bool dayEnded;
        private PlayerUIBlocker playerUIBlocker;

        public bool AllowTimeUpdate { get; set; } = true;

        public int Hour => Mathf.FloorToInt(totalMinutes / 60f);
        public int Minute => Mathf.FloorToInt(totalMinutes % 60f);
        public int TotalMinutes => Mathf.FloorToInt(totalMinutes);

        public event System.Action<bool> OnNightTimeChanged;
        public event System.Action OnMinutePassed;

        private int previousMinute;
        private const float MinutesPerDay = 24f * 60f;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            var sunObj = new GameObject("Sun");
            sun = sunObj.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.bounceIntensity = 0f;
            sun.shadows = LightShadows.None;
            sun.renderMode = LightRenderMode.ForceVertex;
            sun.cullingMask = 0;
            RenderSettings.sun = sun;

            totalMinutes = 480f;
            wasNightTime = !nightTime.IsWithinRange(TotalMinutes);
            UpdateSunRotation();

            var player = FindFirstObjectByType<PlayerController>();
            if (player != null)
                playerUIBlocker = new PlayerUIBlocker(player);

            if (skipDialog != null)
                skipDialog.SetActive(false);

            if (skipButton != null)
            {
                skipButton.onClick.RemoveAllListeners();  // ❗ Her şeyden önce kaldır
                skipButton.onClick.AddListener(() =>
                {
                    SkipToNextDay();
                });
            }
        }


        private void Update()
        {
            if (!AllowTimeUpdate || dayEnded || IsPausedBySummary) return;

            totalMinutes += Time.deltaTime * timeScale;

            if (totalMinutes >= 1080f && !dayEnded)
            {
                dayEnded = true;
                AllowTimeUpdate = false;

                if (DataManager.Instance.Data.TotalDays >= 9)
                {
                    skipDialog?.SetActive(false);
                    playerUIBlocker?.Block();

                    SummaryScreen.Instance.Show(new SummaryData(DataManager.Instance.Data.PlayerMoney), skip =>
                    {
                        playerUIBlocker?.Unblock();
                    });

                    return;
                }

                skipDialog?.SetActive(true);
                playerUIBlocker?.Block();
            }

            UpdateSunRotation();
            UpdateTimeState();
            DataManager.Instance.Data.TotalMinutes = Mathf.FloorToInt(totalMinutes);
        }


        public void SkipToNextDay()
        {
            if (dayEnded == false) return;
            dayEnded = false;
            
            if (skipDialog != null)
                skipDialog.SetActive(false);

            totalMinutes = 480f;
            dayEnded = false;

            var data = DataManager.Instance.Data;

            data.TotalDays++;
            data.DaysInCurrentLevel++;

            UpdateSunRotation();
            UpdateTimeState();
            if (data.TotalDays >= 9)
            {
                dayEnded = true;
                AllowTimeUpdate = false;

                if (skipDialog != null)
                    skipDialog.SetActive(false);

                playerUIBlocker?.Block();

                SummaryScreen.Instance.Show(new SummaryData(data.PlayerMoney), skip =>
                {
                    playerUIBlocker?.Unblock();
                });

                return;
            }

            if ((data.CurrentLevel == 1 && data.DaysInCurrentLevel >= 3) ||
                (data.CurrentLevel == 2 && data.DaysInCurrentLevel >= 5))
            {
                data.CurrentLevel++;
                data.DaysInCurrentLevel = 0;

                AllowTimeUpdate = false;
                playerUIBlocker?.Block();

                SummaryScreen.Instance.Show(new SummaryData(data.PlayerMoney), skip =>
                {
                    AllowTimeUpdate = true;
                });

                return;
            }

            AllowTimeUpdate = true;
            playerUIBlocker?.Unblock();
            

        }

        private void UpdateSunRotation()
        {
            float timeNormalized = totalMinutes / MinutesPerDay;
            var targetRotation = Quaternion.Euler(
                360f * timeNormalized + sunMidnightOffset,
                sunDirectionOffset,
                0f
            );
            sun.transform.rotation = targetRotation;
        }

        private void UpdateTimeState()
        {
            bool isCurrentlyNightTime = IsNightTime();

            if (isCurrentlyNightTime != wasNightTime)
            {
                wasNightTime = isCurrentlyNightTime;
                OnNightTimeChanged?.Invoke(isCurrentlyNightTime);
                UpdateEmissiveMaterials(isCurrentlyNightTime);
                UpdateFogColor(isCurrentlyNightTime);
            }

            if (previousMinute != Minute)
            {
                previousMinute = Minute;
                OnMinutePassed?.Invoke();
            }
        }

        private void UpdateEmissiveMaterials(bool isNight)
        {
            foreach (var mat in emissiveMaterials)
            {
                if (isNight) mat.EnableKeyword("_EMISSION");
                else mat.DisableKeyword("_EMISSION");
            }
        }

        private void UpdateFogColor(bool isNight)
        {
            Color targetColor = isNight ? nightFogColor : dayFogColor;
            DOTween.To(() => RenderSettings.fogColor, x => RenderSettings.fogColor = x, targetColor, 3f)
                   .SetEase(Ease.Linear);
        }

        public bool IsNightTime()
        {
            return nightTime.IsWithinRange(TotalMinutes);
        }

        public void SetTime(int newHour, int newMinute)
        {
            totalMinutes = Mathf.Clamp(newHour, 0, 23) * 60f + Mathf.Clamp(newMinute, 0, 59);
            UpdateSunRotation();
        }

        public void SetTimeScale(float newTimeScale)
        {
            timeScale = Mathf.Max(0, newTimeScale);
        }

        public string GetFormattedTime()
        {
            System.DateTime currentTime = new System.DateTime(1, 1, 1, Hour, Minute, 0);
            return currentTime.ToString("hh:mm tt", CultureInfo.InvariantCulture);
        }
    }
}

