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
            CalculateArrivalTimeAndCostAsssociated();

            var cost = _chromosome.GetGenes().Sum(ge => ((Gene) ge.Value).Cost);

            return cost;
        }
        
        /// <summary>
        /// Calculates arrival time for each aircraft
        /// </summary>
        private void CalculateArrivalTimeAndCostAsssociated()
        {
            // Initialize array of times
            var arrayOfArrivals = new double[_chromosome.GetAirport().Runways.Count];
            for (var i = 0; i < arrayOfArrivals.Length; i++)
                arrayOfArrivals[i] = 0;


            var lastRunway = string.Empty;
            var timeOfFirstLanding = 0.0; // ETA for the first landing in seconds
            
            for (var i = 0; i < _chromosome.Length; i++)
            {
                var currentGene = (Gene)_chromosome.GetGene(i).Value;

                if (lastRunway == string.Empty)
                {
                    lastRunway = currentGene.GetRunway().GetIdentification();
                    _chromosome.LastLanding[lastRunway] =
                        currentGene.GetRadarAircraft().GetAircraft().GetAircraftType().ToString();
                    
                    var distanceToCover = currentGene.GetRadarAircraft().GetDistanceToAirport();
                    
                    // TODO: Should this speed be in mutations ?
                    var useSpeed = currentGene.GetRadarAircraft().GetAircraft().OptimalSpeed;

                    timeOfFirstLanding = distanceToCover / useSpeed;
                }
                else
                {
                    var runwayToLand = currentGene.GetRunway();
                    var lastAircrType = _chromosome.LastLanding[lastRunway];
                    var currentAircType = currentGene.GetRadarAircraft().GetAircraft().GetAircraftType().ToString();
                    var timeToAdd = runwayToLand.GetTimeDependency(lastRunway, (lastAircrType, currentAircType));

                    // Updating values from array of arrivals
                    for (var j = 0; j < arrayOfArrivals.Length; j++)
                    {
                        arrayOfArrivals[j] += timeToAdd;
                    }

                    var runwayIndex = _chromosome.GetAirport().ConvertRunwayInIndex(runwayToLand.GetIdentification());

                    ((Gene)_chromosome.GetGene(i).Value).SetArrivalTime(timeOfFirstLanding + arrayOfArrivals[runwayIndex]);

                    timeOfFirstLanding += timeOfFirstLanding + arrayOfArrivals[runwayIndex];
                }
            }
        }
    }
}