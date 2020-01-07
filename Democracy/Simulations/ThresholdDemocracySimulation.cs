using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Democracy.Simulations
{
    class ThresholdDemocracySimulation
    {
        public class Settings
        {
            public int VotersAmount { get; set; }

            public int ForcedWrongVotersAmount { get; set; }

            public double WantedChooseProbability { get; set; }

            public double ThresholdQuantile { get; set; }

            public HackMode HackMode { get; set; }
        }

        public class SimulationResult
        {
            public double RightVoteProbability { get; set; }

            public double AverageRealVotersAmount { get; set; }
        }

        public SimulationResult ComputeRightVoteProbability(Settings settings)
        {
            var result = new Benchmarks.BenchmarkRunner().Run(new Benchmarks.BenchmarkOptions<StepResult>
            {
                BatchSize = 16,
                Seed = new StepResult(),
                Combine = (a, b) => new StepResult
                {
                    AllDecisions = a.AllDecisions + b.AllDecisions,
                    RightDecisions = a.RightDecisions + b.RightDecisions,
                    RealVotersAmount = a.RealVotersAmount + b.RealVotersAmount
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
                        RightDecisions = crowdVote.vote == Vote.Right ? 1 : 0,
                        RealVotersAmount = crowdVote.realVotersAmount,
                        AllDecisions = 1
                    };
                }
            });

            return new SimulationResult
            {
                RightVoteProbability = (double)result.RightDecisions / result.AllDecisions,
                AverageRealVotersAmount = (double)result.RealVotersAmount / result.AllDecisions
            };
        }

        class StepResult
        {
            public int RightDecisions { get; set; }

            public int AllDecisions { get; set; }

            public int RealVotersAmount { get; set; }
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
                this.Settings = settings;
                this.Votes = new Vote?[Settings.VotersAmount];
            }

            public (Vote vote, int realVotersAmount) MakeDecision()
            {
                InitHumanIQ();
                HackHumans();
                EstimateHumanIQ();
                DecideWhoCanVote();
                GetHumanDecision();

                var realVotersAmount = this.VoteFactor.Where(c => c > 0).Count();

                return (GetDecision(), realVotersAmount);
            }

            private void InitHumanIQ()
            {
                this.IQ = new double[Settings.VotersAmount];
                for (var voterIndex = 0; voterIndex < Settings.VotersAmount; voterIndex++)
                {
                    this.IQ[voterIndex] = Normal.Sample(mean: Settings.WantedChooseProbability, stddev: 0.15);
                }
            }

            private void HackHumans()
            {
                for (var voterIndex = 0; voterIndex < Settings.ForcedWrongVotersAmount; voterIndex++)
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

                if (this.Settings.HackMode == HackMode.ForceNoVote)
                {
                    return Vote.NoVote;
                }

                throw new ArgumentOutOfRangeException("this.Settings.HackMode is unknown");
            }

            private void EstimateHumanIQ()
            {
                this.EstimatedIQ = new double[Settings.VotersAmount];
                for (var voterIndex = 0; voterIndex < Settings.VotersAmount; voterIndex++)
                {
                    double estimateError = Normal.Sample(mean: 0, stddev: 0.5);
                    this.EstimatedIQ[voterIndex] = MathExt.Clamp(this.IQ[voterIndex] + estimateError, 0, 1);
                }
            }

            private void DecideWhoCanVote()
            {
                this.VoteFactor = new double[Settings.VotersAmount];
                var thresholdProbability = Statistics.Quantile(this.EstimatedIQ, Settings.ThresholdQuantile);
                for (var voterIndex = 0; voterIndex < Settings.VotersAmount; voterIndex++)
                {
                    this.VoteFactor[voterIndex] = this.EstimatedIQ[voterIndex] >= thresholdProbability ? 1 : 0;
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
