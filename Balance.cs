using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;



namespace BankingSystem
{
    abstract class Balance
    {
        public decimal Amount { get; set; }

        public Balance(decimal initialAmount = 0)
        {
            Amount = initialAmount;
        }



        private void Deposit(decimal value)
        {
            Amount += value;
        }

        public bool Withdraw(decimal value)
        {
            if (value > Amount)
                return false;
            Amount -= value;
            return true;
        }
        public decimal GetAmount()
        {
            return Amount;
        }

        //public abstract decimal GetOTB();
        public virtual decimal GetOTB()
        {
            return Amount;
        }
    }
    // Счет картотеки

    

    
    // Счет на котором учитывается баланс клиента и на который начисляются проценты
    class TotalBalance : Balance
    {
        
        public TotalBalance(decimal initialAmount) : base(initialAmount) { }
        public override decimal GetOTB()
        {
            return Amount;
        }

        public decimal GetTotalBalance()
        {
            return Amount;
        }
    }
   
     
   
}