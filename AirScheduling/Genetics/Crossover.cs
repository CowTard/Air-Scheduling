using System;
using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;

namespace AirScheduling.Genetics
{
    public class Crossover : ICrossover
    {
        public bool IsOrdered { get; }
        public IList<IChromosome> Cross(IList<IChromosome> parents)
        {
            
            var children = new List<Chromosome>();
            
            // Choose cut index
            var cutIndex = (new Random()).Next(0, parents.Count);

            for (var i = 0; i < parents.Count; i += 2)
            {
                // fist parent
                var parent1 = (Chromosome)parents[i];
                
                // second parent
                var parent2 = (Chromosome)parents[i+1];
                
                
                
            }
            
        }

        public int ParentsNumber { get; }
        public int ChildrenNumber { get; }
        public int MinChromosomeLength { get; }
    }
}