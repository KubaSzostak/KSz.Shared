using System;
using System.Collections.Generic;
using System.Text;

namespace CoordinateSystems
{

    public class Ellipsoid
    {
        /// <summary>
        /// Major semiaxis of the ellipsoid
        /// </summary>
        public readonly double a;

        /// <summary>
        /// Minor semiaxis of the ellipsoid
        /// </summary>
        public readonly double b;

        /// <summary>
        /// Flattering = (a-b)/a
        /// </summary>
        public readonly double f;

        public readonly double FlatteringFactor;

        /// <summary>
        /// Thrid flattering 
        /// </summary>
        public readonly double n;

        /// <summary>
        /// Eccentricity
        /// </summary>
        public readonly double e;

        /// <summary>
        /// Power of eccentricity (e)
        /// </summary>
        public readonly double e2;

        /// <summary>
        /// Power of second eccentricity (e&apos;)
        /// </summary>
        public readonly double e_2;

        public readonly double R0;



        //double eccSquared;
        //double eccPrimeSquared;


        public static Ellipsoid WGS72 { get; private set; }
        public static Ellipsoid WGS84 { get; private set; }
        public static Ellipsoid Bessel { get; private set; }


        static Ellipsoid()
        {
            WGS72 = new Ellipsoid(6378135.0, 298.26);
            WGS84 = new Ellipsoid(6378137.0, 298.257223563);
            Bessel = new Ellipsoid(6377397.155, 299.15281285);

        }

        public Ellipsoid(double semiMajorA, double flatteningFactor)
        {
            this.a = semiMajorA;
            this.FlatteringFactor = flatteningFactor;
            f = 1 / flatteningFactor;

            b = a - f * a;

            //var a2 = a * a;
            //var b2 = b * b;

            e2 = f * (2.0 - f);// (a2 - b2) / (a2);
            e = Math.Sqrt(e2);
            e_2 = e2 / (1.0 - e2);// (a2 - b2) / b2;

            n = f / (2.0 - f);
            R0 = (a / (1.0 + n)) * (
                1.0 + Math.Pow(n, 2) / 4.0 + Math.Pow(n, 4) / 64.0 + Math.Pow(n, 6) / 256.0 + Math.Pow(n, 8) / 16384.0);
        }

        /// <summary>
        /// Promień krzywizny przekroju w pierwszym wertykale wyrażony wzorem
        /// </summary>
        private double Rn(double B)
        {
            double sinB = Math.Sin(B);
            return a / Math.Sqrt(1 - e2 * sinB * sinB);
        }

        private double Rp(double B, double H)
        {
            return (Rn(B) + H) * Math.Cos(B);
        }

        public XyzPoint GetXyz(BlhPoint p)
        {
            XyzPoint res = new XyzPoint();
            double rn = Rn(p.B);
            double rp = (rn + p.H) * Math.Cos(p.B);

            res.Id = p.Id;
            res.X = rp * Math.Cos(p.L);
            res.Y = rp * Math.Sin(p.L);
            res.Z = (rn * (1 - e2) + p.H) * Math.Sin(p.B);


            return res;
        }

        public BlhPoint GetBlh(XyzPoint p)
        {
            BlhPoint res = new BlhPoint();
            double rp = Calc.Dist(p.X, p.Y);


            res.Id = p.Id;

            double B0 = EstimateB(p.Z, rp, 0.0); //Przybliżona wartość B
            res.B = IterateB(p.Z, rp, B0); // B obliczone iteracyjnie - dokładne

            res.L = Math.Acos(p.X / rp);

            double sinB = Math.Sin(res.B);
            double rn = a / Math.Sqrt(1 - e2 * sinB * sinB);
            double DeltaR = rp - rn * Math.Cos(res.B);
            double DeltaZ = p.Z - rn * Math.Sin(res.B) + Delta(res.B);
            res.H = Calc.Dist(DeltaR, DeltaZ);
            if ((DeltaR < 0) || (DeltaZ < 0))
                res.H = -res.H;


            return res;
        }

        public List<XyzPoint> GetXyz(IEnumerable<BlhPoint> points)
        {
            List<XyzPoint> res = new List<XyzPoint>();
            foreach (var p in points)
            {
                res.Add(GetXyz(p));
            }
            return res;
        }

        public List<BlhPoint> GetBlh(IEnumerable<XyzPoint> points)
        {
            List<BlhPoint> res = new List<BlhPoint>();
            foreach (var p in points)
            {
                res.Add(GetBlh(p));
            }
            return res;
        }


        private double IterateB(double Z, double rp, double B0)
        {
            double delta = Delta(B0);
            double estimatedB = EstimateB(Z, rp, delta);

            if (Math.Abs(estimatedB - B0) < 0.000001)
                return estimatedB;
            else
                return IterateB(Z, rp, estimatedB);
        }

        private double EstimateB(double Z, double rp, double delta)
        {
            return Math.Atan((Z + delta) / rp);
        }

        private double Delta(double B)
        {
            double ck = e * Math.Sin(B);
            return a * e * ck / Math.Sqrt(1 - ck * ck);
        }

        // Promień krzywizny pierwszego wertykału
        public double N(double B)
        {
            double sinB = Math.Sin(B);
            return a / Math.Sqrt(1 - e2 * sinB * sinB);
        }

    }

}
