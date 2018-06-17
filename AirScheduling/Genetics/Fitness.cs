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

            return 1 / (cost + 1.0);
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

                // TODO: Should this speed be in mutations ?
                var useSpeed = currentGene.GetRadarAircraft().GetAircraft().OptimalSpeed;

                if (lastRunway == string.Empty)
                {
                    lastRunway = currentGene.GetRunway().GetIdentification();
                    _chromosome.LastLanding[lastRunway] =
                        currentGene.GetRadarAircraft().GetAircraft().GetAircraftType().ToString();

                    var distanceToCover = currentGene.GetRadarAircraft().GetDistanceToAirport();

                    // Time In Minutes
                    timeOfFirstLanding = TimeSpan.Zero; //TimeSpan.FromHours( distanceToCover / useSpeed);

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
                    var distanceYetToTravel = currentGene.GetRadarAircraft().GetDistanceToAirport();

                    // Times to travel each distances
                    //var timeTravelRequiredDistance = distanceRequired / useSpeed;
                    /*var timeTravelToAirport = currentGene.GetRadarAircraft()
                        .GetTimeToLand(distanceYetToTravel,
                            _chromosome.GetAirport().GetApproachLength(currentGene.GetRunway().GetIdentification()));*/

                    // Time to take into consideration
                    //var _time = Math.Max(timeTravelToAirport, timeTravelRequiredDistance);

                    // Updating values from array of arrivals
                    /*for (var j = 0; j < arrayOfArrivals.Length; j++)
                    {
                        arrayOfArrivals[j] += TimeSpan.FromSeconds(distanceRequired);//TimeSpan.FromHours(distanceRequired) + TimeSpan.FromMinutes(0.5);
                    }*/

                    var runwayIndex = _chromosome.GetAirport().ConvertRunwayInIndex(runwayToLand.GetIdentification());

                    for (var ti = 0; ti < arrayOfArrivals.Length; ti++)
                    {
                        arrayOfArrivals[ti] += TimeSpan.FromSeconds(distanceRequired);
                    }

                    ((Gene) _chromosome.GetGene(i).Value).SetArrivalTime(
                        timeOfFirstLanding + arrayOfArrivals[runwayIndex]);
                }
            }
        }
    }
}