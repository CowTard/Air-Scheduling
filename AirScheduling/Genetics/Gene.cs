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
        private TimeSpan _estimatedLandingTime;
        private Airport.Runway _runway;

        public double Cost;

        public Gene(AircraftRadar aircraft, Airport.Runway runway, TimeSpan estimatedLandingTime)
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
        public void SetArrivalTime(TimeSpan time)
        {
            _estimatedLandingTime = time;

            CalculateFitnessFunction();

            return;
        }

        /// <summary>
        /// Returns estimated landing time in seconds
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetArrivalTime()
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

            Cost = 1 / (Cost + 1);
        }

        /// <summary>
        /// Calculates the value to be added to the cost function related to the aircraft's arrival time
        /// </summary>
        /// <returns>Cost to be added</returns>
        private double CalculateArrivalFitness()
        {

            var optTime = _aircraft.GetDesiredLandingTime();

            // Optimal Interval
            if (optTime.Ticks * 0.8 >= _estimatedLandingTime.Ticks &&
                _estimatedLandingTime.Ticks <= optTime.Ticks * 1.2)
                return 0;

            // Between fastest and optimal
            // 66.55 $/min for crew
            if (_estimatedLandingTime.Ticks >= optTime.Ticks * 1.2)
            {
                var delayMinutes = (optTime - _estimatedLandingTime).Minutes;
                return 66.55 * delayMinutes * (_aircraft.GetEmergencyState() + 1);
            }
            
            //Before predicted
            // 21.25 $/min for pilots + 20 $/min per fuel spent by increasing speed
            if (_estimatedLandingTime.Ticks < optTime.Ticks * 0.8)
            {
                var delayMinutes = (optTime - _estimatedLandingTime).Minutes;
                return (21.24 + 20.0) * delayMinutes;
            }

            return double.MaxValue;
        }

        /// <summary>
        /// Calculates the value to be added to the cost function related to the next flight time
        /// </summary>
        /// <returns>Amount of cost to be added</returns>
        private double CalculateTripArrivalFitness()
        {
            if (_aircraft.GetNextFlightTime() - _estimatedLandingTime.TotalMinutes > 30)
                return 0;
            else
            {

                var nextFlightTime = _aircraft.GetNextFlightTime();
                var minimumInterval = TimeSpan.FromMinutes(30);
                
                var exceedingTimeInMinutes = nextFlightTime - _estimatedLandingTime.TotalMinutes -
                                             minimumInterval.TotalMinutes;

                return (62.55 + 37.6 / 60) * Math.Abs(exceedingTimeInMinutes);
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