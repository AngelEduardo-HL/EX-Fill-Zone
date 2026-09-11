using System;

namespace ExFillZone.Gameplay.Loot
{
    public static class RunLoot
    {
        public static int TotalMoney { get; private set; }

        public static event Action<int, int> MoneyAdded;

        public static void Reset()
        {
            TotalMoney = 0;
        }

        public static void AddMoney(int amount)
        {
            if (amount <= 0) return;

            TotalMoney += amount;
            MoneyAdded?.Invoke(amount, TotalMoney);
        }
    }
}