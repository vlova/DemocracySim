using CsvHelper.Configuration.Attributes;
using Democracy.Csv;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Democracy.Simulations
{
    class ResistanceSimulationAggregator
    {
        public static void GenerateAndWriteToCSV()
        {
            CsvWriter.WriteCSV(ComputeRecords(), "resistance-simulation.csv");
        }

        private static IEnumerable<CsvRecord> ComputeRecords()
        {
            var votersAmounts = new[] { 1, 2, 5, 10, 100, 500, 1000 };
            var meanIQs = new[] { 0.2, 0.3, 0.4, 0.45, 0.49, 0.5, 0.51, 0.55, 0.6, 0.7, 0.8 };
            var forcedWrongVotersPercentages = Enumerable.Range(0, 101).Select(i => i * 0.01);
            var hackModes = new[] { HackMode.ForceRandom, HackMode.ForceWrong };

            foreach (var votersAmount in votersAmounts)
            {
                foreach (var meanIQ in meanIQs)
                {
                    foreach (var hackMode in hackModes)
                    {
                        foreach (var forcedWrongVotersPercentage in forcedWrongVotersPercentages)
                        {
                            var record = ComputeRecord(votersAmount, meanIQ, hackMode, forcedWrongVotersPercentage);

                            Console.WriteLine($"" +
                                $"votersAmount={votersAmount} " +
                                $"meanIQ={meanIQ} " +
                                $"hackMode={hackMode} " +
                                $"forcedWrongVotersPercentage={forcedWrongVotersPercentage}");

                            yield return record;
                        }
                    }
                }
            }
        }

        private static CsvRecord ComputeRecord(int votersAmount, double meanIQ, HackMode hackMode, double forcedWrongVotersPercentage)
        {

            var naiveCrowdIQ = new NaiveDemocracySimulation().ComputeRightVoteProbability(new NaiveDemocracySimulation.Settings
            {
                VotersAmount = votersAmount,
                WantedChooseProbability = meanIQ,
                ForcedWrongVotersPercentage = forcedWrongVotersPercentage,
                HackMode = hackMode,
            });

            var optimisticPercentileWeightedCrowdIQ = new FactorDemocracySimulation().ComputeRightVoteProbability(new FactorDemocracySimulation.Settings
            {
                VotersAmount = votersAmount,
                WantedChooseProbability = meanIQ,
                ForcedWrongVotersPercentage = forcedWrongVotersPercentage,
                HackMode = hackMode,
                WeightModel = FactorDemocracySimulation.WeightModel.OptimisticPercentile
            });

            var pessimisticPercentileWeightedCrowdIQ = new FactorDemocracySimulation().ComputeRightVoteProbability(new FactorDemocracySimulation.Settings
            {
                VotersAmount = votersAmount,
                WantedChooseProbability = meanIQ,
                ForcedWrongVotersPercentage = forcedWrongVotersPercentage,
                HackMode = hackMode,
                WeightModel = FactorDemocracySimulation.WeightModel.PessimisticPercentile
            });

            var strictWeightedCrowdIQ = new FactorDemocracySimulation().ComputeRightVoteProbability(new FactorDemocracySimulation.Settings
            {
                VotersAmount = votersAmount,
                WantedChooseProbability = meanIQ,
                ForcedWrongVotersPercentage = forcedWrongVotersPercentage,
                HackMode = hackMode,
                WeightModel = FactorDemocracySimulation.WeightModel.Strict
            });

            var record = new CsvRecord
            {
                HackMode = hackMode,
                VotersAmount = votersAmount,
                ForcedWrongVotersPercentage = forcedWrongVotersPercentage,
                WantedMeanIQ = meanIQ,

                NaiveCrowdIQ = naiveCrowdIQ,
                OptimisticPercentileWeightedIQ = optimisticPercentileWeightedCrowdIQ,
                PessimisticPercentileWeightedIQ = pessimisticPercentileWeightedCrowdIQ,
                StrictWeightedCrowdIQ = strictWeightedCrowdIQ,
            };

            return record;
        }

        class CsvRecord
        {
            public int VotersAmount { get; set; }

            [Format("0.000")]
            public double WantedMeanIQ { get; set; }

            public HackMode HackMode { get; set; }

            [Format("0.000")]
            public double ForcedWrongVotersPercentage { get; set; }

            [Format("0.000")]
            public double NaiveCrowdIQ { get; set; }

            [Format("0.000")]
            public double OptimisticPercentileWeightedIQ { get; set; }

            [Format("0.000")]
            public double PessimisticPercentileWeightedIQ { get; set; }

            [Format("0.000")]
            public double StrictWeightedCrowdIQ { get; set; }
        }
    }
}
