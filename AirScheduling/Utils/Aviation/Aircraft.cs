using System;

namespace AirScheduling.Aviation
{
    /// <summary>
    /// The current problem only takes into consideration three different type of aircrafts
    /// </summary>
    public enum AircraftType
    {
        Light,
        Medium,
        Heavy
    }

    /// <summary>
    /// Class holding information about a specific model of aircraft
    /// </summary>
    public class Aircraft
    {
        private string _model;
        private readonly AircraftType _aircraftType;
        public double MaxSpeed { get; }
        public double OptimalSpeed { get; }
        public double MinSpeed { get; }

        /// <summary>
        /// Constructor of the class Aircraft
        /// </summary>
        /// <param name="aircraftType">Type of Aircraft <see cref="AircraftType"/></param>
        /// <param name="model">Model by which the aircraft is known</param>
        /// <param name="maxSpeed">Max speed of this model in kmh</param>
        /// <param name="optimalSpeed">The Stall speed of this model in kmh</param>
        /// <param name="minSpeed">The minimum speed of this model in kmh</param>
        public Aircraft(AircraftType aircraftType, string model, double minSpeed, double optimalSpeed, double maxSpeed)
        {
            _aircraftType = aircraftType;
            _model = model;
            MaxSpeed = maxSpeed;
            OptimalSpeed = optimalSpeed;
            MinSpeed = minSpeed;
        }

        /// <summary>
        /// Static function responsible for converting a string into an AircraftType
        /// </summary>
        /// <param name="typeOfAircraft">A string found in AircraftDatabase.csv</param>
        /// <returns>An AircraftType equivalent of the passed string</returns>
        public static AircraftType convert_string_into_AicraftType(string typeOfAircraft)
        {
            switch (typeOfAircraft)
            {
                case "Light":
                    return AircraftType.Light;
                case "Medium":
                    return AircraftType.Medium;
                default:
                    return AircraftType.Heavy;
            }
        }

        /// <summary>
        /// Gets the aircraft's type
        /// </summary>
        /// <returns></returns>
        public AircraftType GetAircraftType()
        {
            return _aircraftType;
        }
    }

    /// <summary>
    /// Class that holds information about aircrafts that appear on airport's radar
    /// </summary>
    public class AircraftRadar
    {
        private readonly string _flightId;
        private readonly double _distance;
        private readonly Aircraft _aircraft;
        private readonly double _timeOfNextFlight;
        private readonly bool _emergency;

        private readonly TimeSpan _time;

        /// <summary>
        /// Constructor of the class
        /// </summary>
        /// <param name="flightId">Flight Identification</param>
        /// <param name="distanceToAirport">Distance in meters that separates the aircraft and the airport</param>
        /// <param name="aircraft">Object of the class <see cref="Aircraft"/></param>
        /// <param name="timeOfNextFlight">Time left in minutes for the next flight of this aircraft in minutes</param>
        /// <param name="emergency">Whether aircraft is in emergengy mode or not </param>
        public AircraftRadar(string flightId, string distanceToAirport, Aircraft aircraft, double timeOfNextFlight,
            bool emergency, TimeSpan time)
        {
            _aircraft = aircraft;
            double.TryParse(distanceToAirport, out _distance);
            // Convert to km
            _distance = _distance / 1000;
            _timeOfNextFlight = timeOfNextFlight;
            _emergency = emergency;
            _flightId = flightId;
            _time = time;
        }

        /// <summary>
        /// Returns the desired time to landing
        /// </summary>
        /// <returns>Double</returns>
        public TimeSpan GetDesiredLandingTime()
        {
            return this._time;
        }

        /// <summary>
        /// Returns the aircraft
        /// </summary>
        /// <returns>Aircraft</returns>
        public Aircraft GetAircraft()
        {
            return this._aircraft;
        }

        /// <summary>
        /// Returns the distance between airport and aircraft
        /// </summary>
        /// <returns>Distance in KM to the airport</returns>
        public double GetDistanceToAirport()
        {
            return this._distance;
        }

        /// <summary>
        /// Returns the state of emergency of the present aircraft in state of integer
        /// </summary>
        public int GetEmergencyState()
        {
            return _emergency ? 1 : 0;
        }

        /// <summary>
        /// Returns the flight identification of this flight
        /// </summary>
        /// <returns></returns>
        public string GetFlightIdentification()
        {
            return _flightId;
        }

        /// <summary>
        /// Returns in seconds the time left for the next flight
        /// </summary>
        /// <returns>A double representing the time in seconds</returns>
        public double GetNextFlightTime()
        {
            return TimeSpan.FromMinutes(_timeOfNextFlight).TotalMinutes;
        }

        /// <summary>
        /// Returns a string composed by flightID and emergency status
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _flightId + " " + _emergency;
        }

        /// <summary>
        ///  Function that calculates based on chosen aircraft speed and the obligatory speed on landing approaches
        ///  the time it will take for this aircraft to be able to land
        /// </summary>
        /// <param name="distanceToCover">Distance in KM between the current position to the airport</param>
        /// <param name="landingDistance">Distance in KM between the first landing approach point till the last</param>
        /// <returns>Amount of hours it will take</returns>
        public double GetTimeToLand(double distanceToCover, double landingDistance)
        {
            return (distanceToCover / this._aircraft.OptimalSpeed) + (landingDistance / 259);
        }
    }
}