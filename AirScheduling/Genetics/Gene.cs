﻿using System;
using AirScheduling.Aviation;

namespace AirScheduling.Genetics
{
    /// <summary>
    /// The most elementary object of the genetic algorithm
    /// </summary>
    public class Gene
    {
        public AircraftRadar Aircraft { get; }
        public AircraftSpeed usedSpeed = AircraftSpeed.Optimal;
        private TimeSpan _estimatedLandingTime;
        private Airport.Runway _runway;

        public (bool, double) penaltyForHavingToWait;
        public double Cost;

        public Gene(AircraftRadar aircraft, Airport.Runway runway, TimeSpan estimatedLandingTime)
        {
            _estimatedLandingTime = estimatedLandingTime;
            Aircraft = aircraft;
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
        }

        public void SetRunway(Airport.Runway _ru)
        {
            this._runway = _ru;
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
        /// Mutates the speed to the passed one
        /// </summary>
        /// <param name="t"></param>
        public void MutateSpeed(AircraftSpeed t)
        {
            this.usedSpeed = t;
        }

        /// <summary>
        /// Convertes AircraftSpeed used in a usable double speed
        /// </summary>
        /// <returns></returns>
        public double GetNumericalSpeedInUse()
        {
            switch (usedSpeed)
            {
                    case AircraftSpeed.Min:
                        return Aircraft.GetAircraft().MinSpeed;
                    case AircraftSpeed.Optimal:
                        return Aircraft.GetAircraft().OptimalSpeed;
                    case AircraftSpeed.Max:
                        return Aircraft.GetAircraft().MaxSpeed;
                    default:
                        return Aircraft.GetAircraft().OptimalSpeed;
            }
        }

        /// <summary>
        /// Returns the aircraft object
        /// </summary>
        /// <returns></returns>
        public AircraftRadar GetRadarAircraft()
        {
            return Aircraft;
        }


        /// <summary>
        /// Responsible for calculating all the parts of the fitness function
        /// </summary>
        private void CalculateFitnessFunction()
        {
            Cost = 0;
            
            IncrementCost(CalculateDelayNecessaryFitness());
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
            var optTime = Aircraft.GetDesiredLandingTime();

            // Optimal Interval
            if (_estimatedLandingTime < optTime.Add(new TimeSpan(0, 0, 5)) &&
                _estimatedLandingTime > optTime.Subtract(new TimeSpan(0, 0, 5)))
                return 0;

            // Between fastest and optimal
            // 66.55 $/min for crew
            if (_estimatedLandingTime >= optTime.Add(new TimeSpan(0, 0, 5)))
            {
                var delayMinutes = (optTime - _estimatedLandingTime).Minutes;
                return 66.48 * delayMinutes * (Aircraft.GetEmergencyState() * 3 + 1);
            }

            //Before predicted
            // 21.25 $/min for pilots + 20 $/min per fuel spent by increasing speed
            if (_estimatedLandingTime <= optTime.Subtract(new TimeSpan(0, 0, 5)))
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
            if (Aircraft.GetNextFlightTime() - _estimatedLandingTime.TotalMinutes > 30)
                return 0;
            
            var nextFlightTime = Aircraft.GetNextFlightTime();
            var minimumInterval = TimeSpan.FromMinutes(30);

            var exceedingTimeInMinutes = nextFlightTime - _estimatedLandingTime.TotalMinutes -
                                         minimumInterval.TotalMinutes;

            return (81 + (Aircraft.GetAircraft().Passengers * 37.6) / 60) * Math.Abs(exceedingTimeInMinutes);
        }

        /// <summary>
        /// Calculates the value to be added to the cost function related to the runway availability to receive this type of aircraft
        /// </summary>
        /// <returns>0, if possible : 10000 if it's not possible for this type of aircraft to land on this runway</returns>
        private double CalculateRunwayFitness()
        {
            double penalty = 1000;
            // Check if penalty from winds
            var windTimes = _runway.RnwGroup == 1 ? AirScheduling.group1 : AirScheduling.group2;

            foreach (var t in windTimes)
            {
                var t1 = TimeSpan.FromMinutes(double.Parse(t.Split(':')[0]));
                var t2 = TimeSpan.FromMinutes(double.Parse(t.Split(':')[1]));

                if (_estimatedLandingTime >= t1 && _estimatedLandingTime < t2)
                {
                    penalty = 0;
                }

                if (_estimatedLandingTime < t1)
                    break;
            }

            return _runway.PossibilityOfLanding(Aircraft.GetAircraft().GetAircraftType().ToString())
                ? 0 + penalty
                : double.MaxValue;
        }

        /// <summary>
        /// Calculates the value to be added if there is a penalty necessary
        /// </summary>
        /// <returns></returns>
        private double CalculateDelayNecessaryFitness()
        {
            if (penaltyForHavingToWait.Item1)
            {
                // FUEL
                return penaltyForHavingToWait.Item2 * 20;
            }

            return 0;
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
            return Aircraft.GetFlightIdentification() == gene.Aircraft.GetFlightIdentification() &&
                   Cost == ((Gene) obj).Cost;
        }
    }
}