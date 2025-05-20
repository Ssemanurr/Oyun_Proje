using System.Collections.Generic;

namespace CryingSnow.CheckoutFrenzy
{
    [System.Serializable]
    public class GameData
    {
        public string StoreName { get; set; }
        public decimal PlayerMoney { get; set; }
        public int CurrentLevel { get; set; }
        public int CurrentExperience { get; set; }

        public List<FurnitureData> SavedFurnitures { get; set; }
        public List<BoxData> SavedBoxes { get; set; }

        public List<CustomPrice> CustomPrices { get; set; }

        public decimal PendingOrdersValue { get; set; }
        public decimal UnpaidProductsValue { get; set; }

        public HashSet<int> LicensedProducts { get; set; }
        public HashSet<int> OwnedLicenses { get; set; }

        public int ExpansionLevel { get; set; }
        public bool IsWarehouseUnlocked { get; set; }

        public int TotalDays { get; set; }
        public int TotalMinutes { get; set; }

        public SummaryData CurrentSummary { get; set; }
        public MissionData CurrentMission { get; set; }

        public List<EmployeeType> HiredEmployees { get; set; }

        public System.DateTime LastSaved { get; set; }
        public System.TimeSpan TotalPlaytime { get; set; }

        public void Initialize()
        {
            StoreName = GameConfig.Instance.DefaultStoreName;

            PlayerMoney = GameConfig.Instance.StartingMoney;
            CurrentLevel = 1;

            SavedFurnitures = new List<FurnitureData>();
            SavedBoxes = new List<BoxData>();
            CustomPrices = new List<CustomPrice>();
            LicensedProducts = new HashSet<int>();
            OwnedLicenses = new HashSet<int>();

            TotalDays = 1;

            var openTime = GameConfig.Instance.OpenTime;
            int totalMinutes = TimeRange.ToMinutes(openTime.StartHour, openTime.StartMinute);
            TotalMinutes = totalMinutes;

            CurrentSummary = new SummaryData(PlayerMoney);
            CurrentMission = new MissionData(1);

            HiredEmployees = new List<EmployeeType>();
        }
    }
}
