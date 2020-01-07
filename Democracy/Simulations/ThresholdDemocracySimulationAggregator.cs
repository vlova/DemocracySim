using CsvHelper.Configuration.Attributes;
using Democracy.Csv;
using Democracy.Simulations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Democracy
{
    class ThresholdDemocracySimulationAggregator
    {
        public static void GenerateAndWriteToCSV()
        {
            CsvWriter.WriteCSV(GetRecords(0.51), "treshold-democracy-051.csv");
            CsvWriter.WriteCSV(GetRecords(0.49), "treshold-democracy-049.csv");

            CsvWriter.WriteCSV(GetRecords(0.4), "treshold-democracy-04.csv");
            CsvWriter.WriteCSV(GetRecords(0.6), "treshold-democracy-06.csv");
        }

        private static IEnumerable<CsvRecord> GetRecords(double wantedChooseProbability)
        {
            foreach (var votersAmount in Enumerable.Range(1, 500).Select(i => i * 10))
            {
                var probabilityByThreshold = new Dictionary<double, ThresholdDemocracySimulation.SimulationResult>();
                foreach (var treshold in new[] { 0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 })
                {
                    var simulation = new ThresholdDemocracySimulation();
                    var result = simulation.ComputeRightVoteProbability(new ThresholdDemocracySimulation.Settings
                    {
                        WantedChooseProbability = wantedChooseProbability,
                        ThresholdQuantile = treshold,
                        VotersAmount = votersAmount
                    });

                    probabilityByThreshold.Add(treshold, result);
                    Console.WriteLine("Treshold " + treshold.ToString("0.0") +
                        " | Wanted Voters " + votersAmount +
                        " | Real Voters " + result.AverageRealVotersAmount +
                        " | " + result.RightVoteProbability.ToString("0.00"));
                }

                yield return new CsvRecord()
                {
                    VotersAmount = votersAmount,

                    // I'm asking myself: do I really want to github this?
                    // If srsly, I had no success on forcing CSVHelper to write arrays/dictionary/expandobjects properly
                    Threshold00Probability = probabilityByThreshold[0.0].RightVoteProbability,
                    Threshold10Probability = probabilityByThreshold[0.1].RightVoteProbability,
                    Threshold20Probability = probabilityByThreshold[0.2].RightVoteProbability,
                    Threshold30Probability = probabilityByThreshold[0.3].RightVoteProbability,
                    Threshold40Probability = probabilityByThreshold[0.4].RightVoteProbability,
                    Threshold50Probability = probabilityByThreshold[0.5].RightVoteProbability,
                    Threshold60Probability = probabilityByThreshold[0.6].RightVoteProbability,
                    Threshold70Probability = probabilityByThreshold[0.7].RightVoteProbability,
                    Threshold80Probability = probabilityByThreshold[0.8].RightVoteProbability,
                    Threshold90Probability = probabilityByThreshold[0.9].RightVoteProbability,
                    Threshold100Probability = probabilityByThreshold[1].RightVoteProbability,

                    Threshold00AverageVotersAmount = probabilityByThreshold[0.0].AverageRealVotersAmount,
                    Threshold10AverageVotersAmount = probabilityByThreshold[0.1].AverageRealVotersAmount,
                    Threshold20AverageVotersAmount = probabilityByThreshold[0.2].AverageRealVotersAmount,
                    Threshold30AverageVotersAmount = probabilityByThreshold[0.3].AverageRealVotersAmount,
                    Threshold40AverageVotersAmount = probabilityByThreshold[0.4].AverageRealVotersAmount,
                    Threshold50AverageVotersAmount = probabilityByThreshold[0.5].AverageRealVotersAmount,
                    Threshold60AverageVotersAmount = probabilityByThreshold[0.6].AverageRealVotersAmount,
                    Threshold70AverageVotersAmount = probabilityByThreshold[0.7].AverageRealVotersAmount,
                    Threshold80AverageVotersAmount = probabilityByThreshold[0.8].AverageRealVotersAmount,
                    Threshold90AverageVotersAmount = probabilityByThreshold[0.9].AverageRealVotersAmount,
                    Threshold100AverageVotersAmount = probabilityByThreshold[1].AverageRealVotersAmount,
                };
            }
        }

        class CsvRecord
        {
            public double VotersAmount { get; set; }

            [Format("0.00")]
            public double Threshold00Probability { get; set; }
            [Format("0.00")]
            public double Threshold10Probability { get; set; }
            [Format("0.00")]

            public double Threshold20Probability { get; set; }
            [Format("0.00")]

            public double Threshold30Probability { get; set; }
            [Format("0.00")]

            public double Threshold40Probability { get; set; }
            [Format("0.00")]

            public double Threshold50Probability { get; set; }
            [Format("0.00")]

            public double Threshold60Probability { get; set; }
            [Format("0.00")]

            public double Threshold70Probability { get; set; }
            [Format("0.00")]

            public double Threshold80Probability { get; set; }
            [Format("0.00")]

            public double Threshold90Probability { get; set; }
            [Format("0.00")]
            public double Threshold100Probability { get; set; }


            [Format("0.00")]
            public double Threshold00AverageVotersAmount { get; set; }
            [Format("0.00")]
            public double Threshold10AverageVotersAmount { get; set; }
            [Format("0.00")]

            public double Threshold20AverageVotersAmount { get; set; }
            [Format("0.00")]

            public double Threshold30AverageVotersAmount { get; set; }
            [Format("0.00")]

            public double Threshold40AverageVotersAmount { get; set; }
            [Format("0.00")]

            public double Threshold50AverageVotersAmount { get; set; }
            [Format("0.00")]

            public double Threshold60AverageVotersAmount { get; set; }
            [Format("0.00")]

            public double Threshold70AverageVotersAmount { get; set; }
            [Format("0.00")]

            public double Threshold80AverageVotersAmount { get; set; }
            [Format("0.00")]

            public double Threshold90AverageVotersAmount { get; set; }
            [Format("0.00")]
            public double Threshold100AverageVotersAmount { get; set; }
        }
    }
}
