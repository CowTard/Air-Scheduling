using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AirScheduling.Aviation
{
    public class Airport
    {
        public Dictionary<string, Runway> Runways { get; }
        public ConcurrentDictionary<string, AircraftRadar> Radar { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="runways">Dictionary of all the runways</param>
        public Airport(Dictionary<string, Runway> runways)
        {
            Runways = runways;
            Radar = new ConcurrentDictionary<string, AircraftRadar>();;
        }

        /// <summary>
        /// Returns a random runway that is present in the airport
        /// </summary>
        /// <returns>Available runway</returns>
        public Runway GetRandomRunway()
        {
            var rand = new Random();
            return Runways.ElementAt(rand.Next(Runways.Count)).Value;
        }
        
        /// <summary>
        /// A Class that holds identification of a runway as well as its abilty to accept types of aircrafts
        /// </summary>
        public class Runway
        {
            private string _identication;
            private Dictionary<string, bool> _permissions;
            
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
            public Runway(string identication, string permissions)
            {
                this._identication = identication;
                _permissions = new Dictionary<string, bool>
                {
                    {"Heavy", permissions.Contains("Heavy")},
                    {"Medium", permissions.Contains("Medium")},
                    {"Light", permissions.Contains("Light")}
                };
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
            
        }
    }
}