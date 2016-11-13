using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace System.IO
{

    public class PointStorageProvider<T> : StorageProvider<T> where T: PointBase
    {
        public PointStorageProvider()
        {
            SampleData = GetDefaultSampleData();
            OnSettingsChanged();
        }

        protected T GetDefaultSampleData()
        {
            var res = Activator.CreateInstance<T>();
            res.Coord1.Value = 11.11111111111111;
            res.Coord2.Value = 22.22222222222222;
            res.Coord3.Value = 33.33333333333333;
            res.Id = "Pt100";
            res.Code = "S_CODE";

            return res;
        }
    }



}
