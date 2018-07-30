using System;
using System.Linq;
using AirScheduling.Aviation;
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
        
        public double Evaluate(IChromosome chromosome, bool fifo)
        {
            _chromosome = (Chromosome) chromosome;
            CalculateFIFOArrivalTimeAndCostAssociated();

            var cost = (int) _chromosome.GetGenes().Sum(ge => ((Gene) ge.Value).Cost);

            return cost;
        }


        private Airport.Runway RetrieveNextFifoRunway(string lastRunway, TimeSpan ll)
        {

            if (ll >= TimeSpan.FromMinutes(15) && ll < TimeSpan.FromMinutes(40) || ll >= TimeSpan.FromMinutes(60) &&
                                                                                ll < TimeSpan.FromMinutes(135)
                                                                                || ll >= TimeSpan.FromMinutes(155) &&
                                                                                ll < TimeSpan.FromMinutes(400))
            {
                if (lastRunway == "10L")
                    return this._chromosome.GetAirport().Runways["28L"];
                else if (lastRunway == "10R")
                    return this._chromosome.GetAirport().Runways["28R"];
                else if (lastRunway == "28R")
                    return this._chromosome.GetAirport().Runways["28L"];
                else
                    return this._chromosome.GetAirport().Runways["28R"];
            }
            else if (ll >= TimeSpan.FromMinutes(0) &&
                ll < TimeSpan.FromMinutes(15) || ll >= TimeSpan.FromMinutes(40) &&
                                              ll < TimeSpan.FromMinutes(60)
                                              || ll >= TimeSpan.FromMinutes(135) &&
                                              ll < TimeSpan.FromMinutes(155))
            {
                if (lastRunway == "10L")
                    return this._chromosome.GetAirport().Runways["10R"];
                else if (lastRunway == "10R")
                    return this._chromosome.GetAirport().Runways["10L"];
                else if (lastRunway == "28R")
                    return this._chromosome.GetAirport().Runways["10R"];
                else
                    return this._chromosome.GetAirport().Runways["10L"];
            }
            else
            {
                return null;
            }
        }

        public void CalculateFIFOArrivalTimeAndCostAssociated()
        {
            var arrayOfArrivals = new TimeSpan[_chromosome.GetAirport().Runways.Count];
            for (var i = 0; i < arrayOfArrivals.Length; i++)
                arrayOfArrivals[i] = TimeSpan.Zero;
            
            var lastRunway = string.Empty;
            var timeOfFirstLanding = TimeSpan.Zero;
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

                    timeOfFirstLanding = TimeSpan.FromHours(distanceToCover / useSpeed);

                    arrayOfArrivals = Enumerable.Repeat(timeOfFirstLanding, arrayOfArrivals.Length).ToArray();

                    ((Gene) _chromosome.GetGene(i).Value).penaltyForHavingToWait = (false, 0);
                    ((Gene) _chromosome.GetGene(i).Value).SetArrivalTime(timeOfFirstLanding);
                }
                else
                {
                    var runwayToLand = RetrieveNextFifoRunway(lastRunway,((Gene) _chromosome.GetGene(i).Value).GetArrivalTime() );
                    
                    var lastAircrType = _chromosome.LastLanding[lastRunway];
                    var currentAircType = currentGene.GetRadarAircraft().GetAircraft().GetAircraftType().ToString();

                    // Distances inherited
                    var distanceRequired =
                        runwayToLand.GetRequiredDistance(lastRunway, (currentAircType, lastAircrType));
                    var distanceYetToTravel = TimeSpan
                        .FromHours(currentGene.GetRadarAircraft().GetDistanceToAirport(runwayToLand) / useSpeed)
                        .TotalSeconds;

                    // Time to take into consideration

                    var runwayIndex = _chromosome.GetAirport().ConvertRunwayInIndex(runwayToLand.GetIdentification());
                    var _time = Math.Max(distanceRequired,
                        distanceYetToTravel - arrayOfArrivals[runwayIndex].TotalSeconds);

                    if (_time == distanceRequired)
                    {
                        currentGene.penaltyForHavingToWait = (true,
                            distanceRequired - (distanceYetToTravel - arrayOfArrivals[runwayIndex].TotalSeconds));
                    }
                    else
                    {
                        currentGene.penaltyForHavingToWait = (false, 0);
                    }

                    for (var ti = 0; ti < arrayOfArrivals.Length; ti++)
                    {
                        arrayOfArrivals[ti] += TimeSpan.FromSeconds(_time);
                    }

                    ((Gene) _chromosome.GetGene(i).Value).SetArrivalTime(
                        arrayOfArrivals[runwayIndex]);
                }
            }
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

                    timeOfFirstLanding = TimeSpan.FromHours(distanceToCover / useSpeed);

                    arrayOfArrivals = Enumerable.Repeat(timeOfFirstLanding, arrayOfArrivals.Length).ToArray();

                    ((Gene) _chromosome.GetGene(i).Value).penaltyForHavingToWait = (false, 0);
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
                    var distanceYetToTravel = TimeSpan
                        .FromHours(currentGene.GetRadarAircraft().GetDistanceToAirport(runwayToLand) / useSpeed)
                        .TotalSeconds;

                    // Time to take into consideration

                    var runwayIndex = _chromosome.GetAirport().ConvertRunwayInIndex(runwayToLand.GetIdentification());
                    var _time = Math.Max(distanceRequired,
                        distanceYetToTravel - arrayOfArrivals[runwayIndex].TotalSeconds);

                    if (_time == distanceRequired)
                    {
                        currentGene.penaltyForHavingToWait = (true,
                            distanceRequired - (distanceYetToTravel - arrayOfArrivals[runwayIndex].TotalSeconds));
                    }
                    else
                    {
                        currentGene.penaltyForHavingToWait = (false, 0);
                    }

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