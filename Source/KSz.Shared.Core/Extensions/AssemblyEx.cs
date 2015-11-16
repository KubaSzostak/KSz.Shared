using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace System
{
    public static class AssemblyEx
    {
        public static Version Version(this Assembly asm)
        {
            //return asm.GetName().Version;
            return new AssemblyName(asm.FullName).Version;
        }

        public static DateTime VersionDate(this Assembly asm)
        {
            var ver = asm.Version();

            var d = new DateTime(2000, 1, 1); //Build dates start from 01/01/2000
            d = d.AddDays(ver.Build);
            d = d.AddSeconds(ver.Revision * 2);

            if (d.Year > 2010)
                return d;

            return new DateTime(1, 1, 1);

        }
        
        /// <summary>
        /// Returns version in format vMajor.vMinor.YY.MMDD
        /// </summary>
        public static Version VersionDateMix(this Assembly asm)
        {
            var ver = asm.Version();
            var vDate = asm.VersionDate();

            var build = ver.Build;
            var rev = ver.Revision;

            if (vDate.Year > 2010)
            {
                build = vDate.Year - 2000;
                    
                //eg. "802" for 2015-08-02
                var sRev = vDate.Month.ToString() + vDate.Day.ToString("00");
                rev = int.Parse(sRev);
            }

            return new Version(ver.Major, ver.Minor,  build, rev);
        }
    }
}
