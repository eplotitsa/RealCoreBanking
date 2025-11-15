using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace BankingSystem
{

    // Класс для хранения информации о транзакциях типа Hold
    public class Transaction
    {
        public DateTime Timestamp { get; set; }
        public decimal Amount { get; set; }
        public string Code { get; set; } = "";
        public string MerchantName { get; set; } = "";
        public string AccountNumber { get; set; } = "";

    }
    // Счет холдов
    // Hold - это временная блокировка средств, которая не влияет на общую сумму счета 
    class Hold : Balance
    {
        public Hold(decimal initialAmount) : base()
        {
            // Загрузка данных из CSV файла
            //if (LoadFromCSV("C:\\Users\\Yevhen\\OneDrive\\C#\\realcorebanking\\RealCoreBanking\\Hold.csv"))
            if (ReadHoldFile.ReadRecords<Transaction>("C:\\Users\\Yevhen\\OneDrive\\C#\\realcorebanking\\RealCoreBanking\\Hold.csv", "123456789") is List<Transaction> transactions){
                this.transactions = transactions;
                
                Amount = GetHoldsTotal();
            }
            else
            {
                Console.WriteLine("Ошибка загрузки данных из Hold.csv");
            }
        }
        private readonly List<Transaction> transactions = new();
        public void Add(Transaction tr)
        {
            transactions.Add(tr);
            Amount = GetHoldsTotal();
        }
        /* public bool LoadFromCSV(string filePath)
        {
            try
            {
                // Пример загрузки из CSV
                var lines = File.ReadAllLines(filePath);
                int counter = 0;
                foreach (var line in lines)
                {
                    if (counter == 0) // Пропустить заголовок
                    {
                        counter++; // Пропустить заголовок
                        continue;
                    }
                    var parts = line.Split(',');
                    if (parts.Length < 4) continue; // Пропустить некорректные строки

                    var transaction = new Transaction
                    {
                        Timestamp = DateTime.Parse(parts[0]),
                        Amount = decimal.Parse(parts[1]),
                        Code = parts[2],
                        MerchantName = parts[3]
                    };
                    Add(transaction);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки: {ex.Message}");
                return false;
            }
        }

 */
        public decimal GetHoldsTotal() => transactions.Sum(t => t.Amount);
        public override decimal GetOTB()
        {
            return -Amount;
        }

    }
}