using System;
using System.Linq;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;

namespace AirScheduling.Genetics
{
    public class Fitness : IFitness
    {
        private Chromosome _chromosome;

        public double Evaluate(IChromosome chromosome)
        {
            _chromosome = (Chromosome) chromosome;
            CalculateArrivalTimeAndCostAsssociated();

            var cost = (int) _chromosome.GetGenes().Sum(ge => ((Gene) ge.Value).Cost);

            return cost;
        }

        /// <summary>
        /// Calculates arrival time for each aircraft
        /// </summary>
        private void CalculateArrivalTimeAndCostAsssociated()
        {
            // Initialize array of times
            var arrayOfArrivals = new TimeSpan[_chromosome.GetAirport().Runways.Count];
            for (var i = 0; i < arrayOfArrivals.Length; i++)
                arrayOfArrivals[i] = TimeSpan.Zero;


            var lastRunway = string.Empty;
            var timeOfFirstLanding = TimeSpan.Zero; // ETA for the first landing in seconds

            for (var i = 0; i < _chromosome.Length; i++)
            {
                var currentGene = (Gene) _chromosome.GetGene(i).Value;

                var useSpeed = currentGene.GetNumericalSpeedInUse();

                if (lastRunway == string.Empty)
                {
                    lastRunway = currentGene.GetRunway().GetIdentification();
                    _chromosome.LastLanding[lastRunway] =
                        currentGene.GetRadarAircraft().GetAircraft().GetAircraftType().ToString();

                    var distanceToCover = currentGene.GetRadarAircraft().GetDistanceToAirport(currentGene.GetRunway());

                    timeOfFirstLanding = TimeSpan.FromHours( distanceToCover / useSpeed);

                    arrayOfArrivals = Enumerable.Repeat(timeOfFirstLanding, arrayOfArrivals.Length).ToArray();
                    
                    ((Gene) _chromosome.GetGene(i).Value).SetArrivalTime(timeOfFirstLanding);
                }
                else
                {
                    var runwayToLand = currentGene.GetRunway();
                    var lastAircrType = _chromosome.LastLanding[lastRunway];
                    var currentAircType = currentGene.GetRadarAircraft().GetAircraft().GetAircraftType().ToString();

                    // Distances inherited
                    var distanceRequired =
                        runwayToLand.GetRequiredDistance(lastRunway, (currentAircType, lastAircrType));
                    var distanceYetToTravel = TimeSpan.FromHours(currentGene.GetRadarAircraft().GetDistanceToAirport(runwayToLand) / useSpeed).TotalSeconds;

                    // Time to take into consideration
                    
                    var runwayIndex = _chromosome.GetAirport().ConvertRunwayInIndex(runwayToLand.GetIdentification());
                    var _time = Math.Max(distanceRequired, distanceYetToTravel - arrayOfArrivals[runwayIndex].TotalSeconds);


                    for (var ti = 0; ti < arrayOfArrivals.Length; ti++)
                    {
                        arrayOfArrivals[ti] += TimeSpan.FromSeconds(_time);
                    }

                    ((Gene) _chromosome.GetGene(i).Value).SetArrivalTime(
                        arrayOfArrivals[runwayIndex]);
                }
            }
        }
    }
}