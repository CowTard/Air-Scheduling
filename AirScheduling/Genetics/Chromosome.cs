using GeneticSharp.Domain.Chromosomes;

namespace AirScheduling.Genetics
{
    public class Chromosome: ChromosomeBase
    {
        public Chromosome(int length) : base(length)
        {
        }

        public override GeneticSharp.Domain.Chromosomes.Gene GenerateGene(int geneIndex)
        {
            throw new System.NotImplementedException();
        }

        public override IChromosome CreateNew()
        {
            return new Chromosome(10);
        }
    }
}