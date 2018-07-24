using System.Collections.Generic;
using System.Linq;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Reinsertions;

namespace AirScheduling.Genetics
{
    public class CustomReinsertion: ReinsertionBase
    {
        public CustomReinsertion() : base(true, true)
        {
        }

        protected override IList<IChromosome> PerformSelectChromosomes(IPopulation population, IList<IChromosome> offspring, IList<IChromosome> parents)
        {
            var diff = population.MinSize - offspring.Count;

            if (diff > 0)
            {
                var bestParents = parents.OrderBy(p => p.Fitness).Take(diff);

                foreach (var p in bestParents)
                {
                    offspring.Add(p);
                }
            }

            return offspring;
        }
    }
}