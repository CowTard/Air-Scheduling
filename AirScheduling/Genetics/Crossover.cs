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

            // fist parent
            var parent1 = (Chromosome) parents[0];

            // second parent
            var parent2 = (Chromosome) parents[1];

            // Choose cut index
            var cutIndex = (new Random()).Next(0, parent1.Length);

            var firstChild = (new Chromosome(parent1.GetAirport(), parent1.PrependGenes(parent2.GetSliceOfChromosome(0, cutIndex))));
            var secondChild =new Chromosome(parent2.GetAirport(), parent2.PrependGenes(parent1.GetSliceOfChromosome(0, cutIndex)));

            return new List<IChromosome>() {firstChild, secondChild};
        }
    }
}