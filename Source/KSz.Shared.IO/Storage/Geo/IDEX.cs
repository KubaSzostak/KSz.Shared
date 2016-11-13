using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO
{

    /*
     
HEADER
	VERSION 1.20	
	SYSTEM	"0"
	UNITS
		ANGULAR GRADS
		LINEAR  METRE
		TEMP    CELSIUS
		PRESS   HPA
		TIME    DMY
	END UNITS
	PROJECT
		CREATION_DATE	01-01-2000/00:00:00.0
	END PROJECT
END HEADER
DATABASE
	POINTS(PointNo, PointID, East, North, Elevation, Code, Date, CLASS)
		1900001,	"D1965",	6535026.390000,	6042802.380000,	0.000000,	"",		,	FIX;
		1900003,	"990",	6534991.920000,	6042798.240000,	0.000000,	"",		,	FIX;
		1900004,	"1002",	6535047.640000,	6042806.830000,	0.000000,	"",		,	FIX;
	END POINTS
END DATABASE

     */

    public class IDEXFileWriter : TextStorageWriter
    {

        public override void Init(Stream stream, string path)
        {
            base.Init(stream, path);

            Storage.WriteLine("HEADER");
            Storage.WriteLine("	VERSION 1.20	");
            Storage.WriteLine(@"	SYSTEM	""0""");
            Storage.WriteLine("	UNITS");
            Storage.WriteLine("		ANGULAR GRADS");
            Storage.WriteLine("		LINEAR  METRE");
            Storage.WriteLine("		TEMP    CELSIUS");
            Storage.WriteLine("		PRESS   HPA");
            Storage.WriteLine("		TIME    DMY");
            Storage.WriteLine("	END UNITS");
            Storage.WriteLine("	PROJECT");
            Storage.WriteLine("		CREATION_DATE	01-01-2000/00:00:00.0");
            Storage.WriteLine("	END PROJECT");
            Storage.WriteLine("END HEADER");

            Storage.WriteLine("DATABASE");
            Storage.WriteLine("	POINTS(PointNo, PointID, East, North, Elevation, Code, Date, CLASS)");
        }

        private int NextPointNo = 1900001;

        private string ToString(double v)
        {
            var res = v.ToString("0.000000");
            return res.Replace(',', '.');
        }


        // 		1900001,	"D1965",	6535026.390000,	6042802.380000,	0.000000,	"",		,	FIX;
        private string PointLineTemplate = "		{0},	\"{1}\",	{2},	{3},	{4},	\"{5}\",		,	FIX;";
        public void AddPoint(string number, double easting, double northing, double height, string code)
        {
            var s = string.Format(PointLineTemplate, NextPointNo++, number, ToString(easting), ToString(northing), ToString(height), code);
            Storage.WriteLine(s);
        }



        protected override void OnDisposing()
        {
            if (Storage == null)
                return;

            Storage.WriteLine("	END POINTS");
            Storage.WriteLine("END DATABASE");

            base.OnDisposing();
        }
    }
}
