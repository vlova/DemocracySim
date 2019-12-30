using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Democracy.Simulations
{
    class SkillDistributionAggregator
    {
        class SkillDistributionItem
        {
            public double Mean { get; set; }

            public double Stdev { get; set; }

            public double Skill { get; set; }

            public double HumanCount { get; set; }

            public double HumanPercentage { get; set; }
        }

        public static void GenerateCSV(double mean)
        {
            var probabilities = new List<double>();
            foreach (var human in Enumerable.Range(0, 1000000))
            {
                probabilities.Add(Normal.Sample(mean, stddev: 0.15).Clamp(0, 1));
            }

            probabilities.Sort();

            var skills = new List<SkillDistributionItem>();
            var skillStep = 0.01M;
            for (var skill = skillStep; skill <= 1; skill += skillStep)
            {
                var humanCount = probabilities.Where(p => p < (double)skill && p >= (double)(skill - skillStep)).Count();
                skills.Add(new SkillDistributionItem
                {
                    Mean = mean,
                    Stdev = 0.15,
                    Skill = (double)skill,
                    HumanCount = humanCount,
                    HumanPercentage = (double)humanCount / probabilities.Count
                });
                Console.WriteLine("Skill <= " + skill.ToString("0.00") + " | Humans: " + humanCount);
            }

            Csv.CsvWriter.WriteCSV(skills, $"Skill-distribution-Mean-{mean}-Stdev-0.15.csv");
        }
    }
}
