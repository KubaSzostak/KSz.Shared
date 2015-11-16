using System;
using System.Collections.Generic;
using System.Text;

namespace CoordinateSystems
{


    public abstract class ProjectedCoordinateSystem : CoordinateSystem
    {

        public ProjectedCoordinateSystem()
        {
            Ellipsoid = CoordinateSystems.Ellipsoid.WGS84;

            MinE = double.NaN;
            MinN = double.NaN;
            MaxE = double.NaN;
            MaxN = double.NaN;
        }

        public double MinE { get; protected set; }
        public double MinN { get; protected set; }
        public double MaxE { get; protected set; }
        public double MaxN { get; protected set; }

        public Ellipsoid Ellipsoid { get; protected set; }

        private NehPoint mSamplePoint;
        public override PointBase SamplePoint
        {
            get
            {
                if (mSamplePoint == null)
                {
                    if (double.IsNaN(MinE) || double.IsNaN(MinN))
                        mSamplePoint = new NehPoint(12.123456789, 34.5678901234);
                    else
                        mSamplePoint = new NehPoint(0.5 * (MaxN + MinN), 0.5 * (MaxE + MinE));
                }
                return mSamplePoint;
            }
        }



        public override bool CoodinatesValid(double n, double e)
        {
            if (double.IsNaN(MinE) || double.IsNaN(MinN))
                return true;
            return Calc.IsBetween(MinN, MaxN, n) && Calc.IsBetween(MinE, MaxE, e);
        }

        protected void InitBounds(double minB_deg, double minL_deg, double maxB_deg, double maxL_deg)
        {
            double n, e;

            GetNE(Calc.DegToRad(minB_deg), Calc.DegToRad(minL_deg), out n, out e);
            MinE = e;
            MinN = n;

            GetNE(Calc.DegToRad(maxB_deg), Calc.DegToRad(maxL_deg), out n, out e);
            MaxE = e;
            MaxN = n;

            mSamplePoint = null;
        }

        public abstract void GetBL(double N, double E, out double B, out double L);
        //{
        //    B = 0.0;
        //    L = 0.0;
        //}

        public abstract void GetNE(double B, double L, out double N, out double E);
        //{
        //    N = 0.0;
        //    E = 0.0;
        //}

        public BlhPoint GetBL(string Id, string N, string E, string H)
        {
            NehPoint geoPoint = new NehPoint();
            geoPoint.Id = Id;
            geoPoint.N = N.ToDouble();
            geoPoint.E = E.ToDouble();
            geoPoint.H = H.ToDouble();

            return GetBL(geoPoint);
        }

        public NehPoint GetNE(string Id, string B, string L, string H)
        {
            BlhPoint blhPoint = new BlhPoint();
            blhPoint.Id = Id;
            blhPoint.B = Angle.FromString(B, AngleUnits.Degrees);
            blhPoint.L = Angle.FromString(B, AngleUnits.Degrees);
            blhPoint.H = H.ToDouble();

            return GetNE(blhPoint);
        }



        public BlhPoint GetBL(NehPoint pnt)
        {
            double n = pnt.N;
            double e = pnt.E;
            double b;
            double l;
            GetBL(n, e, out b, out l);

            BlhPoint res = new BlhPoint();
            res.Id = pnt.Id;
            res.B = b;
            res.L = l;
            res.H = pnt.H;

            return res;
        }

        public List<BlhPoint> GetBL(IEnumerable<NehPoint> points)
        {
            List<BlhPoint> res = new List<BlhPoint>();
            foreach (var p in points)
            {
                res.Add(GetBL(p));
            }
            return res;
        }

        public NehPoint GetNE(BlhPoint pnt)
        {
            double n;
            double e;
            double b = pnt.B;
            double l = pnt.L;
            GetNE(b, l, out n, out e);

            NehPoint res = new NehPoint();
            res.Id = pnt.Id;
            res.N = n;
            res.E = e;
            res.H = pnt.H;

            return res;
        }

        public List<NehPoint> GetNE(IEnumerable<BlhPoint> points)
        {
            List<NehPoint> res = new List<NehPoint>();
            foreach (var p in points)
            {
                res.Add(GetNE(p));
            }
            return res;
        }

        public List<NehPoint> GetNE(IEnumerable<XyzPoint> points)
        {
            List<NehPoint> res = new List<NehPoint>();
            foreach (var xyzP in points)
            {
                BlhPoint blhP = this.Ellipsoid.GetBlh(xyzP);
                NehPoint geoP = GetNE(blhP);
                res.Add(geoP);
            }
            return res;
        }

        public List<XyzPoint> GetXyz(IEnumerable<NehPoint> points)
        {
            List<XyzPoint> res = new List<XyzPoint>();
            foreach (var geoP in points)
            {
                BlhPoint blhP = this.GetBL(geoP);
                XyzPoint xyzP = this.Ellipsoid.GetXyz(blhP);
                res.Add(xyzP);
            }
            return res;
        }

        public override BlhPoint ToWgs84(PointBase p)
        {
            double b, l;
            GetBL(p.Coord1.Value, p.Coord2.Value, out b, out l);

            var res = new BlhPoint();
            res.Assign(p);
            res.B = b;
            res.L = l;
            return res;
        }

        public override PointBase FromWgs84(BlhPoint p)
        {
            double n, e;
            GetNE(p.B, p.L, out n, out e);
            var res = new NehPoint();
            res.Assign(p);
            res.N = n;
            res.E = e;
            return res;
        }
    }

}
