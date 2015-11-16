using System;
using System.Collections.Generic;
using System.Text;

namespace System
{


    public enum AngleUnits { Radians, Grads, Degrees };
    public enum DegreesFormat { Deg, DegMin, DegMinSec };

    public struct Angle
    {
        private double Value; // Radians

        public const double RoGrad = 200.0 / Calc.PI;
        public const double RoDeg = 180.0 / Calc.PI;
        public const double DblPi = Calc.PI * 2.0;

        public Angle(double radValue)
        {
            Value = 0.0;
            Value = NormalizeAngle(radValue);
        }
        public Angle(double value, AngleUnits units)
        {
            Value = 0.0;
            SetAngle(value, units);
        }
        public Angle(double deg, double min, double sec)
        {
            Value = 0.0;
            AsDeg = deg + min / 60.0 + sec / 3600.0;
        }


        public static Angle Mean(Angle a1, Angle a2)
        {
            double rad1 = NormalizeAngle(a1.AsRad);
            double rad2 = NormalizeAngle(a2.AsRad);

            // np. a1 = 399g, a2 = 3g => 1. A nie 399+3/2=201
            if (rad1 > rad2)
                rad2 = rad2 + DblPi;

            double mean = (rad1 + rad2) / 2.0;
            return new Angle(mean);
        }

        #region Operators

        public static implicit operator Angle(double value)
        {
            return new Angle(value);
        }

        public static implicit operator double (Angle value)
        {
            return value.Value;
        }

        public static Angle operator +(Angle v1, Angle v2)
        {
            return new Angle(v1.Value + v2.Value);
        }

        public static Angle operator -(Angle v1, Angle v2)
        {
            return new Angle(v1.Value - v2.Value);
        }

        #endregion

        internal double GetAngle(AngleUnits units)
        {
            switch (units)
            {
                case AngleUnits.Radians:
                    return AsRad;
                case AngleUnits.Grads:
                    return AsGrad;
                case AngleUnits.Degrees:
                    return AsDeg;
                default:
                    throw new Exception("Unsupported AngleUnits: " + units.ToString());
            }
        }

        internal void SetAngle(double value, AngleUnits units)
        {
            switch (units)
            {
                case AngleUnits.Radians:
                    AsRad = value;
                    break;
                case AngleUnits.Grads:
                    AsGrad = value;
                    break;
                case AngleUnits.Degrees:
                    AsDeg = value;
                    break;
                default:
                    throw new Exception("Unsupported AngleUnits: " + units.ToString());
            }
        }


        public double AsGrad
        {
            get
            {
                if (double.IsNaN(AsRad))
                    return double.NaN;
                return AsRad * RoGrad;
            }
            set
            {
                if (double.IsNaN(value))
                    AsRad = double.NaN;
                else
                    AsRad = value / RoGrad;
            }
        }

        public double AsDeg
        {
            get
            {
                if (double.IsNaN(AsRad))
                    return double.NaN;
                return AsRad * RoDeg;
            }
            set
            {
                if (double.IsNaN(value))
                    AsRad = double.NaN;
                else
                    AsRad = value / RoDeg;
            }
        }

        public double AsRad
        {
            get { return Value; }
            set { Value = NormalizeAngle(value); }
        }

        public static double NormalizeAngle(double radValue)
        {
            if (double.IsNaN(radValue))
                return radValue;

            if (radValue > DblPi * 3)
                throw new Exception("Invalid angle value: " + Calc.RadToDeg(radValue).ToString("0.00") + "°");

            if (radValue > DblPi * 100) // With large (eg. 555555555) values there is stack exception
                return NormalizeAngle(radValue - DblPi * 100);
            if (radValue > DblPi)
                return NormalizeAngle(radValue - DblPi);
            else if (radValue < -DblPi * 100)
                return NormalizeAngle(radValue + DblPi * 100);
            else if (radValue < 0)
                return NormalizeAngle(radValue + DblPi);
            else
                return radValue;
        }

        #region *** String convertion ***

        public static Angle FromString(string s, AngleUnits units)
        {
            Angle res = new Angle();
            switch (units)
            {
                case AngleUnits.Radians:
                    res.AsRad = s.ToDouble();
                    break;
                case AngleUnits.Grads:
                    res.AsGrad = s.ToDouble();
                    break;
                case AngleUnits.Degrees:
                    res.AsDeg = ConvertDegText(s);
                    break;
                default:
                    throw new Exception("Angle.FormString(). Not supported units: " + units.ToString());
            }
            return res;
        }

