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
            var votersAmounts = new[] { 10, 100, 500, 1000, 10000 };
            var meanIQs = new[] { 0.5, 0.51, 0.49 }.Concat(Enumerable.Range(0, 21).Select(i => i * 0.05)).Distinct();
            var forcedWrongVotersPercentages = new[] { 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0 };
            var hackModes = new[] { HackMode.ForceRandom, HackMode.ForceWrong, HackMode.ForceNoVote };

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
            var forcedWrongVotersAmount = (int)Math.Floor(votersAmount * forcedWrongVotersPercentage);

            var naiveCrowdIQ = new NaiveDemocracySimulation().ComputeRightVoteProbability(new NaiveDemocracySimulation.Settings
            {
                VotersAmount = votersAmount,
                WantedChooseProbability = meanIQ,
                ForcedWrongVotersAmount = forcedWrongVotersAmount,
                HackMode = hackMode,
            });

            var threshold20IQ = new ThresholdDemocracySimulation().ComputeRightVoteProbability(new ThresholdDemocracySimulation.Settings
            {
                VotersAmount = votersAmount,
                WantedChooseProbability = meanIQ,
                ForcedWrongVotersAmount = forcedWrongVotersAmount,
                ThresholdQuantile = 0.2,
                HackMode = hackMode,
            });

            var threshold50IQ = new ThresholdDemocracySimulation().ComputeRightVoteProbability(new ThresholdDemocracySimulation.Settings
            {
                VotersAmount = votersAmount,
                WantedChooseProbability = meanIQ,
                ForcedWrongVotersAmount = forcedWrongVotersAmount,
                ThresholdQuantile = 0.5,
                HackMode = hackMode,
            });

            var threshold80IQ = new ThresholdDemocracySimulation().ComputeRightVoteProbability(new ThresholdDemocracySimulation.Settings
            {
                VotersAmount = votersAmount,
                WantedChooseProbability = meanIQ,
                ForcedWrongVotersAmount = forcedWrongVotersAmount,
                ThresholdQuantile = 0.8,
                HackMode = hackMode,
            });

            var factorCrowdIQ = new FactorDemocracySimulation().ComputeRightVoteProbability(new FactorDemocracySimulation.Settings
            {
                VotersAmount = votersAmount,
                WantedChooseProbability = meanIQ,
                ForcedWrongVotersAmount = forcedWrongVotersAmount,
                HackMode = hackMode,
            });

            var record = new CsvRecord
            {
                HackMode = hackMode,
                VotersAmount = votersAmount,
                ForcedWrongVotersPercentage = forcedWrongVotersPercentage,
                WantedMeanIQ = meanIQ,

                NaiveCrowdIQ = naiveCrowdIQ,
                FactorCrowdIQ = factorCrowdIQ,
                Treshold20CrowdIQ = threshold20IQ.RightVoteProbability,
                Treshold50CrowdIQ = threshold50IQ.RightVoteProbability,
                Treshold80CrowdIQ = threshold80IQ.RightVoteProbability
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
            public double Treshold20CrowdIQ { get; set; }

            [Format("0.000")]
            public double Treshold50CrowdIQ { get; set; }

            [Format("0.000")]
            public double Treshold80CrowdIQ { get; set; }

            [Format("0.000")]
            public double FactorCrowdIQ { get; set; }
        }
    }
}
