using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Democracy.Csv
{
    class CsvWriter
    {
        public static void WriteCSV<T>(IEnumerable<T> simulationResults, string fileName)
        {
            var csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            Console.WriteLine(csvFilePath);

            using (var fileStream = new FileStream(csvFilePath, FileMode.Create))
            {
                using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    using (var csvWriter = new CsvHelper.CsvWriter(writer))
                    {
                        csvWriter.WriteHeader<T>();
                        csvWriter.Flush();
                        writer.WriteLine();
                        writer.Flush();

                        foreach (var result in simulationResults)
                        {
                            csvWriter.WriteRecord(result);
                            csvWriter.Flush();
                            writer.WriteLine();
                            writer.Flush();

                        }
                    }
                }
            }
        }
    }
}
