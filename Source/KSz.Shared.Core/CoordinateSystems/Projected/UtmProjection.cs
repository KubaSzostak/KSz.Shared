using System;
using System.Collections.Generic;
using System.Text;

namespace CoordinateSystems
{

    public class UtmProjection : GaussKrugerProjection
    {

        public double Scale { get; protected set; }
        public double NorthOrigin { get; protected set; }
        public double EastOrigin { get; protected set; }

        public UtmProjection(double l0Deg)
        {
            //współrzędne punktów początkowych układów współrzędnych prostokątnych w każdym pasie: E = 500 km, N = 0 km,
            Scale = 0.9996;
            NorthOrigin = 0.0;
            EastOrigin = 500000.0;
            this.LOrigin = Calc.DegToRad(l0Deg);

            InitBounds(0.0, l0Deg - 3.0, 84.0, l0Deg + 3.0);
        }

        public static UtmProjection FromUtmZone(int zone)
        {
            //double l0Deg = (zone - 31) * 6 + 3;
            double l0Deg = (-180 + (zone - 1) * 6) + 3;
            var res = new UtmProjection(l0Deg);
            return res;
        }

        public override string ToString()
        {
            return "UTM/" + GetZoneNo(Calc.RadToDeg(LOrigin));
        }

        public static string GetZoneNo(double lDeg)
        {
            double zoneStartL = -180.0;
            for (int zoneNo = 1; zoneNo <= 60 + 1; zoneNo++)
            {
                zoneStartL = zoneStartL + 6.0;
                if (lDeg < zoneStartL)
                {
                    return zoneNo.ToText();
                }
            }
            return "0";
        }

        public static string GetZoneChar(double bDeg)
        {
            Dictionary<double, string> charsDict = new Dictionary<double, string>();
            charsDict[0] = "N";
            charsDict[8] = "P";
            charsDict[16] = "Q";
            charsDict[24] = "R";
            charsDict[32] = "S";
            charsDict[40] = "T";
            charsDict[48] = "U";
            charsDict[56] = "V";
            charsDict[64] = "W";
            charsDict[72] = "X";

            foreach (var kv in charsDict)
            {
                if (bDeg > kv.Key)
                    return kv.Value;
            }

            return "";
        }

        // stefa ma nr 1-60
        public UtmProjection(int zone)
            : this(GetUtmL0(zone))
        {
        }

        private static double GetUtmL0(int zone)
        {
            // odwzorowanie dzieli powierzchnię elipsoidy na 60 stref po 6° długości każda, 
            // ponumerowanych od 1 do 60 (zaczynając od południka 180° – wzrost numerów na wschód, 
            // czyli pierwszy południk osiowy to 177° W), 
            // wyjątkiem jest strefa 32 (między 56° i 64°szerokości geograficznej północnej), 
            // która jest rozszerzona do 9° (od 3° do 12° E), co skutkuje tym, iż strefa 31 ma szerokość 3° (od 0° do 3° W)

            double no = zone;
            return 180 - zone * 6.0;
        }

        public override void GetBL(double N, double E, out double B, out double L)
        {
            base.GetBL(N, E, out B, out L);

            double xGK = (N - NorthOrigin) / Scale;
            double yGK = (E - EastOrigin) / Scale;
            base.GetBL(xGK, yGK, out B, out L);
        }

        public override void GetNE(double B, double L, out double N, out double E)
        {
            double xGK;
            double yGK;
            base.GetNE(B, L, out xGK, out yGK);

            N = Scale * xGK + NorthOrigin;
            E = Scale * yGK + EastOrigin;
        }
    }
}
