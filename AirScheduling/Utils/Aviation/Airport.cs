using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AirScheduling.Utils;

namespace AirScheduling.Aviation
{
    public class Airport
    {
        private Random rnd;
        public Dictionary<string, Runway> Runways { get; }
        public ConcurrentDictionary<string, AircraftRadar> Radar { get; set; }
        public bool Ready;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="runways">Dictionary of all the runways</param>
        public Airport(Dictionary<string, Runway> runways, Dictionary<string, (string, double)> landingTimes)
        {
            Runways = runways;
            Radar = new ConcurrentDictionary<string, AircraftRadar>();
            Ready = false;

            foreach (var key in landingTimes.Keys)
            {
                Runways[key].ApproachDistance = landingTimes[key].Item2;
                Runways[key].Loc.Latitude = double.Parse(landingTimes[key].Item1.Split(':')[0], CultureInfo.InvariantCulture);
                Runways[key].Loc.Longitude = double.Parse(landingTimes[key].Item1.Split(':')[1], CultureInfo.InvariantCulture);
            }
            
            rnd = new Random();
        }

        /// <summary>
        /// Returns a random runway that is present in the airport
        /// </summary>
        /// <returns>Available runway</returns>
        public Runway GetRandomRunway()
        {
            var vL = rnd.Next(Runways.Count);

            return Runways.ElementAt(vL).Value;
        }

        /// <summary>
        /// Used to convert index into runways
        /// </summary>
        /// <param name="runway"></param>
        /// <returns></returns>
        public int ConvertRunwayInIndex(string runway)
        {
            if (!Runways.ContainsKey(runway))
                throw new ArgumentOutOfRangeException();

            return Runways.ToList().IndexOf(new KeyValuePair<string, Runway>(runway, Runways[runway]));
        }

        /// <summary>
        /// Function that retrieves the length of a landing approach
        /// </summary>
        /// <param name="runway"></param>
        /// <returns></returns>
        public double GetApproachLength(string runway)
        {
            return Runways[runway].ApproachDistance;
        }

        /// <summary>
        /// A Class that holds identification of a runway as well as its abilty to accept types of aircrafts
        /// </summary>
        public class Runway
        {
            private readonly string _identication;
            private Dictionary<string, bool> _permissions;
            public double ApproachDistance;
            public Location Loc;
            public int RnwGroup;
            
            /// <summary>
            /// Dictionary of (runway, runway) (aircraf's type, aircraft's  type) (int)
            /// </summary>
            private Dictionary<string, Dictionary<(string, string), int>> _timeDependency;

            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="identication">Indentification of the runway</param>
            /// <param name="permissions">A string separed by '-' containing whether or not an aircraft of type n can
            /// land on this runway</param>
            public Runway(string identication, string permissions, int group)
            {
                this._identication = identication;
                _permissions = new Dictionary<string, bool>
                {
                    {"Heavy", permissions.Contains("Heavy")},
                    {"Medium", permissions.Contains("Medium")},
                    {"Light", permissions.Contains("Light")}
                };
                RnwGroup = group;
                _timeDependency = new Dictionary<string, Dictionary<(string, string), int>>();
            }

            /// <summary>
            /// Function to add the dependencies of runways
            /// </summary>
            /// <param name="runway"></param>
            /// <param name="pairTypeAircraft"></param>
            /// <param name="timeDependency">Integer representing time between landings</param>
            public void AddTimeDependecy(string runway, (string, string) pairTypeAircraft, int timeDependency)
            {
                if (!_timeDependency.ContainsKey(runway))
                    _timeDependency.Add(runway, new Dictionary<(string, string), int>());

                if (_timeDependency[runway].ContainsKey(pairTypeAircraft))
                    throw new InvalidOperationException();

                _timeDependency[runway].Add(pairTypeAircraft, timeDependency);
            }

            /// <summary>
            /// Function to retrieve time dependency between a runway and two types of aircrafts
            /// </summary>
            /// <param name="runway">Runway to be used</param>
            /// <param name="pairTypeAircraft"></param>
            /// <returns>Returns thee distance in km that needs to exist between landings</returns>
            public double GetRequiredDistance(string runway, (string, string) pairTypeAircraft)
            {
                return _timeDependency[runway][pairTypeAircraft]; //* 1.875;
            }

            /// <summary>
            /// Returns the identification of the runway
            /// </summary>
            /// <returns></returns>
            public string GetIdentification()
            {
                return this._identication;
            }

            /// <summary>
            /// Checks if either an aircraft can or cannot land on this runway
            /// </summary>
            /// <param name="typeOfAircraft">Type of the aircraft</param>
            /// <returns>True if possible, false otherwise</returns>
            public bool PossibilityOfLanding(string typeOfAircraft)
            {
                return _permissions[typeOfAircraft];
            }

            /// <summary>
            /// Override of the function ToString()
            /// </summary>
            /// <returns>The identification of the runway</returns>
            public override string ToString()
            {
                return GetIdentification();
            }
        }
    }
}