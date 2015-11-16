using System;
using System.Collections.Generic;
using System.Text;

namespace CoordinateSystems
{

    public class WebMercatorProjection : ProjectedCoordinateSystem
    {
        // http://www.gal-systems.com/2011/07/convert-coordinates-between-web.html

        public WebMercatorProjection()
        {
            this.MaxE = 20037508.3427892;
            this.MinE = -this.MaxE;

            this.MaxN = 20037508.3427892;
            this.MinN = -this.MaxN;
        }

        public override void GetBL(double N, double E, out double B, out double L)
        {
            if (Math.Abs(E) < 180 && Math.Abs(N) < 90)
                throw new Exception("Invlaid WebMercator coordinates");

            if ((Math.Abs(E) > 20037508.3427892) || (Math.Abs(N) > 20037508.3427892))
                throw new Exception("Invlaid WebMercator coordinates" + N.ToString("0.000") + "; " + E.ToString("0.000"));

            double x = E;
            double y = N;
            double num3 = x / 6378137.0;
            double num4 = num3 * 57.295779513082323;
            double num5 = Math.Floor((double)((num4 + 180.0) / 360.0));
            double num6 = num4 - (num5 * 360.0);
            double num7 = 1.5707963267948966 - (2.0 * Math.Atan(Math.Exp((-1.0 * y) / 6378137.0)));

            L = Calc.DegToRad(num6);
            B = Calc.DegToRad(num7 * 57.295779513082323);
        }

        public override void GetNE(double B, double L, out double N, out double E)
        {
            B = Calc.RadToDeg(B);
            L = Calc.RadToDeg(L);
            if ((Math.Abs(L) > 180 || Math.Abs(B) > 90))
                throw new Exception(this.GetType().Name + ": Invlaid geographic coordinates: " + B.ToString("0.000") + "; " + L.ToString("0.000"));

            double num = L * 0.017453292519943295;
            double x = 6378137.0 * num;
            double a = B * 0.017453292519943295;

            E = x;
            N = 3189068.5 * Math.Log((1.0 + Math.Sin(a)) / (1.0 - Math.Sin(a)));
        }
    }
}
