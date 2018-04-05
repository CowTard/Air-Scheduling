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
        }   
    }
}