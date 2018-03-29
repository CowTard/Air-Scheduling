using System.Collections.Generic;
using System.Linq;

namespace AirScheduling.Aviation
{
    public class Airport
    {
        public List<Runway> Runways { get; set; }


        public class Runway
        {
            private string _identication;
            private int[] _permissions;

            public Runway(string identication, string permissions)
            {
                this._identication = identication;
                this._permissions = permissions.Select(s => int.Parse(s.ToString())).ToArray();
            }
        }   
    }
}