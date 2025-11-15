using System;
using System.Collections.Generic;
using BankingSystem;

namespace RealCoreBanking
{


    partial class Program
    {
        static void Main()
        {
            Account account = new Account("123456789", "John Doe");
            account.PrintBalances();

            // Application logic will be implemented here
            Console.WriteLine("OTB balance: " + account.GetOTB());
            Console.WriteLine("Total balance: " + account.GetTotalBalance());
            if (account.Withdraw(120))
            {
                Console.WriteLine("Withdrawal successful.");
                Console.WriteLine("Total balance: " + account.GetTotalBalance());
                Console.WriteLine("OTB balance: " + account.GetOTB());
            }
            else
            {
                Console.WriteLine("Withdrawal failed due to insufficient OTB.");
            }
        }
    }
}