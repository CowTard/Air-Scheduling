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
            return this._runway;
        }

        /// <summary>
        /// Adds time seconds into TimeSpan.Zero
        /// </summary>
        /// <param name="time"></param>
        public void SetArrivalTime(double time)
        {
            _estimatedLandingTime = time;
            
            // Time it would take if aircraft would be approaching in optimal speed
            var optTime = _aircraft.GetDistanceToAirport() / _aircraft.GetAircraft().OptimalSpeed;

            if ( (0.9 * _estimatedLandingTime < optTime) && (1.1 * _estimatedLandingTime >= optTime) )
            {
                Cost = 0;
                return;
            }
            
            // Time it would take if aircraft would be as fast as possible
            var fasTime = _aircraft.GetDistanceToAirport() / _aircraft.GetAircraft().MaxSpeed;
            
            if (_estimatedLandingTime < fasTime)
            {
                Cost = double.MaxValue;
                return;
            }
            else
            {
                Cost = Math.Pow((_estimatedLandingTime - fasTime), _aircraft.GetEmergencyState() + 2);
                return;
            }
            
            // Since fuel it is not being used
            
            Cost = Math.Pow((_estimatedLandingTime - fasTime), _aircraft.GetEmergencyState() + 1);
            return;
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
        public AircraftRadar GetAircraft()
        {
            return _aircraft;
        }
    }
}