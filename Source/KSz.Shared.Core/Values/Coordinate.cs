using System;
using System.Collections.Generic;
using System.Text;

namespace System
{

    public abstract class Coordinate : ObservableObject
    {
        private string mPrecision = null;
        public string Precision
        {
            get
            {
                if (mPrecision == null)
                    return GetDefaultPrecision();
                return mPrecision;
            }
            set
            {
                mPrecision = value;
                OnCoordinateChanged();
            }
        }

        protected virtual string GetDefaultPrecision()
        {
            return "0.00";
        }

        private double mValue;
        public virtual double Value
        {
            get { return mValue; }
            set
            {
                if (mValue != value)
                {
                    mValue = value;
                    OnCoordinateChanged();
                }
            }
        }

        protected virtual void OnCoordinateChanged()
        {
            OnPropertyChanged(() => Value);
            OnPropertyChanged(() => Text);
        }

        public string Text
        {
            get
            {
                if (double.IsNaN(Value))
                    return null;
                return CoordToString(Value);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    Value = double.NaN;
                else
                    Value = StringToCoord(value);
            }
        }

        protected virtual string CoordToString(double v)
        {
            return v.ToString(Precision);
        }

        protected virtual double StringToCoord(string v)
        {
            return v.ToDouble();
        }

        public override string ToString()
        {
            return Text;
        }
    }

    public class XYCoordinate : Coordinate
    {
        public static string DefaultPrecision = "0.00";

        protected override string GetDefaultPrecision()
        {
            return DefaultPrecision;
        }

    }

    public class HCoordinate : Coordinate
    {
        public static string DefaultPrecision = "0.000";

        protected override string GetDefaultPrecision()
        {
            return DefaultPrecision;
        }
    }

    public class DegreesCoordinate : Coordinate
    {
        public static string DefaultPrecision = "0.000";
        public static DegreesFormat DefaultDegreesFormat = DegreesFormat.DegMinSec;

        public const double MaxLatitude = 90.0;
        public const double MinLatitude = -90.0;
        public const double MaxLongitude = 180.0;
        public const double MinLongitude = -180.0;

        protected override string GetDefaultPrecision()
        {
            return DefaultPrecision;
        }

        protected override void OnCoordinateChanged()
        {
            base.OnCoordinateChanged();
            OnPropertyChanged(() => Angle);
        }

        public override double Value
        {
            get { return mAngle.AsDeg; }
            set
            {
                if (mAngle.AsDeg != value)
                {
                    mAngle.AsDeg = value;
                    OnCoordinateChanged();
                }
            }
        }

        private Angle mAngle;
        public Angle Angle
        {
            get { return mAngle; }
            set
            {
                mAngle = value;
                OnCoordinateChanged();
            }
        }


        private DegreesFormat? mDegFormat = null;
        public DegreesFormat DegreesFormat
        {
            get
            {
                if (mDegFormat.HasValue)
                    return mDegFormat.Value;
                else
                    return DefaultDegreesFormat;
            }
            set
            {
                mDegFormat = value;
                OnCoordinateChanged();
            }
        }

        protected override double StringToCoord(string v)
        {
            return Angle.FromString(v, AngleUnits.Degrees).AsDeg;
        }

        protected override string CoordToString(double v)
        {
            var a = new Angle(v, AngleUnits.Degrees);
            return a.ToString(AngleUnits.Degrees, this.Precision, this.DegreesFormat);
        }
    }
}
