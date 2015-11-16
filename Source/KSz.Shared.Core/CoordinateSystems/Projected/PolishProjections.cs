using System;
using System.Collections.Generic;
using System.Text;

namespace CoordinateSystems
{


    public class PL1992Projection : UtmProjection
    {
        public PL1992Projection()
            : base(19.0)
        {
            Scale = 0.9993;
            NorthOrigin = -5300.0E+3;
            EastOrigin = 500.0E+3;

            InitBounds(49.000, 14.1400, 56.0, 24.1600);
        }

        public override string ToString()
        {
            return "PL-1992";
        }

        public override PointBase SamplePoint
        {
            get
            {
                return new NehPoint(469247.78, 741267.17);
            }
        }
    }

    public class PL2000Projection : UtmProjection
    {
        public PL2000Projection(double l0Deg)
            : base(l0Deg)
        {
            double c = l0Deg / 3.0;

            Scale = 0.999923;
            NorthOrigin = 0.0;
            EastOrigin = c * 1000.0E+3 + 500.0E+3;

            // Obszar Polski obejmują cztery pasy południkowe układu współrzędnych PL-2000 o rozciągłości równej 3º długości
            // geodezyjnej każdy, o południkach osiowych: 15ºE, 18ºE, 21ºE i 24ºE, oznaczane odpowiednio numerami: 5, 6, 7 i 8.
            Zone = Calc.Round(l0Deg / 3.0);

            //5:15 //14.1400, 50.2500, 16.5000, 54.5000 
            //6:18 //16.5000, 49.3300, 19.5000, 54.8300
            //7:21 //19.5000, 49.0900, 22.5000, 54.5000
            //8:24 //22.5000, 49.0300, 24.1600, 54.4500
            InitBounds(49.0, l0Deg - 1.7, 56.0, l0Deg + 1.7);
        }

        public int Zone { get; private set; }

        public override string ToString()
        {
            return "PL-2000/" + Zone.ToString("0"); ;
        }


    }

}
