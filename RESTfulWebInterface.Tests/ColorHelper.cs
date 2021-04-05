using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using RESTfulWebInterface.Models;

namespace RESTfulWebInterface.Tests
{
    public static class ColorHelper
    {
        public static string ToDisplayString(this Color color)
        {
            var colorType = typeof(Color);
            var mi = colorType.GetMember(color.ToString()).Single();
            var attr = (EnumMemberAttribute)mi.GetCustomAttributes(typeof(EnumMemberAttribute), false).Single();
            return attr.Value;
        }
    }
}
