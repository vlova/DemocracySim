using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Democracy.Simulations
{
    class NaiveDemocracySimulation
    {
        public class Settings
        {
            public int VotersAmount { get; set; }

            public int ForcedWrongVotersAmount { get; set; }

            public double WantedChooseProbability { get; set; }

            public HackMode HackMode { get; set; }
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

            private double[] RightChooseProbability { get; set; }

            public DecisionMaker(Settings settings)
            {
                this.Settings = settings;
                this.Votes = new Vote?[Settings.VotersAmount];
            }

            public Vote MakeDecision()
            {
                InitHumanIQ();
                HackHumans();
                GetHumanDecision();

                return GetDecision();
            }

            private void InitHumanIQ()
            {
                this.RightChooseProbability = new double[Settings.VotersAmount];
                for (var voterIndex = 0; voterIndex < Settings.VotersAmount; voterIndex++)
                {
                    this.RightChooseProbability[voterIndex] = Normal.Sample(
                        mean: Settings.WantedChooseProbability,
                        stddev: 0.15).Clamp(0, 1);
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

            private void GetHumanDecision()
            {
                for (var voterIndex = 0; voterIndex < Settings.VotersAmount; voterIndex++)
                {
                    double rightChooseProbability = this.RightChooseProbability[voterIndex];
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
            Wrong,
            NoVote
        }
    }
}
