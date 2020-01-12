using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Democracy.Simulations
{
    class FactorDemocracySimulation
    {
        public class Settings
        {
            public int VotersAmount { get; set; }

            public double ForcedWrongVotersPercentage { get; set; }

            public double WantedChooseProbability { get; set; }

            public HackMode HackMode { get; set; }

            public WeightModel WeightModel { get; set; }

            public double[] IQ { get; set; }

            public double[] EstimatedIQ { get; set; }
        }

        public enum WeightModel
        {
            OptimisticPercentile,
            PessimisticPercentile,
            Strict2Groups
        }

        public double ComputeRightVoteProbability(Settings settings)
        {
            var result = new Benchmarks.BenchmarkRunner().Run(new Benchmarks.BenchmarkOptions<StepResult>
            {
                BatchSize = 16,
                Seed = new StepResult(),
                Combine = (a, b) => new StepResult
                {
                    AllDecisions = a.AllDecisions + b.AllDecisions,
                    RightDecisions = a.RightDecisions + b.RightDecisions
                },
                QuantifiedValues = new List<Benchmarks.QuantifiedValueOptions<StepResult>> {
                    new Benchmarks.QuantifiedValueOptions<StepResult>{
                        ConfidenceLevel = Benchmarks.ConfidenceLevel.L99,
                        DesiredAbsoluteError = 0.0025,
                        DesiredRelativeError = null,
                        GetQuantifiedValue = r => (double)r.RightDecisions / r.AllDecisions
                    }
                },
                RunOnce = () =>
                {
                    var crowdVote = new DecisionMaker(settings).MakeDecision();

                    return new StepResult
                    {
                        RightDecisions = crowdVote == Vote.Right ? 1 : 0,
                        AllDecisions = 1
                    };
                }
            });

            return (double)result.RightDecisions / result.AllDecisions;
        }

        class StepResult
        {
            public int RightDecisions { get; set; }

            public int AllDecisions { get; set; }
        }

        class DecisionMaker
        {
            public Settings Settings { get; set; }

            public Vote?[] Votes { get; set; }

            private double[] IQ { get; set; }

            private double[] EstimatedIQ { get; set; }

            private double[] VoteFactor { get; set; }

            public DecisionMaker(Settings settings)
            {
                Settings = settings;
                if (settings.ForcedWrongVotersPercentage > 1)
                {
                    throw new ArgumentOutOfRangeException("Constraint failed: settings.ForcedWrongVotersPercentage <= 1");
                }

                this.Votes = new Vote?[Settings.VotersAmount];
            }

            public Vote MakeDecision()
            {
                this.IQ = this.Settings.IQ ?? GenerateHumanIQ(this.Settings);
                HackHumans();
                this.EstimatedIQ = this.Settings.EstimatedIQ ?? EstimateHumanIQ(this.Settings, this.IQ);
                ComputeVoteFactor();
                GetHumanDecision();

                return GetDecision();
            }

            public static double[] GenerateHumanIQ(Settings settings)
            {
                var iq = new double[settings.VotersAmount];
                for (var voterIndex = 0; voterIndex < settings.VotersAmount; voterIndex++)
                {
                    iq[voterIndex] = Normal.Sample(mean: settings.WantedChooseProbability, stddev: 0.15);
                }

                return iq;
            }

            private void HackHumans()
            {
                var hackAmount = Settings.VotersAmount.GetPercentageWithRandomRounding(Settings.ForcedWrongVotersPercentage);
                for (var voterIndex = 0; voterIndex < hackAmount; voterIndex++)
                {
                    this.Votes[voterIndex] = GetVote();
                }
            }

            private Vote GetVote()
            {
                if (this.Settings.HackMode == HackMode.ForceWrong)
                {
                    return Vote.Wrong;
                }


                if (this.Settings.HackMode == HackMode.ForceRandom)
                {
                    return GenerateVote(0.5);
                }

                throw new ArgumentOutOfRangeException("this.Settings.HackMode is unknown");
            }

            public static double[] EstimateHumanIQ(Settings settings, double[] IQ)
            {
                var estimatedIQ = new double[settings.VotersAmount];
                for (var voterIndex = 0; voterIndex < settings.VotersAmount; voterIndex++)
                {
                    double estimateError = Normal.Sample(mean: 0, stddev: 0.5);
                    estimatedIQ[voterIndex] = MathExt.Clamp(IQ[voterIndex] + estimateError, 0, 1);
                }

                return estimatedIQ;
            }

            private void ComputeVoteFactor()
            {
                this.VoteFactor = new double[Settings.VotersAmount];

                if (this.Settings.WeightModel == WeightModel.OptimisticPercentile)
                {
                    ComputeVoteFactorByOptimisticPercentile();
                }
                else if (this.Settings.WeightModel == WeightModel.PessimisticPercentile)
                {
                    ComputeVoteFactorByPessimisticPercentile();
                }
                else if (this.Settings.WeightModel == WeightModel.Strict2Groups)
                {
                    ComputeVoteFactorByStrict2Groups();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            private void ComputeVoteFactorByOptimisticPercentile()
            {
                var quantile25 = Statistics.Quantile(this.EstimatedIQ, 0.25);
                var quantile50 = Statistics.Quantile(this.EstimatedIQ, 0.50);
                var quantile75 = Statistics.Quantile(this.EstimatedIQ, 0.75);

                for (var voterIndex = 0; voterIndex < Settings.VotersAmount; voterIndex++)
                {
                    var estimatedIq = this.EstimatedIQ[voterIndex];

                    if (estimatedIq < quantile25)
                    {
                        this.VoteFactor[voterIndex] = -1;
                    }
                    else if (estimatedIq < quantile50)
                    {
                        this.VoteFactor[voterIndex] = -0.5;
                    }
                    else if (estimatedIq < quantile75)
                    {
                        this.VoteFactor[voterIndex] = +0.5;
                    }
                    else
                    {
                        this.VoteFactor[voterIndex] = +1;
                    }
                }
            }

            private void ComputeVoteFactorByPessimisticPercentile()
            {
                var quantile25 = Statistics.Quantile(this.EstimatedIQ, 0.25);
                var quantile50 = Statistics.Quantile(this.EstimatedIQ, 0.50);
                var quantile75 = Statistics.Quantile(this.EstimatedIQ, 0.75);

                for (var voterIndex = 0; voterIndex < Settings.VotersAmount; voterIndex++)
                {
                    var estimatedIq = this.EstimatedIQ[voterIndex];

                    if (estimatedIq <= quantile25)
                    {
                        this.VoteFactor[voterIndex] = -1;
                    }
                    else if (estimatedIq <= quantile50)
                    {
                        this.VoteFactor[voterIndex] = -0.5;
                    }
                    else if (estimatedIq <= quantile75)
                    {
                        this.VoteFactor[voterIndex] = +0.5;
                    }
                    else
                    {
                        this.VoteFactor[voterIndex] = +1;
                    }
                }
            }

            private void ComputeVoteFactorByStrict2Groups()
            {
                var voterIndexesSortedByIQ = this.EstimatedIQ.Select((iq, voterIndex) => (iq, voterIndex))
                                        .OrderBy(p => p.iq).ThenBy(p => p.voterIndex)
                                        .Select(p => p.voterIndex)
                                        .ToArray();

                var lengthAdopted = (voterIndexesSortedByIQ.Length / 2) * 2; // Floor happens here
                var differenceCausedByFloor = voterIndexesSortedByIQ.Length - lengthAdopted;
                for (var i = 0; i < voterIndexesSortedByIQ.Length; i++)
                {
                    var voterIndex = voterIndexesSortedByIQ[i];
                    var weight = 0.0;

                    if (i < lengthAdopted * 0.5)
                    {
                        weight = -1;
                    }
                    else if (i < lengthAdopted * 0.5 + differenceCausedByFloor)
                    {
                        weight = 0;
                    }
                    else
                    {
                        weight = +1;
                    }

                    this.VoteFactor[voterIndex] = weight;
                }
            }

            private void GetHumanDecision()
            {
                for (var voterIndex = 0; voterIndex < Settings.VotersAmount; voterIndex++)
                {
                    double rightChooseProbability = MathExt.Clamp(this.IQ[voterIndex], min: 0, max: 1);
                    if (this.Votes[voterIndex] == null)
                    {
                        this.Votes[voterIndex] = GenerateVote(rightChooseProbability);
                    }
                }
            }

            private Vote GenerateVote(double rightChooseProbability)
            {
                return ContinuousUniform.Sample(0, 1) > rightChooseProbability
                    ? Vote.Wrong
                    : Vote.Right;
            }

            private Vote GetDecision()
            {
                var rightAmount = this.Votes
                    .Zip(this.VoteFactor, (vote, factor) => (vote, factor))
                    .Where(v => v.vote == Vote.Right)
                    .Sum(v => v.factor);

                var wrongAmount = this.Votes
                    .Zip(this.VoteFactor, (vote, factor) => (vote, factor))
                    .Where(v => v.vote == Vote.Wrong)
                    .Sum(v => v.factor);

                if (rightAmount == wrongAmount)
                {
                    return GenerateVote(0.5);
                }

                return rightAmount > wrongAmount ? Vote.Right : Vote.Wrong;
            }
        }

        enum Vote
        {
            Right,
            Wrong,
            NoVote
        }
    }
}
