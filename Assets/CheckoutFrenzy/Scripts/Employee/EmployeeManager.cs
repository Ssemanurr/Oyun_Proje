using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CryingSnow.CheckoutFrenzy
{
    public class EmployeeManager : MonoBehaviour
    {
        public static EmployeeManager Instance { get; private set; }

        [SerializeField] private Transform janitorPoint;
        [SerializeField] private Transform sorterPoint;
        [SerializeField] private Transform stockerPoint;

        [SerializeField] private List<Employee> employeePrefabs;

        public List<Employee> Employees => employeePrefabs;
        public event System.Action OnUnpaidEmployeeFired;

        private Dictionary<EmployeeType, Employee> employeeLookup;

        private void Awake()
        {
            Instance = this;

            employeeLookup = employeePrefabs.ToDictionary(emp => emp.Type);
        }

        private void Start()
        {
            StoreManager.Instance.OnDayStarted += HandleDayStarted;

            foreach (var hiredEmployee in DataManager.Instance.Data.HiredEmployees)
            {
                SpawnEmployee(hiredEmployee);
            }
        }

        private void OnDisable()
        {
            StoreManager.Instance.OnDayStarted -= HandleDayStarted;
        }

        private void HandleDayStarted()
        {
            for (int i = DataManager.Instance.Data.HiredEmployees.Count - 1; i >= 0; i--)
            {
                var hiredEmployee = DataManager.Instance.Data.HiredEmployees[i];

                if (!employeeLookup.TryGetValue(hiredEmployee, out var employeePrefab))
                {
                    Debug.LogWarning($"Employee type {hiredEmployee} not found in lookup.");
                    continue;
                }

                int cost = employeePrefab.Cost;

                if (DataManager.Instance.PlayerMoney < cost)
                {
                    FireEmployee(hiredEmployee);
                    OnUnpaidEmployeeFired?.Invoke();
                }
                else
                {
                    DataManager.Instance.PlayerMoney -= cost;
                    UIManager.Instance.Message.Log($"{hiredEmployee} paid!");
                }
            }
        }

        public void HireEmployee(EmployeeType type)
        {
            if (!CanHireEmployee(type)) return;

            int cost = employeeLookup[type].Cost;
            if (!IsFundSufficient(cost)) return;

            SpawnEmployee(type);

            DataManager.Instance.Data.HiredEmployees.Add(type);
            UIManager.Instance.Message.Log($"{type} hired successfully!");

            DataManager.Instance.PlayerMoney -= cost;
            AudioManager.Instance.PlaySFX(AudioID.Kaching);
        }

        private bool CanHireEmployee(EmployeeType type)
        {
            switch (type)
            {
                case EmployeeType.Cashier:
                    var freeCounter = StoreManager.Instance.GetCounter(false);
                    if (freeCounter == null)
                    {
                        UIManager.Instance.Message.Log("All counters are taken!");
                        return false;
                    }
                    break;

                case EmployeeType.Janitor:
                    if (janitorPoint.childCount > 0)
                    {
                        UIManager.Instance.Message.Log("A janitor already hired!");
                        return false;
                    }
                    break;

                case EmployeeType.Sorter:
                    if (!DataManager.Instance.Data.IsWarehouseUnlocked)
                    {
                        UIManager.Instance.Message.Log("Unlock warehouse first!");
                        return false;
                    }

                    if (sorterPoint.childCount > 0)
                    {
                        UIManager.Instance.Message.Log("A sorter already hired!");
                        return false;
                    }
                    break;

                case EmployeeType.Stocker:
                    if (!DataManager.Instance.Data.IsWarehouseUnlocked)
                    {
                        UIManager.Instance.Message.Log("Unlock warehouse first!");
                        return false;
                    }

                    if (stockerPoint.childCount > 0)
                    {
                        UIManager.Instance.Message.Log("A stocker already hired!");
                        return false;
                    }
                    break;

                default:
                    break;
            }

            return true;
        }

        private bool IsFundSufficient(int cost)
        {
            if (DataManager.Instance.PlayerMoney < cost)
            {
                UIManager.Instance.Message.Log("You don't have enough money!", Color.red);
                return false;
            }

            return true;
        }

        private void SpawnEmployee(EmployeeType type)
        {
            var employeePrefab = employeeLookup[type];

            switch (type)
            {
                case EmployeeType.Cashier:
                    var freeCounter = StoreManager.Instance.GetCounter(false);
                    freeCounter.AssignCashier(Instantiate(employeePrefab as Cashier));
                    break;

                case EmployeeType.Janitor:
                    Instantiate(employeePrefab as Janitor, janitorPoint.position, janitorPoint.rotation, janitorPoint);
                    break;

                case EmployeeType.Sorter:
                    Instantiate(employeePrefab as Sorter, sorterPoint.position, sorterPoint.rotation, sorterPoint);
                    break;

                case EmployeeType.Stocker:
                    Instantiate(employeePrefab as Stocker, stockerPoint.position, stockerPoint.rotation, stockerPoint);
                    break;

                default:
                    break;
            }
        }

        public void FireEmployee(EmployeeType employee)
        {
            DataManager.Instance.Data.HiredEmployees.Remove(employee);
            UIManager.Instance.Message.Log($"{employee} fired!", Color.red);

            switch (employee)
            {
                case EmployeeType.Cashier:
                    var occupiedCounter = StoreManager.Instance.GetCounter(true);
                    occupiedCounter.AssignCashier(null);
                    break;

                case EmployeeType.Janitor:
                    janitorPoint.GetComponentInChildren<Employee>().Dismiss();
                    break;

                case EmployeeType.Sorter:
                    sorterPoint.GetComponentInChildren<Employee>().Dismiss();
                    break;

                case EmployeeType.Stocker:
                    stockerPoint.GetComponentInChildren<Employee>().Dismiss();
                    break;

                default:
                    break;
            }
        }
    }
}
