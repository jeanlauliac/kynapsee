using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.PowerPoint;

namespace Kynapsee
{
    public static class TagsExtensions
    {
        public static void Add<T>(this Tags tags, string name, T value)
        {
            tags.Add(name, Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        public static T Get<T>(this Tags tags, string name)
        {
            return (T)Convert.ChangeType(tags[name], typeof (T), CultureInfo.InvariantCulture);
        }

        public static T Get<T>(this Tags tags, string name, T def)
        {
            if (tags[name] == "")
                return def;
            return tags.Get<T>(name);
        }
    
    }
}
