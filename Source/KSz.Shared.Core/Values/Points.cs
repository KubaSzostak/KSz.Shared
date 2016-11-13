using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;
using System.Linq;
using System.Collections;
using System.Collections.ObjectModel;


namespace System
{


    
    public enum PointType
    {
        Meas,
        ConnNEH, // Connection (Nawiązanie) NEH
        ConnNE,
        ConnH,
        FixedNEH,
        FixedNE,
        FixedH
    };


    public enum PointField
    {
        Id,
        Coord1,
        Coord2,
        Coord3,
        Code
    }

    public abstract class PointBase : ObservableObject, ITagedObject
    {

        public PointBase(Coordinate c1, Coordinate c2, Coordinate c3)
        {
            Coord1 = c1;
            Coord2 = c2;
            Coord3 = c3;
        }
        
        public virtual string GetFieldName(PointField field)
        {
            return field.ToString();
        }

        public string[] FieldNames(params PointField[] fields)
        {
            var names = new List<string>();
            foreach (var f in fields)
            {
                names.Add(GetFieldName(f));
            }
            return names.ToArray();
        }

        public string FieldNamesText(params PointField[] fields)
        {
            return string.Join(",", FieldNames(fields));
        }

        public string GetFieldValue(PointField field)
        {
            switch (field)
            {
                case PointField.Id: return this.Id;
                case PointField.Coord1: return this.Coord1.Text;
                case PointField.Coord2: return this.Coord2.Text;
                case PointField.Coord3: return this.Coord3.Text;
                case PointField.Code: return this.Code;
                default:
                    throw new ArgumentException("Unsupported field: " + field.ToString());
            }
        }

        public IList<string> GetFieldValues(IEnumerable<PointField> fields)
        {
            var values = new List<string>();
            foreach (var f in fields)
            {
                values.Add(GetFieldValue(f));
            }
            return values;
        }

        public IList<string> GetFieldValues(params PointField[] fields)
        {
            return GetFieldValues((IEnumerable<PointField>)fields);
        }

        public void SetFieldValue(PointField field, string value)
        {
            switch (field)
            {
                case PointField.Id:
                    this.Id = value;
                    break;
                case PointField.Coord1:
                    this.Coord1.Text = value;
                    break;
                case PointField.Coord2:
                    this.Coord2.Text = value;
                    break;
                case PointField.Coord3:
                    this.Coord3.Text = value;
                    break;
                case PointField.Code:
                    this.Code = value;
                    break;
                default:
                    throw new ArgumentException("Unsupported field: " + field.ToString());
            }
        }

        public readonly List<string> AdditionalFields = new List<string>();

        public Coordinate this[int coordNo]
        {
            get
            {
                switch (coordNo)
                {
                    case 1: return this.Coord1;
                    case 2: return this.Coord2;
                    case 3: return this.Coord3;
                    default: throw new ArgumentOutOfRangeException("Coordinate index out of range: " + coordNo.ToString());
                }
            }
        }

        private string mId;
        public string Id
        {
            get { return mId; }
            set
            {
                OnPropertyChanged(ref mId, value, nameof(Id));
            }
        }

        private string mCode;
        public string Code
        {
            get { return mCode; }
            set
            {
                OnPropertyChanged(ref mCode, value, nameof(Code));
            }
        }

        private IDictionary mTags = new Dictionary<object, object>();
        public IDictionary Tags
        {
            get { return mTags; }
        }

        public Coordinate Coord1 { get; private set; }
        public Coordinate Coord2 { get; private set; }
        public Coordinate Coord3 { get; private set; }
        
        public virtual void Assign(PointBase pnt)
        {
            this.Id = pnt.Id;

            AssignCoord(pnt.Coord1, this.Coord1);
            AssignCoord(pnt.Coord2, this.Coord2);
            AssignCoord(pnt.Coord3, this.Coord3);

            this.Code = pnt.Code;

            this.Tags.Clear();
            foreach (var k in pnt.Tags.Keys)
            {
                this.Tags[k] = pnt.Tags[k];
            }
        }

