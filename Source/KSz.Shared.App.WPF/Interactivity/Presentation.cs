using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;

namespace System.Windows
{

    public class ItemGroup
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class ItemDescriptor<T>
    {

        public ItemDescriptor(T item, ItemGroup group)
        {
            this.Item = item;
            this.Group = group;
        }

        public ItemGroup Group { get; set; }
        public T Item { get; private set; }

        public class GroupDescription : System.ComponentModel.GroupDescription
        {
            public readonly List<ItemDescriptor<T>> Items = new List<ItemDescriptor<T>>();
            Dictionary<string, ItemGroup> Groups = new Dictionary<string, ItemGroup>();
            
            public override object GroupNameFromItem(object item, int level, Globalization.CultureInfo culture)
            {
                var dsr = item as ItemDescriptor<T>;
                if (dsr == null)
                    throw new NotSupportedException("Only items of type "+typeof(T).Name +" are supported.");
                return dsr.Group;
            }

            public void Add(T item, ItemGroup group)
            {
                if (!Groups.TryGetValue(group.Name, out group)) // W razie czego użyj istniejącej grupy
                {
                    Groups[group.Name] = group;
                }

                Items.Add(new ItemDescriptor<T>(item, group));
            }

            public void Add(T item, string groupName)
            {
                ItemGroup group = null;
                if (!Groups.TryGetValue(groupName, out group)){
                    group = new ItemGroup() { Name = groupName };
                    Groups[groupName] = group;
                }
                Items.Add(new ItemDescriptor<T>(item, group));
            }
        }

        
    }
}
