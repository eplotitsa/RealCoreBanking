using System;
using System.Reflection.Metadata;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;



namespace BankingSystem
{
    static class ReadHoldFile
    {
        public static List<T> ReadRecords<T>(string filePath, string targetAccountNumber)
        {
            var filteredRecords = new List<T>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null
            };

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<T>();
                foreach (var record in records)
                {
                    var accountNumberProp = typeof(T).GetProperty("AccountNumber");
                    if (accountNumberProp != null)
                    {
                        var value = accountNumberProp.GetValue(record)?.ToString();
                        if (value == targetAccountNumber)
                        {
                            filteredRecords.Add(record);
                        }
                    }
                }
            }

            return filteredRecords;
        }
        //public static List<T> DeleteRecords<T>(string filePath){; }
        /*public static List<T> ReadRecords<T>( string filePath, string targetAccountNumber)
        {
            var filteredRecords = new List<T>();
            
            var propertyMap = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name, p => p);

            using (var reader = new StreamReader(filePath))
            {
                // Чтение первой строки для получения заголовков
                string headerLine = reader.ReadLine();
                if (headerLine == null)
                {
                    Console.WriteLine("Файл пустой");
                    return filteredRecords;
                }

                var headers = headerLine.Split(',');
                var columnIndices = new Dictionary<string, int>();

                // Найти индексы столбцов, соответствующих свойствам класса
                foreach (var propName in propertyMap.Keys)
                {
                    int index = Array.IndexOf(headers, propName);
                    if (index >= 0)
                        columnIndices[propName] = index;
                }

                if (!columnIndices.ContainsKey("AccountNumber"))
                {
                    Console.WriteLine("Не найден столбец 'AccountNumber'");
                    return filteredRecords;
                }

                // Чтение оставшихся строк и фильтрация по номеру счета
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    if (values.Length <propertyMap.Count)
                        continue;

                    if (values[columnIndices["AccountNumber"]].Trim() != targetAccountNumber)
                        continue;

                    var record = Activator.CreateInstance<T>();

                    foreach (var kvp in columnIndices)
                    {
                        var prop = propertyMap[kvp.Key];
                        var value = values[kvp.Value];

                        try
                        {
                            object parsedValue = Convert.ChangeType(value, prop.PropertyType, CultureInfo.InvariantCulture);
                            prop.SetValue(record, parsedValue);
                        }
                        catch
                        {
                            // можно логировать ошибку, если нужно
                            continue;
                        }
                    }

                    filteredRecords.Add(record);
                }
            }

            return filteredRecords;
        }*/
    } 

}