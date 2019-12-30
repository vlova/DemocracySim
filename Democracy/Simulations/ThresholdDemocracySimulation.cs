using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using System.Collections.Generic;
using System.Linq;

namespace Democracy.Simulations
{
    class ThresholdDemocracySimulation
    {
        public class Settings
        {
            public int VotersAmount { get; set; }

            public double WantedChooseProbability { get; set; }

            public double TresholdQuantile { get; internal set; }
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
                    var crowdVote = new DecisionMaker(
                        votersAmount: settings.VotersAmount,
                        avgRightChooseProbability: settings.WantedChooseProbability,
                        thresholdQuantile: settings.TresholdQuantile
                    ).MakeDecision();

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
            public int VotersAmount { get; }

            private double AverageRightChooseProbability { get; }

            private double TresholdQuantile { get; }

            public Vote?[] Votes { get; set; }

            private double[] IQ { get; set; }

            private double[] EstimatedIQ { get; set; }

            private bool[] CanVote { get; set; }

            public DecisionMaker(int votersAmount, double avgRightChooseProbability, double thresholdQuantile)
            {
                VotersAmount = votersAmount;
                AverageRightChooseProbability = avgRightChooseProbability;
                TresholdQuantile = thresholdQuantile;
            }

            public (Vote vote, int realVotersAmount) MakeDecision()
            {
                InitHumanIQ();
                EstimateHumanIQ();
                DecideWhoCanVote();
                GetHumanDecision();

                var realVotersAmount = this.CanVote.Where(c => c).Count();

                return (GetDecision(), realVotersAmount);
            }

            private void InitHumanIQ()
            {
                this.IQ = new double[VotersAmount];
                for (var voterIndex = 0; voterIndex < VotersAmount; voterIndex++)
                {
                    this.IQ[voterIndex] = Normal.Sample(mean: AverageRightChooseProbability, stddev: 0.15);
                }
            }

            private void EstimateHumanIQ()
            {
                this.EstimatedIQ = new double[VotersAmount];
                for (var voterIndex = 0; voterIndex < VotersAmount; voterIndex++)
                {
                    double estimateError = Normal.Sample(mean: 0, stddev: 0.5);
                    this.EstimatedIQ[voterIndex] = MathExt.Clamp(this.IQ[voterIndex] + estimateError, 0, 1);
                }
            }

            private void DecideWhoCanVote()
            {
                this.CanVote = new bool[VotersAmount];
                var thresholdProbability = Statistics.Quantile(this.EstimatedIQ, this.TresholdQuantile);
                for (var voterIndex = 0; voterIndex < VotersAmount; voterIndex++)
                {
                    this.CanVote[voterIndex] = this.EstimatedIQ[voterIndex] >= thresholdProbability;
                }
            }

            private void GetHumanDecision()
            {
                this.Votes = new Vote?[VotersAmount];
                for (var voterIndex = 0; voterIndex < VotersAmount; voterIndex++)
                {
                    if (this.CanVote[voterIndex])
                    {
                        double rightChooseProbability = MathExt.Clamp(this.IQ[voterIndex], min: 0, max: 1);
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
                var rightAmount = this.Votes.Where(v => v == Vote.Right).Sum(v => 1);
                var wrongAmount = this.Votes.Where(v => v == Vote.Wrong).Sum(v => 1);

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
            Wrong
        }
    }
}