        private void AssignCoord(Coordinate src, Coordinate dst)
        {
            // Eg. XYCoordinate <-> AngleCoordinate
            if (src.GetType() == dst.GetType())
                dst.Value = src.Value;
        }

        public override string ToString()
        {
            return string.Format("Id: {0}; Coords: {1}, {2}, {3}; Code: '{4}'", Id, Coord1.Text, Coord2.Text, Coord3.Text, Code);
        }

    }


    public class TuplePoint : PointBase
    {
        public TuplePoint() 
            : base(new XYCoordinate(), new XYCoordinate(), new XYCoordinate())
        {

        }

        public TuplePoint(double c1, double c2, double c3) : this()
        {
            Coord1.Value = c1;
            Coord2.Value = c2;
            Coord3.Value = c3;
        }

    }

    public class XyzPoint : PointBase
    {

        public XyzPoint() : base(new XYCoordinate(), new XYCoordinate(), new XYCoordinate())
        { }
        public override string GetFieldName(PointField field)
        {
            switch (field)
            {
                case PointField.Coord1: return "X";
                case PointField.Coord2: return "Y";
                case PointField.Coord3: return "Z";
                default: return base.GetFieldName(field); 
            }
        }

        public double X { get { return Coord1.Value; } set { Coord1.Value = value; } }
        public double Y { get { return Coord2.Value; } set { Coord2.Value = value; } }
        public double Z { get { return Coord3.Value; } set { Coord3.Value = value; } }

        public override string ToString()
        {
            return string.Format("Id: '{0}';   X: {1};   Y: {2};   Z: {3};   Code: '{4}'", Id, Coord1.Text, Coord2.Text, Coord3.Text, Code);
        }

        public XyzPoint MultiplyVector(XyzPoint vector)
        {
            XyzPoint r = new XyzPoint();
            r.X = this.Y * vector.Z - this.Z * vector.Y;
            r.Y = this.Z * vector.X - this.X * vector.Z;
            r.Z = this.X * vector.Y - this.Y * vector.X;
            return r;
        }

        public double MultiplyScalar(XyzPoint vector)
        {
            return this.X * vector.X + this.Y * vector.Y + this.Z * vector.Z;
        }

        public static XyzPoint CenterOfGravity(params XyzPoint[] points)
        {
            double xSum = 0.0;
            double ySum = 0.0;
            double zSum = 0.0;
            double pntCount = (double)points.Length;

            foreach (XyzPoint pnt in points)
            {
                xSum += pnt.X;
                ySum += pnt.Y;
                zSum += pnt.Z;
            }
            XyzPoint res = new XyzPoint();
            res.X = xSum / pntCount;
            res.Y = ySum / pntCount;
            res.Z = zSum / pntCount;
            return res;
        }


        public XyzPoint VectorTo(XyzPoint pnt)
        {
            return pnt - this;
        }

        public double DistTo(XyzPoint pnt)
        {
            XyzPoint d = VectorTo(pnt);
            return Math.Sqrt(d.X * d.X + d.Y * d.Y + d.Z * d.Z);
        }

        public static XyzPoint operator +(XyzPoint p1, XyzPoint p2)
        {
            XyzPoint res = new XyzPoint();
            res.X = p1.X + p2.X;
            res.Y = p1.Y + p2.Y;
            res.Z = p1.Z + p2.Z;
            return res;
        }

        public static XyzPoint operator -(XyzPoint p1, XyzPoint p2)
        {
            XyzPoint res = new XyzPoint();
            res.X = p1.X - p2.X;
            res.Y = p1.Y - p2.Y;
            res.Z = p1.Z - p2.Z;
            return res;
        }

        public static XyzPoint operator *(double s, XyzPoint p)
        {
            XyzPoint res = new XyzPoint();
            res.X = s * p.X;
            res.Y = s * p.Y;
            res.Z = s * p.Z;
            return res;
        }
    }


