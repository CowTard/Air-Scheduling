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
    
    public class Aircraft
    {

        private AircraftType _aircraftType;
        private double _maxSpeed, _optimalSpeed, _minSpeed;
        private DateTime _timeOfNextFlight;
        private bool _emergency;

        public Aircraft(AircraftType aircraftType, double maxSpeed, double optimalSpeed, double minSpeed, 
            DateTime timeOfNextFlight, bool emergency)
        {
            _aircraftType = aircraftType;
            _maxSpeed = maxSpeed;
            _optimalSpeed = optimalSpeed;
            _minSpeed = minSpeed;
            _timeOfNextFlight = timeOfNextFlight;
            _emergency = emergency;
        }
    }
}