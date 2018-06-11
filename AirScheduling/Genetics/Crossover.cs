using System;
using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;

namespace AirScheduling.Genetics
{
    public class Crossover : CrossoverBase
    {
        public Crossover() : base(2, 2)
        {
        }

        public Crossover(int parentsNumber, int childrenNumber) : base(parentsNumber, childrenNumber)
        {
        }

        public Crossover(int parentsNumber, int childrenNumber, int minChromosomeLength) : base(parentsNumber,
            childrenNumber, minChromosomeLength)
        {
        }

        protected override IList<IChromosome> PerformCross(IList<IChromosome> parents)
        {
            var chr = new List<IChromosome>();

            for (var i = 0; i < parents.Count; i += 2)
            {
                // fist parent
                var parent1 = (Chromosome) parents[i];

                // second parent
                var parent2 = (Chromosome) parents[i + 1];

                // Choose cut index
                var cutIndex = (new Random()).Next(0, parent1.Length);

                // First Child
                var firstChild = parent1.PrependGenes(parent2.GetSliceOfChromosome(0, cutIndex));

                var secondChild = parent2.PrependGenes(parent1.GetSliceOfChromosome(0, cutIndex));

                chr.Add(parent1);
                chr.Add(parent2);
                chr.Add(new Chromosome(parent1.GetAirport(), firstChild));
                chr.Add(new Chromosome(parent2.GetAirport(), secondChild));
            }

            return chr;
        }
    }
}