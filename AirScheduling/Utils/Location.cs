using System;

namespace AirScheduling.Utils
{
    
    public struct Location
    {
        public double Latitude;
        public double Longitude;
        public const double Altitude = 0;


        private static double deg2rad(double deg)
        {
            return deg * (Math.PI / 180);
        }
        
        public double Distance(Location l)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = deg2rad(l.Latitude - this.Latitude); // deg2rad below
            var dLon = deg2rad(l.Longitude - this.Longitude);
            var a =
                    Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(deg2rad(this.Latitude)) * Math.Cos(deg2rad(l.Latitude)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
                ;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d;
        }

    }

}