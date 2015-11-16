using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace CoordinateSystems
{

    public abstract class CoordinateSystem
    {
        public static readonly UtmProjection Utm33 = UtmProjection.FromUtmZone(33);
        public static readonly UtmProjection Utm34 = UtmProjection.FromUtmZone(34);
        public static readonly UtmProjection Utm35 = UtmProjection.FromUtmZone(35);
        public static readonly PL1992Projection PL1992 = new PL1992Projection();
        public static readonly PL2000Projection PL2000Zone5 = new PL2000Projection(15.0);
        public static readonly PL2000Projection PL2000Zone6 = new PL2000Projection(18.0);
        public static readonly PL2000Projection PL2000Zone7 = new PL2000Projection(21.0);
        public static readonly PL2000Projection PL2000Zone8 = new PL2000Projection(24.0);
        public static readonly Wgs84CoordinateSystem Wgs84 = new Wgs84CoordinateSystem();
        public static readonly WebMercatorProjection WebMercator = new WebMercatorProjection();
        public static CoordinateSystem Default = CoordinateSystem.Wgs84;
        public abstract PointBase SamplePoint
        {
            get;
        }
        public abstract BlhPoint ToWgs84(PointBase p);
        public abstract PointBase FromWgs84(BlhPoint p);
        public virtual bool CoodinatesValid(double n, double e)
        {
            return true;
        }
        public List<PointBase> ConvertTo(CoordinateSystem destCs, IEnumerable<PointBase> points)
        {
            List<PointBase> list = new List<PointBase>();
            foreach (PointBase current in points)
            {
                BlhPoint p = this.ToWgs84(current);
                PointBase pointBase = destCs.FromWgs84(p);
                pointBase.Id = current.Id;
                pointBase.AddTags(current);
                list.Add(pointBase);
            }
            return list;
        }
    }



    public class CoordinateSystemFormatPresenter : ObservableObject
    {
        private CoordinateSystem mCoordinateSystem = CoordinateSystem.Default;
        private string mDegreesPrecision = "0.000";
        private string mXYPrecision = "0.00";
        private bool mFlipCoordinates = false;
        private DegreesFormat mDegreesFormat = DegreesFormat.DegMinSec;
        
        public event EventHandler Changed;
        public IList<CoordinateSystem> CoordinateSystemValues
        {
            get;
            private set;
        }
        public CoordinateSystem CoordinateSystem
        {
            get
            {
                return this.mCoordinateSystem;
            }
            set
            {
                this.mCoordinateSystem = value;
                base.OnPropertyChanged<CoordinateSystem>(() => this.CoordinateSystem);
                base.OnPropertyChanged<string>(() => this.SampleText);
                base.OnPropertyChanged<bool>(() => this.IsDegreesFormatEnabled);
                base.OnPropertyChanged<string>(() => this.Precision);
                this.OnChanged();
            }
        }
        public IList<string> PrecisionValues
        {
            get;
            private set;
        }
        public string Precision
        {
            get
            {
                bool flag = this.CoordinateSystem is GeodeticCoordinateSystem;
                string result;
                if (flag)
                {
                    result = this.mDegreesPrecision;
                }
                else
                {
                    result = this.mXYPrecision;
                }
                return result;
            }
            set
            {
                bool flag = this.CoordinateSystem is GeodeticCoordinateSystem;
                if (flag)
                {
                    this.mDegreesPrecision = value;
                }
                else
                {
                    this.mXYPrecision = value;
                }
                base.OnPropertyChanged<string>(() => this.Precision);
                base.OnPropertyChanged<string>(() => this.SampleText);
            }
        }
        public string SampleText
        {
            get
            {
                PointBase samplePoint = this.CoordinateSystem.SamplePoint;
                samplePoint.Coord1.Precision = this.Precision;
                samplePoint.Coord2.Precision = this.Precision;
                bool flag = this.CoordinateSystem is GeodeticCoordinateSystem;
                if (flag)
                {
                    ((DegreesCoordinate)samplePoint.Coord1).DegreesFormat = this.DegreesFormat;
                    ((DegreesCoordinate)samplePoint.Coord2).DegreesFormat = this.DegreesFormat;
                }
                bool flipCoordinates = this.FlipCoordinates;
                string result;
                if (flipCoordinates)
                {
                    result = samplePoint.Coord2.Text + "  " + samplePoint.Coord1.Text;
                }
                else
                {
                    result = samplePoint.Coord1.Text + "  " + samplePoint.Coord2.Text;
                }
                return result;
            }
        }
        public bool FlipCoordinates
        {
            get
            {
                return this.mFlipCoordinates;
            }
            set
            {
                this.mFlipCoordinates = value;
                base.OnPropertyChanged<bool>(() => this.FlipCoordinates);
                base.OnPropertyChanged<string>(() => this.SampleText);
                this.OnChanged();
            }
        }
        public Array DegreesFormatValues
        {
            get;
            private set;
        }
        public DegreesFormat DegreesFormat
        {
            get
            {
                return this.mDegreesFormat;
            }
            set
            {
                this.mDegreesFormat = value;
                base.OnPropertyChanged<DegreesFormat>(() => this.DegreesFormat);
                base.OnPropertyChanged<string>(() => this.SampleText);
            }
        }
        public bool IsDegreesFormatEnabled
        {
            get
            {
                return this.CoordinateSystem is GeodeticCoordinateSystem;
            }
        }
        public CoordinateSystemFormatPresenter()
        {
            this.PrecisionValues = ListProvider.Create<string>(new string[]
            {
                "0",
                "0.0",
                "0.00",
                "0.000",
                "0.0000"
            });
            this.DegreesFormatValues = Enum.GetValues(typeof(DegreesFormat));
            this.CoordinateSystemValues = ListProvider.Create<CoordinateSystem>(new CoordinateSystem[]
            {
                CoordinateSystem.Wgs84,
                CoordinateSystem.PL1992,
                CoordinateSystem.PL2000Zone5,
                CoordinateSystem.PL2000Zone6,
                CoordinateSystem.PL2000Zone7,
                CoordinateSystem.PL2000Zone8,
                CoordinateSystem.Utm33,
                CoordinateSystem.Utm34,
                CoordinateSystem.Utm35
            });
            this.LoadFromDefaults();
        }
        public void LoadFromDefaults()
        {
            this.mDegreesFormat = DegreesCoordinate.DefaultDegreesFormat;
            this.mDegreesPrecision = DegreesCoordinate.DefaultPrecision;
            this.mXYPrecision = XYCoordinate.DefaultPrecision;
            this.mCoordinateSystem = CoordinateSystem.Default;
        }
        public void SaveToDefaults()
        {
            DegreesCoordinate.DefaultDegreesFormat = this.mDegreesFormat;
            DegreesCoordinate.DefaultPrecision = this.mDegreesPrecision;
            XYCoordinate.DefaultPrecision = this.mXYPrecision;
            CoordinateSystem.Default = this.mCoordinateSystem;
        }
        protected void OnChanged()
        {
            bool flag = this.Changed != null;
            if (flag)
            {
                this.Changed(this, EventArgs.Empty);
            }
        }
    }








}
