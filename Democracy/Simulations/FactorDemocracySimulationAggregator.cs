using CsvHelper.Configuration.Attributes;
using Democracy.Csv;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Democracy.Simulations
{

    class FactorDemocracySimulationAggregator
    {
        public static void GenerateAndWriteToCSV()
        {
            CsvWriter.WriteCSV(GetRecords(0.51), "factor-democracy-051.csv");
            CsvWriter.WriteCSV(GetRecords(0.49), "factor-democracy-049.csv");
            CsvWriter.WriteCSV(GetRecords(0.6), "factor-democracy-060.csv");
            CsvWriter.WriteCSV(GetRecords(0.4), "factor-democracy-040.csv");
            CsvWriter.WriteCSV(GetRecords(0.8), "factor-democracy-080.csv");
            CsvWriter.WriteCSV(GetRecords(0.3), "factor-democracy-030.csv");
        }

        private static IEnumerable<CsvRecord> GetRecords(double wantedChooseProbability)
        {
            foreach (var votersAmount in Enumerable.Concat(new[] { 1 }, Enumerable.Range(1, 500).Select(i => i * 10)))
            {
                var naiveCrowdIQ = new NaiveDemocracySimulation().ComputeRightVoteProbability(new NaiveDemocracySimulation.Settings
                {
                    VotersAmount = votersAmount,
                    WantedChooseProbability = wantedChooseProbability
                });

                var factorCrowdIQ = new FactorDemocracySimulation().ComputeRightVoteProbability(new FactorDemocracySimulation.Settings
                {
                    VotersAmount = votersAmount,
                    WantedChooseProbability = wantedChooseProbability,
                    WeightModel = FactorDemocracySimulation.WeightModel.OptimisticPercentile
                });

                Console.WriteLine("Voters " + votersAmount +
                    " | Naive " + naiveCrowdIQ.ToString("0.000") +
                    " | Factor " + factorCrowdIQ.ToString("0.000"));

                yield return new CsvRecord
                {
                    VotersAmount = votersAmount,
                    NaiveCrowdIQ = naiveCrowdIQ,
                    FactorCrowdIQ = factorCrowdIQ
                };

                if (Math.Round(factorCrowdIQ, 2) >= 1 || Math.Round(factorCrowdIQ, 2) == 0)
                {
                    break;
                }
            };
        }

        class CsvRecord
        {
            public double VotersAmount { get; set; }

            [Format("0.000")]
            public double WantedMeanIQ { get; set; }

            [Format("0.000")]
            public double NaiveCrowdIQ { get; set; }

            [Format("0.000")]
            public double FactorCrowdIQ { get; set; }
        }
    }
}