        private static double ConvertDegText(string s)
        {
            //TODO: http://www.codeproject.com/Articles/18622/Coordinate-and-CoordinateList-classes-ISO-com

            s = s.Trim().Replace("''", @""""); // Podwójny znak minuty na znak sekundy
            // s = s.RemoveChars(' '); Nie usuwaj spacji

            // dodatki typu W,E,N,S
            s = s.TrimEnd('N');
            s = s.TrimEnd('E');
            // W,S -> zmień znak

            char[] separators = @" °°ºo-–�ř""˝''´’:/  ".ToCharArray();
            string[] words = s.Trim().Split(separators, StringSplitOptions.RemoveEmptyEntries);
            double deg = words[0].ToDouble();
            double min = 0.0;
            double sec = 0.0;
            if (words.Length > 1)
                min = words[1].ToDouble();
            if (words.Length > 2)
                sec = words[2].ToDouble();

            // a co jeżeli to jest -52o24'57.7895", 
            double sign = 1.0; // muszę recznie ustawiać bo dla Math.Sign(0.0) wszystko będzie 0
            if (Math.Sign(deg) < 0)
                sign = -1.0;

            // 52o24'57.7895"
            double angle = deg + sign * min / 60.0 + sign * sec / 3600.0;
            return angle;

        }

        public override string ToString()
        {
            if (double.IsNaN(Value))
                return "NaN";
            //return ToString(SysUtils.Formats.AngleUnits, SysUtils.Formats.AnglePrecision);
            return AsRad.ToString("0.00000000") + "rad, (" + AsGrad.ToString("0.0000") + "g, " + ToDegMinSecString("0.000") + ")";
        }

        public string ToString(AngleUnits units, string precision, DegreesFormat degFormat)
        {
            switch (units)
            {
                case AngleUnits.Radians:
                    return this.AsRad.ToString(precision);

                case AngleUnits.Grads:
                    return this.AsGrad.ToString(precision);

                case AngleUnits.Degrees:
                    return ToDegreesString(degFormat, precision);
            }
            return this.Value.ToString(precision);
        }

        public string ToString(AngleUnits units)
        {
            return ToString(units, null, DegreesFormat.DegMinSec);
        }

        public string ToDegreesString(DegreesFormat degFormat, string precision)
        {
            if (double.IsNaN(AsRad))
                return "NaN°";
            if (degFormat == DegreesFormat.Deg)
            {
                return this.AsDeg.ToString(precision) + "°";
            }

            // 0.000 -> 00.000
            if (precision == "0")
                precision = "00";
            else
                precision = precision.Replace("0.", "00.");

            double deg = 0.0;
            double min = 0.0;
            double sec = 0.0;

            // 52.9233222
            double angle = this.AsDeg;

            deg = Math.Floor(angle);

            angle = (angle - deg) * 60.0;
            min = angle;
            if (degFormat == DegreesFormat.DegMin)
            {
                // 52°59.9999 może wyświetlić jako 52°60.0 zamiast 53°00.0 dlatego
                if (min.ToString(precision) == 60.ToString(precision))
                {
                    min = 0;
                    deg = deg + 1;
                }
                return deg.ToString("0") + "°" + min.ToString(precision) + "'";
            }
            min = Math.Floor(angle);


            angle = (angle - min) * 60.0;
            sec = angle;

            // dla kąta 52°09'59.999" może wyświetlić 52°09'60" zamiast 52°10'00" dlatego
            if (sec.ToString(precision) == 60.ToString(precision))
            {
                sec = 0;
                min = min + 1.0;
            }
            if (min.ToString("00") == 60.ToString("00"))
            {
                min = 0.0;
                deg = deg + 1;
            }

            return deg.ToString("0") + "°" + min.ToString("00") + "'" + sec.ToString(precision) + @"""";
        }

        public string ToDegMinSecString(string secFormat)
        {
            return ToDegreesString(DegreesFormat.DegMinSec, secFormat);
        }

        public string ToDegMinSecString()
        {
            return ToDegMinSecString("0.0000");
        }
        #endregion

        #region *** Trygonometry ***

        public double Sin
        {
            get { return Math.Sin(Value); }
        }
        public double Cos
        {
            get { return Math.Cos(Value); }
        }
        public double Tan
        {
            get { return Math.Tan(Value); }
        }
        public double Acos
        {
            get { return Math.Acos(Value); }
        }
        public double Asin
        {
            get { return Math.Asin(Value); }
        }
        public double Atan
        {
            get { return Math.Atan(Value); }
        }
        public double Cosh
        {
            get { return Math.Cosh(Value); }
        }
        public double Sinh
        {
            get { return Math.Sinh(Value); }
        }
        public double Tanh
        {
            get { return Math.Tanh(Value); }
        }

        #endregion

    }

}
