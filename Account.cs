using System;
using System.Collections.Generic;
using System.Linq;

// основные операции по счету:
// узнать баланс по чету
// узнать доступную сумму OTB
// снять деньги со счета, если есть OTB
// Добавить деньги на счет 
// добавить Hold - временную блокировку средств, которая не влияет на общую сумму счета
// добавить Block - картотеку, которая влияет на общую сумму счета
// Снять Hold - временную блокировку средств, которая не влияет на общую сумму счета
// Снять Block - картотеку, которая влияет на общую сумму счета

namespace BankingSystem
{
    class Account : IDisposable
    {
        private bool _disposed;
        // Factory method to create Balance instances based on class name
        // счет может хранить несколько типов балансов. типы балансов определяются классами, наследуемыми от Balance
        // пока присутствуют следующие типы балансов: Hold, TotalBalance, Blocks
        // hold - временная блокировка средств, которая не влияет на доступную сумму но может быть 
        // применено только если на счету есть достаточная сумма
        // TotalBalance - основной баланс счета, на который начисляются проценты
        // Blocks - картотека, которая влияет на общую сумму счета и доступную сумму OTB но может быть 
        // установлено даже если на счету нет достаточной суммы
        static Balance Create(string name, int value)
        {
            return name switch
            {
                "Hold" => new Hold(value),
                "TotalBalance" => new TotalBalance(value),
                "Blocks" => new Blocks(value),
                _ => throw new ArgumentException($"Unknown class: {name}")
            };
        }
        // List of balances supported by account
        static List<(string ClassName, int Value)> InitBalance()
        {
            var data = new List<(string ClassName, int Value)>
            {
                ("Hold", 23),
                ("TotalBalance", 12233),
                ("Blocks", 11),


            };
            return data;
        }
        public decimal TotalBlocks { get; set; }
        public decimal TotalHold { get; set; }
        public string AccountNumber { get; set; }
        public string AccountHolder { get; set; }
        public Balance AccountBalance { get; set; }
        Balance[] arr = InitBalance()
        .Select(tuple => Create(tuple.ClassName, tuple.Value))
        .ToArray();

        public void PrintBalances()
        {
            foreach (var balance in arr)
            {
                Console.WriteLine($"{balance.GetAmount()}: {balance.GetType().Name}");
            }
        }

        public decimal GetOTB()
        {
            decimal OTB = 0;
            foreach (var balance in arr)
            {
                OTB += balance.GetOTB();
            }
            return OTB;
        }
        // вернуть общую сумму Accounta - TotalBalance без блокировок
        // если нет TotalBalance, то вернуть 0  
        public decimal GetTotalBalance()
        {

            TotalBalance instance = arr.OfType<TotalBalance>().FirstOrDefault();
            return instance?.GetTotalBalance() ?? 0;
        }

        // Списание денег со счета, возможно только когда есть доступная сумма OTB 
        // в противном случае  вернется false

        public bool Withdraw(decimal amount)
        {
            decimal OTB = GetOTB();
            if (OTB >= amount)
            {
                // Необходимо уменьшить баланс счета средств 

                arr.OfType<TotalBalance>().FirstOrDefault().Withdraw(amount);

                return true;
            }
            else
            {
                return false;
            }
        }

        public Account(string accountNumber, string accountHolder)
        {
            AccountNumber = accountNumber;
            AccountHolder = accountHolder;

        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // освобождение управляемых ресурсов
            }
            _disposed = true;
            // освобождение неуправляемых ресурсов
        }

    }
}