using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CryingSnow.CheckoutFrenzy.sara_code
{
    public static class CustomerSatisfactionTracker
    {
        private static List<int> satisfactionScores = new List<int>();
        private static int customersWhoDidNotBuy = 0;


        public static void RecordSatisfaction(int score)
        {
            satisfactionScores.Add(score);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateSatisfactionDisplay(GetAverageSatisfaction());
            }
        }


        public static float GetAverageSatisfaction()
        {
            if (satisfactionScores.Count == 0) return 0f;

            float rawAverage = (float)satisfactionScores.Average(); // 

            float penalty = customersWhoDidNotBuy * 3f; //  Your 3% per customer idea

            return Mathf.Max(rawAverage - penalty, 0f); // Never go below 0%
        }

        public static void RecordNonBuyer()
        {
            customersWhoDidNotBuy++;
        }


        public static void Reset()
        {
            satisfactionScores.Clear();
            customersWhoDidNotBuy = 0; // 
        }
    }
}
