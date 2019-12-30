using MathNet.Numerics.Distributions;
using System.Collections.Generic;
using System.Linq;

namespace Democracy.Simulations
{
    class NaiveDemocracySimulation
    {
        public class Settings
        {
            public int VotersAmount { get; set; }

            public double WantedChooseProbability { get; set; }
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
                    var crowdVote = new DecisionMaker(
                        votersAmount: settings.VotersAmount,
                        avgRightChooseProbability: settings.WantedChooseProbability
                    ).MakeDecision();

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
            public int VotersAmount { get; }

            private double AverageRightChooseProbability { get; }

            public Vote[] Votes { get; set; }

            private double[] RightChooseProbability { get; set; }

            public DecisionMaker(int votersAmount, double avgRightChooseProbability)
            {
                VotersAmount = votersAmount;
                AverageRightChooseProbability = avgRightChooseProbability;
            }

            public Vote MakeDecision()
            {
                InitHumanIQ();
                GetHumanDecision();

                return GetDecision();
            }

            private void InitHumanIQ()
            {
                this.RightChooseProbability = new double[VotersAmount];
                for (var voterIndex = 0; voterIndex < VotersAmount; voterIndex++)
                {
                    this.RightChooseProbability[voterIndex] = Normal.Sample(mean: AverageRightChooseProbability, stddev: 0.15).Clamp(0, 1);
                }
            }

            private void GetHumanDecision()
            {
                this.Votes = new Vote[VotersAmount];
                for (var voterIndex = 0; voterIndex < VotersAmount; voterIndex++)
                {
                    double rightChooseProbability = this.RightChooseProbability[voterIndex];
                    this.Votes[voterIndex] = GenerateVote(rightChooseProbability);
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
