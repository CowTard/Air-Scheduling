using System;
using System.Collections.Generic;
using System.Linq;

namespace AirScheduling.Aviation
{
    public class Airport
    {
        private readonly Dictionary<string, Runway> _runways;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="runways">Dictionary of all the runways</param>
        public Airport(Dictionary<string, Runway> runways)
        {
            _runways = runways;
        }
        
        /// <summary>
        /// A Class that holds identification of a runway as well as its abilty to accept types of aircrafts
        /// </summary>
        public class Runway
        {
            private string _identication;
            private Dictionary<string, bool> _permissions;
            private Dictionary<(string, string), Dictionary<(string, string), int>> _timeDependency;

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
            }

            /// <summary>
            /// Function to add the dependencies of runways
            /// </summary>
            /// <param name="runways"></param>
            /// <param name="timeDependency">A dictionary where keys are (runw1, runw1) and values are dictionaries are
            /// (typeOfAircraft, typeOfAircraft) and values doubles</param>
            private void AddTimeDependecy((string, string) runways, Dictionary<(string, string), int> timeDependency)
            {
                if (!_timeDependency.ContainsKey(runways))
                    throw new InvalidOperationException();
                
                _timeDependency.Add(runways, timeDependency);
            }
            
        }   
    }
}