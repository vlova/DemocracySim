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
            foreach (var wantedChooseProbability in Enumerable.Range(0, 41).Select(i => i * 0.025).Reverse())
            {
                var naiveHumanIQ = new NaiveDemocracySimulation().ComputeRightVoteProbability(
                    new NaiveDemocracySimulation.Settings
                    {
                        VotersAmount = 1,
                        WantedChooseProbability = wantedChooseProbability
                    });


                foreach (var votersAmount in new[] { 50, 100 }) // new[] { 5, 10, 25, 50, 75, 100, 150, 1000, 10000 })
                {
                    var naiveCrowdIQ = new NaiveDemocracySimulation().ComputeRightVoteProbability(
                        new NaiveDemocracySimulation.Settings
                        {
                            VotersAmount = votersAmount,
                            WantedChooseProbability = wantedChooseProbability,
                            CombinationMode = NaiveDemocracySimulation.CombinationMode.Naive
                        });

                    var naiveGroupedCrowdIQ = new NaiveDemocracySimulation().ComputeRightVoteProbability(
                        new NaiveDemocracySimulation.Settings
                        {
                            VotersAmount = votersAmount,
                            WantedChooseProbability = wantedChooseProbability,
                            CombinationMode = NaiveDemocracySimulation.CombinationMode.Grouped
                        });


                    var threshold20CrowdIQ = new ThresholdDemocracySimulation().ComputeRightVoteProbability(
                        new ThresholdDemocracySimulation.Settings
                        {
                            VotersAmount = votersAmount,
                            WantedChooseProbability = wantedChooseProbability,
                            ThresholdQuantile = 0.2
                        });

                    var threshold50CrowdIQ = new ThresholdDemocracySimulation().ComputeRightVoteProbability(
                        new ThresholdDemocracySimulation.Settings
                        {
                            VotersAmount = votersAmount,
                            WantedChooseProbability = wantedChooseProbability,
                            ThresholdQuantile = 0.5
                        });

                    var optimisticPercentileWeightedCrowdIQ = new FactorDemocracySimulation().ComputeRightVoteProbability(
                        new FactorDemocracySimulation.Settings
                        {
                            VotersAmount = votersAmount,
                            WantedChooseProbability = wantedChooseProbability,
                            VoteFactorStrategy = new FactorDemocracySimulation.VoteFactorStrategyByOptimisticPercentile(),
                        });

                    var pessimisticPercentileWeightedCrowdIQ = new FactorDemocracySimulation().ComputeRightVoteProbability(
                        new FactorDemocracySimulation.Settings
                        {
                            VotersAmount = votersAmount,
                            WantedChooseProbability = wantedChooseProbability,
                            VoteFactorStrategy = new FactorDemocracySimulation.VoteFactorStrategyByPessimisticPercentile(),
                        });


                    var strict2GroupsWeightedCrowdIQ = new FactorDemocracySimulation().ComputeRightVoteProbability(
                        new FactorDemocracySimulation.Settings
                        {
                            VotersAmount = votersAmount,
                            WantedChooseProbability = wantedChooseProbability,
                            VoteFactorStrategy = new FactorDemocracySimulation.VoteFactorStrategyByStrict2Groups(),
                        });

                    Console.WriteLine("MeanIQ " + wantedChooseProbability.ToString("0.000") +
                        " | Voters " + votersAmount +
                        " | Naive " + naiveCrowdIQ.ToString("0.000") +
                        " | Factor " + optimisticPercentileWeightedCrowdIQ.ToString("0.000"));

                    yield return new CsvRecordSummary
                    {
                        VotersAmount = votersAmount,
                        MeanIQ = naiveHumanIQ,
                        NaiveCrowdIQ = naiveCrowdIQ,
                        NaiveGroupedCrowdIQ = naiveGroupedCrowdIQ,
                        Threshold20IQ = threshold20CrowdIQ.RightVoteProbability,
                        Threshold50IQ = threshold50CrowdIQ.RightVoteProbability,
                        OptimisticPercentileWeightedCrowdIQ = optimisticPercentileWeightedCrowdIQ,
                        PessimisticPercentileWeightedCrowdIQ = pessimisticPercentileWeightedCrowdIQ,
                        Strict2GroupsWeightedCrowdIQ = strict2GroupsWeightedCrowdIQ,
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
            public double NaiveGroupedCrowdIQ { get; set; }

            [Format("0.000")]
            public double Threshold20IQ { get; set; }

            [Format("0.000")]
            public double Threshold50IQ { get; set; }

            [Format("0.000")]
            public double OptimisticPercentileWeightedCrowdIQ { get; set; }

            [Format("0.000")]
            public double PessimisticPercentileWeightedCrowdIQ { get; set; }

            public double Strict2GroupsWeightedCrowdIQ { get; internal set; }
        }
    }
}
