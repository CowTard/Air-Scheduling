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

            return ((Gene)_chromosome.GetGene(_chromosome.Length-1).Value).GetArrivalTime();
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


            var lastRunway = string.Empty;
            for (var i = 0; i < _chromosome.Length; i++)
            {
                var currentGene = _chromosome.GetGene(i);

                if (lastRunway == string.Empty)
                {
                    lastRunway = ((Gene) currentGene.Value).GetRunway().GetIdentification();
                }
                else
                {
                    var runwayToLand = ((Gene) currentGene.Value).GetRunway();
                    var timeToAdd = runwayToLand.GetTimeDependency(lastRunway, ("Heavy", "Heavy"));

                    // Updating values from array of arrivals
                    for (var j = 0; j < arrayOfArrivals.Length; j++)
                    {
                        arrayOfArrivals[j] += timeToAdd;
                    }

                    var runwayIndex = _chromosome.GetAirport().ConvertRunwayInIndex(runwayToLand.GetIdentification());
                    ((Gene)_chromosome.GetGene(i).Value).SetArrivalTime(arrayOfArrivals[runwayIndex]);
                }
            }
            
            
        }
    }
}