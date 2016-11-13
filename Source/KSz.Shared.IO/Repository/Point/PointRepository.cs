using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace System.IO
{
    


    public class PointRepository<T> : StorageRepository<T> where T : PointBase
    {
        // Should be valid for:
        //   [X Y Z]
        //   [North East Height]
        //   [Latitude Longitude Elevation]
        //   ...
                
        public PointRepository()
        {
            this.Min = Activator.CreateInstance<T>();
            this.Max = Activator.CreateInstance<T>();
            this.Clear();
        }      

        public T Min { get; private set; }
        public T Max { get; private set; }

        public override void Add(T point)
        {
            base.Add(point);

            this.Max.Coord1.Value = Math.Max(this.Max.Coord1.Value, point.Coord1.Value);
            this.Max.Coord2.Value = Math.Max(this.Max.Coord2.Value, point.Coord2.Value);
            this.Max.Coord3.Value = Math.Max(this.Max.Coord3.Value, point.Coord3.Value);

            this.Min.Coord1.Value = Math.Min(this.Min.Coord1.Value, point.Coord1.Value);
            this.Min.Coord2.Value = Math.Min(this.Min.Coord2.Value, point.Coord2.Value);
            this.Min.Coord3.Value = Math.Min(this.Min.Coord3.Value, point.Coord3.Value);
        }

        public override void Clear()
        {
            base.Clear();

            this.Min.Coord1.Value = double.MaxValue;
            this.Min.Coord2.Value = double.MaxValue;
            this.Min.Coord3.Value = double.MaxValue;

            this.Max.Coord1.Value = double.MinValue;
            this.Max.Coord2.Value = double.MinValue;
            this.Max.Coord3.Value = double.MinValue;
        }
    }

    
    
    

}
