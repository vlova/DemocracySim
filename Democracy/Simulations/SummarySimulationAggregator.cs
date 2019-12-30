using CsvHelper.Configuration.Attributes;
using Democracy.Csv;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Democracy.Simulations
{
    class SummarySimulationAggregator
    {
        public static void GenerateAndWriteToCSV()
        {
            CsvWriter.WriteCSV(GetSummaryRecords(), "democracy-summary.csv");
        }

        private static IEnumerable<CsvRecordSummary> GetSummaryRecords()
        {
            foreach (var wantedChooseProbability in Enumerable.Range(0, 21).Select(i => i * 0.05))
            {
                var naiveHumanIQ = new NaiveDemocracySimulation().ComputeRightVoteProbability(
                    new NaiveDemocracySimulation.Settings
                    {
                        VotersAmount = 1,
                        WantedChooseProbability = wantedChooseProbability
                    });


                foreach (var votersAmount in new[] { 10, 100, 1000, 10000, 100000 })
                {
                    var naiveCrowdIQ = new NaiveDemocracySimulation().ComputeRightVoteProbability(
                        new NaiveDemocracySimulation.Settings
                        {
                            VotersAmount = votersAmount,
                            WantedChooseProbability = wantedChooseProbability
                        });

                    var threshold20CrowdIQ = new ThresholdDemocracySimulation().ComputeRightVoteProbability(
                        new ThresholdDemocracySimulation.Settings
                        {
                            VotersAmount = votersAmount,
                            WantedChooseProbability = wantedChooseProbability,
                            TresholdQuantile = 0.2
                        });

                    var threshold50CrowdIQ = new ThresholdDemocracySimulation().ComputeRightVoteProbability(
                        new ThresholdDemocracySimulation.Settings
                        {
                            VotersAmount = votersAmount,
                            WantedChooseProbability = wantedChooseProbability,
                            TresholdQuantile = 0.5
                        });

                    var factorCrowdIQ = new FactorDemocracySimulation().ComputeRightVoteProbability(
                        new FactorDemocracySimulation.Settings
                        {
                            VotersAmount = votersAmount,
                            WantedChooseProbability = wantedChooseProbability
                        });

                    Console.WriteLine("MeanIQ " + wantedChooseProbability.ToString("0.000") +
                        " | Voters " + votersAmount +
                        " | Naive " + naiveCrowdIQ.ToString("0.000") +
                        " | Factor " + factorCrowdIQ.ToString("0.000"));

                    yield return new CsvRecordSummary
                    {
                        VotersAmount = votersAmount,
                        MeanIQ = naiveHumanIQ,
                        NaiveCrowdIQ = naiveCrowdIQ,
                        Threshold20IQ = threshold20CrowdIQ.RightVoteProbability,
                        Threshold50IQ = threshold50CrowdIQ.RightVoteProbability,
                        FactorCrowdIQ = factorCrowdIQ
                    };
                };
            }
        }

        class CsvRecordSummary
        {
            public double VotersAmount { get; set; }

            [Format("0.000")]
            public double MeanIQ { get; set; }

            [Format("0.000")]
            public double NaiveCrowdIQ { get; set; }

            [Format("0.000")]
            public double Threshold20IQ { get; set; }

            [Format("0.000")]
            public double Threshold50IQ { get; set; }

            [Format("0.000")]
            public double FactorCrowdIQ { get; set; }
        }
    }
}
