using System;
using System.Collections.Generic;
using System.Text;

namespace CoordinateSystems
{

    public abstract class GeodeticCoordinateSystem : CoordinateSystem
    {
        private BlhPoint mSamplePoint = new BlhPoint(new Angle(54.123456, AngleUnits.Degrees), new Angle(18.4902345934, AngleUnits.Degrees));

        public override PointBase SamplePoint
        {
            get { return mSamplePoint; }
        }

        public override bool CoodinatesValid(double n, double e)
        {
            return (Math.Abs(n) <= 90) && (Math.Abs(e) <= 180);
        }
    }

}
