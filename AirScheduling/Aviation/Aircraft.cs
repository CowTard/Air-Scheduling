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
        private AircraftType _aircraftType;
        private double _maxSpeed, _optimalSpeed, _minSpeed;
        
        /// <summary>
        /// Constructor of the class Aircraft
        /// </summary>
        /// <param name="aircraftType">Type of Aircraft <see cref="AircraftType"/></param>
        /// <param name="model">Model by which the aircraft is known</param>
        /// <param name="maxSpeed">Max speed of this model in Knots</param>
        /// <param name="optimalSpeed">The Stall speed of this model in knots</param>
        /// <param name="minSpeed">The minimum speed of this model in knots</param>
        public Aircraft(AircraftType aircraftType, string model, double maxSpeed, double optimalSpeed, double minSpeed)
        {
            _aircraftType = aircraftType;
            _model = model;
            _maxSpeed = maxSpeed;
            _optimalSpeed = optimalSpeed;
            _minSpeed = minSpeed;
        }
        
        /// <summary>
        /// Static function responsible for converting a string into an AircraftType
        /// </summary>
        /// <param name="typeOfAircraft">A string found in AircraftDatabase.csv</param>
        /// <returns>An AircraftType equivalent of the passed string</returns>
        public static AircraftType convert_string_into_AicraftType(string typeOfAircraft)
        {

            if (typeOfAircraft == "Light")
                return AircraftType.Light;
            else if (typeOfAircraft == "Medium")
                return AircraftType.Medium;
            else
                return AircraftType.Heavy;

        }
    }
    
    /// <summary>
    /// Class that holds information about aircrafts that appear on airport's radar
    /// </summary>
    public class AircraftRadar
    {
        private string _flightId;
        private Aircraft _aircraft;
        private double _timeOfNextFlight;
        private bool _emergency;

        /// <summary>
        /// Constructor of the class
        /// </summary>
        /// <param name="flightId">Flight Identification</param>
        /// <param name="aircraft">Object of the class <see cref="Aircraft"/></param>
        /// <param name="timeOfNextFlight">Time left in minutes for the next flight of this aircraft</param>
        /// <param name="emergency">Whether aircraft is in emergengy mode or not </param>
        public AircraftRadar(string flightId, Aircraft aircraft, double timeOfNextFlight, bool emergency)
        {
            _aircraft = aircraft;
            _timeOfNextFlight = timeOfNextFlight;
            _emergency = emergency;
            _flightId = flightId;
        }
    }
}