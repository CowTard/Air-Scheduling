using System;
using AirScheduling.Aviation;

namespace AirScheduling.Genetics
{
    /// <summary>
    /// The most elementary object of the genetic algorithm
    /// </summary>
    public class Gene
    {
        private Aircraft _aircraft;
        private TimeSpan _estimatedLandingTime;
        private readonly Airport.Runway _runway;

        public Gene(Aircraft aircraft, Airport.Runway runway, TimeSpan estimatedLandingTime)
        {
            _estimatedLandingTime = estimatedLandingTime;
            _aircraft = aircraft;
            _runway = runway;
        }

        /// <summary>
        /// Returns the runway's object designated to the current gene
        /// </summary>
        /// <returns>Runway's object</returns>
        public Airport.Runway GetRunway()
        {
            return this._runway;
        }
        
    }
}