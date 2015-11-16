using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Threading;

namespace System
{

    // globalization vs. localization vs. internationalization http://www.siao2.com/2007/01/11/1449754.aspx

    public class Formats
    {

        public AngleUnits AngleUnits = AngleUnits.Grads;
        public AngleUnits BLUnits = AngleUnits.Degrees;


        static public readonly CultureInfo Culture;
        static public readonly NumberFormatInfo NumberFormat;
        static public readonly DateTimeFormatInfo DateTimeFormat;

        static Formats()
        {
            // Make CurrentCulture IsReadOnly == true;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Thread.CurrentThread.CurrentCulture.Name);

            Culture = Thread.CurrentThread.CurrentUICulture;
            NumberFormat = Culture.NumberFormat;
            DateTimeFormat = Culture.DateTimeFormat;

            NumberFormat.NumberDecimalSeparator = ".";
            NumberFormat.CurrencyDecimalSeparator = ".";
        }


        #region Format


        public bool HeightVisible { get; set; }

        [DefaultValue(".")]
        public string DecSep
        {
            get { return NumberFormat.NumberDecimalSeparator; }
            set
            {
                string v = value.Trim();
                if (v.StartsWith("','"))
                    v = ",";
                if (v.StartsWith("'.'"))
                    v = ".";
                NumberFormat.NumberDecimalSeparator = v;
            }
        }

        [DefaultValue("dd-MM-yyyy")]
        public string DateFormat
        {
            get { return DateTimeFormat.ShortDatePattern; }
            set { DateTimeFormat.ShortDatePattern = value; }
        }

        [DefaultValue("HH:mm")]
        public string TimeFormat
        {
            get { return DateTimeFormat.ShortTimePattern; }
            set { DateTimeFormat.ShortTimePattern = value; }
        }

        #endregion
    }


}
