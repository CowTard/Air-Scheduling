using System.Collections.Generic;
using System.Linq;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;

namespace AirScheduling.Genetics
{
    public class Selection: ISelection
    {
        public IList<IChromosome> SelectChromosomes(int number, Generation generation)
        {
            var ordered = generation.Chromosomes.OrderBy(c => c.Fitness);
            var toRet = ordered.Take(number).ToList();
            return toRet;
        }
    }
}