    public class NehPoint : PointBase
    {

        public NehPoint()  : base(new XYCoordinate(), new XYCoordinate(), new HCoordinate())
        { }
        public override string GetFieldName(PointField field)
        {
            switch (field)
            {
                case PointField.Coord1: return "E";
                case PointField.Coord2: return "N";
                case PointField.Coord3: return "H";
                default: return base.GetFieldName(field);
            }
        }

        public NehPoint(double n, double e, double h, string code) : this()
        {
            N = n;
            E = e;
            H = h;
            Code = code;
        }

        public NehPoint(double n, double e)
            : this(n, e, 0.0, null)
        { }

        public NehPoint(double n, double e, double h)
            : this(n, e, h, null)
        { }

        public double N { get { return Coord2.Value; } set { Coord2.Value = value; } }
        public double Northing { get { return N; } set { this.N = value; } }

        public double E { get { return Coord1.Value; } set { Coord1.Value = value; } }
        public double Easting { get { return E; } set { this.E = value; } }

        /// <summary>
        /// Ortogonal Height
        /// </summary>
        public double H { get { return Coord3.Value; } set { Coord3.Value = value; } }

        public override string ToString()
        {
            return string.Format("Id: {0}, N: {1}, E: {2}, H: {3}, Code: '{4}'", Id, N, E, H, Code);
        }

        public NehPoint Move(double dist, Angle azimuth)
        {
            var res = new NehPoint();
            res.N = this.N + dist * Math.Cos(azimuth);
            res.E = this.E + dist * Math.Sin(azimuth);
            return res;
        }
    }


    // Latitude (North: 54.5), Longitude (East: 18.5)
    // Microsoft.Maps.MapControl: public Location (double latitude, double longitude)
    // Google Maps JavaScript: center: new google.maps.LatLng(54.5, 18.5)
    // leafletjs.com: L.latLng( <Number> latitude, <Number> longitude )

    public class BlhPoint : PointBase
    {

        public BlhPoint()
            : base(new DegreesCoordinate(), new DegreesCoordinate(), new HCoordinate())
        { }
        public override string GetFieldName(PointField field)
        {
            switch (field)
            {
                case PointField.Coord1: return "ɸ";
                case PointField.Coord2: return "λ";
                case PointField.Coord3: return "H";
                default: return base.GetFieldName(field);
            }
        }

        public BlhPoint(Angle b, Angle l) : this()
        {
            B = b;
            L = l;            
        }


        // Fi, ɸ, North
        public Angle B { get { return ((DegreesCoordinate)Coord1).Angle; } set { ((DegreesCoordinate)Coord1).Angle = value; } }
        public Angle ɸ { get { return B; } set { B = value; } }
        public Angle Latitude { get { return B; } set { B = value; } }
        public Angle Northing { get { return B; } set { B = value; } }


        // Lambda, λ, East
        public Angle L { get { return ((DegreesCoordinate)Coord2).Angle; } set { ((DegreesCoordinate)Coord2).Angle = value; } }
        public Angle λ { get { return L; } set { L = value; } }
        public Angle Longitude { get { return L; } set { L = value; } }
        public Angle Easting { get { return L; } set { L = value; } }

        /// <summary>
        /// Ellipsoidal Height
        /// </summary>
        public double H { get { return Coord3.Value; } set { Coord3.Value = value; } }


    }

    public class NehLine: ObservableObject
    {

        public NehLine(NehPoint start, NehPoint end)
        {
            mStartPoint = start;
            mEndPoint = end;
            mStartPoint.PropertyChanged += OnCoordinateChanged;
            mEndPoint.PropertyChanged += OnCoordinateChanged;
        }

        private NehPoint mStartPoint;
        public NehPoint StartPoint
        {
            get { return mStartPoint; }
            set
            {
                mStartPoint.PropertyChanged -= OnCoordinateChanged;
                mStartPoint = value;
                mStartPoint.PropertyChanged += OnCoordinateChanged;

                OnCoordinateChanged(this, null);
                OnPropertyChanged(ref mStartPoint, value, nameof(StartPoint));
            }
        }

