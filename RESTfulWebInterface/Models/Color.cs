using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace RESTfulWebInterface.Models
{
    public enum Color
    {
        [EnumMember(Value = "blau")]
        Blue = 1,
        [EnumMember(Value = "grün")]
        Green = 2,
        [EnumMember(Value = "violett")]
        Violet = 3,
        [EnumMember(Value = "rot")]
        Red = 4,
        [EnumMember(Value = "gelb")]
        Yellow = 5,
        [EnumMember(Value = "türkis")]
        Turquoise = 6,
        [EnumMember(Value = "weiß")]
        White = 7
    }

    static public class ColorHelper
    {
        public static Color? TryParseEnumName(string name)
        {
            var enumType = typeof(Color);
            foreach (var enumName in Enum.GetNames(enumType))
            {
                var enumMemberAttribute =
                    ((EnumMemberAttribute[])
                        enumType
                          .GetField(enumName)!//что это за знак восклицания?
                          .GetCustomAttributes(typeof(EnumMemberAttribute), true))
                          .Single();
                if (enumMemberAttribute.Value == name)
                    return Enum.Parse<Color>(enumName);
            }
            return null;
        }
    }
}
