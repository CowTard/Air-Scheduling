using System;
using System.Linq;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;

namespace AirScheduling.Genetics
{
    public class Fitness: IFitness
    {
        private Chromosome _chromosome;
        
        public double Evaluate(IChromosome chromosome)
        {
            _chromosome = (Chromosome)chromosome;
            CalculateArrivalTime();

            return 0;
        }
        
        /// <summary>
        /// Calculates arrival time for each aircraft
        /// </summary>
        private void CalculateArrivalTime()
        {
            // Initialize array of times
            var arrayOfArrivals = new double[_chromosome.GetAirport().Runways.Count];
            for (var i = 0; i < arrayOfArrivals.Length; i++)
                arrayOfArrivals[i] = 0;
            
            
            for (var i = 0; i < _chromosome.Length; i++)
            {
                var currentGene = _chromosome.GetGene(i);
                
                var runwayToLand = ((Gene) currentGene.Value).GetRunway();

                // Updating values from array of arrivals
                for (var j = 0; j < arrayOfArrivals.Length; j++)
                {
                    var runwayToCompare = _chromosome.GetAirport().ConvertIndexInRunwayIdentification(j);
                    arrayOfArrivals[j] += runwayToLand.GetTimeDependency(runwayToCompare, ("Heavy", "Heavy"));
                }
            }
        }
    }
}