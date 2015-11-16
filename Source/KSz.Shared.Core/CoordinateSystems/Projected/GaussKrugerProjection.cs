using System;
using System.Collections.Generic;
using System.Text;

namespace CoordinateSystems
{

    public class GaussKrugerProjection : ProjectedCoordinateSystem
    {
        public double LOrigin { get; protected set; }

        private double[] LagrRev = new double[9];
        private double[] GkForv = new double[9]; // Forvard paramters
        private double[] GkRev = new double[9];  // Reverse paramteters

        public GaussKrugerProjection()
        {
            Ellipsoid = CoordinateSystems.Ellipsoid.WGS84;
            InitTransfParams();
        }



        public override string ToString()
        {
            return "Gauss Kruger: " + new Angle(LOrigin).AsDeg.ToString("0");
        }

        private void InitTransfParams()
        {
            //spherical to ellipsoidal geographical, KW p. 190--191, (61)-(62)
            // k
            double n = Ellipsoid.n;
            LagrRev[2] = n * (2.0 + n * (-2.0 / 3.0 + n * (-2.0 + n * 116.0 / 45.0)));
            LagrRev[4] = Math.Pow(n, 2) * (7.0 / 3.0 + n * (-8.0 / 5.0 + n * (-227.0 / 45.0)));
            LagrRev[6] = Math.Pow(n, 3) * (56.0 / 15.0 + n * (-136.0 / 35.0));
            LagrRev[8] = Math.Pow(n, 4) * 4279.0 / 630.0;

            //spherical to ellipsoidal N, E, KW p. 196, (69)
            // W
            GkForv[2] = n * (0.5 + n * (-2.0 / 3.0 + n * (5.0 / 16.0 + n * 41.0 / 180.0)));
            GkForv[4] = Math.Pow(n, 2) * (13.0 / 48.0 + n * (-3.0 / 5.0 + n * 557.0 / 1440.0));
            GkForv[6] = Math.Pow(n, 3) * (61.0 / 240.0 + n * (-103.0 / 140.0));
            GkForv[8] = Math.Pow(n, 4) * 49561.0 / 161280.0;

            //ellipsoidal to spherical N, E, KW p. 194, (65)
            // w
            GkRev[2] = n * (-0.5 + n * (2.0 / 3.0 + n * (-37.0 / 96.0 + n * 1.0 / 360.0)));
            GkRev[4] = Math.Pow(n, 2) * (-1.0 / 48.0 + n * (-1.0 / 15.0 + n * 437.0 / 1440.0));
            GkRev[6] = Math.Pow(n, 3) * (-17.0 / 480.0 + n * 37.0 / 840.0);
            GkRev[8] = Math.Pow(n, 4) * (-4397.0 / 161280.0);


            // WGS-84
            //LagrRev[2] = 0.3356551485597E-2;
            //LagrRev[4] = 0.6571873148459E-5;
            //LagrRev[6] = 0.1764656426454E-7;
            //LagrRev[8] = 0.5400482187760E-10;

            //GkRev[2] = -0.8377321681641E-3;
            //GkRev[4] = -0.5905869626083E-7;
            //GkRev[6] = -0.1673488904988E-9;
            //GkRev[8] = -0.2167737805597E-12;

            //GkForv[2] = 0.8377318247344E-3;
            //GkForv[4] = 0.7608527788826E-6;
            //GkForv[6] = 0.1197638019173E-8;
            //GkForv[8] = 0.2443376242510E-11;
        }


        private double GkSumX(double x, double y, double[] w)
        {
            double sum = 0.0;
            for (int i = 1; i <= 4; i++)
            {
                int i2 = i * 2;
                sum = sum + w[i2] * Math.Sin(i2 * x) * Math.Cosh(i2 * y);
            }
            return sum;
        }

        private double GkSumY(double x, double y, double[] w)
        {
            double sum = 0.0;
            for (int i = 1; i <= 4; i++)
            {
                int i2 = i * 2;
                sum = sum + w[i2] * Math.Cos(i2 * x) * Math.Sinh(i2 * y);
            }
            return sum;
        }

