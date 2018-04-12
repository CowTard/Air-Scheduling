using System;
using AirScheduling.Aviation;

namespace AirScheduling.Genetics
{
    /// <summary>
    /// The most elementary object of the genetic algorithm
    /// </summary>
    public class Gene
    {
        private readonly AircraftRadar _aircraft;
        private double _estimatedLandingTime;
        private readonly Airport.Runway _runway;

        public double Cost;

        public Gene(AircraftRadar aircraft, Airport.Runway runway, double estimatedLandingTime)
        {
            _estimatedLandingTime = estimatedLandingTime;
            _aircraft = aircraft;
            _runway = runway;

            Cost = 0;
        }

        /// <summary>
        /// Returns the runway's object designated to the current gene
        /// </summary>
        /// <returns>Runway's object</returns>
        public Airport.Runway GetRunway()
        {
            return _runway;
        }

        /// <summary>
        /// Adds time seconds into TimeSpan.Zero
        /// </summary>
        /// <param name="time"></param>
        public void SetArrivalTime(double time)
        {
            _estimatedLandingTime = time;
            
            // Time it would take if aircraft would be approaching in differet speed
            var optTime = _aircraft.GetDistanceToAirport() / _aircraft.GetAircraft().OptimalSpeed;
            var slowTime = _aircraft.GetDistanceToAirport() / _aircraft.GetAircraft().MinSpeed;
            var fastTime = _aircraft.GetDistanceToAirport() / _aircraft.GetAircraft().MaxSpeed;
            
            
        }

        /// <summary>
        /// Returns estimated landing time in seconds
        /// </summary>
        /// <returns></returns>
        public double GetArrivalTime()
        {
            return _estimatedLandingTime;
        }

        /// <summary>
        /// Returns the aircraft object
        /// </summary>
        /// <returns></returns>
        public AircraftRadar GetRadarAircraft()
        {
            return _aircraft;
        }
    }
}