        private NehPoint mEndPoint;
        public NehPoint EndPoint
        {
            get { return mEndPoint; }
            set
            {
                mStartPoint.PropertyChanged -= OnCoordinateChanged;
                mEndPoint = value;
                mStartPoint.PropertyChanged += OnCoordinateChanged;

                OnCoordinateChanged(this, null);
                OnPropertyChanged(ref mEndPoint, value, nameof(EndPoint));
            }
        }

        void OnCoordinateChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        public NehPoint Delta
        {
            get
            {
                var res = new NehPoint();
                res.N = EndPoint.N - StartPoint.N;
                res.E = EndPoint.E - StartPoint.E;
                res.H = EndPoint.H - StartPoint.H;
                return res;
            }
        }


        public double Length
        {
            get
            {
                var d = this.Delta;
                return Math.Sqrt(d.N * d.N + d.E * d.E + d.H * d.H);
            }
        }

        public double Length2D
        {
            get
            {
                var d = this.Delta;
                return Math.Sqrt(d.N * d.N + d.E * d.E);
            }
        }

        public Angle Azimuth
        {
            get
            {
                var d = this.Delta;

                double rad = Math.Atan2(d.E, d.N);
                if (rad < 0)
                    rad = rad + Angle.DblPi;
                return new Angle(rad);
            }
        }

        private double CosAz
        {
            get { return Delta.N / Length; }
        }

        private double SinAz
        {
            get { return Delta.E / Length; }
        }

        /// <summary>
        /// Parperdicular dinstance from point to Line
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public double OrtoDistTo(NehPoint point)
        {
            var dn = point.N - StartPoint.N;
            var de = point.E - StartPoint.E;

            return de * CosAz - dn * SinAz;    
        }

        //Current, bieząca
        /// <summary>
        /// Length from StartPoint to point moved parperdicular to Line
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public double AlongLengthTo(NehPoint point)
        {
            var dn = point.N - StartPoint.N;
            var de = point.E - StartPoint.E;

            return dn * CosAz + de * SinAz;  
        }

        public NehPoint MoveOrto(double alongLength, double ortoDist)
        {
            var dn = CosAz * alongLength - SinAz * ortoDist;
            var de = CosAz * ortoDist + SinAz * alongLength;

            return new NehPoint(this.StartPoint.N + dn, this.StartPoint.E + de);
        }


    }

    public static class PointEx
    {
        public static double Area(this IEnumerable<NehPoint> points)
        {
            var ptsList = points.ToList();
            if (ptsList.Count < 3)
                return 0;

            ptsList.Add(ptsList[0]);
            double minN = ptsList.Min(p => p.N);
            double minE = ptsList.Min(p => p.E);

            double res = 0.0;
            for (int i = 0; i < ptsList.Count-1; i++)
			{
			    var thisPt = ptsList[i];
                var nextPt = ptsList[i+1];
                // For accuracy reasons  use Min(N), Min(E) - all segmentArea would be closer to zero.
                // Instead of 482356.3 * 725079.4 it would be 356.3 * 79.4
                var dn = (thisPt.N - minN) + (nextPt.N - minN); // * 0.5;
                var de = (nextPt.E - minE) - (thisPt.E - minE);
                var segmentArea = dn * de;
                res += segmentArea;
			}
            return Math.Abs(res / 2.0); // res/2.0 Instead of multiple *0.5;
        }

        public static NehPoint Add(this ICollection<NehPoint> points, double n, double e, double h)
        {
            var res = new NehPoint(n, e, h);
            points.Add(res);
            res.Id = (points.Count -1).ToString();
            return res;
        }

        public static NehPoint Add(this ICollection<NehPoint> points, double n, double e)
        {
            return points.Add(n, e, 0.0);
        }
    }
}