        private void GkForward(double mercatorX, double mercatorY, out double gkX, out double gkY)
        {
            double r0 = 1.0;

            gkX = mercatorX + GkSumX(mercatorX, mercatorY, GkForv);
            gkX = gkX * r0;

            gkY = mercatorY + GkSumY(mercatorX, mercatorY, GkForv);
            gkY = gkY * r0;

        }

        private void GkReverse(double gkX, double gkY, out double mercX, out double mercY)
        {
            double r0 = 1.0;

            mercX = gkX + GkSumX(gkX, gkY, GkRev);
            mercX = mercX * r0;

            mercY = gkY + GkSumY(gkX, gkY, GkRev);
            mercY = mercY * r0;
        }

        private Angle LagrangeOut(Angle B)
        {
            double U = 1 - Ellipsoid.e * Math.Sin(B);
            double V = 1 + Ellipsoid.e * Math.Sin(B);
            double K = Math.Pow(U / V, Ellipsoid.e / 2);
            double C = K * Math.Tan(B / 2 + Calc.PI / 4.0);
            double fi = 2 * Math.Atan(C) - Calc.PI / 2.0;
            return fi;
        }


        private Angle LagrangeBack(Angle fi)
        {
            double sum = 0.0;
            for (int i = 1; i <= 4; i++)
            {
                int i2 = i * 2;
                sum = sum + LagrRev[i2] * Math.Sin(i2 * fi);

            }
            return fi + sum;
        }

        private void MercatorOut(Angle fi, Angle DeltaL, out double mercatorX, out double mercatorY)
        {
            double r0 = 1.0;
            double p = Math.Sin(fi);
            double q = Math.Cos(fi) * Math.Cos(DeltaL);

            // Współrzędne dla elipsoidy jednostkowej, Dla zadanej Elipsoidy należy pomonożyć przez R0
            mercatorX = Math.Atan(p / q) * r0;

            double r = 1.0 + Math.Cos(fi) * Math.Sin(DeltaL);
            double s = 1.0 - Math.Cos(fi) * Math.Sin(DeltaL);
            mercatorY = 0.5 * Math.Log(r / s) * r0;
        }

        private void MercatorBack(double mercatorX, double mercatorY, out double fi, out double DeltaL)
        {
            double r0 = 1.0;
            double alpha = mercatorX / r0;
            double beta = mercatorY / r0;
            double w = 2.0 * Math.Atan(Math.Exp(beta)) - Calc.PI / 2.0;

            fi = Math.Asin(Math.Cos(w) * Math.Sin(alpha));
            DeltaL = Math.Atan(Math.Tan(w) / Math.Cos(alpha));
        }


        public override void GetBL(double N, double E, out double B, out double L)
        {
            double xMerc;
            double yMerc;

            // Oblicz współrzędne dla sferoidy jednostkowej
            N = N / Ellipsoid.R0;
            E = E / Ellipsoid.R0;
            GkReverse(N, E, out xMerc, out yMerc);

            double fi;
            double DeltaL;
            MercatorBack(xMerc, yMerc, out fi, out DeltaL);

            B = LagrangeBack(fi);
            L = DeltaL + LOrigin;
        }

        public override void GetNE(double B, double L, out double N, out double E)
        {
            // 1. Lagrange
            double fi = LagrangeOut(B);
            double DeltaL = L - LOrigin;


            // 2. Mercator
            // Współrzędne dla elipsoidy jednostkowej, Dla zadanej Elipsoidy należy pomonożyć przez R0
            double xMerc;
            double yMerc;
            MercatorOut(fi, DeltaL, out xMerc, out yMerc);

            // 3. Kruger
            double xGK;
            double yGK;
            GkForward(xMerc, yMerc, out xGK, out yGK);

            // To były współrzędne dla sferoidy jednostkowej
            N = xGK * Ellipsoid.R0;
            E = yGK * Ellipsoid.R0;
        }

    }
}
