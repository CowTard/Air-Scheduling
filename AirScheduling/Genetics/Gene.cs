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
        private Airport.Runway _runway;

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
            
            CalculateFitnessFunction();

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
        /// Mutates the runways assigned to this gene
        /// </summary>
        /// <param name="runway"></param>
        public void MutateRunway(Airport.Runway runway)
        {
            this._runway = runway;
        }

        /// <summary>
        /// Returns the aircraft object
        /// </summary>
        /// <returns></returns>
        public AircraftRadar GetRadarAircraft()
        {
            return _aircraft;
        }

        
        /// <summary>
        /// Responsible for calculating all the parts of the fitness function
        /// </summary>
        private void CalculateFitnessFunction()
        {
            IncrementCost(CalculateArrivalFitness());
            IncrementCost(CalculateTripArrivalFitness());
            IncrementCost(CalculateRunwayFitness());
        }
        
        /// <summary>
        /// Calculates the value to be added to the cost function related to the aircraft's arrival time
        /// </summary>
        /// <returns>Cost to be added</returns>
        private double CalculateArrivalFitness()
        {
            // Time it would take if aircraft would be approaching in differet speed
            var optTime = _aircraft.GetDistanceToAirport() / _aircraft.GetAircraft().OptimalSpeed;
            var slowTime = _aircraft.GetDistanceToAirport() / _aircraft.GetAircraft().MinSpeed;
            var fastTime = _aircraft.GetDistanceToAirport() / _aircraft.GetAircraft().MaxSpeed;
            
            
            // Optimal Interval
            if (optTime * 0.9 >= _estimatedLandingTime && _estimatedLandingTime <= optTime * 1.1)
                return 0;

            // Impossible Interval
            if (_estimatedLandingTime < fastTime)
                return double.MaxValue;
            
            // Between fastest and optimal
            if (_estimatedLandingTime >= fastTime && _estimatedLandingTime < optTime)
                return  Math.Pow((optTime - _estimatedLandingTime), _aircraft.GetEmergencyState() + 2);
            
            // Between opt and slowest
            if (_estimatedLandingTime > optTime)
                return Math.Pow(1/(1+ Math.Exp(-(_estimatedLandingTime - optTime))), _aircraft.GetEmergencyState() + 1);

            return 0;
        }

        /// <summary>
        /// Calculates the value to be added to the cost function related to the next flight time
        /// </summary>
        /// <returns>Amount of cost to be added</returns>
        private double CalculateTripArrivalFitness()
        {
            if (_estimatedLandingTime - _aircraft.GetNextFlightTime() > 30 * 60)
                return 0;
            else
            {
                var exceedingTimeInSeconds = (_aircraft.GetNextFlightTime() / 60 ) - (_estimatedLandingTime / 60) - (30 * 60);
                return 1000 * Math.Pow(exceedingTimeInSeconds, 2);
            }
        }
        
        /// <summary>
        /// Calculates the value to be added to the cost function related to the runway availability to receive this type of aircraft
        /// </summary>
        /// <returns>0, if possible : 10000 if it's not possible for this type of aircraft to land on this runway</returns>
        private double CalculateRunwayFitness()
        {
            return _runway.PossibilityOfLanding(_aircraft.GetAircraft().GetAircraftType().ToString())
                ? 0
                : double.MaxValue;
        }
        
        /// <summary>
        /// Function that should be used to increment cost by n amount
        /// </summary>
        /// <param name="amount">Amount to be increment</param>
        private void IncrementCost(double amount)
        {
            Cost += amount;
        }

        /// <summary>
        /// Returns either two genes contain the same aircraft ID
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                throw new NullReferenceException("Gene passed to compared is null.");
            }
            
            var gene = ((Gene) obj);
            return this._aircraft.GetFlightIdentification() == gene._aircraft.GetFlightIdentification();
        }
    }
}