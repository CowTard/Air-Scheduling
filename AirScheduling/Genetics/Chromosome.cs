using System.Collections;
using System.Collections.Concurrent;
using AirScheduling.Aviation;
using GeneticSharp.Domain.Chromosomes;

namespace AirScheduling.Genetics
{
    public class Chromosome: ChromosomeBase
    {
        private Airport _airport;
        
        public Chromosome(Airport airport) : base(airport.Radar.Count)
        {
            _airport = airport;
        }
        
        public override GeneticSharp.Domain.Chromosomes.Gene GenerateGene(int geneIndex)
        {
            throw new System.NotImplementedException();
        }

        public override IChromosome CreateNew()
        {
            throw new System.NotImplementedException();
        }
    }
}