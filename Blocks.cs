using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace BankingSystem
{
public record BlockType
    {
        
        public DateTime Timestamp { get; set; }
        public decimal Amount { get; set; }
        public string Code { get; set; } = "";
        public int Priority { get; set; } = 500;
        public string AccountNumber { get; set; } = "";
      
    }
    class Blocks : Balance
    {
        private readonly List<BlockType> blocks = new();
        public Blocks(decimal initialAmount) : base()
        {
            if (ReadHoldFile.ReadRecords<BlockType>("C:\\Users\\Yevhen\\OneDrive\\C#\\realcorebanking\\RealCoreBanking\\Block.csv", "123456789") is List<BlockType> blocks)
            {
                this.blocks = blocks;

                Amount = GetBlocksTotal();
            }
            else
            {
                Console.WriteLine("Ошибка загрузки данных из Hold.csv");
            }
        }
        public decimal GetBlocksTotal() => blocks.Sum(t => t.Amount);
        public void Add(BlockType block)
        {
            blocks.Add(block);
            Amount = GetBlocksTotal();
        }
        public override decimal GetOTB()
        {
            return -Amount;
        }
    }
}