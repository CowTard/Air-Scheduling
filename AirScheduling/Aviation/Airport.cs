using System.Linq;

namespace AirScheduling.Aviation
{
    public class Airport
    {
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