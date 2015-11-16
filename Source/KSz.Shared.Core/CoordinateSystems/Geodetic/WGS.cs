using System;
using System.Collections.Generic;
using System.Text;

namespace CoordinateSystems
{


    public class Wgs84CoordinateSystem : GeodeticCoordinateSystem
    {
        // Pomocnicze odwzorowanie dodające możliwość dodania odwzorowania WGS84 
        // do listy układów współrzędnych. To pomocnicze odwzorowanie nic nie oblicza
        // bo projektowane współrzędne to właśnie współrzędne WGS ;)

        public override string ToString()
        {
            return "WGS-84/GPS";
        }


        public override BlhPoint ToWgs84(PointBase p)
        {
            var res = new BlhPoint();
            res.Assign(p);
            return res;
        }

        public override PointBase FromWgs84(BlhPoint p)
        {
            var res = new BlhPoint();
            res.Assign(p);
            return res;
        }
    }

    public class Wgs72CoordinateSystem : GeodeticCoordinateSystem
    {
        public override BlhPoint ToWgs84(PointBase p)
        {
            var w72 = (BlhPoint)p;
            return Wgs72ToWgs84(w72);
        }

        public override PointBase FromWgs84(BlhPoint p)
        {
            return Wgs72ToWgs84(p);
        }

        public override string ToString()
        {
            return "WGS-72";

        }

        #region *   Wgs72 to Wgs84 transformations   *


        // Formulas from:
        // DEPARTMENT OF DEFENSE WORLD GEODETIC SYSTEM 1984
        // Its Definition and Relationships with Local Geodetic Systems

        private const double Δf = 0.3121057E-7;
        private const double a = 6378135.0;
        private const double Δa = 2.0;
        private const double Δr = 1.4;
        private static readonly Angle Δλ = new Angle(0, 0, 0.554);
        //Δφλ

        private static BlhPoint GetDeltas(double ɸ)
        {
            double Cosɸ = Math.Cos(ɸ);
            double Sinɸ = Math.Sin(ɸ);
            double Sin2ɸ = Math.Sin(2 * ɸ);

            BlhPoint deltas = new BlhPoint();
            deltas.ɸ = (4.5 * Cosɸ) / (a) + (Δf * Sin2ɸ);
            deltas.λ = Δλ;
            deltas.H = 4.5 * Sinɸ + a * Δf * Sinɸ * Sinɸ - Δa + Δr;

            return deltas;
        }

        public static BlhPoint Wgs72ToWgs84(BlhPoint pWgs72)
        {
            BlhPoint delta = GetDeltas(pWgs72.ɸ);

            BlhPoint res = new BlhPoint();
            res.Assign(pWgs72);

            res.Id = pWgs72.Id;
            res.B = pWgs72.B + delta.B;
            res.L = pWgs72.L + delta.L;
            res.H = pWgs72.H + delta.H;
            return res;
        }

        public static BlhPoint Wgs84ToWgs72(BlhPoint pWgs84)
        {
            BlhPoint delta = GetDeltas(pWgs84.ɸ);

            BlhPoint res = new BlhPoint();
            res.Assign(pWgs84);

            res.Id = pWgs84.Id;
            res.B = pWgs84.B - delta.B;
            res.L = pWgs84.L - delta.L;
            res.H = pWgs84.H - delta.H;
            return res;
        }

        #endregion
    }

}
